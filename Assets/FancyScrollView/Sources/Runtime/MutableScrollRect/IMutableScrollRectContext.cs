using System;
using UnityEngine;

namespace FancyScrollView
{
    public interface IMutableScrollRectContext
    {
        ScrollDirection ScrollDirection { get; set; }
        Func<(float ScrollSize, float ReuseMargin)> CalculateScrollSize { get; set; }
        Action<int,Vector3> OnCellSizeChanged { get; set; }
    }
}
