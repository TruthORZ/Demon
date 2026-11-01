using UnityEngine;

namespace Demon.Slice
{
    public sealed class SliceBed : SliceInteractable
    {
        [SerializeField] SliceSceneController world;
        public override string Prompt => "睡到次日（可以不修复石板）";
        public void Configure(SliceSceneController controller) => world = controller;
        public override void Use(SlicePlayer player) => world.Sleep();
    }
}
