using Demon.Slice.Domain;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Demon.Slice
{
    [MovedFrom(false, sourceClassName: "SliceTablet")]
    public sealed class RuneTablet : Interactable, ICloseViewInteraction
    {
        [SerializeField] Transform closeView;
        [SerializeField] ListeningTool listeningTool;
        [SerializeField] RuneSlot[] slots;
        [SerializeField] RuneTrayItem[] tray;
        [SerializeField] Renderer[] lines;
        [SerializeField] Material frozenLineMaterial;
        ITabletProgram program;
        RuneBoardModel board;
        bool bound;
        Material[] authoredLineMaterials;
        public override string Prompt => board != null && board.Frozen ? "查看已固化石板" : "查看并补全石板";
        public Transform CloseView => closeView;
        public RuneBoardModel Board => board;
        public void ConfigureListening(ListeningTool owner) => listeningTool = owner;
        public bool TryCompile() => program.TryCompile();
        bool ICloseViewInteraction.Confirm() => TryCompile();

        public void Configure(Transform anchor, RuneSlot[] authoredSlots, RuneTrayItem[] authoredTray,
            Renderer[] authoredLines, Material frozen)
        { closeView = anchor; slots = authoredSlots; tray = authoredTray; lines = authoredLines; frozenLineMaterial = frozen; }

        public void Initialize(ITabletProgram owner)
        {
            if (!closeView || slots == null || slots.Length != 9)
                throw new System.InvalidOperationException("Tablet requires an anchor and nine saved slots.");
            if (bound && board != null) board.Changed -= Refresh;
            bound = false;
            if (authoredLineMaterials == null || authoredLineMaterials.Length != lines.Length)
            {
                authoredLineMaterials = new Material[lines.Length];
                for (int i = 0; i < lines.Length; i++) authoredLineMaterials[i] = lines[i].sharedMaterial;
            }
            else for (int i = 0; i < lines.Length; i++) lines[i].sharedMaterial = authoredLineMaterials[i];
            program = owner ?? throw new System.ArgumentNullException(nameof(owner));
            board = program.Board;
            Bind(); Refresh();
        }

        void Bind() { if (board != null && !bound) { board.Changed += Refresh; bound = true; } }
        void OnEnable() => Bind();
        void OnDisable() { if (board != null && bound) board.Changed -= Refresh; bound = false; }
        public override void Use(PlayerController player)
        {
            if (program.BeginInteraction()) player.EnterTabletView(this, closeView);
        }

        public void Click(Collider target)
        {
            var slot = target.GetComponentInParent<RuneSlot>();
            if (slot && System.Array.IndexOf(slots, slot) >= 0) { board.ClickSlot(slot.Index); return; }
            var item = target.GetComponentInParent<RuneTrayItem>();
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
                case RuneKind.On: return "ON";
                case RuneKind.Enter: return "ENTER";
                case RuneKind.Demon: return "DEMON";
                case RuneKind.Region: return "REGION";
                case RuneKind.Ring: return "RING";
                case RuneKind.Chime: return "CHIME";
                default: return "·";
            }
        }
    }
}
