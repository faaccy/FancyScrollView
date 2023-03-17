using UnityEngine;
using UnityEngine.UI;

namespace FancyScrollView.Examples.Sources.ResizeList
{
    public class MutableCell : MutableScrollRectCell<MutableItemData, Context>
    {
        [SerializeField] Text message = default;
        [SerializeField] Image image = default;
        [SerializeField] Button button = default;
        private MutableScrollView scrollView = default;

        public override void Initialize()
        {
            button.onClick.AddListener(() => Context.OnCellClicked?.Invoke(Index));
         
            scrollView = transform.GetComponentInParent<MutableScrollView>();
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