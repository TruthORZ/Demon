using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Demon.Slice
{
    // Saved cells and layout are authored in the scene; only overflow clones a saved template.
    public sealed class JournalInventory : MonoBehaviour
    {
        public sealed class Item
        {
            public readonly string Id, Title, Body, Category;
            public Item(string id, string title, string body, string category)
            { Id = id; Title = title; Body = body; Category = category; }
        }
        [SerializeField] RectTransform content;
        [SerializeField] JournalSlot template;
        [SerializeField] Sprite paperIcon, flashlightIcon;
        [SerializeField] Sprite[] tabletIcons;
        Sprite Picture(Item item)
        {
            if (item == null) return null;
            if (item.Id == "flashlight") return flashlightIcon;
            if (item.Id.StartsWith("T0") && int.TryParse(item.Id.Substring(1), out var index)
                && tabletIcons != null && index >= 1 && index <= tabletIcons.Length) return tabletIcons[index - 1];
            return paperIcon;
        }
        [SerializeField] JournalSlot[] authoredSlots;
        [SerializeField] Text detailTitle, detailBody;
        [SerializeField] ScrollRect gridScroll, detailScroll;
        [SerializeField] string emptyTitle = "Inventory empty";
        [SerializeField, TextArea] string emptyBody = "Notes, tablets and equipment are kept here. Select an item to inspect it.";
        readonly List<JournalSlot> extraSlots = new List<JournalSlot>();
        readonly List<Item> items = new List<Item>();
        public event Action<Item> ItemClicked;
        public string SelectedId { get; private set; }
        public int ItemCount => items.Count;
        public IReadOnlyList<JournalSlot> AuthoredSlots => authoredSlots;
        public string DetailText => detailBody.text;
        public void Configure(RectTransform grid, JournalSlot source, JournalSlot[] cells, Text heading, Text body, ScrollRect scroll, ScrollRect reader)
        { content = grid; template = source; authoredSlots = cells; detailTitle = heading; detailBody = body; gridScroll = scroll; detailScroll = reader; }
        public void Show(IReadOnlyList<Item> records)
        {
            if (!content || !template || authoredSlots == null || authoredSlots.Length == 0 || !detailTitle || !detailBody || !gridScroll || !detailScroll)
                throw new InvalidOperationException("Journal grid requires saved cells, template, reader and scrolling references.");
            items.Clear(); var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var item in records) if (seen.Add(item.Id)) items.Add(item);
            while (authoredSlots.Length + extraSlots.Count < items.Count)
            {
                var cell = Instantiate(template, content); cell.name = "OverflowSlot_" + extraSlots.Count; extraSlots.Add(cell);
            }
            int count = authoredSlots.Length + extraSlots.Count;
            for (int i = 0; i < count; i++)
            {
                var cell = i < authoredSlots.Length ? authoredSlots[i] : extraSlots[i - authoredSlots.Length];
                cell.gameObject.SetActive(i < authoredSlots.Length || i < items.Count);
                var item = i < items.Count ? items[i] : null;
                cell.Bind(item, Click, Picture(item));
            }
            string keep = SelectedId;
            SelectedId = null; detailTitle.text = emptyTitle; detailBody.text = emptyBody;
            if (items.Count > 0) Select(items.Exists(x => x.Id == keep) ? keep : items[0].Id);
            Canvas.ForceUpdateCanvases(); gridScroll.verticalNormalizedPosition = 1; detailScroll.verticalNormalizedPosition = 1;
        }
        void Click(string id)
        {
            Select(id);
            var item = items.Find(x => x.Id == id);
            if (item != null) ItemClicked?.Invoke(item);
        }
        void Select(string id)
        {
            var item = items.Find(x => x.Id == id); if (item == null) return;
            SelectedId = id; detailTitle.text = item.Title; detailBody.text = item.Body;
            foreach (var cell in authoredSlots) cell.SetSelected(cell.ItemId == id);
            foreach (var cell in extraSlots) cell.SetSelected(cell.ItemId == id);
            Canvas.ForceUpdateCanvases(); detailScroll.verticalNormalizedPosition = 1;
        }
    }
}
