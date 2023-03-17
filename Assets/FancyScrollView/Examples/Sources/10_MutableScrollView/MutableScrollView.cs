using System;
using System.Collections.Generic;
using EasingCore;
using UnityEngine;

namespace FancyScrollView.Examples.Sources.ResizeList
{
    public class MutableScrollView : MutableScrollRect<MutableItemData, Context>
    {
        [SerializeField] GameObject cellPrefab = default;
        
        protected override GameObject CellPrefab => cellPrefab;

        public void OnCellClicked(Action<int> callback)
        {
            Context.OnCellClicked = callback;
        }

        public void UpdateData(IList<MutableItemData> items,IList<MutablePrefabMapping> mappings)
        {
            UpdateContents(items,mappings);
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






