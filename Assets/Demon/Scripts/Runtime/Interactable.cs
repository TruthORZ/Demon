using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Demon.Slice
{
    [MovedFrom(false, sourceClassName: "SliceInteractable")]
    public abstract class Interactable : MonoBehaviour
    {
        public abstract string Prompt { get; }
        public abstract void Use(PlayerController player);
    }
}
