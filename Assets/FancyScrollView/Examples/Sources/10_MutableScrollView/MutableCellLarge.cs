using UnityEngine;
using UnityEngine.UI;

namespace FancyScrollView.Examples.Sources.ResizeList
{
    public class MutableCellLarge : MutableScrollRectCell<MutableItemData, Context>
    {
        [SerializeField] Text message = default;
        [SerializeField] Image image = default;
        [SerializeField] Button button = default;

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
            if(rectTransform == null)
                return;
            Debug.Log($"OnRectTransformDimensionsChange:{ rectTransform.sizeDelta } {initialSizeData}");
        }

        public override void UpdateContent(MutableItemData itemData)
        {
            message.text = itemData.Message;

            var selected = Context.SelectedIndex == Index;
            image.color = selected
                ? new Color32(0, 255, 255, 100)
                : new Color32(255, 255, 255, 77);
        }
    }
}