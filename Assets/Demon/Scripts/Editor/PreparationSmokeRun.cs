using System;
using System.IO;
using System.Linq;
using Demon.Slice.Domain;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Demon.Slice.Editor
{
    // Walks the saved map with real Input System events. Never moves or disables the production pursuer.
    public sealed class PreparationSmokeRun : ISmokeRun
    {
        readonly string operation;
        readonly Action<bool, string> finish;
        readonly double started = EditorApplication.timeSinceStartup;
        GameSceneController world; ExplorationFlow flow;
        SmokePlayerDriver driver;
        Mouse mouse => driver.Mouse;
        bool finished, sawAgent, sawLock, released;
        int stage, click, repeat, routeIndex, lastFrame = -1;
        double since, lastAim;
        Vector3 frozen;
        Quaternion vane;
        float metres;
        Transform[] targets;
        bool Late => operation == "prep-late";
        bool Early => operation == "prep-early";
        bool Tuning => operation == "prep-tuning";
        public PreparationSmokeRun(string op, Action<bool, string> callback) { operation = op; finish = callback; }
        static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
        public void Tick()
        {
            if (finished || !Application.isPlaying) return;
            try
            {
                if (EditorApplication.timeSinceStartup - started > 300) throw new TimeoutException("Preparation stage " + stage);
                if (!world) world = UnityEngine.Object.FindAnyObjectByType<GameSceneController>();
                if (world && !world.enabled) throw new InvalidOperationException("World disabled; inspect initialization error.");
                if (!world || world.Session == null || !world.Exploration || world.Exploration.Log == null) return;
                if (lastFrame == Time.frameCount) return; lastFrame = Time.frameCount;
                flow = world.Exploration; double now = Time.timeAsDouble;
                if (stage == 0 && !Early && !world.Chase.HasNavigation) return;
                if (stage > 0)
                {
                    Require(world.Chase.enabled, "Preparation disabled the production chase.");
                    if (world.Chase.HasNavigation && world.Chase.Agent.velocity.sqrMagnitude > .05f) sawAgent = true;
                }
                if (stage == 0)
                {
                    Require(flow.HasFinalPreparation && world.Campaign.Progress.FoundCount == 0 && !world.Cipher.Acquired && !world.Listening.Active, "Missing clean saved preparation state.");
                    if (operation == "prep-boundary")
                    {
                        Require(world.Session.Clock.Day == 6 && !flow.SolarRecord.gameObject.activeSelf && !flow.HandoverRecord.gameObject.activeSelf, "Records appeared early.");
                        flow.ReadPreparation(flow.SolarRecord); flow.ReadPreparation(flow.HandoverRecord);
                        Require(!flow.SolarRead && !flow.HandoverRead && !world.Player.Reading, "Direct early record call bypassed date."); Next(80, now); return;
                    }
                    Require(world.Session.Clock.Day == (Early ? 1 : Late ? 8 : 7) && flow.SolarRecord.gameObject.activeSelf == !Early && flow.HandoverRecord.gameObject.activeSelf == Late && world.Presentation.PermanentNight == !Early, "Saved preparation record or LIGHT state failed.");
                    if (operation == "prep-skip") { Next(90, now); return; }
                    SetupInput();
                    if (Early) { WalkTo(flow.SolarRecord.Approach.position); Next(60, now); }
                    else if (Late) { WalkTo(flow.HandoverRecord.Approach.position); Next(40, now); }
                    else { WalkTo(flow.SolarRecord.Approach.position); Next(1, now); }
                }
                else if (stage == 60 && Walk(now)) { WalkTo(new Vector3(-8, .04f, -2.5f)); Next(6, now); }
                else if (stage == 1 && Walk(now)) { Aim(flow.SolarRecord.AimTarget.position); Next(2, now); }
                else if (stage == 2 && now - since > .4) { Keys(Key.E); Next(3, now); }
                else if (stage == 3 && now - since > .8)
                {
                    Keys(); Require(world.Player.Reading && flow.SolarRead && world.ReadingText.Contains(flow.Definition.SolarRecord) && flow.Status == (Early ? flow.Definition.Day7BeamGuide : flow.Definition.Day7CipherGuide), "Physical D7 solar record/early-tool guide failed: " + world.Player.FocusPrompt);
                    Require(world.Campaign.Progress.FoundCount == (Early ? 1 : 0) && !world.Finale.TimeReserved && world.Session.ExchangeCount == 0 && world.Cipher.Acquired == Early, "Solar reading granted tools or changed independent rules.");
                    Capture("solar-record"); Next(4, now);
                }
                else if (stage == 4 && now - since > .5) { Keys(Key.Escape); Next(5, now); }
                else if (stage == 5 && now - since > .8)
                {
                    Keys(); Require(world.Player.CanUseTools, "Solar Esc failed.");
                    WalkTo(Early ? flow.PracticePoint.position : new Vector3(-8, .04f, -2.5f)); Next(Early ? 15 : 6, now);
                }
                else if (stage == 6 && Walk(now)) { Aim(world.Cipher.transform.position + Vector3.up * .5f); Next(7, now); }
                else if (stage == 7 && now - since > .4) { Keys(Key.E); Next(8, now); }
                else if (stage == 8 && now - since > .8)
                {
                    Keys(); Require(world.Player.InspectingLock, "Physical A lock entry failed: " + world.Player.FocusPrompt);
                    var digits = world.Cipher.GetComponentsInChildren<CipherDigit>(); var code = world.Cipher.Wheel.ReadCode();
                    targets = Enumerable.Range(0, 3).SelectMany(i => Enumerable.Repeat(digits.Single(d => d.Index == i).transform, (code[i] - world.Cipher.Model.At(i) + 10) % 10)).ToArray(); click = 0; Next(9, now);
                }
                else if (stage == 9 && Clicks(now)) { Keys(Key.Enter); Next(10, now); }
                else if (stage == 10 && now - since > .8)
                { Keys(); Require(world.Cipher.Model.Unlocked && !world.Cipher.Acquired, "A unlock incorrectly granted T06."); Next(11, now); }
                else if (stage == 11 && now - since > .3) { Keys(Key.E); Next(12, now); }
                else if (stage == 12 && now - since > .8)
                {
                    Keys(); Require(world.Cipher.Acquired && world.Player.Reading && world.Campaign.Progress.FoundCount == 1 && (Early ? world.Session.Clock.Day == 1 && !flow.SolarRead && !flow.HandoverRead : flow.Status == flow.Definition.Day7BeamGuide), "Separate physical early/normal T06 acquisition/guide failed.");
                    Capture("t06-acquired"); Next(13, now);
                }
                else if (stage == 13 && now - since > .5) { Keys(Key.Escape); Next(14, now); }
                else if (stage == 14 && now - since > .8)
                {
                    Keys();
                    if (Early)
                    {
                        while (world.Session.Clock.Day < 7) world.Sleep();
                        Require(world.Cipher.Acquired && !flow.SolarRead && flow.Status == flow.Definition.Day7Guide && world.Session.ExchangeCount == 0, "Early T06 was lost or D7 observation forced re-acquisition.");
                        WalkTo(flow.SolarRecord.Approach.position); Next(1, now);
                    }
                    else { WalkTo(flow.PracticePoint.position); Next(15, now); }
                }
                else if (stage == 15 && Walk(now)) { Next(16, now); }
                else if (stage == 16)
                {
                    if (now - lastAim > .08) { Aim(world.Tether.AimRegion.position, true); lastAim = now; }
                    if (world.Tether.Rule.Locked)
                    {
                        Require(flow.Status == flow.Definition.Day7LockedGuide, "Actual beam lock did not update the guide.");
                        Next(160, now);
                    }
                    else if (now - since > 35) throw new InvalidOperationException("No actual beam lock at observation point; target=" + world.Tether.AimRegion.position);
                }
                // Tether writes in LateUpdate; Chase reads in Update on the following frame.
                else if (stage == 160 && now - since > .15)
                {
                    Require(world.Tether.Rule.Locked && world.Chase.Agent.isStopped, "Real lock did not stop the sole production agent after Update.");
                    sawLock = true; frozen = world.Chase.Agent.transform.position; vane = world.Tether.VaneBlade.localRotation; Next(17, now);
                }
                else if (stage == 17 && now - since > .7)
                {
                    Require(world.Tether.Rule.Locked && Vector3.Distance(frozen, world.Chase.Agent.transform.position) < .02f && Quaternion.Angle(vane, world.Tether.VaneBlade.localRotation) > 10, "Lock did not preserve actor/vane motion exchange.");
                    Capture("beam-locked"); Next(18, now);
                }
                else if (stage == 18 && now - since > .5) { InputSystem.QueueStateEvent(mouse, new MouseState()); vane = world.Tether.VaneBlade.localRotation; Next(19, now); }
                else if (stage == 19 && now - since > .8)
                {
                    Require(!world.Tether.Rule.Locked && flow.PracticedBeam && flow.Status == flow.Definition.Day7ReadyGuide && !world.Tether.VaneCanMove && world.Tether.DemonCanMove, "Release did not restore motion/optional practice guide.");
                    released = true; Require(world.Session.BoxSeal == 1 && world.Session.LakeSeal == 0 && world.Session.ExchangeCount == 0 && !world.Finale.TimeReserved, "Practice changed SEAL/window.");
                    Capture("beam-released"); WalkTo(flow.ReturnPoint.position); Next(20, now);
                }
                else if (stage == 20 && Walk(now)) { world.Sleep(); Next(21, now); }
                else if (stage == 21 && now - since > .6)
                {
                    Require(world.Session.Clock.Day == 8 && flow.HandoverRecord.gameObject.activeSelf && !flow.HandoverRead && world.Cipher.Acquired && flow.PracticedBeam && flow.Status == flow.Definition.Day8Guide, "D8 lost tools/practice or fabricated record read.");
                    WalkTo(flow.HandoverRecord.Approach.position); Next(40, now);
                }
                else if (stage == 40 && Walk(now)) { Aim(flow.HandoverRecord.AimTarget.position); Next(41, now); }
                else if (stage == 41 && now - since > .4) { Keys(Key.E); Next(42, now); }
                else if (stage == 42 && now - since > .8)
                {
                    Keys(); Require(world.Player.Reading && flow.HandoverRead && world.ReadingText.Contains(flow.Definition.HandoverRecord) && !world.HasReadBox && !world.Finale.TimeReserved, "Physical D8 handover reading failed or triggered B finale.");
                    Require(world.Campaign.Progress.FoundCount == (Late ? 0 : 1) && !world.Retrieval.Progress.Acquired && world.Session.ExchangeCount == 0 && !world.Listening.Active && world.Cipher.Acquired != Late, "Reading changed unrelated progress/tools.");
                    Require(flow.Status == (Late ? flow.Definition.Day8ToolGuide : flow.Definition.Day8ListeningGuide), "D8 preparation ignored actual tool state.");
                    Require(Vector3.Distance(world.Player.ViewCamera.transform.position, flow.HandoverRecord.CloseView.position) < .05f && world.ReadingText.Contains("保存参数检查") == Tuning, "Saved record/view tuning was ignored.");
                    Capture("handover-record-" + repeat); Next(43, now);
                }
                else if (stage == 43 && now - since > .6) { Keys(Key.Escape); Next(44, now); }
                else if (stage == 44 && now - since > .8)
                {
                    Keys(); Require(world.Player.CanUseTools && !world.Finale.IsDirecting, "Handover Esc entered finale or failed to return.");
                    if (++repeat < 2) { Aim(flow.HandoverRecord.AimTarget.position); Next(41, now); }
                    else if (Late) { world.Sleep(); Next(91, now); }
                    else { routeIndex = 0; WalkTo(flow.ListeningRoute[0].position); Next(50, now); }
                }
                else if (stage == 50 && Walk(now))
                { if (++routeIndex < flow.ListeningRoute.Length) WalkTo(flow.ListeningRoute[routeIndex].position); else { Aim(flow.FifthTablet.transform.position); Next(51, now); } }
                else if (stage == 51 && now - since > .4) { Keys(Key.E); Next(52, now); }
                else if (stage == 52 && now - since > .8)
                {
                    Keys(); Require(world.Player.InspectingTablet && world.Player.ActiveTablet == world.Listening.Tablet, "D8 optional return to T05 failed.");
                    var tray = world.Listening.Tablet.GetComponentsInChildren<RuneTrayItem>(); var slots = world.Listening.Tablet.GetComponentsInChildren<RuneSlot>();
                    targets = new[] { tray.Single(t => t.Rune == RuneKind.Ring).transform, slots.Single(s => s.Index == 4).transform, tray.Single(t => t.Rune == RuneKind.Chime).transform, slots.Single(s => s.Index == 7).transform }; click = 0; Next(53, now);
                }
                else if (stage == 53 && Clicks(now)) { Keys(Key.Enter); Next(54, now); }
                else if (stage == 54 && now - since > .8)
                {
                    Keys(); Require(world.Listening.Active && flow.Status.Contains("22:00") && world.Campaign.Progress.FoundCount == 2 && !world.Finale.TimeReserved, "D8 existing T05 activation/ready guide failed.");
                    WalkTo(flow.ReturnPoint.position); Next(55, now);
                }
                else if (stage == 55 && Walk(now))
                {
                    Require(sawAgent && sawLock && released && world.Consequences.Rule.SoundExchangeCount == 1 && world.Consequences.Rule.LightTransferCount == 1 && world.Session.ExchangeCount == 0, "Production chase/practice/dated attributes incomplete.");
                    Capture("day8-ready"); Next(56, now);
                }
                else if (stage == 56 && now - since > .5)
                {
                    // Fixture skips idle waiting only; production has no extra jump/wait ability.
                    world.Session.Clock.AdvanceTo(7 * 1440 + world.Campaign.Definition.FinalTabletHour * 60 + .01); world.Retrieval.RefreshPresentation(); Next(57, now);
                }
                else if (stage == 57 && now - since > .5)
                {
                    Require(world.Retrieval.Available && world.Retrieval.Pickup.gameObject.activeSelf && !flow.ShowStatus && !world.Finale.TimeReserved && !world.Tablet.gameObject.activeSelf && !world.Session.Board.Frozen, "Night did not hand off to original physical T07 retrieval.");
                    Complete(true, "Real WASD/E/Esc solar record / physical A342 mouse/Enter and separate T06 → sole chase-agent right-mouse lock/vane rotation/release → sleep D8 → two handover reads → optional physical forest T05 → camp return → original night gate; no SEAL/time-window/discovery prerequisites. metres=" + metres + ", closePasses=" + world.Chase.Gate.PassCount + ", tuning=" + Tuning + ", D1-early=" + Early);
                }
                else if (stage == 80 && world.Session.Clock.Day == 7 && now - since > 1)
                {
                    Require(flow.SolarRecord.gameObject.activeSelf && !flow.HandoverRecord.gameObject.activeSelf && flow.ShowStatus && !flow.SolarRead && !flow.HandoverRead && world.Presentation.PermanentNight && world.Consequences.Rule.LightTransferCount == 1, "Natural D6→D7 LIGHT/solar boundary failed."); world.Sleep(); Next(81, now);
                }
                else if (stage == 81 && now - since > .5)
                {
                    Require(world.Session.Clock.Day == 8 && flow.HandoverRecord.gameObject.activeSelf && !flow.HandoverRead && world.Campaign.Progress.FoundCount == 0 && !world.Cipher.Acquired && !world.Finale.TimeReserved, "D8 record required prior discoveries or opened T07 early.");
                    Complete(true, "Natural D6→D7 LIGHT/solar availability, D8 handover availability, direct early calls rejected, zero prior discovery/tools/SEAL/window.");
                }
                else if (stage == 90 && now - since > .5)
                {
                    Require(!flow.SolarRead && !flow.HandoverRead && world.Campaign.Progress.FoundCount == 0 && !world.Cipher.Acquired && !world.Listening.Active && world.Session.ExchangeCount == 0, "Sleep was gated or granted preparation.");
                    if (world.Session.Clock.Day < 9) { world.Sleep(); Next(90, now); }
                    else FinishSkip();
                }
                else if (stage == 91 && now - since > .5) FinishSkip();
            }
            catch (Exception e) { Complete(false, "Stage " + stage + ", position=" + (world ? world.Player.transform.position.ToString() : "missing") + ": " + e); }
        }
        void FinishSkip()
        {
            Require(world.Session.Descended && world.Session.Clock.Day == 9 && world.Session.ExchangeCount == 0 && !flow.ShowStatus && !flow.SolarRecord.gameObject.activeSelf && !flow.HandoverRecord.gameObject.activeSelf && !world.Finale.TimeReserved, "Skip/late cleanup or normal descent failed.");
            Complete(true, Late ? "Direct D8, no D7/solar/tools: real handover read twice without grants, actual view/record respected, Esc returns normally; optional content does not block sleep to D9." : "D7→D9 sleep only, no observations/records/discoveries/tools/SEAL; scheduled descent and record/guide cleanup.");
        }
        bool Clicks(double now)
        {
            if (now - since < .16) return false;
            if (click >= targets.Length * 2) return true;
            var target = targets[click / 2]; Vector3 screen = world.Player.ViewCamera.WorldToScreenPoint(target.position);
            Require(screen.z > 0 && screen.x > 0 && screen.x < Screen.width && screen.y > 0 && screen.y < Screen.height, "Click target outside camera.");
            var hit = Physics.RaycastAll(world.Player.ViewCamera.ScreenPointToRay(screen), 20).Where(h => !h.collider.transform.IsChildOf(world.Player.transform)).OrderBy(h => h.distance).FirstOrDefault();
            Require(hit.collider && hit.collider.transform.IsChildOf(target), "Click blocked by " + (hit.collider ? hit.collider.name : "nothing"));
            InputSystem.QueueStateEvent(mouse, new MouseState { position = new Vector2(screen.x, screen.y), buttons = (ushort)(click % 2 == 0 ? 1 : 0) }); click++; since = now; return false;
        }
        void WalkTo(Vector3 target) => driver.WalkTo(target);
        bool Walk(double now) { bool arrived = driver.Walk(now); metres = driver.Metres; return arrived; }
        void Aim(Vector3 point, bool right = false) => driver.Aim(point, right);
        void SetupInput() => driver = new SmokePlayerDriver(world.Player, world.Settings, 35);
        void Keys(params Key[] keys) => driver.Keys(keys);
        void Next(int value, double now) { stage = value; since = now; }
        void Capture(string name) => ScreenCapture.CaptureScreenshot(Path.GetFullPath("Temp/DemonSliceReports/" + operation + "-" + name + ".png"));
        public void Cleanup() { driver?.Dispose(); driver = null; }
        void Complete(bool success, string message) { finished = true; Cleanup(); finish(success, message); EditorApplication.delayCall += EditorApplication.ExitPlaymode; }
    }
}
