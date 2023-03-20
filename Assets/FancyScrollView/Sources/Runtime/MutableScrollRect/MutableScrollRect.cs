using System;
using System.Collections.Generic;
using System.Linq;
using EasingCore;
using UnityEngine;

namespace FancyScrollView
{
    [RequireComponent(typeof(Scroller))]
    public abstract class MutableScrollRect<TItemData, TContext> : MutableScrollView<TItemData, TContext>
        where TContext : class, IMutableScrollRectContext, new()
    {
        protected virtual bool Scrollable => MaxScrollPosition > 0f;

        Scroller cachedScroller;

        protected Scroller Scroller => cachedScroller ?? (cachedScroller = GetComponent<Scroller>());

        float ScrollLength = 0f;

        /// <summary>
        /// view port length.(unit:cell count)
        /// </summary>
        float ViewportLength => ScrollLength - reuseCellMarginCount * 2f;

        /// <summary>
        /// padding head length.(unit:cell count)
        /// </summary>
        float PaddingHeadLength => (paddingHead - spacing * 0.5f) / (flex + spacing);
        
        float HeadLength => head>0?(head - spacing * 0.5f) / (flex + spacing) : 0f;

        /// <summary>
        /// max scrollable cell count.
        /// </summary>
        float MaxScrollPosition => flexCount
                                   - ScrollLength
                                   + reuseCellMarginCount * 2f
                                   + (paddingHead + paddingTail + head - spacing) / (flex + spacing);

        float flexCount => Mathf.Ceil(ItemMappings.Sum(c => c.CellSize) / flex);  
        /// <inheritdoc/>
        protected override void Initialize()
        {
            base.Initialize();
            Debug.Log("Initialize");

            Context.ScrollDirection = Scroller.ScrollDirection;
            Context.CalculateScrollSize = () => {  
                var interval = flex + spacing;
                var reuseMargin = interval * reuseCellMarginCount;
                var scrollSize = Scroller.ViewportSize + interval + reuseMargin * 2f;
                return (scrollSize, reuseMargin);
            };
            
            Context.OnCellSizeChanged = OnCellSizeChanged;

            AdjustCellIntervalAndScrollOffset();
            Scroller.OnValueChanged(OnScrollerValueChanged);
        }

        /// <summary>
        /// update cell size and relayout.
        /// </summary>
        /// <param name="index">index of data</param>
        /// <param name="v"></param>
        private void OnCellSizeChanged(int index,Vector3 v)
        {
            Debug.Log($"OnCellSizeChanged:{index} {v}");
            ItemMappings[index].CellSize = v.y;
            pool[CircularIndex(index,pool.Count)].CellSize = v.y;
            Relayout();
        }

        /// <summary>
        /// scroll position changed event handler.
        /// </summary>
        /// <param name="p"></param>
        void OnScrollerValueChanged(float p)
        {
            var position = ToFancyScrollViewPosition(Scrollable ? p : 0);
            Debug.Log($"p:{p} position:{position}");
            base.UpdatePosition(position);

            if (Scroller.Scrollbar)
            {
                if (p > flexCount-1)
                {
                    ShrinkScrollbar(p - (flexCount - 1));
                }
                else if (p < 0f)
                {
                    ShrinkScrollbar(-p);
                }
            }
        }

        /// <summary>
        /// update scroll bar size.
        /// </summary>
        /// <param name="offset"></param>
        void ShrinkScrollbar(float offset)
        {
            var scale = 1f - ToFancyScrollViewPosition(offset) / (ViewportLength - PaddingHeadLength);
            UpdateScrollbarSize((ViewportLength - PaddingHeadLength) * scale);
        }

        /// <inheritdoc/>
        protected override void Refresh()
        {
            AdjustCellIntervalAndScrollOffset();
            RefreshScroller();
            base.Refresh();
        }

        /// <inheritdoc/>
        protected override void Relayout()
        {
            AdjustCellIntervalAndScrollOffset();
            RefreshScroller();
            base.Relayout();
        }

        /// <summary>
        /// refresh scroller.
        /// </summary>
        protected void RefreshScroller()
        {
            Scroller.Draggable = Scrollable;
            Scroller.ScrollSensitivity = ToScrollerPosition(ViewportLength - PaddingHeadLength);
            Scroller.Position = ToScrollerPosition(currentPosition);

            if (Scroller.Scrollbar)
            {
                Scroller.Scrollbar.gameObject.SetActive(Scrollable);
                UpdateScrollbarSize(ViewportLength);
            }
        }

        /// <summary>
        /// update datasource and prefab mappings with cell size.
        /// </summary>
        /// <param name="items"></param>
        /// <param name="mappings"></param>
        protected override void UpdateContents(IList<TItemData> items,IList<MutablePrefabMapping> mappings)
        {
            AdjustCellIntervalAndScrollOffset();
            base.UpdateContents(items,mappings);

            Scroller.SetTotalCount(items.Count);
            RefreshScroller();
        }

        /// <summary>
        /// update Scroller position.
        /// </summary>
        /// <param name="position"></param>
        protected new void UpdatePosition(float position)
        {
            Scroller.Position = ToScrollerPosition(position, 0.5f);
        }

        /// <summary>
        /// jump to specified item with alignment.(range:0-1)
        /// </summary>
        /// <param name="itemIndex"></param>
        /// <param name="alignment"></param>
        protected virtual void JumpTo(int itemIndex, float alignment = 0.5f)
        {
            Scroller.Position = ToScrollerPosition(itemIndex, alignment);
        }

        /// <summary>
        /// 指定したアイテムの位置まで移動します.
        /// </summary>
        /// <param name="index">アイテムのインデックス.</param>
        /// <param name="duration">移動にかける秒数.</param>
        /// <param name="alignment">ビューポート内におけるセル位置の基準. 0f(先頭) ~ 1f(末尾).</param>
        /// <param name="onComplete">移動が完了した際に呼び出されるコールバック.</param>
        protected virtual void ScrollTo(int index, float duration, float alignment = 0.5f, Action onComplete = null)
        {
            Scroller.ScrollTo(ToScrollerPosition(index, alignment), duration, onComplete);
        }

        /// <summary>
        /// 指定したアイテムの位置まで移動します.
        /// </summary>
        /// <param name="index">アイテムのインデックス.</param>
        /// <param name="duration">移動にかける秒数.</param>
        /// <param name="easing">移動に使用するイージング.</param>
        /// <param name="alignment">ビューポート内におけるセル位置の基準. 0f(先頭) ~ 1f(末尾).</param>
        /// <param name="onComplete">移動が完了した際に呼び出されるコールバック.</param>
        protected virtual void ScrollTo(int index, float duration, Ease easing, float alignment = 0.5f, Action onComplete = null)
        {
            Scroller.ScrollTo(ToScrollerPosition(index, alignment), duration, easing, onComplete);
        }

        /// <summary>
        /// ビューポートとコンテンツの長さに基づいてスクロールバーのサイズを更新します.
        /// </summary>
        /// <param name="viewportLength">ビューポートのサイズ.</param>
        protected void UpdateScrollbarSize(float viewportLength)
        {
            var contentLength = Mathf.Max(flexCount + (paddingHead + paddingTail + head - spacing) / (flex + spacing), 1);
            Scroller.Scrollbar.size = Scrollable ? Mathf.Clamp01(viewportLength / contentLength) : 1f;
        }

        /// <summary>
        /// <see cref="Scroller"/> が扱うスクロール位置を <see cref="FancyScrollRect{TItemData, TContext}"/> が扱うスクロール位置に変換します.
        /// </summary>
        /// <param name="position"><see cref="Scroller"/> が扱うスクロール位置.</param>
        /// <returns><see cref="FancyScrollRect{TItemData, TContext}"/> が扱うスクロール位置.</returns>
        protected float ToFancyScrollViewPosition(float position)
        {
            return position / Mathf.Max(flexCount - 1, 1) * MaxScrollPosition - PaddingHeadLength;
        }

        /// <summary>
        /// <see cref="FancyScrollRect{TItemData, TContext}"/> が扱うスクロール位置を <see cref="Scroller"/> が扱うスクロール位置に変換します.
        /// </summary>
        /// <param name="position"><see cref="FancyScrollRect{TItemData, TContext}"/> が扱うスクロール位置.</param>
        /// <returns><see cref="Scroller"/> が扱うスクロール位置.</returns>
        protected float ToScrollerPosition(float position)
        {
            return (position + PaddingHeadLength) / MaxScrollPosition * Mathf.Max(flexCount - 1, 1);
        }

        /// <summary>
        /// <see cref="FancyScrollRect{TItemData, TContext}"/> が扱うスクロール位置を <see cref="Scroller"/> が扱うスクロール位置に変換します.
        /// </summary>
        /// <param name="position"><see cref="FancyScrollRect{TItemData, TContext}"/> が扱うスクロール位置.</param>
        /// <param name="alignment">ビューポート内におけるセル位置の基準. 0f(先頭) ~ 1f(末尾).</param>
        /// <returns><see cref="Scroller"/> が扱うスクロール位置.</returns>
        protected float ToScrollerPosition(float position, float alignment = 0.5f)
        {
            var offset = alignment * (ScrollLength - (1f + reuseCellMarginCount * 2f))
                + (1f - alignment - 0.5f) * spacing / (flex + spacing);
            return ToScrollerPosition(Mathf.Clamp(position - offset, 0f, MaxScrollPosition));
        }

        /// <summary>
        /// compute the cell interval and scroll offset.
        /// </summary>
        protected void AdjustCellIntervalAndScrollOffset()
        {
            cellInterval = (flex + spacing) / totalSize;
            headInterval = (head+ spacing) / totalSize;
            scrollOffset = cellInterval * (1f + reuseCellMarginCount) ;  

            if (ItemMappings.Count > 0)
            {
                var average = ItemMappings.Select(c => c.CellSize).Average();
                var maxCellSize = ItemMappings.Select(c => c.CellSize).Max();
                var ratio = maxCellSize / flex;
                ScrollLength = 1f / Mathf.Max(cellInterval, 1e-2f) - ratio;
            }
        }
        
        protected override float totalSize => Scroller.ViewportSize+ (flex + spacing) * (1f + reuseCellMarginCount * 2f);

        protected virtual void OnValidate()
        {
            AdjustCellIntervalAndScrollOffset();

            if (loop)
            {
                loop = false;
                Debug.LogError("Loop is currently not supported in FancyScrollRect.");
            }

            if (Scroller.SnapEnabled)
            {
                Scroller.SnapEnabled = false;
                Debug.LogError("Snap is currently not supported in FancyScrollRect.");
            }

            if (Scroller.MovementType == MovementType.Unrestricted)
            {
                Scroller.MovementType = MovementType.Elastic;
                Debug.LogError("MovementType.Unrestricted is currently not supported in FancyScrollRect.");
            }
        }
    }

  
    public abstract class MutableScrollRect<TItemData> : MutableScrollRect<TItemData, MutableScrollRectContext> { }
}
