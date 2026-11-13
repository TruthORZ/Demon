using System;
using UnityEngine;
using UnityEngine.UI;

namespace Demon.Slice
{
    public sealed class JournalSlot : MonoBehaviour
    {
        [SerializeField] Button button;
        [SerializeField] Text title, category;
        [SerializeField] Image selection, icon;
        [SerializeField] string emptyLabel = "Empty";
        Action<string> select;
        public string ItemId { get; private set; }
        public Button Button => button;
        public void Configure(Button target, Text nameLabel, Text kindLabel, Image highlight)
        { button = target; title = nameLabel; category = kindLabel; selection = highlight; }
        public void Bind(JournalInventory.Item item, Action<string> onSelect, Sprite picture = null)
        {
            if (!button || !title || !category || !selection) throw new InvalidOperationException("Journal slot needs its authored labels, button and highlight.");
            ItemId = item?.Id; select = onSelect;
            title.text = item == null ? emptyLabel : item.Title; category.text = item?.Category ?? "";
            if (icon) { icon.sprite = picture; icon.enabled = picture; category.enabled = false; }
            button.interactable = item != null; selection.enabled = false;
            button.onClick.RemoveListener(Choose); button.onClick.AddListener(Choose);
        }
        void Choose() { if (ItemId != null) select?.Invoke(ItemId); }
        public void SetSelected(bool value) => selection.enabled = value;
        void OnDestroy() { if (button) button.onClick.RemoveListener(Choose); }
    }
}
