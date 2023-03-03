﻿using UnityEngine;
using UnityEngine.UI;

namespace FancyScrollView.Examples.Sources.ResizeList
{
    public class ResizeCell : FancyScrollRectCell<ItemData, Context>
    {
        [SerializeField] Text message = default;
        [SerializeField] Image image = default;
        [SerializeField] Button button = default;
        private ResizeScrollView scrollView = default;
        
        private RectTransform rectTransform { get; set; }
        private Vector3 initialSizeData { get; set; }

        public override void Initialize()
        {
            button.onClick.AddListener(() => Context.OnCellClicked?.Invoke(Index));
            rectTransform = (RectTransform)transform;
            initialSizeData = rectTransform.sizeDelta;
            scrollView = transform.GetComponentInParent<ResizeScrollView>();
        }

        private void OnRectTransformDimensionsChange()
        {
            Context.OnCellSizeChanged?.Invoke(rectTransform.sizeDelta);
            Debug.Log($"OnRectTransformDimensionsChange:{ rectTransform.sizeDelta } {initialSizeData}");
        }

        public override void UpdateContent(ItemData itemData)
        {
            message.text = itemData.Message;

            var selected = Context.SelectedIndex == Index;
            image.color = selected
                ? new Color32(0, 255, 255, 100)
                : new Color32(255, 255, 255, 77);
        }

        protected override void UpdatePosition(float normalizedPosition, float localPosition)
        {
            base.UpdatePosition(normalizedPosition, localPosition);

            var wave = Mathf.Sin(normalizedPosition * Mathf.PI * 2) * 65;
            transform.localPosition += Vector3.right * wave;
        }
    }
}