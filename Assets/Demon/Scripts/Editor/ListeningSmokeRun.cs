using System;
using System.IO;
using System.Linq;
using Demon.Slice.Domain;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Demon.Slice.Editor
{
    public sealed class ListeningSmokeRun
    {
        readonly string operation;
        readonly Action<bool, string> finish;
        readonly double started = EditorApplication.timeSinceStartup;
        SliceSceneController world;
        ListeningTool tool;
        ListeningChime a, b;
        ListeningTrailProxy proxy;
        Keyboard keyboard; Mouse mouse;
        InputSettings.BackgroundBehavior oldBackground;
        InputSettings.EditorInputBehaviorInPlayMode oldInput;
        bool finished, oldRun;
        int stage, click;
        double since, before;
        Transform[] targets;
        Vector3 startPosition;
        public ListeningSmokeRun(string operation, Action<bool, string> finish) { this.operation = operation; this.finish = finish; }
        static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
        public void Tick()
        {
            if (finished || !Application.isPlaying) return;
            try
            {
                double now = Time.timeAsDouble;
                if (EditorApplication.timeSinceStartup - started > 95) throw new TimeoutException("Listening smoke timed out at stage " + stage);
                if (!world) world = UnityEngine.Object.FindAnyObjectByType<SliceSceneController>();
                if (!world || world.Session == null || !world.Listening || world.Listening.Board == null) return;
                tool = world.Listening;
                if (stage == 0)
                {
                    a = tool.Chimes[0]; b = tool.Chimes[1]; proxy = UnityEngine.Object.FindAnyObjectByType<ListeningTrailProxy>(); proxy.enabled = false;
                    Require(world.Session.Clock.Day == 6 && !tool.Active && !world.Finale.TimeReserved, "D6 quick state incorrect.");
                    Require(!tool.TryCompile() && !tool.Board.Frozen && world.Session.ExchangeCount == 0, "Incomplete T05 compiled or changed final state.");
                    if (operation == "listening-tuning") Require(Math.Abs(a.RadiusMetres - 2.25f) < .001f && Math.Abs(proxy.MetresPerSecond - 1.25f) < .001f, "Saved radius/speed tuning ignored.");
                    oldRun = Application.runInBackground; Application.runInBackground = true;
                    oldBackground = InputSystem.settings.backgroundBehavior; oldInput = InputSystem.settings.editorInputBehaviorInPlayMode;
                    InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
                    InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
                    keyboard = InputSystem.AddDevice<Keyboard>(); mouse = InputSystem.AddDevice<Mouse>(); InputSystem.onAfterUpdate += Current;
                    Current(); Cursor.lockState = CursorLockMode.Locked;
                    startPosition = world.Player.transform.position; InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.W)); Next(1, now);
                }
                else if (stage == 1 && now - since > .3)
                {
                    Require(world.Player.transform.position.z > startPosition.z + .15f, "W did not move the actual player.");
                    InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E)); Next(2, now);
                }
                else if (stage == 2 && now - since > 1)
                {
                    InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                    Require(world.Player.InspectingTablet && world.Player.ActiveTablet == tool.Tablet && !world.HasReadBox, "E did not select T05, or entered B reading.");
                    Require(Vector3.Distance(world.Player.ViewCamera.transform.position, tool.Tablet.CloseView.position) < .03f, "Saved T05 camera ignored.");
                    Require(world.Campaign.Progress.FoundCount == 1 && !world.Finale.TimeReserved, "T05 discovery duplicated or reserved T07 time.");
                    var tray = tool.Tablet.GetComponentsInChildren<SliceRuneTrayItem>(); var slots = tool.Tablet.GetComponentsInChildren<SliceRuneSlot>();
                    targets = new[] { tray.Single(t => t.Rune == RuneKind.Open).transform, slots.Single(s => s.Index == 4).transform };
                    Next(3, now);
                }
                else if (stage == 3 && now - since > .16)
                {
                    if (click < targets.Length * 2) { Click(targets[click / 2], click % 2 == 0); click++; since = now; }
                    else { InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Enter)); Next(4, now); }
                }
                else if (stage == 4 && now - since > .4)
                {
                    InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                    Require(!tool.Active && !world.Session.Board.Frozen && world.Session.ExchangeCount == 0, "Wrong T05 attempt changed rules.");
                    var tray = tool.Tablet.GetComponentsInChildren<SliceRuneTrayItem>(); var slots = tool.Tablet.GetComponentsInChildren<SliceRuneSlot>();
                    targets = new[] { tray.Single(t => t.Rune == RuneKind.Ring).transform, slots.Single(s => s.Index == 4).transform,
                        tray.Single(t => t.Rune == RuneKind.Chime).transform, slots.Single(s => s.Index == 7).transform };
                    click = 0; Next(5, now);
                }
                else if (stage == 5 && now - since > .16)
                {
                    if (click < targets.Length * 2) { Click(targets[click / 2], click % 2 == 0); click++; since = now; }
                    else { Require(tool.Board.MatchesTarget(), "Mouse did not complete the listening layout."); Capture("layout"); before = world.Session.Clock.AbsoluteMinutes; InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Enter)); Next(6, now); }
                }
                else if (stage == 6 && now - since > .6)
                {
                    InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                    Require(tool.Active && !world.Session.Board.Frozen && !world.Finale.TimeReserved && world.Session.ExchangeCount == 0 && world.Session.BoxSeal == 1 && world.Session.LakeSeal == 0,
                        "T05 activation leaked into T07/world state.");
                    Require(world.Session.Clock.Day == 6 && world.Session.Clock.AbsoluteMinutes - before < 5, "T05 advanced the date.");
                    Require(a.RingCount == 0 && b.RingCount == 0, "Activation fabricated an entry.");
                    a.MonitoredTarget.position = a.transform.position; Next(7, now);
                }
                else if (stage == 7 && now - since > 1.2)
                {
                    Require(a.RingCount == 1 && b.RingCount == 0, "Entry or stationary dedup failed.");
                    Require(a.Source.isPlaying, "A chime counted an event without playing its audio.");
                    a.MonitoredTarget.position = a.transform.position + Vector3.left * (a.RadiusMetres + .2f); Next(8, now);
                }
                else if (stage == 8 && now - since > .3) { a.MonitoredTarget.position = a.transform.position; Next(9, now); }
                else if (stage == 9 && now - since > .3)
                {
                    Require(a.RingCount == 2, "Exit did not rearm entry.");
                    a.enabled = false; a.enabled = true; Next(10, now);
                }
                else if (stage == 10 && now - since > .3)
                {
                    Require(a.RingCount == 2, "Reenable inside fabricated a ring.");
                    a.MonitoredTarget.position = b.transform.position; Next(11, now);
                }
                else if (stage == 11 && now - since > .3)
                {
                    Require(b.RingCount == 1 && a.RingCount == 2, "Independent B region failed.");
                    Require(b.Source.isPlaying, "B chime counted an event without playing its audio.");
                    Require(a.Source.spatialBlend == 1 && b.Source.spatialBlend == 1 && a.Source.maxDistance == 16 && a.Source.minDistance == 1, "Spatial attenuation lost.");
                    Require(a.Source.clip == null && a.Source != b.Source, "Chimes must own separate audio sources.");
                    tool.Board.ClickSlot(4); Require(tool.Active, "Removing surface rune stopped compiled listening.");
                    var view = GameObject.Find("Listening_Authored/OccludedView").transform;
                    var body = world.Player.GetComponent<CharacterController>(); body.enabled = false;
                    world.Player.transform.position = view.position - Vector3.up * 1.65f; body.enabled = true;
                    world.Player.EnterSequenceView(view); a.MonitoredTarget.position = a.transform.position + Vector3.left * (a.RadiusMetres + .3f);
                    Next(12, now);
                }
                else if (stage == 12 && now - since > .6)
                {
                    a.MonitoredTarget.position = a.transform.position; Next(13, now);
                }
                else if (stage == 13 && now - since > .7)
                {
                    Require(a.RingCount == 3, "Occluded entry failed.");
                    Vector3 delta = a.MonitoredTarget.position - world.Player.ViewCamera.transform.position;
                    Require(Physics.Raycast(world.Player.ViewCamera.transform.position, delta.normalized, out var hit, delta.magnitude) && hit.collider.name == "ListeningOccluder", "Test wall does not block the proxy sightline.");
                    Capture("occluded"); proxy.enabled = true;
                    world.Player.EnterSequenceView(GameObject.Find("Listening_Authored/VisibleView").transform); Next(14, now);
                }
                else if (stage == 14 && now - since > 4)
                {
                    Capture("area"); Complete(true, "D6 W/E, wrong target retry, physical mouse/Enter on independent T05, no time/SEAL/finale mutation, spatial A/B entry/stay/reentry/lifecycle, frozen snapshot and occluded alarm passed. Tuning=" + (operation == "listening-tuning"));
                }
            }
            catch (Exception e) { Complete(false, "Stage " + stage + ": " + e); }
        }
        void Click(Transform target, bool down)
        {
            Vector3 screen = world.Player.ViewCamera.WorldToScreenPoint(target.position);
            Require(screen.z > 0 && screen.x > 0 && screen.x < Screen.width && screen.y > 0 && screen.y < Screen.height, "T05 click target is outside camera.");
            var hit = Physics.RaycastAll(world.Player.ViewCamera.ScreenPointToRay(screen), 20).Where(h => !h.collider.transform.IsChildOf(world.Player.transform)).OrderBy(h => h.distance).FirstOrDefault();
            Require(hit.collider && hit.collider.transform.IsChildOf(target), "T05 click blocked by " + (hit.collider ? hit.collider.name : "nothing"));
            InputSystem.QueueStateEvent(mouse, new MouseState { position = new Vector2(screen.x, screen.y), buttons = (ushort)(down ? 1 : 0) });
        }
        void Current() { keyboard?.MakeCurrent(); mouse?.MakeCurrent(); }
        void Next(int value, double time) { stage = value; since = time; }
        void Capture(string name) { ScreenCapture.CaptureScreenshot(Path.GetFullPath("Temp/DemonSliceReports/" + operation + "-" + name + ".png")); }
        public void Cleanup()
        {
            if (keyboard == null) return;
            InputSystem.onAfterUpdate -= Current; InputSystem.RemoveDevice(keyboard); InputSystem.RemoveDevice(mouse);
            InputSystem.settings.backgroundBehavior = oldBackground; InputSystem.settings.editorInputBehaviorInPlayMode = oldInput;
            Application.runInBackground = oldRun; keyboard = null; mouse = null;
        }
        void Complete(bool success, string message)
        {
            finished = true; Cleanup(); finish(success, message); EditorApplication.delayCall += EditorApplication.ExitPlaymode;
        }
    }
}
