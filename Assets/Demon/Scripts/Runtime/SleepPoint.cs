using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Demon.Slice
{
    [MovedFrom(false, sourceClassName: "SliceBed")]
    public sealed class SleepPoint : Interactable
    {
        [SerializeField] GameSceneController world;
        public override string Prompt => world && world.Playthrough ? "完成关键事项后，夜间睡到次日" : "睡到次日（可以不修复石板）";
        public void Configure(GameSceneController controller) => world = controller;
        public override void Use(PlayerController player) => world.Sleep();
    }
}
