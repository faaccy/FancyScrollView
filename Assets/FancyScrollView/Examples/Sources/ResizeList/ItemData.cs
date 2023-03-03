using UnityEngine;

namespace FancyScrollView.Examples.Sources.ResizeList
{
    public class ItemData
    {
        public float CellSize { get; }
        public string Message { get; }

        public ItemData(string message)
        {
            Message = message;
        }

        public ItemData(string message,float cellSize)
        {
            Message = message;
            CellSize = cellSize;
        }
    }
}