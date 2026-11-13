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
        [SerializeField] bool includePlacedChimes;
        ListeningChime[] sessionChimes;
        GameSession session;
        NineDayFlow campaign;
        PlayerController player;
        ISceneFeedback feedback;
        public RuneBoardModel Board { get; private set; }
        public bool Active => isActiveAndEnabled && Board != null && Board.Frozen;
        public RuneTablet Tablet => tablet;
        public ListeningDefinition Definition => definition;
        public ListeningChime[] Chimes => (ListeningChime[])(sessionChimes ?? chimes).Clone();
        public void Configure(ListeningDefinition data, RuneTablet stone, ListeningChime[] sensors)
        { definition = data; tablet = stone; chimes = sensors; }
        public void Initialize(GameSession rules, NineDayFlow flow, PlayerController actor, ISceneFeedback messages, Transform pursuitTarget = null)
        {
            if (rules == null || !actor || messages == null || !definition || !tablet || (!includePlacedChimes && (chimes == null || chimes.Length == 0)))
                throw new InvalidOperationException("T05 requires its saved definition, tablet and chimes.");
            session = rules; campaign = flow; player = actor; feedback = messages;
            Board = new RuneBoardModel(definition.Initial, definition.Target, definition.LooseRunes);
            sessionChimes = chimes;
            if (includePlacedChimes)
            {
                var placed = new System.Collections.Generic.List<ListeningChime>();
                if (chimes != null) foreach (var configured in chimes) if (configured) placed.Add(configured);
                foreach (var root in gameObject.scene.GetRootGameObjects())
                    foreach (var chime in root.GetComponentsInChildren<ListeningChime>(true))
                        if (!placed.Contains(chime)) placed.Add(chime);
                sessionChimes = placed.ToArray();
            }
            foreach (var chime in sessionChimes)
            {
                if (!chime) throw new InvalidOperationException("Missing saved T05 chime.");
                chime.Initialize(this, session, pursuitTarget);
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
                { feedback.SetMessage("The listening tablet has yet to be found."); return false; }
                flow.Progress.Discover(4, session.Clock.Day);
            }
            feedback.SetMessage(includePlacedChimes ? (Active ? "The clapper trembles." : "Several stones have worked loose.") : Active ? definition.ActivatedText : definition.Instructions);
            return true;
        }
        public bool TryCompile()
        {
            if (!BeginInteraction()) return false;
            if (!Board.TryFreeze())
            {
                feedback.SetMessage(Active ? "The listening lines have fused." : "The lines flicker and go dark.");
                return false;
            }
            foreach (var chime in sessionChimes) chime.SetListening(true);
            feedback.SetMessage(includePlacedChimes ? "A bell stirs among the trees." : definition.ActivatedText);
            player.ExitCloseView();
            return true;
        }
    }
}
