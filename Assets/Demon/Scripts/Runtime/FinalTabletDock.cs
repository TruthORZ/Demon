using UnityEngine;
namespace Demon.Slice
{
    public sealed class FinalTabletDock : Interactable
    {
        [SerializeField] FinalTabletRetrieval retrieval;
        public FinalTabletRetrieval Retrieval => retrieval;
        public void Configure(FinalTabletRetrieval owner) => retrieval = owner;
        public override string Prompt => retrieval && retrieval.Progress != null && retrieval.Progress.Acquired ? "Set Tablet VII on the stand" : "Inspect the empty stand";
        public override void Use(PlayerController player)
        {
            if (!retrieval) throw new System.InvalidOperationException("T07 dock requires saved retrieval reference.");
            retrieval.Place(player);
        }
    }
}
