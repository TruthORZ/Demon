using Demon.Slice.Domain;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Demon.Slice
{
    [MovedFrom(false, sourceClassName: "SliceRuneTrayItem")]
    public sealed class RuneTrayItem : MonoBehaviour
    {
        [SerializeField] Renderer physicalRune;
        public void SetAvailable(int count)
        {
            Label.text = RuneTablet.RuneLabel(Rune) + "  " + count;
            if (physicalRune) physicalRune.enabled = count > 0;
        }
        public RuneKind Rune;
        public TextMesh Label;
    }
}
