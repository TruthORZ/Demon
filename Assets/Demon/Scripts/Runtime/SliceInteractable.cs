using UnityEngine;

namespace Demon.Slice
{
    public abstract class SliceInteractable : MonoBehaviour
    {
        public abstract string Prompt { get; }
        public abstract void Use(SlicePlayer player);
    }
}
