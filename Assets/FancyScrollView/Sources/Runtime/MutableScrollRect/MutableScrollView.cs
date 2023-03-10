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

        readonly IList<BaseCell<TItemData,TContext>> pool = new List<BaseCell<TItemData,TContext>>();

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

        private float sumCellInterval = 0f;
        /// <summary>
        /// 根绝当前的scroll view 位置计算当前pool构造
        /// </summary>
        /// <param name="position"></param>
        private void UpdatePool(float position)
        {
            var count = ItemMappings.Count;
            if (count <= 0) return;
        
            for (var i = 0; i < count; i++)
            {
                var cellSize = ItemMappings[i].CellSize;
                var currentInterval = cellInterval * i;
                sumCellInterval += currentInterval;
                if(sumCellInterval > maxCellInterval) break;
                
                var prefab = prefabList[ItemMappings[i].PrefabIndex];
                var cell = Instantiate(prefab, cellContainer)
                    .GetComponent<BaseCell<TItemData,TContext>>();
                cell.Initialize();
                cell.CellSize = cellSize;
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
            for (var i = 0; i < pool.Count; i++)
            {
                var index = firstIndex + i;
                var cell = pool[CircularIndex(index, pool.Count)];
                position += CaculateInterval(cell.CellSize);
           
                if (loop)
                {
                    index = CircularIndex(index, ItemsSource.Count);
                }

                if (index < 0 || index >= ItemsSource.Count || position > 1)
                {
                    cell.SetVisible(false);
                    continue;
                }
            
                if (forceRefresh || cell.Index != index || !cell.IsVisible)
                {
                    var cellSize =  ItemMappings[index].CellSize;
                    if(cell.CellSize != cellSize)
                    {
                        position = position - cell.CellSize * cellInterval;
                        var prefab = prefabList[ItemMappings[index].PrefabIndex];
                        cell = Instantiate(prefab, cellContainer)
                            .GetComponent<BaseCell<TItemData,TContext>>();
                        position = position + cell.CellSize * cellInterval;
                        cell.Initialize();
                        cell.CellSize = ItemMappings[index].CellSize;
                        cell.SetContext(Context);
                        cell.SetVisible(true);
                        pool.Add(cell);
                        sumCellInterval += cell.CellSize * cellInterval;
                        cell.UpdateContent(ItemsSource[index]);
                        
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

        float CaculateInterval(float cellSize)
        {
            return (cellSize+spacing) / ScrollSize;
        }       

        int CircularIndex(int i, int size) => size < 1 ? 0 : i < 0 ? size - 1 + (i + 1) % size : i % size;

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