using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FancyScrollView
{
    public abstract class MutableScrollView<TItemData, TContext> : MonoBehaviour where TContext : class, new()
    {
        [SerializeField] public float reuseCellMarginCount = 0f;

        [SerializeField] public float paddingHead = 0f;

        [SerializeField] public float paddingTail = 0f;

        [SerializeField] public float head = 0f;

        /// <summary>
        /// cell基准大小,当cell大小不一致时,取平均值或者较小值
        /// </summary>
        [SerializeField] public float flex = 100f;

        [SerializeField] public float spacing = 0f;

        [SerializeField, Range(1e-2f, 1f)] protected float cellInterval = 0.2f;
        
        [SerializeField] protected float headInterval = 0f;

        [SerializeField, Range(0f, 1f)] protected float scrollOffset = 0.5f;

        protected abstract GameObject CellPrefab { get; }
        
        protected abstract GameObject Header { get; }
    
        [SerializeField] protected bool loop = false;

        [SerializeField] protected Transform cellContainer = default;
     
        public int DataCount => ItemsSource.Count;
        
        public float PaddingTop
        {
            get => paddingHead;
            set
            {
                paddingHead = value;
                Relayout();
            }
        }

        public float PaddingBottom
        {
            get => paddingTail;
            set
            {
                paddingTail = value;
                Relayout();
            }
        }

        public float Spacing
        {
            get => spacing;
            set
            {
                spacing = value;
                Relayout();
            }
        }

        protected  IList<MutableCell<TItemData, TContext>> pool { get; set; } = new List<MutableCell<TItemData,TContext>>();

        protected bool initialized;

        protected float currentPosition { get; set; }

        protected IList<TItemData> ItemsSource { get; set; } = new List<TItemData>();

        protected IList<MutablePrefabMapping> ItemMappings { get; set; } = new List<MutablePrefabMapping>();

        protected TContext Context { get; } = new TContext();

        protected virtual void Initialize()
        {
        }

        protected virtual void UpdateContents(IList<TItemData> itemsSource,IList<MutablePrefabMapping> mappings)
        {
            ItemsSource = itemsSource;
            ItemMappings = mappings;
            Refresh();
        }

        protected virtual void Relayout() => UpdatePosition(currentPosition, false);

        protected virtual void Refresh() => UpdatePosition(currentPosition, true);

        protected virtual void UpdatePosition(float position) => UpdatePosition(position, false);

        private void UpdatePosition(float position, bool forceRefresh)
        {
            if (!initialized)
            {
                Initialize();
                initialized = true;
            }

            currentPosition = position;

            var (firstIndex,firstPosition) = UpdateFirst(position);
            
            UpdatePool(firstPosition);

            UpdateCells(firstPosition, firstIndex, forceRefresh);
        }

        /// <summary>
        /// update current first cell index and position as scroll view offset.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        protected (int first, float pos) UpdateFirst(float position)
        {
            var p = position - (scrollOffset) / cellInterval ;
            var firstIndex = Mathf.CeilToInt(p);
            var firstPosition = (Mathf.Ceil(p) - p) * cellInterval;
        
            Debug.Log($"index {firstIndex}");
          
            return (firstIndex, firstPosition);
        }

        /// <summary>
        ///  根据平均值或者较小的cell大小,计算出cell pool的大小
        /// </summary>
        /// <param name="firstPosition">first cell position</param>
        private void UpdatePool(float firstPosition)
        {
            var count = ItemMappings.Count;
            if (count <= 0) return;
            if (firstPosition + pool.Count * cellInterval > 1f) return;
   
            var addCount = Mathf.CeilToInt((1f - firstPosition) / cellInterval) - pool.Count;
            for (var i = 0; i < addCount; i++)
            {
                var cellSize = ItemMappings[i].CellSize;
              
                var cell = Instantiate(CellPrefab, cellContainer)
                    .GetComponent<MutableCell<TItemData,TContext>>();
                cell.SetContext(Context);
                cell.Initialize();
                cell.UpdateSize(cellSize);
                cell.SetVisible(false);
                pool.Add(cell);
            }
        }

        /// <summary>
        /// 更新cell的数据和位置
        /// </summary>
        /// <param name="firstPosition"></param>
        /// <param name="firstIndex"></param>
        /// <param name="forceRefresh"></param>
        private void UpdateCells(float firstPosition, int firstIndex, bool forceRefresh)
        {
            var position = firstPosition;
            var pre = 0f;
            for (var i = 0; i < pool.Count; i++)
            {
                var index = firstIndex + i;
                var cell = pool[CircularIndex(index, pool.Count)];
                //var position = firstPosition + i * cellInterval;
                var current= cell.CellSize;
                var interval = GetCurrentInterval(i,current,pre);
                var prePosition = position;
                position += interval;
                pre = current;
                
                if (loop)
                {
                    index = CircularIndex(index, ItemsSource.Count);
                }

                //cell超出范围判定
                if (index < 0 || index >= ItemsSource.Count || position > 1f)
                {
                    cell.SetVisible(false);
                    continue;
                }
                //替换cell
                if (forceRefresh || cell.Index != index || !cell.IsVisible)
                {
                    var cellSize= ItemMappings[index].CellSize;
                    if(cell.CellSize != cellSize)
                    {
                        Debug.Log($"replace cell {cell.Index}-{cell.CellSize} : {index}-{cellSize}");
                        var changeInterval = GetCurrentInterval(i,current,pre);
                        position = prePosition + changeInterval;
                        cell.UpdateSize(cellSize);
                    }
                    
                    cell.Index = index;
                    cell.SetVisible(true);
                    cell.UpdateContent(ItemsSource[index]);
                }
                cell.UpdatePosition(position);
                
                UpdateHeader(index);
            }
        }
        
        protected float GetCurrentInterval(int i,float current,float pre) => i>0 ? ((current + pre) * 0.5f + spacing ) / totalSize: (((current-flex)* 0.5f))/ totalSize;

        protected virtual float totalSize => (flex+spacing) /cellInterval;
        
        protected int CircularIndex(int i, int size) => size < 1 ? 0 : i < 0 ? size - 1 + (i + 1) % size : i % size;
        
        /// <summary>
        /// update header position.
        /// </summary>
        /// <param name="index"></param>
        protected virtual void UpdateHeader(int index)
        {
            if (index != 0 || Header == null) return;
            
            var cell = pool[CircularIndex(index, pool.Count)];
            var firstCellPosition = cell.transform.localPosition;
            firstCellPosition.y+=(head+ cell.CellSize + 2 * spacing) * 0.5f;
            Header.transform.localPosition = firstCellPosition;
        }

#if UNITY_EDITOR
        bool cachedLoop;
        float cachedCellInterval, cachedScrollOffset;

        void LateUpdate()
        {
            if (cachedLoop != loop ||
                cachedCellInterval != cellInterval ||
                cachedScrollOffset != scrollOffset)
            {
                cachedLoop = loop;
                cachedCellInterval = cellInterval;
                cachedScrollOffset = scrollOffset;

                UpdatePosition(currentPosition);
            }
        }
#endif
    }

    public abstract class MutableScrollView<TItemData> : MutableScrollView<TItemData, NullContext>{}
   
}