using System;
using Demon.Slice.Domain;
using NUnit.Framework;

namespace Demon.Slice.Tests
{
    public class DailyProgressTests
    {
        [Test] public void RepeatedInspectionCountsOnceAndNewDayClearsIt()
        { var p = new DailyProgress(1); Assert.That(p.Record("radio"), Is.True); Assert.That(p.Record("radio"), Is.False); p.Begin(2); Assert.That(p.Contains("radio"), Is.False); }
        [Test] public void SleepingNeedsAllTasksAndNightAndStopsOnDayEight()
        {
            var p = new DailyProgress(7); string[] required = { "radio", "T06" };
            p.Record("radio"); Assert.That(p.CanSleep(required, 23, 18), Is.False);
            p.Record("T06"); Assert.That(p.CanSleep(required, 17.99, 18), Is.False);
            Assert.That(p.CanSleep(required, 18, 18), Is.True); p.Begin(8);
            Assert.That(p.CanSleep(Array.Empty<string>(), 23, 18), Is.False);
        }
        [Test] public void MidnightGuardPreventsDateEffectsEvenWithLargeTimeStep()
        {
            var clock = new GameClock(2, 1439); int events = 0;
            clock.Midnight += day => events++; clock.CanEnterDay = day => false;
            clock.AdvanceBy(5000); Assert.That((clock.Day, events), Is.EqualTo((2, 0)));
        }
        [Test] public void LegalSleepPublishesStableWakeTimeAfterMidnightListeners()
        {
            var clock = new GameClock(2, 1200); bool applied = false, captured = false;
            clock.Midnight += day => applied = true; clock.CanEnterDay = day => true;
            clock.Advanced += () => captured = applied && clock.Day == 3 && clock.MinuteOfDay == 480;
            clock.AdvanceTo(clock.NextMidnight + 480); Assert.That(captured, Is.True);
        }
        [Test] public void LargeAdvanceCannotReuseCompletedTasksForTheFollowingDay()
        {
            var clock = new GameClock(1, 1430); var tasks = new DailyProgress(1); tasks.Record("inspect");
            int effects = 0;
            clock.CanEnterDay = day => tasks.Complete(new[] { "inspect" });
            clock.Midnight += day => effects++;
            clock.DayEntered += day => tasks.Begin(day);
            clock.AdvanceBy(4000);
            Assert.That((clock.Day, tasks.Day, effects), Is.EqualTo((2, 2, 1)));
        }
        [Test] public void PlayerRecordSnapshotDoesNotAddUncollectedOptionalItems()
        {
            var records = new RecordCollection(); records.Add("T01"); string[] start = records.Capture();
            records.Add("note-b"); records.Restore(start); start[0] = "changed";
            Assert.That(records.Contains("T01"), Is.True); Assert.That(records.Contains("note-b"), Is.False);
        }
        [Test] public void SessionSnapshotPreservesPartialBoardAndCanRestoreRepeatedly()
        {
            var target = new[] { RuneKind.Nine, RuneKind.GreaterEqual, RuneKind.Day, RuneKind.None, RuneKind.Exchange, RuneKind.None, RuneKind.Seal, RuneKind.Box, RuneKind.Lake };
            var initial = (RuneKind[])target.Clone(); initial[4] = RuneKind.None;
            using var session = new GameSession(4, 480, 9, new RuneBoardModel(initial, target, new[] { RuneKind.Exchange }), false, 8);
            session.Board.PickFromTray(RuneKind.Exchange); var saved = session.Capture();
            using var restored = GameSession.FromSnapshot(saved); restored.Board.ClickSlot(4);
            using var again = GameSession.FromSnapshot(saved);
            Assert.That(again.Board.At(4), Is.EqualTo(RuneKind.None)); Assert.That(again.Board.Held, Is.EqualTo(RuneKind.Exchange));
            Assert.That((again.Clock.Day, again.Clock.MinuteOfDay), Is.EqualTo((4, 480d)));
        }
        [Test] public void RestoredCompiledProgramRetainsItsNextMidnightEffect()
        {
            var target = new[] { RuneKind.Nine, RuneKind.GreaterEqual, RuneKind.Day, RuneKind.None, RuneKind.Exchange, RuneKind.None, RuneKind.Seal, RuneKind.Box, RuneKind.Lake };
            using var session = new GameSession(8, 1380, 9, new RuneBoardModel(target, target, null));
            Assert.That(session.TryCompile(), Is.True); var saved = session.Capture();
            using var restored = GameSession.FromSnapshot(saved); restored.SleepToNextDay(480);
            Assert.That((restored.ExchangeCount, restored.LakeSeal, restored.Descended), Is.EqualTo((2, 0, true)));
        }
    }
}
