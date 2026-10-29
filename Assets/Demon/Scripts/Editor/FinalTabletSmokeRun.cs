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
    public sealed class FinalTabletSmokeRun
    {
        readonly string operation;
        readonly Action<bool, string> finish;
        readonly double started = EditorApplication.timeSinceStartup;
        SliceSceneController world;
        FinalTabletRetrieval retrieval;
        ThreatPickup pickup;
        Keyboard keyboard; Mouse mouse;
        InputSettings.BackgroundBehavior oldBackground;
        InputSettings.EditorInputBehaviorInPlayMode oldInput;
        bool oldRun, finished, capturedPickup, seenClock, capturedMidnight, clickDown;
        int stage, clickStep, passBefore;
        double since, clickAt, earliest;
        Transform[] clickTargets;
        bool Seeded => operation == "retrieval-smoke" || operation == "retrieval-sleep";
        public FinalTabletSmokeRun(string op, Action<bool, string> callback) { operation = op; finish = callback; }
        static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
        public void Tick()
        {
            if (finished || !Application.isPlaying) return;
            try
            {
                if (EditorApplication.timeSinceStartup - started > 180) throw new TimeoutException("T07 timed out at stage " + stage);
                if (!world) world = UnityEngine.Object.FindAnyObjectByType<SliceSceneController>();
                if (world && !world.enabled) throw new InvalidOperationException("World disabled: inspect missing authored references before Play.");
                if (!world || world.Session == null || !world.Retrieval || world.Retrieval.Progress == null  ) return;
                if (stage == 0 && !world.Chase.HasNavigation) return;
                retrieval = world.Retrieval; pickup = retrieval.Pickup; double now = Time.timeAsDouble;
                Require(world.Session.Clock.AbsoluteMinutes >= earliest, "Clock moved backwards during T07."); earliest = world.Session.Clock.AbsoluteMinutes;
                if (stage == 0)
                {
                    Require(world.Session.Clock.Day == 8 && !retrieval.Available && !pickup.gameObject.activeSelf && !world.Tablet.gameObject.activeSelf, "T07 appeared before configured night.");
                    Require(Math.Abs(pickup.HoldSeconds - (operation == "retrieval-tuning" ? 1.8f : 1.2f)) < .001f, "Saved T07 hold ignored.");
                    pickup.Use(world.Player); Require(!retrieval.Progress.Acquired && !world.TryCompile() && !world.Finale.TimeReserved, "Early pickup/compile bypassed night gate.");
                    UnityEngine.Object.FindAnyObjectByType<FinalTabletDock>().Use(world.Player); Require(!retrieval.Progress.Placed, "Empty dock supplied T07.");
                    oldBackground = InputSystem.settings.backgroundBehavior; oldInput = InputSystem.settings.editorInputBehaviorInPlayMode; oldRun = Application.runInBackground;
                    InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
                    InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
                    keyboard = InputSystem.AddDevice<Keyboard>(); mouse = InputSystem.AddDevice<Mouse>(); InputSystem.onAfterUpdate += Current; Current();
                    world.Chase.enabled = false;
                    world.Session.Clock.AdvanceTo(7 * 1440 + world.Campaign.Definition.FinalTabletHour * 60 + .01); retrieval.RefreshPresentation();
                    Require(pickup.gameObject.activeSelf && !world.Tablet.gameObject.activeSelf, "Night must expose pickup, not editable board.");
                    var box = world.Cipher; var code = box.Wheel.ReadCode(); for (int i = 0; i < 3; i++) while (box.Model.At(i) != code[i]) box.Model.Cycle(i);
                    Require(box.TryConfirm(), "A fixture unlock failed."); box.Use(world.Player);
                    var listen = world.Listening.Board; listen.PickFromTray(RuneKind.Ring); listen.ClickSlot(4); listen.PickFromTray(RuneKind.Chime); listen.ClickSlot(7); Require(world.Listening.TryCompile(), "T05 fixture failed.");
                    box.Use(world.Player); Keys(Key.Escape);
                    // Adversarial fixture: even a correct program cannot compile before acquisition/placement.
                    var board = world.Session.Board; if (Seeded) { board.PickFromTray(RuneKind.Exchange); board.ClickSlot(4); board.PickFromTray(RuneKind.Box); board.ClickSlot(7); board.PickFromTray(RuneKind.Lake); board.ClickSlot(8); }
                    Require((!Seeded || board.MatchesTarget()) && !world.TryCompile() && !world.Finale.TimeReserved && world.Session.ExchangeCount == 0, "Correct board bypassed physical retrieval."); Next(1, now);
                }
                else if (stage == 1 && now - since > .8)
                {
                    Keys(); Require(world.Player.CanUseTools && world.Tether.Equipped, "T06 reading did not return.");
                    PlacePlayer(pickup.transform.position + new Vector3(0, .04f, -2.4f));
                    Require(NavMesh.SamplePosition(new Vector3(8.8f, 0, -2.6f), out var hit, 1.5f, NavMesh.AllAreas) && world.Chase.Agent.Warp(hit.position), "T07 test actor lacks saved navigation."); world.Chase.Agent.ResetPath();
                    world.Chase.enabled = true; PointAt(world.Tether.AimRegion.position, true); Next(2, now);
                }
                else if (stage == 2 && now - since > world.Tether.Definition.StableSeconds + .3)
                {
                    Require(world.Tether.Rule.Locked && world.Chase.Agent.isStopped && !world.Finale.TimeReserved, "Physical T06 did not freeze before T07."); Capture("frozen"); Next(3, now);
                }
                else if (stage == 3 && now - since > .35)
                {
                    if (operation == "retrieval-late") world.Session.Clock.AdvanceTo(world.Session.Clock.NextMidnight - .15);
                    PointAt(pickup.transform.position + new Vector3(0, 1.35f, -.3f), true); Keys(Key.E); passBefore = world.Chase.Gate.PassCount; Next(4, now);
                }
                else if (stage == 4)
                {
                    if (world.Player.IsPickingUp)
                    {
                        Require(!world.Tether.BeamOn && world.Finale.TimeReserved, "T07 hand action retained beam or failed to reserve time.");
                        if (!capturedPickup) { Capture("pickup"); capturedPickup = true; }
                    }
                    if (pickup.Attempt.InterruptCount == 1)
                    {
                        Require(!retrieval.Progress.Acquired && !world.Player.IsPickingUp && world.Chase.Gate.PassCount == passBefore + 1 && world.Finale.TimeReserved, "T07 interruption lost retry reservation or acquired prematurely.");
                        Require(world.Cipher.Acquired && world.Listening.Board.Frozen && world.Session.ExchangeCount == 0, "T07 interruption erased completed tools or changed SEAL.");
                        Keys(); InputSystem.QueueStateEvent(mouse, new MouseState()); Capture("interrupted"); Next(5, now);
                    }
                    else if (now - since > 5) throw new InvalidOperationException("Physical T07 hold was not interrupted: picking=" + world.Player.IsPickingUp + ", acquired=" + retrieval.Progress.Acquired + ", phase=" + world.Chase.Phase);
                }
                else if (stage == 5 && now - since > 1.25)
                {
                    Require(world.Finale.TimeReserved && world.Player.CanUseTools, "Cancelled T07 did not leave retry time/exploration.");
                    if (operation == "retrieval-sleep") { world.Sleep(); Next(90, now); return; }
                    world.Session.Clock.AdvanceTo(world.Finale.LimitNaturalTime(world.Session.Clock.NextMidnight + 100));
                    Require(world.Session.Clock.Day == 8 && !world.Session.Descended, "T07 retry window crossed midnight.");
                    PointAt(pickup.transform.position + new Vector3(0, 1.35f, -.3f), false); Keys(Key.E); Next(6, now);
                }
                else if (stage == 6)
                {
                    if (retrieval.Progress.Acquired)
                    {
                        Require(pickup.CollectionCount == 1 && !retrieval.Progress.Placed && !world.Tablet.gameObject.activeSelf && !world.TryCompile() && world.Session.ExchangeCount == 0, "Carried T07 bypassed placement gate.");
                        Keys(); Capture("carrying"); Next(7, now);
                    }
                    else if (now - since > pickup.HoldSeconds + 2) throw new InvalidOperationException("T07 retry failed in retreat window.");
                }
                else if (stage == 7 && now - since > .8)
                { Require(world.Player.CanUseTools, "Acquisition retained close view."); Keys(Key.S); Next(8, now); }
                else if (stage == 8)
                {
                    Require(now - since < 4, "South leg blocked at " + world.Player.transform.position);
                    if (world.Player.transform.position.z <= -.9f) { Keys(Key.A); Next(9, now); }
                }
                else if (stage == 9)
                {
                    Require(now - since < 8, "Physical return to shed blocked at " + world.Player.transform.position);
                    if (world.Player.transform.position.x <= -6.3f) { Keys(Key.W); Next(10, now); }
                }
                else if (stage == 10)
                {
                    Require(now - since < 4, "North dock approach blocked at " + world.Player.transform.position);
                    if (world.Player.transform.position.z >= 1)
                    { PointAt(GameObject.Find("T07Retrieval_Authored/ShedPlacementDock").transform.TransformPoint(new Vector3(0, 1.35f, -.45f)), false); Keys(Key.E); Next(11, now); }
                }
                else if (stage == 11 && now - since > .8)
                {
                    Keys(); Require(retrieval.Progress.Placed && world.Player.InspectingTablet && world.Tablet.gameObject.activeSelf && world.Session.ExchangeCount == 0, "Physical E at shed failed to place/open original board.");
                    Capture("placed"); BuildClicks(!Seeded); clickStep = 0; clickDown = false; clickAt = now + .2; Next(12, now);
                }
                else if (stage == 12 && ClickSwap(now)) { Keys(Key.Enter); Next(13, now); }
                else if (stage == 13 && now - since > .35)
                {
                    Require(!world.Session.Board.Frozen && world.Session.ExchangeCount == 0, "Wrong physical rune arrangement compiled.");
                    Keys(); BuildClicks(false, true); clickStep = 0; clickDown = false; clickAt = now + .2; Next(14, now);
                }
                else if (stage == 14 && ClickSwap(now)) { Keys(Key.Enter); Next(15, now); }
                else if (stage == 15 && now - since > .4)
                {
                    Require(world.Session.Board.Frozen && world.Session.ExchangeCount == 1 && world.Session.BoxSeal == 0 && world.Session.LakeSeal == 1 && world.Chase.Agent.isStopped, "Physical corrected Enter did not seal lake and retreat threat.");
                    Require(world.Session.Board.CompiledAt(7) == RuneKind.Lake && world.Session.Board.CompiledAt(8) == RuneKind.Box, "Reversed object order was not preserved in the compiled snapshot.");
                    Keys(); Capture("compiled"); Next(16, now);
                }
                else if (stage == 16 && now - since > .8)
                {
                    Require(world.Player.CanUseTools, "Compiled board retained near view.");
                    PointAt(world.Player.ViewCamera.transform.position + Vector3.forward * 10, false); Keys(Key.S); Next(17, now);
                }
                else if (stage == 17)
                {
                    Require(now - since < 4, "Return from board blocked.");
                    if (world.Player.transform.position.z <= -.9f) { Keys(Key.D); Next(18, now); }
                }
                else if (stage == 18)
                {
                    Require(now - since < 4, "Box approach blocked.");
                    if (world.Player.transform.position.x >= -4.1f)
                    { PointAt(UnityEngine.Object.FindAnyObjectByType<SliceBox>().transform.position + Vector3.up * .6f, false); Keys(Key.E); Next(19, now); }
                }
                else if (stage == 19 && now - since > .4)
                { Require(world.HasReadBox && world.Player.Reading && world.Finale.TimeReserved && !world.Finale.IsDirecting, "Actual E failed to open B record."); Keys(); Capture("record"); Next(20, now); }
                else if (stage == 20 && now - since > 2)
                { Require(world.Session.Clock.Day == 8 && world.Session.ExchangeCount == 1 && !world.Finale.LakeGlowVisible, "Long reading lost success window."); Keys(Key.Escape); Next(21, now); }
                else if (stage == 21)
                {
                    if (Vector3.Distance(world.Player.ViewCamera.transform.position, world.Finale.ClockView.position) < .2f && world.Finale.IsDirecting) seenClock = true;
                    if (world.Session.Descended && !capturedMidnight) { Capture("midnight"); capturedMidnight = true; }
                    if (world.EndingPresented)
                    {
                        Require(seenClock && world.Session.Clock.Day == 9 && world.Session.ExchangeCount == 2 && world.Session.BoxSeal == 1 && world.Session.LakeSeal == 0 && retrieval.Progress.Placed && pickup.CollectionCount == 1, "Retrieved T07 broke the second exchange/finale.");
                        Complete(true, "D8 night pickup-only gate, correct-board pre-acquisition/carried bypass prevention, actual right-mouse freeze to held-E interruption/retry with reserved monotonic clock, physical WASD carry to shed/E placement, physical rune wrong/fix Enter, B E/long reading/Escape, clock view and D9 second exchange passed. " + operation);
                    }
                }
                else if (stage == 90 && now - since > .4)
                {
                    Require(world.Session.Descended && world.Session.Clock.Day == 9 && !world.Finale.TimeReserved && world.Session.ExchangeCount == 0 && !retrieval.Progress.Acquired, "Sleep from interrupted pickup did not abandon reserved flow.");
                    Complete(true, "Actual interrupted T07 pickup reserves retry time; explicit sleep releases it, no collection/repair/exchange, terminal D9 passed.");
                }
            }
            catch (Exception e) { Complete(false, "Stage " + stage + ": " + e); }
        }
        void BuildClicks(bool fill, bool restoreAndReverse = false)
        {
            var slots = world.Tablet.GetComponentsInChildren<SliceRuneSlot>(); var tray = world.Tablet.GetComponentsInChildren<SliceRuneTrayItem>();
            Transform Slot(int i) => slots.Single(s => s.Index == i).transform; Transform Tray(RuneKind kind) => tray.Single(t => t.Rune == kind).transform;
            foreach (var item in tray) ValidateClick(item.transform);
            var correction = restoreAndReverse ? new[] { Slot(4), Slot(7), Slot(4), Slot(7), Slot(8), Slot(7) } : new[] { Slot(4), Slot(7), Slot(4) };
            clickTargets = fill ? new[] { Tray(RuneKind.Exchange), Slot(4), Tray(RuneKind.Box), Slot(7), Tray(RuneKind.Lake), Slot(8) }.Concat(correction).ToArray() : correction;
        }
        Vector3 ValidateClick(Transform target)
        {
            var tray = target.GetComponent<SliceRuneTrayItem>();
            Vector3 screen = world.Player.ViewCamera.WorldToScreenPoint(tray ? tray.Label.transform.position : target.position);
            Require(screen.z > 0 && screen.x > 0 && screen.x < Screen.width && screen.y > 0 && screen.y < Screen.height, "Docked target outside camera: " + target.name);
            var hit = Physics.RaycastAll(world.Player.ViewCamera.ScreenPointToRay(screen), 20).Where(h => !h.collider.transform.IsChildOf(world.Player.transform)).OrderBy(h => h.distance).FirstOrDefault();
            Require(hit.collider && hit.collider.transform.IsChildOf(target), "Docked target blocked by " + (hit.collider ? hit.collider.name : "nothing")); return screen;
        }
        bool ClickSwap(double now)
        {
            if (now < clickAt) return false;
            Vector3 screen = ValidateClick(clickTargets[clickStep]);
            clickDown = !clickDown; InputSystem.QueueStateEvent(mouse, new MouseState { position = new Vector2(screen.x, screen.y), buttons = (ushort)(clickDown ? 1 : 0) });
            if (!clickDown) clickStep++; clickAt = now + .12; return clickStep == clickTargets.Length;
        }
        void PlacePlayer(Vector3 position)
        { var body = world.Player.GetComponent<CharacterController>(); body.enabled = false; world.Player.transform.SetPositionAndRotation(position, Quaternion.identity); body.enabled = true; Cursor.lockState = CursorLockMode.Locked; }
        void PointAt(Vector3 point, bool right)
        {
            var camera = world.Player.ViewCamera.transform; Vector3 desired = Quaternion.LookRotation(point - camera.position).eulerAngles;
            float yaw = Mathf.DeltaAngle(world.Player.transform.eulerAngles.y, desired.y), pitch = Mathf.DeltaAngle(0, camera.localEulerAngles.x), targetPitch = Mathf.DeltaAngle(0, desired.x);
            InputSystem.QueueStateEvent(mouse, new MouseState { delta = new Vector2(yaw / world.Settings.MouseSensitivity, (pitch - targetPitch) / world.Settings.MouseSensitivity), buttons = (ushort)(right ? 2 : 0) });
        }
        void Keys(params Key[] keys) => InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
        void Current() { keyboard?.MakeCurrent(); mouse?.MakeCurrent(); }
        void Next(int value, double now) { stage = value; since = now; }
        void Capture(string name) => ScreenCapture.CaptureScreenshot(Path.GetFullPath("Temp/DemonSliceReports/" + operation + "-" + name + ".png"));
        public void Cleanup()
        {
            if (keyboard == null) return; InputSystem.onAfterUpdate -= Current; InputSystem.RemoveDevice(keyboard); InputSystem.RemoveDevice(mouse);
            InputSystem.settings.backgroundBehavior = oldBackground; InputSystem.settings.editorInputBehaviorInPlayMode = oldInput; Application.runInBackground = oldRun; keyboard = null; mouse = null;
        }
        void Complete(bool success, string message) { finished = true; Cleanup(); finish(success, message); EditorApplication.delayCall += EditorApplication.ExitPlaymode; }
    }
}
