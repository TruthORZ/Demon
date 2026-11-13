using System;
using UnityEngine;

namespace Demon.Slice
{
    [Serializable] public sealed class DayRequirements
    {
        public string[] Tasks;
    }
    [Serializable] public sealed class JournalEntry
    {
        public string Id;
        public string Title;
        [TextArea(4, 18)] public string Body;
    }
    [CreateAssetMenu(menuName = "Demon/Complete Playthrough")]
    public sealed class PlaythroughDefinition : ScriptableObject
    {
        public DayRequirements[] Days;
        public JournalEntry[] Entries;
        [Range(0, 23)] public float NightHour = 18;
        [TextArea] public string Opening = "Alone at the snow station. Make your rounds and keep the watch until relief arrives. Tab opens the watch book.";
        [Min(1)] public float OpeningSeconds = 5;
        [Min(1)] public float NoticeSeconds = 6;
        [Min(.1f)] public float WaitSeconds = 3;
        [Tooltip("Pursuit speed on days 3–8 (m/s).")]
        public float[] ChaseSpeeds = { 2.6f, 3, 3.5f, 4, 4.6f, 5.7f };
        [Min(1)] public float DetectionMetres = 28;
        [Min(1)] public float LoseMetres = 42;
        [Min(.1f)] public float LostSightSeconds = 4;
        [Min(1)] public float LaughIntervalSeconds = 9;
        [Min(1)] public float MisleadingOffsetMetres = 9;
        [Min(1)] public float PursuitNoticeMetres = 14;
        public JournalEntry Entry(string id) => Array.Find(Entries, x => x.Id == id);
        public string[] Required(int day) => Days[Mathf.Clamp(day - 1, 0, 8)].Tasks;
    }
}
