using System;

namespace FancyScrollView
{
    public interface IMutableScrollRectContext
    {
        ScrollDirection ScrollDirection { get; set; }
        float ScrollSize { get; set; }
    }
}
