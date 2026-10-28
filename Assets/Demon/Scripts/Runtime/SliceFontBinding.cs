using UnityEngine;
using UnityEngine.UI;

namespace Demon.Slice
{
    // The OS-backed .fontsettings asset stores font names, not a redistributable font binary.
    // Recreate its native font face for this Windows prototype; all text objects remain authored assets.
    public sealed class SliceFontBinding : MonoBehaviour
    {
        [SerializeField] bool useWindowsSystemFont = true;
        [SerializeField] Font authoredFont;
        [SerializeField] string[] systemFontNames = { "Microsoft YaHei", "Microsoft YaHei UI", "SimHei", "Arial" };
        [SerializeField] Text[] overlayLabels;
        [SerializeField] TextMesh[] worldLabels;
        Font runtimeFont;

        public void Configure(Font source, Text[] overlay, TextMesh[] meshes)
        { authoredFont = source; overlayLabels = overlay; worldLabels = meshes; }

        void Awake()
        {
            Font resolved = authoredFont;
            if (useWindowsSystemFont)
            {
                runtimeFont = Font.CreateDynamicFontFromOSFont(systemFontNames, 64);
                resolved = runtimeFont;
            }
            if (!resolved) throw new System.InvalidOperationException("Assign a font or an installed Windows system font.");
            foreach (var label in overlayLabels) if (label) label.font = resolved;
            foreach (var label in worldLabels)
            {
                if (!label) continue;
                label.font = resolved;
                label.GetComponent<MeshRenderer>().sharedMaterial = resolved.material;
            }
        }

        void OnDestroy() { if (runtimeFont) Destroy(runtimeFont); }
    }
}
