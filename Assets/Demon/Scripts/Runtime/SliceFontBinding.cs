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
        [SerializeField] Material worldTextMaterial;
        Material runtimeWorldMaterial;
        Font resolvedFont;
        public void ConfigureWorldMaterial(Material source) => worldTextMaterial = source;

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
            resolvedFont = resolved;
            if (worldTextMaterial)
            {
                runtimeWorldMaterial = new Material(worldTextMaterial);
                runtimeWorldMaterial.mainTexture = resolved.material.mainTexture;
                Font.textureRebuilt += OnFontTextureRebuilt;
            }
            foreach (var label in overlayLabels) if (label) label.font = resolved;
            foreach (var label in worldLabels)
            {
                if (!label) continue;
                label.font = resolved;
                label.GetComponent<MeshRenderer>().sharedMaterial = runtimeWorldMaterial ? runtimeWorldMaterial : resolved.material;
            }
        }

        void OnFontTextureRebuilt(Font font)
        {
            if (font == resolvedFont && runtimeWorldMaterial) runtimeWorldMaterial.mainTexture = font.material.mainTexture;
        }
        void OnDestroy()
        {
            Font.textureRebuilt -= OnFontTextureRebuilt;
            if (runtimeWorldMaterial) Destroy(runtimeWorldMaterial);
            if (runtimeFont) Destroy(runtimeFont);
        }
    }
}
