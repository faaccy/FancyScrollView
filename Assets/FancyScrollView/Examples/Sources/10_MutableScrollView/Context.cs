using System;
using UnityEngine;

namespace FancyScrollView.Examples.Sources.ResizeList
{
    public class Context : MutableScrollRectContext
    {
        public int SelectedIndex = -1;
        public Action<int> OnCellClicked;
    }
}