using System;
using UnityEngine;

namespace Demon.Slice
{
    [Serializable] public sealed class DayPlan
    {
        public string Title;
        [TextArea(2, 4)] public string Objective;
        [Range(0, 23.9f)] public float WakeHour = 8;
    }
    [Serializable] public sealed class FixedTabletPlan
    {
        public string Title;
        [Range(1, 9)] public int DiscoveryDay;
        [Range(0, 9)] public int DateNodeDay;
        [TextArea(5, 12)] public string Record;
    }
    [CreateAssetMenu(menuName = "Demon/Nine Day Definition")]
    public sealed class NineDayDefinition : ScriptableObject
    {
        // All campaign consumers use T01's actual authored date, not a second copy in a tool definition.
        public int MotionDay
        {
            get
            {
                if (Tablets == null || Tablets.Length == 0 || Tablets[0] == null || Tablets[0].DateNodeDay < 1 || Tablets[0].DateNodeDay > 9)
                    throw new InvalidOperationException("T01 requires a motion date between D1 and D9.");
                return Tablets[0].DateNodeDay;
            }
        }
        public DayPlan[] Days = new DayPlan[9];
        public FixedTabletPlan[] Tablets = new FixedTabletPlan[6];
        [Tooltip("Temporary pacing for the first-night opening; no cultist sequence yet.")]
        [Range(0, 23.9f)] public float FirstNightOpeningHour = 22;
        [Range(1, 9)] public int FinalTabletDay = 8;
        [Tooltip("T07 appears by clock time, even during the permanent night. Does not itself reserve time.")]
        [Range(0, 23.9f)] public float FinalTabletHour = 22;
        [TextArea(2, 4)] public string FirstNightMessage = "Ice cracks out on the lake.";
    }
}
