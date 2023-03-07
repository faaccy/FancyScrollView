using UnityEngine;
using UnityEngine.UI;

namespace FancyScrollView.Examples.Sources.ResizeList
{
    public class MutableCellLarge : MutableScrollRectCell<MutableItemData, Context>
    {
        [SerializeField] Text message = default;
        [SerializeField] Image image = default;
        [SerializeField] Button button = default;

        public string DisplayName;

        private RectTransform rectTransform { get; set; }
        private Vector3 initialSizeData { get; set; }

        public override void Initialize()
        {
            button.onClick.AddListener(() => Context.OnCellClicked?.Invoke(Index));
            rectTransform = (RectTransform)transform;
            initialSizeData = rectTransform.sizeDelta;
        }

        private void OnRectTransformDimensionsChange()
        {
            Debug.Log($"OnRectTransformDimensionsChange:{ rectTransform.sizeDelta } {initialSizeData}");
        }

        public override void UpdateContent(MutableItemData itemData)
        {
            message.text = itemData.Message;
            DisplayName = itemData.Message;

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