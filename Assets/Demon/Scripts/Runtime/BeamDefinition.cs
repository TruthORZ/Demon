using UnityEngine;
namespace Demon.Slice
{
    [CreateAssetMenu(menuName = "Demon/Beam Tether Definition")]
    public sealed class BeamDefinition : ScriptableObject
    {
        [Header("World MOTION boundary (T01); acquisition itself has no D7 gate")]
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
        [TextArea(2, 4)] public string ReadyText = "T06 · 按住鼠标右键，稳定照住雪痕区域。\n移开、遮挡或超距后会恢复运动。";
        [TextArea(2, 4)] public string LockedText = "运动已交换：雪痕停止，风向标转动。\n保持照射；转身或进入近景会失锁。";
    }
}
