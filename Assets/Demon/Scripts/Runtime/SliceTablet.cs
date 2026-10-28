using Demon.Slice.Domain;
using UnityEngine;

namespace Demon.Slice
{
    public sealed class SliceTablet : SliceInteractable
    {
        [SerializeField] Transform closeView;
        [SerializeField] SliceRuneSlot[] slots;
        [SerializeField] SliceRuneTrayItem[] tray;
        [SerializeField] Renderer[] lines;
        [SerializeField] Material frozenLineMaterial;
        SliceSceneController world;
        RuneBoardModel board;
        bool bound;
        public override string Prompt => board != null && board.Frozen ? "查看已固化石板" : "查看并补全石板";
        public Transform CloseView => closeView;

        public void Configure(Transform anchor, SliceRuneSlot[] authoredSlots, SliceRuneTrayItem[] authoredTray,
            Renderer[] authoredLines, Material frozen)
        { closeView = anchor; slots = authoredSlots; tray = authoredTray; lines = authoredLines; frozenLineMaterial = frozen; }

        public void Initialize(SliceSceneController controller)
        {
            if (!closeView || slots == null || slots.Length != 9)
                throw new System.InvalidOperationException("Tablet requires an anchor and nine saved slots.");
            world = controller;
            board = world.Session.Board;
            Bind(); Refresh();
        }

        void Bind() { if (board != null && !bound) { board.Changed += Refresh; bound = true; } }
        void OnEnable() => Bind();
        void OnDisable() { if (board != null && bound) board.Changed -= Refresh; bound = false; }
        public override void Use(SlicePlayer player)
        {
            if (world.BeginFinalInteraction()) player.EnterTabletView(this, closeView);
        }

        public void Click(Collider target)
        {
            var slot = target.GetComponentInParent<SliceRuneSlot>();
            if (slot && System.Array.IndexOf(slots, slot) >= 0) { board.ClickSlot(slot.Index); return; }
            var item = target.GetComponentInParent<SliceRuneTrayItem>();
            if (item && System.Array.IndexOf(tray, item) >= 0) board.PickFromTray(item.Rune);
        }

        void Refresh()
        {
            foreach (var slot in slots) slot.Label.text = RuneLabel(board.At(slot.Index));
            foreach (var item in tray)
                item.Label.text = RuneLabel(item.Rune) + "  " + board.Available(item.Rune);
            if (board.Frozen)
                foreach (var line in lines) line.sharedMaterial = frozenLineMaterial;
        }

        public static string RuneLabel(RuneKind rune)
        {
            switch (rune)
            {
                case RuneKind.Nine: return "9";
                case RuneKind.GreaterEqual: return ">=";
                case RuneKind.Day: return "DAY";
                case RuneKind.Exchange: return "EXCHANGE";
                case RuneKind.Seal: return "SEAL";
                case RuneKind.Box: return "BOX";
                case RuneKind.Lake: return "LAKE";
                case RuneKind.Open: return "OPEN";
                default: return "·";
            }
        }
    }
}
