using System;

namespace FancyScrollView.Examples.Sources.ResizeList
{
    public class Context : FancyScrollRectContext
    {
        public int SelectedIndex = -1;
        public Action<int> OnCellClicked;
    }
}