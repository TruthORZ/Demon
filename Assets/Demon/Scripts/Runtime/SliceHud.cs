using UnityEngine;
using UnityEngine.UI;

namespace Demon.Slice
{
    public sealed class SliceHud : MonoBehaviour
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
        public void ConfigureToolStatus(Text text) => toolStatus = text;
        SliceSceneController world;
        string lastClock;
        public void Configure(Text clock, Text message, Text help, Text focus, Text held,
            GameObject reading, Text page, GameObject ending, Text end)
        { clockText = clock; messageText = message; helpText = help; focusText = focus; heldText = held;
            readingPanel = reading; readingText = page; endingPanel = ending; endingText = end; }

        public void Initialize(SliceSceneController controller)
        {
            world = controller;
            helpText.text = world.Settings.Instructions;
            readingText.text = world.Settings.BoxText;
            endingText.text = world.Settings.EndingText;
        }

        void Update()
        {
            if (!world || world.Session == null) return;
            var session = world.Session;
            bool directing = world.Finale && world.Finale.IsDirecting;
            clockText.gameObject.SetActive(!directing);
            int minute = (int)session.Clock.MinuteOfDay;
            string clock = $"灰盒样例  第 {session.Clock.Day} 天  {minute / 60:00}:{minute % 60:00}\n" +
                $"箱子 B：{(session.BoxSeal == 1 ? "封闭" : "解封")}　湖：{(session.LakeSeal == 1 ? "封印" : "开启")}";
            if (world.Campaign) clock += $"\n石板记录 {world.Campaign.Progress.FoundCount}/6　{world.Campaign.Title}";
            if (lastClock != clock) { clockText.text = clock; lastClock = clock; }
            messageText.text = world.Message;
            messageText.gameObject.SetActive(!world.Player.Reading && !directing);
            focusText.text = world.Player.FocusPrompt;
            heldText.text = world.Player.InspectingTablet ?
                $"手中符文：{SliceTablet.RuneLabel(world.Player.ActiveTablet.Board.Held)}\n点击符文盘取符文，点击槽位交换。Enter 确认，Esc 退出。" : "";
            readingPanel.SetActive(world.Player.Reading && !session.Descended);
            if (world.Player.Reading && readingText.text != world.ReadingText) readingText.text = world.ReadingText;
            endingPanel.SetActive(world.EndingPresented);
            if (toolStatus)
            {
                toolStatus.gameObject.SetActive(world.Tether && world.Tether.isActiveAndEnabled && world.Tether.Equipped && !world.Player.Reading && !directing && !session.Descended);
                toolStatus.text = world.Tether ? world.Tether.Status : "";
            }
            helpText.gameObject.SetActive(!world.Player.Reading && !session.Descended && !(world.Finale && world.Finale.IsDirecting));
        }
    }
}
