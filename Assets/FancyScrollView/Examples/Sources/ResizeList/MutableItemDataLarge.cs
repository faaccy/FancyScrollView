using UnityEngine;
using UnityEngine.Assertions;

namespace FancyScrollView.Examples.Sources.ResizeList
{
    public class MutableItemDataLarge
    {
        public string Message { get; }
        public int UserId { get; }

        public MutableItemDataLarge(string message,int userId)
        {
            Message = message;
            UserId = userId;
        }
    }
}