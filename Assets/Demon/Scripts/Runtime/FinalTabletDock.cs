using UnityEngine;
namespace Demon.Slice
{
    public sealed class FinalTabletDock : SliceInteractable
    {
        [SerializeField] FinalTabletRetrieval retrieval;
        public FinalTabletRetrieval Retrieval => retrieval;
        public void Configure(FinalTabletRetrieval owner) => retrieval = owner;
        public override string Prompt => retrieval && retrieval.Progress != null && retrieval.Progress.Acquired ? "放置T07并补全" : "查看T07放置台";
        public override void Use(SlicePlayer player)
        {
            if (!retrieval) throw new System.InvalidOperationException("T07 dock requires saved retrieval reference.");
            retrieval.Place(player);
        }
    }
}
