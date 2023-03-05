using System.Collections.Generic;
using UnityEngine;

namespace FancyScrollView
{
    public abstract class MutableScrollView<TItemData, TContext> : MonoBehaviour where TContext : class, new()
    {
        [SerializeField, Range(1e-2f, 1f)] protected float cellInterval = 0.2f;

        [SerializeField, Range(0f, 1f)] protected float scrollOffset = 0.5f;
        
        [SerializeField, Range(1e-2f, 100f)] protected float maxCellInterval = 2f;

        [SerializeField] GameObject[] prefabList;

        [SerializeField] protected bool loop = false;

        [SerializeField] protected Transform cellContainer = default;

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

        /// <summary>
        /// 根绝当前的scroll view 位置计算当前pool构造
        /// </summary>
        /// <param name="position"></param>
        private void UpdatePool(float position)
        {
            var count = ItemMappings.Count;
            if (count <= 0) return;
            var sum = 0f;
            for (var i = 0; i < count; i++)
            {
                var flex = ItemMappings[i].Flex;
                var currentInterval = cellInterval * flex;
                sum+= currentInterval;
                if(sum > maxCellInterval) break;
                
                var prefab = prefabList[ItemMappings[i].PrefabIndex];
                var cell = Instantiate(prefab, cellContainer)
                    .GetComponent<BaseCell<TItemData,TContext>>();
                cell.Initialize();
                cell.Flex = flex;
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
            for (var i = 0; i < pool.Count; i++)
            {
                var index = firstIndex + i;
                var cell = pool[CircularIndex(index, pool.Count)];
                var position = firstPosition + i * cell.Flex * cellInterval;

                if (loop)
                {
                    index = CircularIndex(index, ItemsSource.Count);
                }

                //超出可视范围的cell隐藏
                if (index < 0 || index >= ItemsSource.Count || position > 1)
                {
                    cell.SetVisible(false);
                    continue;
                }
                //更新cell内容：
                //1.替换原油cell（flex1-2 prefab也发生了变换）
                //2.更新cell的内容
                if (forceRefresh || cell.Index != index || !cell.IsVisible)
                {
                    cell.Index = index;
                    cell.SetVisible(true);
                    cell.Flex = ItemMappings[index].Flex;
                    cell.UpdateContent(ItemsSource[index]);
                    
                    //如果当前位置的cell 需要替换
                }
                //更新cell的位置
                cell.UpdatePosition(position);
            }
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