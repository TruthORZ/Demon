using System;
using Demon.Slice.Domain;
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
        [Header("Ordered display rules - first match wins; Day 0 is fallback")]
        public ExplorationGuideRule[] GuideRules = ExplorationGuidance.DefaultRules();
        public string GuideText(ExplorationGuideKey key, int observations, float openingHour)
        {
            switch (key)
            {
                case ExplorationGuideKey.Day1: return Day1Guide + "\n已记录 " + observations + "/3";
                case ExplorationGuideKey.Day2: return Day2Guide;
                case ExplorationGuideKey.Found: return FoundGuide;
                case ExplorationGuideKey.Day3: return Day3Guide;
                case ExplorationGuideKey.Day3Found: return Day3FoundGuide;
                case ExplorationGuideKey.Day4: return Day4Guide;
                case ExplorationGuideKey.Day4Found: return Day4FoundGuide;
                case ExplorationGuideKey.Day5: return Day5Guide;
                case ExplorationGuideKey.Day5Found: return Day5FoundGuide;
                case ExplorationGuideKey.Day6: return Day6Guide;
                case ExplorationGuideKey.Day6Found: return Day6FoundGuide;
                case ExplorationGuideKey.Day6Active: return Day6ActiveGuide;
                case ExplorationGuideKey.Day7: return Day7Guide;
                case ExplorationGuideKey.Day7Cipher: return Day7CipherGuide;
                case ExplorationGuideKey.Day7Beam: return Day7BeamGuide;
                case ExplorationGuideKey.Day7Locked: return Day7LockedGuide;
                case ExplorationGuideKey.Day7Ready: return Day7ReadyGuide;
                case ExplorationGuideKey.Day8: return Day8Guide;
                case ExplorationGuideKey.Day8Tool: return Day8ToolGuide;
                case ExplorationGuideKey.Day8Listening: return Day8ListeningGuide;
                case ExplorationGuideKey.Day8Ready:
                    int opening = Mathf.RoundToInt(openingHour * 60);
                    return Day8ReadyGuide.Replace("{hour}", (opening / 60).ToString("00") + ":" + (opening % 60).ToString("00"));
                default: throw new ArgumentOutOfRangeException(nameof(key));
            }
        }
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
        [Header("D7–D8 optional preparation")]
        [TextArea(4, 10)] public string SolarRecord = "【日出观察】\n\n已经过了预期日出，营地仍然是夜色。太阳只剩黑心与白边，湖中却持续发光。\n对照 T03：转移的是光，不是时间。挂钟和日期仍在继续。\n\n原先难以辨认的符号与太阳轮廓一致。回到工具棚，对照轮盘上的物体方位，检查箱子 A。";
        [TextArea(4, 12)] public string HandoverRecord = "【第八天 · 值守交接记录】\n\n原定今天交班。截至当前记录时刻，仍未见接班人员到达。缺席原因未记录。\n挂钟仍在走，天色不能再用来判断时刻。\n\n交班前重新核对：\nT04 记载的第九天日期；\n箱子 A 中留下的运动交换工具；\n林间风铃提供的区域预警。\n\n最后一块封印石板仍待取回。整理已有线索与工具，夜间再检查湖岸。";
        [TextArea] public string Day7Guide = "预期日出已经过去，仍然没有日光。到轮盘北侧的观察记录处，对照失光的太阳与湖中异光。";
        [TextArea] public string Day7CipherGuide = "太阳轮廓与箱面符号对应。对照轮盘上的电台、太阳、风向标方位，解开 A 后再按 E 取出 T06。";
        [TextArea] public string Day7BeamGuide = "到湖岸观察点，按住右键稳定照住正在移动的雪痕；观察雪痕停止与风向标转动，再松开验证恢复。";
        [TextArea] public string Day7LockedGuide = "运动已交换：雪痕停止，风向标转动。松开右键或移开光束，观察运动恢复。";
        [TextArea] public string Day7ReadyGuide = "已经验证锁定与恢复。风铃判断区域，光束争取行动时间；可以继续熟悉路线或睡到次日。";
        [TextArea] public string Day8Guide = "原定今天交班。到工具棚外的交接台，查看值守交接记录；夜色中的时刻以挂钟为准。";
        [TextArea] public string Day8ToolGuide = "接班者仍未到。可对照轮盘解开箱子 A、取出 T06，再熟悉去湖岸的路线；准备与阅读均为可选。";
        [TextArea] public string Day8ListeningGuide = "T06 已取得。可沿南侧林径到旧监听站补全 T05，取得区域预警；已完成的工具会一直保留，也可直接等待夜间或睡觉。";
        [Tooltip("{hour} is replaced with the authored T07 opening hour.")]
        [TextArea] public string Day8ReadyGuide = "交接记录已核对。T07 在 {hour} 后可到湖岸寻找；风铃判断区域，光束争取时间，取回后带到棚西侧空台。也可直接睡觉。";
        [TextArea] public string FoundGuide = "已记录 T01 的预告。可以沿原路回营地，也可继续探索。";
    }
}
