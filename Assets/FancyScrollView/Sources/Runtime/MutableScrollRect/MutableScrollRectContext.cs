using System;

namespace FancyScrollView
{
    public class MutableScrollRectContext : IMutableScrollRectContext
    {
        ScrollDirection IMutableScrollRectContext.ScrollDirection { get; set; }
        Func<(float ScrollSize, float ReuseMargin)> IMutableScrollRectContext.CalculateScrollSize { get; set; }
    }
}
