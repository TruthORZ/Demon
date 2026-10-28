using UnityEngine;

namespace Demon.Slice
{
    public sealed class SliceBox : SliceInteractable
    {
        [SerializeField] SliceSceneController world;
        public override string Prompt => "打开箱子并查看记录";
        public void Configure(SliceSceneController controller) => world = controller;
        public override void Use(SlicePlayer player) => world.ReadBox();
    }
}
