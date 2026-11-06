using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Demon.Slice
{
    [MovedFrom(false, sourceClassName: "SliceRuneSlot")]
    public sealed class RuneSlot : MonoBehaviour
    {
        [Range(0, 8)] public int Index;
        public TextMesh Label;
    }
}
