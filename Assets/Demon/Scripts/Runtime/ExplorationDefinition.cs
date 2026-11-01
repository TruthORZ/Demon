using System;
using UnityEngine;
namespace Demon.Slice
{
    [Serializable] public sealed class ObservationText
    {
        public string Title;
        [TextArea(3, 7)] public string Normal;
        [TextArea(3, 7)] public string Changed;
        [TextArea(3, 7)] public string Special;
    }
    [CreateAssetMenu(menuName = "Demon/First Days Exploration")]
    public sealed class ExplorationDefinition : ScriptableObject
    {
        public ObservationText Radio = new ObservationText { Title = "电台观察", Normal = "刚才电台正在播放固定短旋律，声音来自电台本身。记住它的节奏与位置。", Changed = "电台只剩静电底噪。原先熟悉的旋律从湖中传来。", Special = "" };
        public ObservationText WindVane = new ObservationText { Title = "风向标观察", Normal = "刚才叶片正在转动，风向标根部的位置固定。", Changed = "风仍然吹着，叶片却已经停止。回想石板上预告的日期。", Special = "刚才稳定照射雪痕时，叶片恢复了转动。检查记录会保留当时的观察；放下光束后情况可能变化。" };
        public ObservationText Lake = new ObservationText { Title = "湖岸观察", Normal = "刚才封印标记完整。湖岸没有对外发出异光。沿岸观察，记住这里的正常状态。", Changed = "封印已经开启，湖面仍然昏暗。观察到的状态与日期一起记下。", Special = "湖中持续发光，即使到了预期日出，营地仍是夜色。" };
        [TextArea] public string Day1Guide = "可选值守观察：检查电台、风向标与湖岸。记录不限制睡觉。";
        [TextArea] public string Day2Guide = "南侧小脚印通向林中山洞。沿痕迹寻找 T01。";
        [TextArea] public string Day3Guide = "风向标已停止。南侧小径向西岔开的大雪痕通向 T02；留意正在接近的雪雾。";
        [TextArea] public string Day3FoundGuide = "T02 预告 D5 声音交换。大雪痕是留下的路线，当前威胁仍会移动。可返营整理。";
        [TextArea] public string Day4Guide = "调查山洞西侧外面的旧仪式遗址，寻找 T03。搜寻不限制睡觉。";
        [TextArea] public string Day4FoundGuide = "T03 预告 D7 光的转移。对照当前日光与电台，记住两条日期预告。";
        [TextArea] public string Day5Guide = "电台只剩静电。循湖中熟悉的广播到湖岸，寻找 T04。";
        [TextArea] public string Day5FoundGuide = "T04 预告 D9 获得实体。广播仍从湖中传来；可返营整理或睡觉。";
        [TextArea] public string Day6Guide = "沿南侧林径向东，寻找旧监听站的 T05 与两只风铃。";
        [TextArea] public string Day6FoundGuide = "补全 T05：中心 RING、下中 CHIME，按 Enter 启用风铃。不改变日期。";
        [TextArea] public string Day6ActiveGuide = "风铃已启用。先在木墙北侧看雪痕经过，再绕墙听高低音辨认区域。沉默不代表安全。";
        [TextArea] public string FoundGuide = "已记录 T01 的预告。可以沿原路回营地，也可继续探索。";
    }
}
