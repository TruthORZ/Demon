using System;
using System.IO;
using System.Linq;
using Demon.Slice.Domain;
using UnityEditor;
using UnityEngine;

namespace Demon.Slice.Editor
{
    // Input/geometry regressions remain in smoke-input; this checks the actual nine-day component composition.
    public sealed class NineDaySmokeRun
    {
        readonly string operation;
        readonly Action<bool, string> finish;
        readonly double started = EditorApplication.timeSinceStartup;
        SliceSceneController world;
        int stage;
        double stageTime;
        bool finished;
        public NineDaySmokeRun(string operation, Action<bool, string> finish) { this.operation = operation; this.finish = finish; }
        static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
        public void Tick()
        {
            if (finished || !Application.isPlaying) return;
            try
            {
                double now = EditorApplication.timeSinceStartup;
                if (now - started > 130) throw new TimeoutException("Nine-day smoke timed out.");
                if (!world) world = UnityEngine.Object.FindAnyObjectByType<SliceSceneController>();
                if (!world || world.Session == null || world.Campaign.Progress == null) return;
                var session = world.Session;
                var flow = world.Campaign;
                if (stage == 0)
                {
                    Require(session.Clock.Day == 1 && session.LakeSeal == 1, "D1 must start with the lake sealed.");
                    if (operation == "nine-finale-tuning")
                        Require(Math.Abs(world.Finale.ClockCountdownSeconds - 3.5f) < .001f, "Saved countdown tuning was ignored.");
                    Require(!world.Tablet.gameObject.activeSelf && !world.TryCompile(), "T07 available before D8.");
                    Capture("day1");
                    if (operation == "nine-skip") { stage = 10; return; }
                    if (operation == "nine-late")
                    {
                        session.OpenLake();
                        session.Clock.AdvanceTo(8 * 1440 - .01);
                        PrepareRetrieval(); Require(world.BeginFinalInteraction(), "Very-late interaction failed.");
                        stage = 31; stageTime = now; return;
                    }
                    session.Clock.AdvanceTo(flow.Definition.FirstNightOpeningHour * 60 - .1);
                    Require(session.LakeSeal == 1, "Lake opened before first-night time.");
                    session.Clock.AdvanceBy(.1);
                    stage = 1; stageTime = now;
                }
                else if (stage == 1 && now - stageTime > .3)
                {
                    Require(session.LakeSeal == 0, "Natural first-night opening did not apply.");
                    world.Sleep(); stage = 2; stageTime = now;
                }
                else if (stage == 2 && now - stageTime > .7)
                {
                    int day = session.Clock.Day;
                    Require(Math.Abs(session.Clock.MinuteOfDay - flow.WakeHourFor(day) * 60) < 3, "Wake-time configuration ignored.");
                    if (operation == "nine-tuning")
                    {
                        Require(day == 2 && Math.Abs(session.Clock.MinuteOfDay - 600) < 3, "Saved D2 wake-time edit was ignored.");
                        UnityEngine.Object.FindObjectsByType<NineDayTablet>().Single(t => t.Index == 0).Use(world.Player);
                        stage = 20; stageTime = now; return;
                    }
                    var tablets = UnityEngine.Object.FindObjectsByType<NineDayTablet>(FindObjectsInactive.Include);
                    foreach (var tablet in tablets)
                        Require(tablet.gameObject.activeSelf == (!(tablet.Index == 5 && world.Cipher) && flow.Progress.Available(tablet.Index, day)), "Tablet discovery gate mismatch.");
                    if (day < 8)
                    {
                        var tablet = tablets.Single(t => t.Index == day - 2);
                        if (day == 7 && world.Cipher)
                        {
                            var box = world.Cipher; var code = box.Wheel.ReadCode();
                            for (int i = 0; i < 3; i++) while (box.Model.At(i) != code[i]) box.Model.Cycle(i);
                            Require(box.TryConfirm(), "Box A could not unlock at D7.");
                            box.Use(world.Player); box.Use(world.Player);
                        }
                        else { tablet.Use(world.Player); tablet.Use(world.Player); }
                        Require((day == 6 && world.Listening ? world.Player.InspectingTablet : world.Player.Reading) && !world.HasReadBox && flow.Progress.FoundCount == day - 1,
                            "Fixed-tablet reading/discovery leaked into box state or duplicated count.");
                        if (day == 2 || day == 7) Capture("day" + day + "-record");
                        stage = 3; stageTime = now;
                    }
                    else
                    {
                        Require(!world.Tablet.gameObject.activeSelf && !world.TryCompile() && flow.Progress.FoundCount == 6,
                            "D8 morning must not expose/compile T07.");
                        session.Clock.AdvanceTo(7 * 1440 + flow.Definition.FinalTabletHour * 60 - 2);
                        stage = 30; stageTime = now;
                    }
                }
                else if (stage == 3 && now - stageTime > .3)
                {
                    world.Player.ExitCloseView(); world.Sleep(); stage = 2; stageTime = now;
                }
                else if (stage == 4 && now - stageTime > 8)
                {
                    Require(!session.Descended && session.ExchangeCount == 1 && world.Player.Reading,
                        "Long reading crossed midnight or old automatic acceleration survived.");
                    Require(!world.ReadingText.Contains("+1"), "Legacy time rune explanation survived.");
                    Capture("box-record"); stage = 5; stageTime = now;
                }
                else if (stage == 5 && now - stageTime > .5)
                {
                    world.Player.ExitCloseView();
                    Require(world.Finale.IsDirecting && !world.Player.Reading, "Record close did not start sequence ownership.");
                    stage = 6; stageTime = now;
                }
                else if (stage == 6 && !session.Descended && now - stageTime > world.Finale.AfterReadingSeconds + 1)
                {
                    Require(Vector3.Distance(world.Player.ViewCamera.transform.position, world.Finale.ClockView.position) < .04f,
                        "Camera did not reach authored clock view.");
                    Capture("clock"); stage = 60;
                }
                else if ((stage == 6 || stage == 60) && session.Descended)
                {
                    Require(session.ExchangeCount == 2 && session.Clock.Day == 9 && session.Clock.MinuteOfDay == 0, "Nine-day finale duplicated/skipped or leaked time.");
                    Require(!world.EndingPresented, "Ending panel hid the midnight effects.");
                    Require(world.Finale.LakeGlowVisible, "Lake glow did not return at second exchange.");
                    Capture("midnight");
                    stage = 7; stageTime = now;
                }
                else if (stage == 7 && now - stageTime > 3)
                {
                    Require(world.EndingPresented, "Finale did not release the ending view.");
                    var lid = UnityEngine.Object.FindAnyObjectByType<SlicePresentation>().LidPivot;
                    Require(Quaternion.Angle(lid.localRotation, Quaternion.identity) < .1f, "Box did not close at midnight.");
                    Capture("ending"); Complete(true, "No+1: D8 night gate, reserved puzzle/long reading, authored clock camera, two exchanges, box slam, lake glow and delayed ending passed: " + operation);
                }
                else if (stage == 10)
                {
                    if (!session.Descended) world.Sleep();
                    else
                    {
                        Require(session.Clock.Day == 9 && session.Clock.MinuteOfDay == 0 && session.ExchangeCount == 0 && flow.Progress.FoundCount == 0,
                            "Continuous sleeping blocked or triggered discoveries/exchanges.");
                        stage = 11; stageTime = now;
                    }
                }
                else if (stage == 11 && now - stageTime > 2)
                {
                    Capture("skip-ending"); Complete(true, "D1-D9 sleep-only route passed with zero discoveries, zero exchanges and terminal midnight.");
                }
                else if (stage == 20 && now - stageTime > 1)
                {
                    var anchor = UnityEngine.Object.FindObjectsByType<NineDayTablet>().Single(t => t.Index == 0).transform.Find("ReadView");
                    Require(world.Player.Reading && Vector3.Distance(world.Player.ViewCamera.transform.position, anchor.position) < .03f,
                        "Saved fixed-tablet camera anchor was ignored.");
                    Capture("tuned-record"); stage = 21; stageTime = now;
                }
                else if (stage == 21 && now - stageTime > .3)
                    Complete(true, "Saved D2 wake-time edit respected at 10:00 and authored read camera respected; restore on exit.");
                else if (stage == 30 && now - stageTime > .5)
                {
                    Require(!world.Tablet.gameObject.activeSelf, "T07 appeared before configured night hour.");
                    session.Clock.AdvanceTo(7 * 1440 + flow.Definition.FinalTabletHour * 60); stage = 31; stageTime = now;
                }
                else if (stage == 31 && now - stageTime > .5)
                {
                    PrepareRetrieval(); Require(world.Tablet.gameObject.activeSelf, "T07 did not become editable after pickup and placement.");
                    world.Tablet.Use(world.Player);
                    Require(world.Finale.TimeReserved, "First T07 interaction did not reserve puzzle time.");
                    if (operation == "nine-sleep-reserved")
                    {
                        world.Player.ExitCloseView(); world.Sleep();
                        Require(session.Descended && session.ExchangeCount == 0 && !world.Finale.TimeReserved,
                            "Sleeping from reserved puzzle window failed to bypass the finale.");
                        Complete(true, "Explicit sleep releases the D8 reservation and reaches D9 without repair."); return;
                    }
                    Require(!world.TryCompile(), "Incomplete board compiled.");
                    session.Clock.AdvanceTo(world.Finale.LimitNaturalTime(session.Clock.NextMidnight + 10000));
                    Require(session.Clock.Day == 8 && !session.Descended, "Puzzle reservation crossed midnight.");
                    var board = session.Board;
                    Require(board.At(5) == RuneKind.None, "Retired time rune survived in saved settings.");
                    board.PickFromTray(RuneKind.Exchange); board.ClickSlot(4);
                    board.PickFromTray(RuneKind.Box); board.ClickSlot(7);
                    board.PickFromTray(RuneKind.Lake); board.ClickSlot(8);
                    double before = session.Clock.AbsoluteMinutes;
                    Require(world.TryCompile() && session.ExchangeCount == 1 && session.LakeSeal == 1, "D8 reconstruction failed.");
                    Require(session.Clock.AbsoluteMinutes == before, "Solidifying advanced time.");
                    UnityEngine.Object.FindAnyObjectByType<SliceBox>().Use(world.Player);
                    Require(world.HasReadBox && world.Player.Reading, "Real box component failed after migration.");
                    stage = 4; stageTime = now;
                }
            }
            catch (Exception exception) { Complete(false, exception.ToString()); }
        }
        void PrepareRetrieval()
        {
            if (!world.Retrieval || world.Retrieval.Progress.Placed) return;
            world.Retrieval.RefreshPresentation(); var pickup = world.Retrieval.Pickup;
            Require(pickup.gameObject.activeSelf && !world.Tablet.gameObject.activeSelf && !world.TryCompile(), "T07 pickup/placement gate missing.");
            pickup.Use(world.Player); pickup.TickHeld(true, pickup.HoldSeconds + .01f);
            Require(world.Retrieval.Progress.Acquired && world.Retrieval.Place(world.Player), "T07 fixture acquisition/placement failed.");
        }
        void Complete(bool success, string message)
        {
            finish(success, message); finished = true;
            EditorApplication.delayCall += EditorApplication.ExitPlaymode;
        }
        void Capture(string name)
        {
            Directory.CreateDirectory("Temp/DemonSliceReports");
            ScreenCapture.CaptureScreenshot(Path.GetFullPath("Temp/DemonSliceReports/" + operation + "-" + name + ".png"));
        }
    }
}
