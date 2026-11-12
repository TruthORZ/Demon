using UnityEngine;
using Demon.Slice.Domain;
namespace Demon.Slice
{
    public sealed class StationObservation : Interactable
    {
        [SerializeField] ExplorationFlow owner;
        [SerializeField] ObservationSite site;
        [SerializeField] bool forestPoint;
        public bool IsForest => forestPoint;
        [SerializeField] Transform closeView;
        [SerializeField] Transform aimTarget;
        [SerializeField] Transform approach;
        public ExplorationFlow Owner => owner;
        public ObservationSite Site => site;
        public Transform AimTarget => aimTarget;
        public Transform Approach => approach;
        public Transform CloseView => closeView;
        public override string Prompt => "观察并记录" + (forestPoint ? "森林" : site == ObservationSite.Radio ? "电台" : site == ObservationSite.WindVane ? "风向标" : "湖岸");
        public void Configure(ExplorationFlow flow, ObservationSite id, Transform view, Transform target, Transform stand)
        { owner = flow; site = id; closeView = view; aimTarget = target; approach = stand; }
        public override void Use(PlayerController player)
        { if (!owner) throw new System.InvalidOperationException("Observation requires saved flow."); owner.Observe(this); }
    }
}
