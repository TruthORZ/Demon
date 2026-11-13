using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Demon.Slice
{
    [MovedFrom(false, sourceClassName: "SliceBox")]
    public sealed class RecordBox : Interactable
    {
        [SerializeField] GameSceneController world;
        public override string Prompt => "Open the chest and read the letter";
        public void Configure(GameSceneController controller) => world = controller;
        public override void Use(PlayerController player) { if (world.Playthrough) world.Playthrough.Record("box-b"); world.ReadBox(); }
    }
}
