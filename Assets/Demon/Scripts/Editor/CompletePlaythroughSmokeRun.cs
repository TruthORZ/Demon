using System;
using System.IO;
using System.Linq;
using Demon.Slice.Domain;
using UnityEditor;
using UnityEngine;

namespace Demon.Slice.Editor
{
    public sealed class CompletePlaythroughSmokeRun : ISmokeRun
    {
        readonly Action<bool, string> finish;
        readonly double started = EditorApplication.timeSinceStartup;
        bool done;
        readonly bool full;
        public CompletePlaythroughSmokeRun(Action<bool, string> completed, bool fullRoute = false) { finish = completed; full = fullRoute; }
        public void Tick()
        {
            if (done || !Application.isPlaying) return;
            EditorApplication.QueuePlayerLoopUpdate();
            try
            {
                if (EditorApplication.timeSinceStartup - started > 90) throw new Exception("Main route smoke timed out.");
                var world = UnityEngine.Object.FindAnyObjectByType<GameSceneController>();
                if (!world || world.Session == null || Time.frameCount < 4) return;
                var flow = world.Playthrough; Require(flow, "Main route is missing."); flow.BeginDebugReplay();
                Require(world.Exploration.Observations.Length == 4, "Four inspections required.");
                world.Sleep(); Require(world.Session.Clock.Day == 1, "Incomplete day allowed sleep.");
                foreach (var point in world.Exploration.Observations) { point.Use(world.Player); world.Player.ReturnToExploration(); }
                flow.Collect("note-a", world.Player.transform); world.Player.ReturnToExploration();
                world.Cipher.Use(world.Player); world.Player.ReturnToExploration();
                UnityEngine.Object.FindAnyObjectByType<RecordBox>().Use(world.Player);
                Require(flow.Tasks.Complete(flow.Definition.Required(1)), "D1 task collection failed.");
                world.Sleep(); Require(world.Session.Clock.Day == 1, "Daytime sleep allowed.");
                var door = world.gameObject.scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<CabinDoorInteraction>(true)).First();
                door.Use(world.Player);
                world.Cipher.Model.Cycle(0); int inputAtStart = world.Cipher.Model.At(0);
                world.Session.Clock.AdvanceTo(20 * 60); world.Sleep();
                Require(world.Session.Clock.Day == 2 && world.Session.Clock.MinuteOfDay == 480, "Legal night sleep did not reach D2 08:00.");
                Require(door.Capture().opened, "Door opening did not reach day checkpoint."); door.Use(world.Player);
                flow.Collect("note-b", world.Player.transform); world.Player.ReturnToExploration();
                world.Cipher.Model.Cycle(0); var oldClock = world.Session.Clock;
                world.Session.Clock.AdvanceTo(world.Session.Clock.NextMidnight + 480);
                Require(flow.Failed && world.Session.Clock.Day == 2 && !world.Session.Descended, "Missing D2 tasks did not fail before next midnight.");
                flow.RetryDay();
                Require(!flow.Failed && world.Session.Clock.Day == 2 && world.Session.Clock.MinuteOfDay == 480, "Retry failed to restore D2 start.");
                Require(flow.Records.Contains("note-a") && !flow.Records.Contains("note-b"), "Retry changed earlier/optional collection history.");
                Require(door.Capture().opened, "Retry lost earlier door state.");
                Require(world.Cipher.Model.At(0) == inputAtStart, "Partial cipher input did not restore.");
                oldClock.AdvanceBy(2000); Require(world.Session.Clock.Day == 2, "Old clock affected restored session.");
                for (int i = 0; i < 3; i++)
                {
                    world.Session.Clock.AdvanceTo(world.Session.Clock.NextMidnight); Require(flow.Failed, "Repeat failure missing."); flow.RetryDay();
                    Require(world.Session.Clock.Day == 2 && Time.timeScale == 1 && !world.Player.MenuOpen, "Repeat retry retained pause/input.");
                }
                if (full)
                {
                    foreach (int day in new[] { 3, 7, 8, 2, 6, 5, 4, 8, 9, 1 })
                    {
                        world.Restart(); CompletedDayReplay.Run(world, day, false);
                        Require(world.Session.Clock.Day == day && !flow.Failed, "Full route failed at D" + day);
                        if (day < 9)
                        {
                            Require(flow.Tasks.Day == day, "Task day mismatch.");
                            world.Session.Clock.AdvanceTo(world.Session.Clock.NextMidnight);
                            Require(flow.Failed, "Target day should remain uncompleted, D" + day);
                            flow.RetryDay(); Require(world.Session.Clock.Day == day && !flow.Failed, "Target retry mismatch.");
                        }
                        else Require(world.Session.Descended && world.Session.ExchangeCount == 2 && world.EndingPresented, "Finale did not finish both exchanges.");
                    }
                    world.Restart(); CompletedDayReplay.Run(world, 8, true);
                    Require(world.Campaign.FinalAvailable && !world.Retrieval.Progress.Acquired && !flow.Failed, "D8 night entry not ready.");
                    File.WriteAllText("Temp/DemonSliceReports/complete-route-evidence.txt", "D1-D9 canonical route, target-day failure/retry on D1-D8, forwards/backwards jumps, D8 night, two exchanges and ending passed.");
                }
                File.WriteAllText("Temp/DemonSliceReports/complete-p1-evidence.txt", "D1 four inspections + note A + two boxes; daytime/missing sleep blocked; legal sleep D2 08:00; midnight fails before D3; optional collection and partial cipher rollback; old clock isolation; three repeat retries passed.");
                done = true; finish(true, full ? "Complete route and daily failure/retry smoke passed." : "P1 main route failure/retry smoke passed. No synthetic previous-day reconstruction used."); EditorApplication.ExitPlaymode();
            }
            catch (Exception error) { done = true; finish(false, error.ToString()); EditorApplication.ExitPlaymode(); }
        }
        public void Cleanup() { Time.timeScale = 1; AudioListener.pause = false; }
        static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    }
}
