using UnityEngine;
namespace Demon.Slice
{
    [CreateAssetMenu(menuName = "Demon/Chase Definition")]
    public sealed class ChaseDefinition : ScriptableObject
    {
        [Min(.1f)] public float MetresPerSecond = 3.6f;
        [Tooltip("追逐加速度，单位米/秒²；由本数据资产统一控制导航参数。"), Min(.1f)] public float AccelerationMetresPerSecondSquared = 16;
        [Tooltip("朝向转动速度，单位度/秒。"), Min(.1f)] public float AngularDegreesPerSecond = 540;
        [Min(.1f)] public float ClosePassMetres = 1.6f;
        [Min(.1f)] public float ClosePassCooldownSeconds = 4;
        [Min(.1f)] public float ReplanSeconds = .25f;
        [Min(.1f)] public float RetreatMinimumMetres = 3.5f;
        [Min(.1f)] public float TargetProjectionMetres = 1;
        [Min(.1f)] public float FootstepSeconds = .45f;
        [TextArea(2, 4)] public string InterruptedMessage = "雪痕逼近，拾取被打断。已完成的解谜和收集保留；退开后可以重试。";
        [TextArea(2, 4)] public string NearPassMessage = "雪痕擦身而过，暂时退开了。保持移动，留意雪地动静；可以先回营地整理线索。";
    }
}
