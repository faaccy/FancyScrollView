using System;
using System.Collections.Generic;
using UnityEngine;

namespace FancyScrollView
{
    public abstract class MutableScrollView<TItemData, TContext> : MonoBehaviour where TContext : class, new()
    {
        [SerializeField] public float reuseCellMarginCount = 0f;

        [SerializeField] public float paddingHead = 0f;

        [SerializeField] public float paddingTail = 0f;
        /// <summary>
        /// cell基准大小,当cell大小不一致时,取平均值或者较小值
        /// </summary>
        [SerializeField] public float flex = 100f;

        [SerializeField] public float spacing = 0f;

        [SerializeField, Range(1e-2f, 1f)] protected float cellInterval = 0.2f;

        [SerializeField, Range(0f, 1f)] protected float scrollOffset = 0.5f;
        
        [SerializeField, Range(1e-2f, 100f)] protected float maxCellInterval = 1f;

        [SerializeField] GameObject[] prefabList;
    
        [SerializeField] protected bool loop = false;

        [SerializeField] protected Transform cellContainer = default;
     
        public int DataCount => ItemsSource.Count;
        
        protected float ScrollSize = 100f;
        
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
            
            var p = position - scrollOffset / cellInterval;
            var firstIndex = Mathf.CeilToInt(p);
            var firstPosition = (Mathf.Ceil(p) - p) * cellInterval;

            UpdatePool(firstPosition);

            UpdateCells(firstPosition, firstIndex, forceRefresh);
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
                var prefab = prefabList[ItemMappings[i].PrefabIndex];
                var cell = Instantiate(prefab, cellContainer)
                    .GetComponent<MutableCell<TItemData,TContext>>();
                cell.Initialize();
                cell.CellSize = cellSize;
                cell.SetVisible(false);
                pool.Add(cell);
            }
        }
        
        private readonly float _floatDelta = 1E-6f;

        /// <summary>
        /// 更新cell的数据和位置
        /// </summary>
        /// <param name="firstPosition"></param>
        /// <param name="firstIndex"></param>
        /// <param name="forceRefresh"></param>
        private void UpdateCells(float firstPosition, int firstIndex, bool forceRefresh)
        {
            Debug.Log($"firstPosition:{firstPosition} firstIndex:{firstIndex}");
            var totalSize = (flex+spacing) /cellInterval;
            var position = firstPosition;
            var pre = flex;
            for (var i = 0; i < pool.Count; i++)
            {
                var index = firstIndex + i;
                var cell = pool[CircularIndex(index, pool.Count)];
                
                if (index >= 0)
                {
                    var current =  cell.CellSize;
                    var interval = ((current + pre) * 0.5f + spacing) / totalSize;
                    position += interval;
                    pre = current;
                }
             
                
                if (loop)
                {
                    index = CircularIndex(index, ItemsSource.Count);
                }

                //cell超出范围判定 不能用index来判断,因为index可能是负数
                if (index < 0 || index >= ItemsSource.Count || position > 1)
                {
                    Debug.Log($"index:{index} pos:{position}");
                    cell.SetVisible(false);
                    continue;
                }
                //替换cell
                if (forceRefresh || cell.Index != index || !cell.IsVisible)
                {
                    var cellSize= ItemMappings[index].CellSize;
                    if(Math.Abs(cell.CellSize - cellSize) > _floatDelta)
                    {
                        Debug.Log("replace cell");
                        var prefab = prefabList[ItemMappings[index].PrefabIndex];
                        var oldCell = cell;
                        pool.Remove(oldCell);
                        oldCell.SetVisible(false);
                        
                        cell = Instantiate(prefab, cellContainer)
                            .GetComponent<MutableCell<TItemData,TContext>>();
                        pool.Add(cell);
                        position += ((cell.CellSize - oldCell.CellSize + pre) * 0.5f + spacing) / totalSize;
                        cell.Initialize();
                        cell.CellSize = ItemMappings[index].CellSize;
                        cell.SetContext(Context);
                        cell.SetVisible(true);
                        cell.UpdateContent(ItemsSource[index]);
                        oldCell.Destroy();
                    }
                    else
                    {
                        cell.Index = index;
                        cell.SetVisible(true);
                        cell.SetContext(Context);
                        cell.UpdateContent(ItemsSource[index]);
                    }
                }
                cell.UpdatePosition(position);
            }
        }

        protected int CircularIndex(int i, int size) => size < 1 ? 0 : i < 0 ? size - 1 + (i + 1) % size : i % size;

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