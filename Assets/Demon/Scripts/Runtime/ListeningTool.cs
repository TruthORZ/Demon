using System;
using Demon.Slice.Domain;
using UnityEngine;

namespace Demon.Slice
{
    public sealed class ListeningTool : MonoBehaviour, ITabletProgram
    {
        [SerializeField] ListeningDefinition definition;
        [SerializeField] RuneTablet tablet;
        [SerializeField] ListeningChime[] chimes;
        GameSession session;
        NineDayFlow campaign;
        PlayerController player;
        ISceneFeedback feedback;
        public RuneBoardModel Board { get; private set; }
        public bool Active => isActiveAndEnabled && Board != null && Board.Frozen;
        public RuneTablet Tablet => tablet;
        public ListeningDefinition Definition => definition;
        public ListeningChime[] Chimes => (ListeningChime[])chimes.Clone();
        public void Configure(ListeningDefinition data, RuneTablet stone, ListeningChime[] sensors)
        { definition = data; tablet = stone; chimes = sensors; }
        public void Initialize(GameSession rules, NineDayFlow flow, PlayerController actor, ISceneFeedback messages)
        {
            if (rules == null || !actor || messages == null || !definition || !tablet || chimes == null || chimes.Length == 0)
                throw new InvalidOperationException("T05 requires its saved definition, tablet and chimes.");
            session = rules; campaign = flow; player = actor; feedback = messages;
            Board = new RuneBoardModel(definition.Initial, definition.Target, definition.LooseRunes);
            foreach (var chime in chimes)
            {
                if (!chime) throw new InvalidOperationException("Missing saved T05 chime.");
                chime.Initialize(this, session);
            }
            tablet.Initialize(this);
        }
        public bool BeginInteraction()
        {
            if (!isActiveAndEnabled || session == null || session.Descended) return false;
            if (campaign)
            {
                var flow = campaign;
                if (!flow.Progress.Available(4, session.Clock.Day))
                { feedback.SetMessage("这块监听石板尚未发现。"); return false; }
                flow.Progress.Discover(4, session.Clock.Day);
            }
            feedback.SetMessage(Active ? definition.ActivatedText : definition.Instructions);
            return true;
        }
        public bool TryCompile()
        {
            if (!BeginInteraction()) return false;
            if (!Board.TryFreeze())
            {
                feedback.SetMessage(Active ? "监听程序已固化；移走表面符文不会取消监听。" : "监听程序尚未补全。可以继续调整，未改变世界状态。");
                return false;
            }
            foreach (var chime in chimes) chime.SetListening(true);
            feedback.SetMessage(definition.ActivatedText);
            player.ExitCloseView();
            return true;
        }
    }
}
