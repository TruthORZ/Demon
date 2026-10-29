using Demon.Slice.Domain;
using UnityEngine;

namespace Demon.Slice
{
    [CreateAssetMenu(menuName = "Demon/Slice Settings")]
    public sealed class SliceSettings : ScriptableObject
    {
        [Header("Time - one game day in real seconds")]
        [Min(10)] public float RealSecondsPerDay = 1440;
        [Min(1)] public int StartDay = 8;
        [Range(0, 23.9f)] public float StartHour = 16;
        [Min(1)] public int FinalDay = 9;
        [Range(0, 23.9f)] public float WakeHour = 8;
        public bool InitialLakeSealed;

        [Header("First person and close view")]
        [Min(.1f)] public float WalkMetresPerSecond = 3.3f;
        [Min(.1f)] public float SprintMetresPerSecond = 5;
        [Min(.01f)] public float MouseSensitivity = .12f;
        [Min(.1f)] public float InteractionMetres = 3.4f;
        [Min(.05f)] public float CameraBlendSeconds = .4f;

        [Header("Prototype layout - slots 1 to 9, not final puzzle design")]
        public RuneKind[] Target = { RuneKind.Nine, RuneKind.GreaterEqual, RuneKind.Day,
            RuneKind.None, RuneKind.Exchange, RuneKind.None, RuneKind.Seal, RuneKind.Box, RuneKind.Lake };
        public RuneKind[] Initial = { RuneKind.Nine, RuneKind.GreaterEqual, RuneKind.Day,
            RuneKind.None, RuneKind.None, RuneKind.None, RuneKind.Seal, RuneKind.None, RuneKind.None };
        public RuneKind[] LooseRunes = { RuneKind.Exchange, RuneKind.Box, RuneKind.Lake, RuneKind.Open };

        [Header("Authored lighting - driven by the world clock")]
        public Color DayAmbient = new Color(.58f, .68f, .78f);
        public Color NightAmbient = new Color(.12f, .17f, .27f);
        public Color DayFog = new Color(.65f, .75f, .84f);
        public Color NightFog = new Color(.025f, .045f, .09f);
        [Min(0)] public float DaySunIntensity = 1.15f;
        [Min(0)] public float NightSunIntensity = .12f;

        [Header("Chinese placeholder text - replace with final authored content")]
        [TextArea(3, 6)] public string Instructions = "WASD 移动　鼠标观察　Shift 奔跑\n靠近石板或箱子按 E　靠近睡袋按 E 跳到次日\n石板近景：鼠标点选符文和槽位　Enter 确认　Esc 退出";
        [TextArea(6, 12)] public string BoxText = "【箱内记录】\n\nIF满足条件后只执行一次，随后结束。\nWHILE沿返回线路重新等待，每个符合日期的日子执行一次。\n\n从末端参数槽回到DAY的那条线，并不是裂痕。\n今天的交换已经结束，下一轮会在午夜到来。\n\n交换是双向的。再交换一次，封闭就会回到箱子，湖将重新开启。\n熔断后，程序留在石板内部，取下符文也无法撤销。\n\n第九天，它将获得实体。\n\nEsc 放下记录。";
        [TextArea(3, 6)] public string EndingText = "第九天\n\n湖中封印开启。\n这一次，站在湖边的东西拥有了实体。\n\n灰盒样例结束　按 R 重新开始";
    }
}
