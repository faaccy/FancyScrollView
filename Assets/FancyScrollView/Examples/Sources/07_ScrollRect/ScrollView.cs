/*
 * FancyScrollView (https://github.com/setchi/FancyScrollView)
 * Copyright (c) 2020 setchi
 * Licensed under MIT (https://github.com/setchi/FancyScrollView/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using EasingCore;

namespace FancyScrollView.Example07
{
    class ScrollView : FancyScrollRect<ItemData, Context>
    {
        [SerializeField] float cellSize = 100f;
        [SerializeField] GameObject cellPrefab = default;
        [SerializeField] GameObject headPrefab = default;

        protected override float CellSize => cellSize;
        protected override GameObject CellPrefab => cellPrefab;

        protected  GameObject HeadPrefab =>headPrefab;
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

        protected override void Initialize()
        {
            base.Initialize();
            CreateHeaderCell();
        }
        
        private FancyCell<ItemData, Context> currentHeader { get; set; }
        private void CreateHeaderCell()
        {
            if (HeadPrefab != null)
                currentHeader = Instantiate(HeadPrefab, cellContainer).GetComponent<FancyCell<ItemData, Context>>();
            currentHeader.SetContext(Context);
            currentHeader.SetVisible(true);
            currentHeader.IsHeader = true;
        }

        private void UpdateHeader(float position)
        {
            if (currentHeader == null) return;
            
            currentHeader.UpdatePosition(position);
        }

        public void OnCellClicked(Action<int> callback)
        {
            Context.OnCellClicked = callback;
        }

        protected override void UpdateCells(float firstPosition, int firstIndex, bool forceRefresh)
        {
            for (var i = 0; i < pool.Count; i++)
            {
                var index = firstIndex + i;
                var position = firstPosition + i * cellInterval;
                var cell = pool[CircularIndex(index, pool.Count)];
                if (loop)
                {
                    index = CircularIndex(index, ItemsSource.Count);
                }

                if (index < 0 || index >= ItemsSource.Count || position > 1f)
                {
                    cell.SetVisible(false);
                    continue;
                }

                if (forceRefresh || cell.Index != index || !cell.IsVisible)
                {
                    cell.Index = index;
                    cell.SetVisible(true);
                    cell.UpdateContent(ItemsSource[index]);
                }

                cell.UpdatePosition(position);
                
                ComputerHeader(index);
            }
        }

        private void ComputerHeader(int index)
        {
            if (index != 0) return;
            
            var cell = pool[CircularIndex(index, pool.Count)];
            var firstCellPosition = cell.transform.localPosition;
            firstCellPosition.y+=(head + cellSize + 2*spacing) * 0.5f;
            currentHeader.transform.localPosition = firstCellPosition;
        }

        public void UpdateData(IList<ItemData> items)
        {
            UpdateContents(items);
        }

        public void ScrollTo(int index, float duration, Ease easing, Alignment alignment = Alignment.Middle)
        {
            UpdateSelection(index);
            ScrollTo(index, duration, easing, GetAlignment(alignment));
        }

        public void JumpTo(int index, Alignment alignment = Alignment.Middle)
        {
            UpdateSelection(index);
            JumpTo(index, GetAlignment(alignment));
        }

        float GetAlignment(Alignment alignment)
        {
            switch (alignment)
            {
                case Alignment.Upper: return 0.0f;
                case Alignment.Middle: return 0.5f;
                case Alignment.Lower: return 1.0f;
                default: return GetAlignment(Alignment.Middle);
            }
        }

        void UpdateSelection(int index)
        {
            if (Context.SelectedIndex == index)
            {
                return;
            }

            Context.SelectedIndex = index;
            Refresh();
        }
    }
}
