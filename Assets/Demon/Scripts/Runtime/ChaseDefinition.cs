using UnityEngine;
namespace Demon.Slice
{
    [CreateAssetMenu(menuName = "Demon/Chase Definition")]
    public sealed class ChaseDefinition : ScriptableObject
    {
        [Min(.1f)] public float MetresPerSecond = 3.6f;
        [Tooltip("Pursuit acceleration (m/s²)."), Min(.1f)] public float AccelerationMetresPerSecondSquared = 16;
        [Tooltip("Turning speed (degrees per second)."), Min(.1f)] public float AngularDegreesPerSecond = 540;
        [Min(.1f)] public float ClosePassMetres = 1.6f;
        [Min(.1f)] public float ClosePassCooldownSeconds = 4;
        [Min(.1f)] public float ReplanSeconds = .25f;
        [Min(.1f)] public float RetreatMinimumMetres = 3.5f;
        [Min(.1f)] public float TargetProjectionMetres = 1;
        [Min(.1f)] public float FootstepSeconds = .45f;
        [TextArea(2, 4)] public string InterruptedMessage = "The tracks broke your grip. Back away and try again.";
        [TextArea(2, 4)] public string NearPassMessage = "Something brushed past. Keep moving. The cabin is close.";
    }
}
