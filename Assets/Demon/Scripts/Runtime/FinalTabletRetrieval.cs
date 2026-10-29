using System;
using Demon.Slice.Domain;
using UnityEngine;
namespace Demon.Slice
{
    public sealed class FinalTabletRetrieval : MonoBehaviour
    {
        [SerializeField] ThreatPickup pickup;
        [SerializeField] GameObject emptyDock;
        [TextArea] [SerializeField] string seekText = "T07已在湖岸出现。风铃辨认区域，光束争取时间；按住E拾取。";
        [TextArea] [SerializeField] string carryText = "已携带T07。回工具棚西侧空台，按E放置石板。";
        [TextArea] [SerializeField] string placedText = "T07已放回工具棚。靠近石板按E补全铭文。";
        [TextArea] [SerializeField] string compiledText = "湖已封闭。打开工具棚内的箱子B，阅读箱内记录。";
        SliceSceneController world;
        bool bound;
        public FinalTabletProgress Progress { get; private set; }
        public ThreatPickup Pickup => pickup;
        public bool ReadyToReconstruct => Progress != null && Progress.Placed;
        public bool Available => world && world.Session != null && !world.Session.Descended && world.Campaign.FinalAvailable;
        public string Status => world.Session.Board.Frozen ? compiledText : Progress.Placed ? placedText : Progress.Acquired ? carryText : seekText;
        public void Configure(ThreatPickup item, GameObject stand) { pickup = item; emptyDock = stand; }
        public void Initialize(SliceSceneController controller)
        {
            if (!pickup || !emptyDock || !controller.Campaign || !controller.Finale)
                throw new InvalidOperationException("T07 retrieval requires saved pickup, empty dock, campaign and finale.");
            world = controller; Progress = new FinalTabletProgress();
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
        public bool Place(SlicePlayer player)
        {
            if (!Available || !Progress.Acquired) { world.SetMessage("先到湖岸找到T07，再带回此处放置。"); return false; }
            Progress.Place(); RefreshPresentation();
            world.SetMessage(placedText); world.Tablet.Use(player); return true;
        }
        void OnDestroy() { if (bound && pickup) pickup.UnbindAttempt(); }
    }
}
