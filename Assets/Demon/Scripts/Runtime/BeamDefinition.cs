using UnityEngine;
namespace Demon.Slice
{
    [CreateAssetMenu(menuName = "Demon/Beam Tether Definition")]
    public sealed class BeamDefinition : ScriptableObject
    {
        [Header("World MOTION boundary (T01); acquisition itself has no D7 gate")]
        [Tooltip("Standalone fallback only. Campaign scenes use NineDayDefinition / T01 / DateNodeDay.")]
        [Range(1, 9)] public int DemonMotionDay = 3;
        [Header("Aiming and stable hold")]
        [Min(.1f)] public float RangeMetres = 16;
        [Min(.05f)] public float RegionRadiusMetres = .85f;
        [Min(.05f)] public float StableSeconds = 1;
        [Header("Saved flashlight and vane tuning")]
        [Range(1, 120)] public float SpotAngleDegrees = 26;
        [Min(0)] public float LightIntensity = 14;
        public Color BeamColor = new Color(.72f, .9f, 1);
        [Min(0)] public float VaneDegreesPerSecond = 100;
        [TextArea(2, 4)] public string ReadyText = "Keep the beam on the tracks. Break the beam and they will move again.";
        [TextArea(2, 4)] public string LockedText = "The tracks have stopped. Keep the light steady.";
    }
}
