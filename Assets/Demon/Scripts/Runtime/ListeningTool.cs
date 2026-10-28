using System;
using Demon.Slice.Domain;
using UnityEngine;

namespace Demon.Slice
{
    public sealed class ListeningTool : MonoBehaviour
    {
        [SerializeField] ListeningDefinition definition;
        [SerializeField] SliceTablet tablet;
        [SerializeField] ListeningChime[] chimes;
        SliceSceneController world;
        public RuneBoardModel Board { get; private set; }
        public bool Active => isActiveAndEnabled && Board != null && Board.Frozen;
        public SliceTablet Tablet => tablet;
        public ListeningDefinition Definition => definition;
        public ListeningChime[] Chimes => (ListeningChime[])chimes.Clone();
        public void Configure(ListeningDefinition data, SliceTablet stone, ListeningChime[] sensors)
        { definition = data; tablet = stone; chimes = sensors; }
        public void Initialize(SliceSceneController controller)
        {
            if (!definition || !tablet || chimes == null || chimes.Length == 0)
                throw new InvalidOperationException("T05 requires its saved definition, tablet and chimes.");
            world = controller;
            Board = new RuneBoardModel(definition.Initial, definition.Target, definition.LooseRunes);
            foreach (var chime in chimes)
            {
                if (!chime) throw new InvalidOperationException("Missing saved T05 chime.");
                chime.Initialize(this, world);
            }
            tablet.Initialize(world);
        }
        public bool BeginInteraction()
        {
            if (!isActiveAndEnabled || !world || world.Session.Descended) return false;
            if (world.Campaign)
            {
                var flow = world.Campaign;
                if (!flow.Progress.Available(4, world.Session.Clock.Day))
                { world.SetMessage("这块监听石板尚未发现。"); return false; }
                flow.Progress.Discover(4, world.Session.Clock.Day);
            }
            world.SetMessage(Active ? definition.ActivatedText : definition.Instructions);
            return true;
        }
        public bool TryCompile()
        {
            if (!BeginInteraction()) return false;
            if (!Board.TryFreeze())
            {
                world.SetMessage(Active ? "监听程序已固化；移走表面符文不会取消监听。" : "监听程序尚未补全。可以继续调整，未改变世界状态。");
                return false;
            }
            foreach (var chime in chimes) chime.SetListening(true);
            world.SetMessage(definition.ActivatedText);
            world.Player.ExitCloseView();
            return true;
        }
    }
}
