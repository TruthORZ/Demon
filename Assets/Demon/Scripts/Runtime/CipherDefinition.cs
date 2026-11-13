using Demon.Slice.Domain;
using UnityEngine;
namespace Demon.Slice
{
    [CreateAssetMenu(menuName = "Demon/Bearing Cipher Definition")]
    public sealed class CipherDefinition : ScriptableObject
    {
        [Tooltip("Digits clockwise from the fixed north marker. Must contain 0-9 exactly once.")]
        public int[] DigitsClockwise = { 4, 7, 2, 9, 1, 6, 8, 0, 5, 3 };
        [TextArea(3, 6)] public string WheelInstructions = "Take the bearing of the radio, the sun and the weather vane from the dial. Enter their digits in the order engraved on the chest. Use the vane's position, not the direction of its blade.";
        [TextArea(2, 4)] public string LockInstructions = "Read the marks from left to right. Click a wheel to turn it. Enter to try the latch. Esc to step back.";
        [TextArea(2, 4)] public string UnlockedText = "The catch gives. Press E to take the tablet inside.";
    }
}
