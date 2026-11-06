using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Demon.Slice.Editor
{
    // Tests replacement boundaries with an unrelated puzzle implementation and deliberately conflicting saved dates.
    public sealed class ArchitectureSmokeRun : ISmokeRun
    {
        sealed class ProbeInteraction : ICloseViewInteraction
        {
            public int Confirmations, Clicks;
            public Collider Target;
            public bool Confirm() { Confirmations++; return true; }
            public void Click(Collider target) { Clicks++; Target = target; }
        }
        readonly Action<bool, string> finish;
        readonly double started = EditorApplication.timeSinceStartup;
        GameSceneController world;
        SmokePlayerDriver driver;
        readonly ProbeInteraction interaction = new ProbeInteraction();
        int stage, lastFrame = -1;
        double since;
        bool finished;
        public ArchitectureSmokeRun(Action<bool, string> callback) => finish = callback;
        static void Require(bool valid, string message) { if (!valid) throw new InvalidOperationException(message); }
        public void Tick()
        {
            if (finished || !Application.isPlaying) return;
            try
            {
                if (EditorApplication.timeSinceStartup - started > 35) throw new TimeoutException("Architecture boundary stage " + stage);
                if (!world) world = UnityEngine.Object.FindAnyObjectByType<GameSceneController>();
                if (!world || world.Session == null || world.Exploration.Log == null || lastFrame == Time.frameCount) return;
                lastFrame = Time.frameCount; double now = Time.timeAsDouble;
                if (stage == 0)
                {
                    Require(world.Session.Clock.Day == 1 && world.Tether.MotionDay == 4 && world.Tether.Definition.DemonMotionDay == 2, "Campaign date did not override standalone fallback.");
                    Require(world.Exploration.Status == world.Exploration.Definition.Day7CipherGuide, "Saved guide-rule edit was ignored.");
                    world.Session.Clock.AdvanceTo(2 * 1440 + 480); Next(1, now);
                }
                else if (stage == 1 && now - since > .4)
                {
                    Require(!world.Tether.MotionReady && !world.Tether.TargetVisible && world.Chase.Phase == ChaseController.ChasePhase.Dormant, "Motion started before authored T01 date.");
                    world.Session.Clock.AdvanceTo(3 * 1440 + 480); Next(2, now);
                }
                else if (stage == 2 && now - since > .7)
                {
                    Require(world.Tether.MotionReady && world.Tether.TargetVisible && world.Chase.HasNavigation, "Motion did not start on authored T01 date.");
                    driver = new SmokePlayerDriver(world.Player, world.Settings, 30);
                    world.Player.EnterInteractionView(interaction, world.Cipher.CloseView); Next(3, now);
                }
                else if (stage == 3 && now - since > 1.5) { driver.Keys(Key.Enter); Next(4, now); }
                else if (stage == 4 && now - since > .15)
                {
                    Require(interaction.Confirmations == 1, "Generic close-up did not receive one physical Enter."); driver.Keys();
                    var digit = world.Cipher.GetComponentInChildren<CipherDigit>();
                    Vector3 screen = world.Player.ViewCamera.WorldToScreenPoint(digit.GetComponent<Collider>().bounds.center);
                    Require(screen.z > 0, "Saved lock digit is behind camera.");
                    InputSystem.QueueStateEvent(driver.Mouse, new MouseState { position = new Vector2(screen.x, screen.y), buttons = 1 }); Next(5, now);
                }
                else if (stage == 5 && now - since > .15)
                {
                    Require(interaction.Clicks == 1 && interaction.Target && interaction.Target.transform.IsChildOf(world.Cipher.transform), "Generic close-up did not receive physical collider click.");
                    InputSystem.QueueStateEvent(driver.Mouse, new MouseState()); driver.Keys(Key.Escape); Next(6, now);
                }
                else if (stage == 6 && now - since > .15) { driver.Keys(); Next(7, now); }
                else if (stage == 7 && world.Player.CanUseTools)
                {
                    Require(world.Campaign.Progress.FoundCount == 0 && world.Session.ExchangeCount == 0 && !world.Finale.TimeReserved, "Presentation/input probe changed discoveries, SEAL or reservation.");
                    Complete(true, "Saved T01 D4 overrides conflicting fallback D2; D3 dormant/D4 native pursuit; saved guide override; unrelated close-up receives real Enter/collider click/Esc; no discovery/SEAL/reservation changes. Restored on exit.");
                }
            }
            catch (Exception e) { Complete(false, e.ToString()); }
        }
        void Next(int value, double now) { stage = value; since = now; }
        public void Cleanup() { driver?.Dispose(); driver = null; }
        void Complete(bool success, string message)
        {
            finished = true; Cleanup(); finish(success, message); EditorApplication.delayCall += EditorApplication.ExitPlaymode;
        }
    }
}
