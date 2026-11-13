using Demon.Slice.Domain;
using UnityEngine;

namespace Demon.Slice
{
    // Each renderer is a physical obsidian insert on the saved tablet prefab.
    public sealed class TabletSurface : MonoBehaviour
    {
        [SerializeField] MeshRenderer[] inserts;
        [SerializeField] Material[] glyphsByRuneKind;
        [SerializeField] Renderer[] paths;
        [SerializeField] Material dormantPath, fusedPath;
        public void Configure(MeshRenderer[] stones, Material[] glyphs, Renderer[] wiring, Material dim, Material lit)
        { inserts = stones; glyphsByRuneKind = glyphs; paths = wiring; dormantPath = dim; fusedPath = lit; }
        public void Display(RuneBoardModel board)
        {
            for (int i = 0; i < inserts.Length; i++)
            {
                RuneKind rune = board.At(i);
                var stone = inserts[i];
                stone.gameObject.SetActive(rune != RuneKind.None);
                if (rune != RuneKind.None) stone.sharedMaterial = glyphsByRuneKind[(int)rune];
            }
            foreach (var path in paths) path.sharedMaterial = board.Frozen ? fusedPath : dormantPath;
        }
    }
}
