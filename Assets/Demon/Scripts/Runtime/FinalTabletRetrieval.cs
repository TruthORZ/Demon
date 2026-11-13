using System;
using Demon.Slice.Domain;
using UnityEngine;
namespace Demon.Slice
{
    public sealed partial class FinalTabletRetrieval : MonoBehaviour
    {
        [SerializeField] ThreatPickup pickup;
        [SerializeField] GameObject emptyDock;
        [TextArea] [SerializeField] string seekText = "Hold E to lift the stone. Listen for bells.";
        [TextArea] [SerializeField] string carryText = "Bring the broken stone to the empty stand in the cabin.";
        [TextArea] [SerializeField] string placedText = "The stone fits. Press E to examine it.";
        [TextArea] [SerializeField] string compiledText = "The lake is sealed. Open Chest B.";
        GameSceneController world;
        bool bound;
        public FinalTabletProgress Progress { get; private set; }
        public ThreatPickup Pickup => pickup;
        public bool ReadyToReconstruct => Progress != null && Progress.Placed;
        public bool Available => world && world.Session != null && !world.Session.Descended && world.Campaign.FinalAvailable;
        public string Status => world.Session.Board.Frozen ? compiledText : Progress.Placed ? placedText : Progress.Acquired ? carryText : seekText;
        public void Configure(ThreatPickup item, GameObject stand) { pickup = item; emptyDock = stand; }
        public void Initialize(GameSceneController controller)
        {
            if (!pickup || !emptyDock || !controller.Campaign || !controller.Finale)
                throw new InvalidOperationException("T07 retrieval requires saved pickup, empty dock, campaign and finale.");
            world = controller; Progress = new FinalTabletProgress();
            if (bound) pickup.UnbindAttempt();
            pickup.ResetAttempt();
            pickup.BindAttempt(TryBeginPickup, Collected); bound = true;
            RefreshPresentation();
        }
        void Update() { if (world) RefreshPresentation(); }
        public void RefreshPresentation()
        {
            if (Progress == null) return;
            bool visible = Available && !Progress.Acquired;
            if (pickup.gameObject.activeSelf != visible) pickup.gameObject.SetActive(visible);
            if (emptyDock.activeSelf == Progress.Placed) emptyDock.SetActive(!Progress.Placed);
            world.Campaign.RefreshFinalTablet();
        }
        bool TryBeginPickup()
        {
            if (!Available || Progress.Acquired) return false;
            world.Finale.ReserveTime();
            return world.Finale.TimeReserved;
        }
        void Collected()
        {
            if (!Progress.Collect()) return;
            RefreshPresentation(); world.SetMessage(carryText);
        }
        public bool Place(PlayerController player)
        {
            if (!Available || !Progress.Acquired) { world.SetMessage("Find the broken tablet on the shore first."); return false; }
            Progress.Place(); RefreshPresentation();
            world.SetMessage(placedText); world.Tablet.Use(player); return true;
        }
        void OnDestroy() { if (bound && pickup) pickup.UnbindAttempt(); }
    }
}
