using Demon.Slice.Domain;
using UnityEngine;
using UnityEngine.UI;

namespace Demon.Slice
{
    public sealed class TabletInventoryView : MonoBehaviour
    {
        [SerializeField] GameObject stage, panel;
        [SerializeField] TabletSurface[] exhibits;
        [SerializeField] Text caption;
        [SerializeField] GameObject originalSealBoard;
        [SerializeField] Button compareSeal;
        [SerializeField] Text compareLabel;
        bool showingOriginal;
        string currentCaption;
        [SerializeField, TextArea] string originalCaption = "Tablet VII · Original inscription\nIF DAY == 9, get (demon, seal, false)\nThe old inscription would have opened the lake on the ninth day. The break severed it.";
        public bool IsOpen => panel && panel.activeSelf;
        public void Configure(GameObject studio, GameObject overlay, TabletSurface[] stones, Text label)
        { stage = studio; panel = overlay; exhibits = stones; caption = label; }
        public void Show(int index, string title, string record, RuneBoardModel board)
        {
            for (int i = 0; i < exhibits.Length; i++) exhibits[i].gameObject.SetActive(i == index);
            if (board != null) exhibits[index].Display(board);
            showingOriginal = false;
            if (originalSealBoard) originalSealBoard.SetActive(false);
            if (compareSeal) compareSeal.gameObject.SetActive(index == 6 && originalSealBoard);
            if (compareLabel) compareLabel.text = "Original inscription";
            currentCaption = title + "   Esc · Inventory\n" + record;
            caption.text = currentCaption;
            stage.SetActive(true); panel.SetActive(true);
        }
        public void ToggleOriginalSeal()
        {
            if (!IsOpen || !compareSeal || !compareSeal.gameObject.activeInHierarchy || !originalSealBoard) return;
            showingOriginal = !showingOriginal;
            exhibits[6].gameObject.SetActive(!showingOriginal);
            originalSealBoard.SetActive(showingOriginal);
            compareLabel.text = showingOriginal ? "Current inscription" : "Original inscription";
            caption.text = showingOriginal ? originalCaption : currentCaption;
        }
        public void Close() { stage.SetActive(false); panel.SetActive(false); }
        void OnDisable() { if (stage && panel) Close(); }
    }
}
