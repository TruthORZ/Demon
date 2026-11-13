using Demon.Slice.Domain;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Demon.Slice
{
    [CreateAssetMenu(menuName = "Demon/Slice Settings")]
    [MovedFrom(false, sourceClassName: "SliceSettings")]
    public sealed class GameSettings : ScriptableObject
    {
        [Header("Time - one game day in real seconds")]
        [Tooltip("Real seconds per full day.")]
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
        [TextArea(3, 6)] public string Instructions = "WASD move · Shift run · E interact · Esc return";
        [TextArea(6, 12)] public string BoxText = "The line runs back to the day mark. At midnight it will test the date again and trade the seals once more.";
        [TextArea(3, 6)] public string EndingText = "Day Nine\n\nThe lake is open. This time it has a body.\n\nR · Start again";
    }
}
