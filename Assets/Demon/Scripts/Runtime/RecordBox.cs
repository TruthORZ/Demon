using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Demon.Slice
{
    [MovedFrom(false, sourceClassName: "SliceBox")]
    public sealed class RecordBox : Interactable
    {
        [SerializeField] GameSceneController world;
        public override string Prompt => "打开箱子并查看记录";
        public void Configure(GameSceneController controller) => world = controller;
        public override void Use(PlayerController player) => world.ReadBox();
    }
}
