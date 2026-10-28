using UnityEngine;
namespace Demon.Slice
{
    public sealed class CipherDigit : MonoBehaviour
    {
        [Range(0, 2)] public int Index;
        public TextMesh Label;
    }
}
