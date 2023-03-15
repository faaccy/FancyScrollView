using System;
using UnityEngine;

namespace FancyScrollView
{
    public class MutableScrollRectContext : IMutableScrollRectContext
    {
        ScrollDirection IMutableScrollRectContext.ScrollDirection { get; set; }
        public Func<(float ScrollSize, float ReuseMargin)> CalculateScrollSize { get; set; }
        public Action<int,Vector3> OnCellSizeChanged { get; set; }
    }
}
