using Demon.Slice.Domain;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Demon.Slice
{
    [MovedFrom(false, sourceClassName: "SliceRuneTrayItem")]
    public sealed class RuneTrayItem : MonoBehaviour
    {
        public RuneKind Rune;
        public TextMesh Label;
    }
}
