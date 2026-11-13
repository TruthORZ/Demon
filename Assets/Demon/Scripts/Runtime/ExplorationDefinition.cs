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
                case ExplorationGuideKey.Day1: return Day1Guide + " · Recorded " + observations + "/3";
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
        public ObservationText Radio = new ObservationText { Title = "Radio log", Normal = "The radio plays a short tune on repeat.", Changed = "Only static here. The tune now comes from the lake.", Special = "" };
        public ObservationText WindVane = new ObservationText { Title = "Vane log", Normal = "The blade turns in the wind.", Changed = "The wind still blows. The blade hangs still.", Special = "When the beam caught the tracks, the vane began to turn." };
        public ObservationText Lake = new ObservationText { Title = "Shore log", Normal = "The seal is intact. The water is dark.", Changed = "The seal has opened. The water is still dark.", Special = "The lake shines, though the camp is still dark long after sunrise." };
        [TextArea] public string Day1Guide = "Check the radio, the vane and the shore.";
        [TextArea] public string Day2Guide = "Follow the boot prints south to the cave.";
        [TextArea] public string Day3Guide = "Follow the deep tracks off the southern trail. Watch for moving snow.";
        [TextArea] public string Day3FoundGuide = "The second tablet bears the fifth day's mark.";
        [TextArea] public string Day4Guide = "Search the old ritual grounds near the cave.";
        [TextArea] public string Day4FoundGuide = "The third tablet bears the seventh day's mark.";
        [TextArea] public string Day5Guide = "Follow the broadcast to the lake.";
        [TextArea] public string Day5FoundGuide = "The voice is still coming from the water.";
        [TextArea] public string Day6Guide = "Follow the forest path to the old listening post.";
        [TextArea] public string Day6FoundGuide = "Fit the missing stones to the listening tablet. Press Enter to set them.";
        [TextArea] public string Day6ActiveGuide = "Listen for which bell rings. Silence alone tells you little.";
        [Header("D7–D8 optional preparation")]
        [TextArea(4, 10)] public string SolarRecord = "Sunrise log\n\nDawn is overdue. A white rim hangs around a black sun. The lake still gives off light. The clock has not stopped. Compare the sun's outline with the bearing dial.";
        [TextArea(4, 12)] public string HandoverRecord = "Handover book · Eighth day\n\nRelief overdue. No word from the next watch. The clock still runs. Check the date on the fourth stone, the tool in Chest A and the bells in the woods. The last tablet remains on the shore.";
        [TextArea] public string Day7Guide = "Dawn is overdue. Read the sunrise log.";
        [TextArea] public string Day7CipherGuide = "Compare the radio, sun and vane with the dial. Open Chest A and take the tablet.";
        [TextArea] public string Day7BeamGuide = "Hold the beam on the tracks. Watch the vane, then release the beam.";
        [TextArea] public string Day7LockedGuide = "The tracks have stopped. Break the beam and watch the vane.";
        [TextArea] public string Day7ReadyGuide = "The bells tell you where it is. The beam buys time.";
        [TextArea] public string Day8Guide = "Relief is due. Read the handover book and check the clock.";
        [TextArea] public string Day8ToolGuide = "Still no relief. Check Chest A and the path to the lake.";
        [TextArea] public string Day8ListeningGuide = "Take the tool to the old listening post. The bells can warn you.";
        [Tooltip("{hour} is replaced with the authored T07 opening hour.")]
        [TextArea] public string Day8ReadyGuide = "After {hour}, search the shore. Bring the stone to the empty stand.";
        [TextArea] public string FoundGuide = "First tablet recorded. Retrace the trail to camp.";
    }
}
