using UnityEngine;
using UnityEngine.Assertions;

namespace FancyScrollView.Examples.Sources.ResizeList
{
    public class MutableItemData
    {
        public string Message { get; }

        public MutableItemData(string message)
        {
            Message = message;
        }
    }
}