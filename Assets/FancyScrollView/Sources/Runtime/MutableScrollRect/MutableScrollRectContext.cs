using System;

namespace FancyScrollView
{
    public class MutableScrollRectContext : IMutableScrollRectContext
    {
        ScrollDirection IMutableScrollRectContext.ScrollDirection { get; set; }
        public float ScrollSize { get; set; }
    }
}
