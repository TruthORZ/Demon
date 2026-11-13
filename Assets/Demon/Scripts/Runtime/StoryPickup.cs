using UnityEngine;

namespace Demon.Slice
{
    // An authored pickup/inspection target; definitions and layout stay in saved assets.
    public sealed class StoryPickup : Interactable
    {
        [SerializeField] CompletePlaythrough owner;
        [SerializeField] string entryId;
        [SerializeField] Transform readView;
        [SerializeField] int firstDay = 1;
        [SerializeField] bool hideAfterPickup;
        Renderer[] visuals; Collider[] targets;
        void Awake() { visuals = GetComponentsInChildren<Renderer>(); targets = GetComponents<Collider>(); }
        public string EntryId => entryId;
        public void Configure(CompletePlaythrough flow, string id, Transform view, int day = 1, bool hide = false)
        { owner = flow; entryId = id; readView = view; firstDay = day; hideAfterPickup = hide; }
        public override string Prompt => entryId == "flashlight" ? "Take torch" : "Read note";
        public override void Use(PlayerController player)
        {
            if (!owner || owner.Failed || owner.World.Session.Clock.Day < firstDay) return;
            owner.Collect(entryId, readView);
        }
        void Update()
        {
            if (!owner || owner.World.Session == null) return;
            bool visible = owner.World.Session.Clock.Day >= firstDay && (!hideAfterPickup || !owner.Records.Contains(entryId) || owner.World.Player.ReadingAt(readView));
            bool inView = !owner.World.Player.Reading || owner.World.Player.ReadingAt(readView);
            foreach (var visual in visuals) visual.enabled = visible && inView;
            foreach (var collider in targets) collider.enabled = visible;
        }
    }
}
