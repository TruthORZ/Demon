using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.UI;

namespace Demon.Slice
{
    [MovedFrom(false, sourceClassName: "SliceHud")]
    public sealed class GameHud : MonoBehaviour
    {
        [SerializeField] Text clockText;
        [SerializeField] Text messageText;
        [SerializeField] Text helpText;
        [SerializeField] Text focusText;
        [SerializeField] Text heldText;
        [SerializeField] GameObject readingPanel;
        [SerializeField] Text readingText;
        [SerializeField] GameObject endingPanel;
        [SerializeField] Text endingText;
        [SerializeField] Text toolStatus;
        [SerializeField] Text finalTabletStatus;
        [SerializeField] Text explorationStatus;
        public void ConfigureExplorationStatus(Text text) => explorationStatus = text;
        public void ConfigureFinalTabletStatus(Text text) => finalTabletStatus = text;
        public void ConfigureToolStatus(Text text) => toolStatus = text;
        IHudState state;
        Demon.Slice.Domain.GameSession session;
        PlayerController player;
        NineDayFlow campaign;
        FinaleSequence finale;
        BeamTether tether;
        FinalTabletRetrieval retrieval;
        ExplorationFlow exploration;
        string lastClock;
        public void Configure(Text clock, Text message, Text help, Text focus, Text held,
            GameObject reading, Text page, GameObject ending, Text end)
        { clockText = clock; messageText = message; helpText = help; focusText = focus; heldText = held;
            readingPanel = reading; readingText = page; endingPanel = ending; endingText = end; }

        public void Initialize(IHudState status, Demon.Slice.Domain.GameSession rules, GameSettings settings, PlayerController actor, NineDayFlow schedule, FinaleSequence sequence, BeamTether tool, FinalTabletRetrieval item, ExplorationFlow guide)
        {
            state = status; session = rules; player = actor; campaign = schedule; finale = sequence; tether = tool; retrieval = item; exploration = guide;
            helpText.text = settings.Instructions;
            readingText.text = settings.BoxText;
            endingText.text = settings.EndingText;
        }

        void Update()
        {
            if (state == null || session == null) return;
            bool directing = finale && finale.IsDirecting;
            clockText.gameObject.SetActive(!directing);
            int minute = (int)session.Clock.MinuteOfDay;
            string clock = $"灰盒样例  第 {session.Clock.Day} 天  {minute / 60:00}:{minute % 60:00}\n" +
                $"箱子 B：{(session.BoxSeal == 1 ? "封闭" : "解封")}　湖：{(session.LakeSeal == 1 ? "封印" : "开启")}";
            if (campaign) clock += $"\n石板记录 {campaign.Progress.FoundCount}/6　{campaign.Title}";
            if (lastClock != clock) { clockText.text = clock; lastClock = clock; }
            messageText.text = state.Message;
            messageText.gameObject.SetActive(!player.Reading && !directing);
            focusText.text = player.IsPickingUp ? $"按住 E 拾取　{player.CurrentPickup.Attempt.Progress:P0}　Esc 放下" : player.FocusPrompt;
            heldText.text = player.InspectingTablet ?
                $"手中符文：{RuneTablet.RuneLabel(player.ActiveTablet.Board.Held)}\n点击符文盘取符文，点击槽位交换。Enter 确认，Esc 退出。" : "";
            readingPanel.SetActive(player.Reading && !session.Descended);
            if (player.Reading && readingText.text != state.ReadingText) readingText.text = state.ReadingText;
            endingPanel.SetActive(state.EndingPresented);
            if (toolStatus)
            {
                toolStatus.gameObject.SetActive(tether && tether.isActiveAndEnabled && tether.Equipped && !player.Reading && !directing && !session.Descended);
                toolStatus.text = tether ? tether.Status : "";
            }
            if (explorationStatus)
            { bool show = exploration && exploration.ShowStatus && !player.Reading && !directing; explorationStatus.gameObject.SetActive(show); if (show) explorationStatus.text = exploration.Status; }
            if (finalTabletStatus)
            { bool show = retrieval && retrieval.Available && !player.Reading && !directing; finalTabletStatus.gameObject.SetActive(show); if (show) finalTabletStatus.text = retrieval.Status; }
            helpText.gameObject.SetActive(!player.Reading && !session.Descended && !(finale && finale.IsDirecting));
        }
    }
}
