using System;

namespace FancyScrollView
{
    public class MutableScrollRectContext : IMutableScrollRectContext
    {
        ScrollDirection IMutableScrollRectContext.ScrollDirection { get; set; }
        public Func<(float ScrollSize, float ReuseMargin)> CalculateScrollSize { get; set; }
    }
}
