using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Demon.Slice
{
    [MovedFrom(false, sourceClassName: "SliceBed")]
    public sealed class SleepPoint : Interactable
    {
        [SerializeField] GameSceneController world;
        public override string Prompt => "睡到次日（可以不修复石板）";
        public void Configure(GameSceneController controller) => world = controller;
        public override void Use(PlayerController player) => world.Sleep();
    }
}
