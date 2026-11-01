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
        public DayPlan[] Days = new DayPlan[9];
        public FixedTabletPlan[] Tablets = new FixedTabletPlan[6];
        [Tooltip("Temporary pacing for the first-night opening; no cultist sequence yet.")]
        [Range(0, 23.9f)] public float FirstNightOpeningHour = 22;
        [Range(1, 9)] public int FinalTabletDay = 8;
        [Tooltip("T07 appears by clock time, even during the permanent night. Does not itself reserve time.")]
        [Range(0, 23.9f)] public float FinalTabletHour = 22;
        [TextArea(2, 4)] public string FirstNightMessage = "暴风雪中的湖发生了变化。封印已开启。\n（当前为流程占位，尚无邪教徒演出。）";
    }
}
