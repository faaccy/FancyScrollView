
using UnityEngine;

namespace FancyScrollView
{
   /// <summary>
   /// mutable scroll rect cell.
   /// </summary>
   /// <typeparam name="TItemData"></typeparam>
   /// <typeparam name="TContext"></typeparam>
    public abstract class MutableScrollRectCell<TItemData, TContext> : MutableCell<TItemData, TContext>
        where TContext : class, IMutableScrollRectContext, new()
   {
       protected RectTransform RectTransform => GetComponent<RectTransform>();

        /// <summary>
         /// update cell position.
         /// </summary>
         /// <param name="position"></param>
        public override void UpdatePosition(float position)
        {
            var (scrollSize, reuseMargin) = Context.CalculateScrollSize();
            var normalizedPosition = (Mathf.Lerp(0f, scrollSize, position) - reuseMargin) / (scrollSize - reuseMargin * 2f);

            var start = 0.5f * scrollSize;
            var end = -start;
            UpdatePosition(normalizedPosition, Mathf.Lerp(start, end, position));
        }

        /// <summary>
        /// update cell normalizedPosition position.
        /// </summary>
        /// <param name="normalizedPosition"></param>
        /// <param name="localPosition"></param>
        protected virtual void UpdatePosition(float normalizedPosition, float localPosition)
        {
            transform.localPosition = Context.ScrollDirection == ScrollDirection.Horizontal
                ? new Vector2(-localPosition, 0)
                : new Vector2(0, localPosition);
        }
        
        /// <summary>
        /// update cell size.
        /// </summary>
        /// <param name="cellSize">size of cell.</param>
        /// <param name="forceUpdate">update layout.</param>
        public override void UpdateSize(float cellSize,bool forceUpdate = false)
        {
            CellSize = cellSize;
            RectTransform.sizeDelta = Context.ScrollDirection == ScrollDirection.Horizontal
                ? new Vector2(cellSize, RectTransform.sizeDelta.y)
                : new Vector2( RectTransform.sizeDelta.x,cellSize);

            if (forceUpdate)
            {
                Context.OnCellSizeChanged?.Invoke(Index,RectTransform.sizeDelta);
            }
        }
    }

    public abstract class MutableScrollRectCell<TItemData> : MutableScrollRectCell<TItemData, MutableScrollRectContext>
    {
        /// <inheritdoc/>
        public sealed override void SetContext(MutableScrollRectContext context) => base.SetContext(context);
    }
}
