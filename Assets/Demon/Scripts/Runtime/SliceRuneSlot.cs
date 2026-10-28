using UnityEngine;

namespace Demon.Slice
{
    public sealed class SliceRuneSlot : MonoBehaviour
    {
        [Range(0, 8)] public int Index;
        public TextMesh Label;
    }
}
