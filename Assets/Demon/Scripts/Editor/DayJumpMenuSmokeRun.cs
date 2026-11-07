using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Demon.Slice.Domain;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

namespace Demon.Slice.Editor
{
    public sealed class DayJumpMenuSmokeRun : ISmokeRun
    {
        readonly Action<bool, string> finish;
        readonly double started = EditorApplication.timeSinceStartup;
        readonly int[] targets = { 8, 2, 7, 6, 5, 4, 3, 1, 80, 9, 9, 1, 80 };
        readonly List<string> checks = new List<string>();
        GameSceneController world;
        DayJumpMenu menu;
        Keyboard keyboard;
        InputSettings.BackgroundBehavior oldBackground;
        InputSettings.EditorInputBehaviorInPlayMode oldEditorInput;
        bool oldRun, finished;
        int stage, index, frame;
        GameSession lastSession;
        double since, pausedMinute, traceAt;
        Vector3 pausedPosition;
        float pausedPickup;

        public DayJumpMenuSmokeRun(Action<bool, string> completed) { finish = completed; }
        public void Tick()
        {
            if (finished || !Application.isPlaying) return;
            try
            {
                EditorApplication.QueuePlayerLoopUpdate();
                double now = EditorApplication.timeSinceStartup;
                if (now >= traceAt)
                {
                    traceAt = now + 2;
                    var observedWorld = world;
                    var observedMenu = menu;
                    File.WriteAllText("Temp/DemonSliceReports/day-jump-trace.txt", "stage=" + stage + "; targetIndex=" + index + "; frame=" + Time.frameCount +
                        "; paused=" + EditorApplication.isPaused + "; day=" + observedWorld?.Session?.Clock.Day +
                        "; found=" + observedWorld?.Campaign?.Progress?.FoundCount + "; ready=" + observedMenu?.IsReady + "; open=" + observedMenu?.IsOpen + "; loading=" + observedMenu?.IsLoading);
                }
                Require(now - started < 240, "Day menu smoke timed out at stage " + stage + ", target " + index);
                if (stage == 0)
                {
                    world = UnityEngine.Object.FindAnyObjectByType<GameSceneController>();
                    menu = UnityEngine.Object.FindAnyObjectByType<DayJumpMenu>();
                    if (!world || world.Session == null || !menu || !menu.IsReady) return;
                    Require(world.Session.Clock.Day == 1 && world.Campaign.Progress.FoundCount == 0 && world.Exploration.Log.Count == 0,
                        "Ordinary startup must remain untouched.");
                    oldRun = Application.runInBackground; Application.runInBackground = true;
                    oldBackground = InputSystem.settings.backgroundBehavior; oldEditorInput = InputSystem.settings.editorInputBehaviorInPlayMode;
                    InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
                    InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
                    keyboard = InputSystem.AddDevice<Keyboard>();
                    InputSystem.onAfterUpdate += MakeCurrent; MakeCurrent();
                    Press(Key.F8); Next(1, now);
                }
                else if (stage == 1 && Elapsed(now))
                {
                    Require(menu.IsOpen && Time.timeScale == 0 && world.Player.MenuOpen && Cursor.visible && Cursor.lockState == CursorLockMode.None,
                        "F8 did not pause/release cursor/block player input.");
                    Release(); pausedMinute = world.Session.Clock.AbsoluteMinutes; pausedPosition = world.Player.transform.position;
                    var panel = new SerializedObject(menu).FindProperty("panel").objectReferenceValue as GameObject;
                    var button = panel.GetComponentsInChildren<Button>(true).Single(x => x.name == "D8");
                    Vector2 point = RectTransformUtility.WorldToScreenPoint(null, button.transform.position);
                    var results = new List<RaycastResult>(); EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = point }, results);
                    Require(results.Count > 0 && results[0].gameObject.GetComponentInParent<Button>() == button, "D8 button is not mouse-raycastable.");
                    ScreenCapture.CaptureScreenshot(Path.GetFullPath("Temp/DemonSliceReports/day-jump-menu.png"));
                    Press(Key.W, Key.Enter); Next(2, now);
                }
                else if (stage == 2 && Elapsed(now))
                {
                    Require(world.Session.Clock.AbsoluteMinutes == pausedMinute && Vector3.Distance(pausedPosition, world.Player.transform.position) < .001f && !world.Session.Board.Frozen,
                        "Menu leaked gameplay movement/time/confirm input.");
                    Release(); Press(Key.Escape); Next(3, now);
                }
                else if (stage == 3 && Elapsed(now))
                {
                    Require(!menu.IsOpen && Time.timeScale == 1 && !world.Player.MenuOpen && !AudioListener.pause, "Menu did not restore pause state.");
                    Release();
                    // Esc from a menu over a reading view must leave the document open.
                    world.Exploration.Observations[0].Use(world.Player); menu.Open();
                    Press(Key.Escape); Next(4, now);
                }
                else if (stage == 4 && Elapsed(now))
                {
                    Require(!menu.IsOpen && world.Player.Reading, "Esc closing the menu also closed the document.");
                    Release(); world.Player.ExitCloseView(); world.Player.ReturnToExploration();
                    Time.timeScale = .5f; menu.Open(); menu.Close();
                    Require(Time.timeScale == .5f, "Menu did not restore the previous time scale."); Time.timeScale = 1;
                    checks.Add("F8, mouse raycast, paused movement/time/confirm, Esc reading isolation and previous time-scale restoration passed.");
                    // Button events are deterministic here; actual pointer input is verified separately in the Editor UI.
                    menu.Open(); var button = menu.GetComponentsInChildren<Button>(true).Single(x => x.name == "D8");
                    lastSession = world.Session;
                    button.onClick.Invoke(); Next(10, now);
                }
                else if (stage == 10)
                {
                    if (!world || world.Session == lastSession || now - since < .5 || Time.frameCount <= frame + 2) return;
                    if (!menu || !menu.IsReady) return;
                    Require(menu && !menu.IsOpen && Time.timeScale == 1, "Reload retained a paused/open menu or replay failed.");
                    Check(targets[index]);
                    checks.Add("Destination " + targets[index] + " passed at frame " + Time.frameCount);
                    File.WriteAllLines("Temp/DemonSliceReports/day-jump-progress.txt", checks);
                    if (++index >= targets.Length)
                    {
                        var pickup = world.Retrieval.Pickup; pickup.Use(world.Player); pickup.TickHeld(true, .2f);
                        Require(world.Player.IsPickingUp && pickup.Attempt.Active, "T07 entry cannot begin a normal pickup.");
                        menu.Open(); pausedPickup = (float)pickup.Attempt.Progress; Next(20, now); return;
                    }
                    lastSession = world.Session;
                    int target = targets[index];
                    if (target == 1 && targets[index - 1] == 9)
                    { Press(Key.R); Next(11, now); return; }
                    menu.Open();
                    if (target == 80) menu.JumpToFinalNight();
                    else if (target == 2) { Press(Key.Digit2); Next(11, now); return; }
                    else menu.GetComponentsInChildren<Button>(true).Single(x => x.name == "D" + target).onClick.Invoke();
                    Next(10, now);
                }
                else if (stage == 11 && Elapsed(now)) { Release(); Next(10, now); }
                else if (stage == 20 && Elapsed(now))
                {
                    Require(world.Player.IsPickingUp && world.Retrieval.Pickup.Attempt.Active && Math.Abs(world.Retrieval.Pickup.Attempt.Progress - pausedPickup) < .0001,
                        "Menu opening interrupted or advanced the active pickup.");
                    menu.Close(); world.Retrieval.Pickup.TickHeld(true, world.Retrieval.Pickup.HoldSeconds);
                    Require(world.Retrieval.Place(world.Player), "Pickup/placement broke after closing menu.");
                    checks.Add("Paused pickup resumed and placed normally. All destinations, backwards/repeated jumps and ending reset passed.");
                    Complete(true, string.Join("\n", checks));
                }
            }
            catch (Exception error) { Complete(false, error.ToString()); }
        }

        void Check(int target)
        {
            int day = target == 80 ? 8 : target;
            var session = world.Session; var guide = world.Exploration;
            if (lastSession != null && !lastSession.Clock.Stopped)
            {
                // Disposed clocks must not keep invoking the newly reset campaign/consequences.
                lastSession.Clock.AdvanceTo(10 * GameClock.MinutesPerDay);
            }
            Require(session.Clock.Day == day && world.Campaign.Progress.FoundCount == Math.Max(0, Math.Min(6, day - 2)),
                "Day/discovery count mismatch: target=" + target + ", actualDay=" + session.Clock.Day + ", found=" + world.Campaign.Progress.FoundCount);
            Require(guide.Log.Count == (day == 1 ? 0 : 3), "D1 inspection log missing/duplicated.");
            Require(world.Listening.Active == (day >= 7) && world.Cipher.Acquired == (day >= 8), "Tool completion gate mismatch.");
            Require(guide.SolarRead == (day >= 8) && guide.PracticedBeam == (day >= 8) && guide.HandoverRead == (target == 80 || day == 9), "Preparation flags mismatch.");
            Require(world.Tether.MotionReady == (day >= world.Campaign.Definition.MotionDay) && !world.Tether.Rule.Locked, "Motion schedule or transient lock mismatch.");
            Require(world.Consequences.Rule.RadioSound == (day < 5 ? 1 : 0) && world.Consequences.Rule.DemonSound == (day < 5 ? 0 : 1) &&
                world.Consequences.Rule.SunLight == (day < 7 ? 1 : 0), "Scheduled SOUND/LIGHT consequences missing.");
            Require(session.BoxSeal == 1 && session.LakeSeal == (day == 1 ? 1 : 0), "SEAL state mismatch.");
            Require(session.Descended == (day == 9) && session.ExchangeCount == (day == 9 ? 2 : 0) && world.EndingPresented == (day == 9), "Terminal state/exchange dedup mismatch.");
            Require(world.Retrieval.Progress.Placed == (day == 9) && session.Board.Frozen == (day == 9), "T07 was prematurely completed or omitted.");
            Require(world.Retrieval.Pickup.CollectionCount == (day == 9 ? 1 : 0), "Pickup collection count leaked across resets.");
            foreach (var slot in world.Listening.Tablet.GetComponentsInChildren<RuneSlot>(true))
                Require(slot.Label.text == RuneTablet.RuneLabel(world.Listening.Board.At(slot.Index)), "Listening board UI did not rebind after reset.");
            if (target == 80)
            {
                Require(world.Retrieval.Available && world.Retrieval.Pickup.gameObject.activeInHierarchy && !world.Finale.TimeReserved,
                    "Night pickup must be visible and uncollected without a reserved puzzle window.");
                Require(Math.Abs(session.Clock.MinuteOfDay - world.Campaign.Definition.FinalTabletHour * 60) < 5, "D8 night clock mismatch.");
            }
            else if (day < 9) Require(Math.Abs(session.Clock.MinuteOfDay - (day == 1 ? world.Settings.StartHour : world.Campaign.WakeHourFor(day)) * 60) < 5, "Destination wake time mismatch.");
            else Require(session.Clock.MinuteOfDay == 0 && session.Clock.Stopped && world.HasReadBox, "D9 must stop at midnight after the B record.");
        }
        void MakeCurrent() { keyboard?.MakeCurrent(); }
        void Press(params Key[] keys) => InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
        void Release() => Press();
        void Next(int value, double now) { stage = value; since = now; frame = Time.frameCount; }
        bool Elapsed(double now) => now - since > .2 && Time.frameCount > frame + 2;
        static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
        void Complete(bool success, string message)
        { if (finished) return; finished = true; finish(success, message); EditorApplication.delayCall += EditorApplication.ExitPlaymode; }
        public void Cleanup()
        {
            InputSystem.onAfterUpdate -= MakeCurrent;
            if (keyboard != null) InputSystem.RemoveDevice(keyboard);
            InputSystem.settings.backgroundBehavior = oldBackground; InputSystem.settings.editorInputBehaviorInPlayMode = oldEditorInput;
            Application.runInBackground = oldRun; Time.timeScale = 1; AudioListener.pause = false;
        }
    }
}
