using System;
using System.Collections.Generic;
using Demon.Slice.Domain;
using NUnit.Framework;

namespace Demon.Slice.Tests
{
    public class SliceRuleTests
    {
        static RuneKind[] Target() => new[] { RuneKind.Nine, RuneKind.GreaterEqual, RuneKind.Day,
            RuneKind.None, RuneKind.Exchange, RuneKind.None, RuneKind.Seal, RuneKind.Box, RuneKind.Lake };
        static RuneBoardModel CompleteBoard() => new RuneBoardModel(Target(), Target(), Array.Empty<RuneKind>());
        static SliceSession Ready() => new SliceSession(8, 960, 9, CompleteBoard());

        [Test] public void ExchangeTwiceRestoresValues()
        {
            Assert.That(AttributeOperations.TryExchange(1, 1, 0, 1, out int a, out int b), Is.True);
            Assert.That((a, b), Is.EqualTo((0, 1)));
            AttributeOperations.TryExchange(a, 1, b, 1, out a, out b);
            Assert.That((a, b), Is.EqualTo((1, 0)));
        }

        [Test] public void ExchangeCapacityFailureLeavesBothSidesUnchanged()
        {
            Assert.That(AttributeOperations.TryExchange(1, 1, 3, 3, out int a, out int b), Is.False);
            Assert.That((a, b), Is.EqualTo((1, 3)));
        }

        [TestCase(0, 0, 1)] [TestCase(1, 1, 1)]
        public void TransferFailureLeavesBothSidesUnchanged(int source, int destination, int capacity)
        {
            Assert.That(AttributeOperations.TryTransfer(source, destination, capacity, out int a, out int b), Is.False);
            Assert.That((a, b), Is.EqualTo((source, destination)));
        }

        [Test] public void TransferConservesUnits()
        {
            Assert.That(AttributeOperations.TryTransfer(2, 0, 3, out int a, out int b), Is.True);
            Assert.That((a, b), Is.EqualTo((1, 1)));
        }

        [Test] public void SleepEnumeratesEveryMidnight()
        {
            var clock = new GameClock(2, 1000);
            var days = new List<int>();
            clock.Midnight += days.Add;
            clock.AdvanceTo(6 * 1440 + 480);
            Assert.That(days, Is.EqualTo(new[] { 3, 4, 5, 6, 7 }));
        }

        [Test] public void StoppedClockCannotLeakToDayTen()
        {
            var clock = new GameClock(8, 960);
            clock.Midnight += _ => clock.Stop();
            clock.AdvanceTo(12 * 1440);
            Assert.That(clock.Day, Is.EqualTo(9));
            Assert.That(clock.MinuteOfDay, Is.EqualTo(0));
        }

        [Test] public void ClockRejectsReverseTime()
        {
            var clock = new GameClock(8, 960);
            Assert.Throws<ArgumentOutOfRangeException>(() => clock.AdvanceBy(-1));
        }

        [Test] public void CompletionAfterMidnightExecutesImmediatelyOnce()
        {
            using (var session = Ready())
            {
                double before = session.Clock.AbsoluteMinutes;
                Assert.That(session.TryCompile(), Is.True);
                Assert.That(session.Clock.AbsoluteMinutes, Is.EqualTo(before), "Compiling must not advance time.");
                Assert.That((session.BoxSeal, session.LakeSeal, session.ExchangeCount), Is.EqualTo((0, 1, 1)));
                Assert.That(session.TryCompile(), Is.False);
                Assert.That(session.ExchangeCount, Is.EqualTo(1));
            }
        }

        [Test] public void RepeatedSameDayTimeUpdatesCannotRepeatExchange()
        {
            using var session = Ready();
            session.TryCompile();
            for (int i = 0; i < 200; i++) session.Clock.AdvanceBy(.1);
            Assert.That(session.ExchangeCount, Is.EqualTo(1));
        }

        [Test] public void NextMidnightExchangesExactlyOnceAndDescends()
        {
            using (var session = Ready())
            {
                session.TryCompile();
                session.Clock.AdvanceTo(session.Clock.NextMidnight);
                session.Clock.AdvanceBy(10000);
                Assert.That((session.BoxSeal, session.LakeSeal, session.ExchangeCount), Is.EqualTo((1, 0, 2)));
                Assert.That(session.Descended, Is.True);
                Assert.That(session.Clock.Day, Is.EqualTo(9));
            }
        }

        [Test] public void SleepingWithoutRepairStillDescends()
        {
            using (var session = Ready())
            {
                session.SleepToNextDay(480);
                Assert.That(session.Descended, Is.True);
                Assert.That(session.ExchangeCount, Is.Zero);
                Assert.That(session.Clock.MinuteOfDay, Is.Zero);
            }
        }

        [Test] public void RemovingSurfaceRuneDoesNotEraseCompiledProgram()
        {
            using (var session = Ready())
            {
                session.TryCompile();
                session.Board.ClickSlot(4);
                Assert.That(session.Board.At(4), Is.EqualTo(RuneKind.None));
                Assert.That(session.Board.CompiledAt(4), Is.EqualTo(RuneKind.Exchange));
                session.Clock.AdvanceTo(session.Clock.NextMidnight);
                Assert.That(session.Descended, Is.True);
                Assert.That(session.ExchangeCount, Is.EqualTo(2));
            }
        }

        [TestCase(false)] [TestCase(true)]
        public void BothBoxLakeOrdersFreezeOnceAndRepeatAtMidnight(bool reversed)
        {
            var initial = Target();
            if (reversed) { initial[7] = RuneKind.Lake; initial[8] = RuneKind.Box; }
            var board = new RuneBoardModel(initial, Target(), Array.Empty<RuneKind>());
            using (var session = new SliceSession(8, 960, 9, board))
            {
                Assert.That(session.TryCompile(), Is.True);
                Assert.That(session.TryCompile(), Is.False);
                Assert.That(board.CompiledAt(7), Is.EqualTo(initial[7]));
                Assert.That(board.CompiledAt(8), Is.EqualTo(initial[8]));
                Assert.That((session.BoxSeal, session.LakeSeal, session.ExchangeCount), Is.EqualTo((0, 1, 1)));
                session.Clock.AdvanceTo(session.Clock.NextMidnight);
                Assert.That((session.BoxSeal, session.LakeSeal, session.ExchangeCount), Is.EqualTo((1, 0, 2)));
                Assert.That(session.Descended, Is.True);
            }
        }

        [TestCase(RuneKind.Box, RuneKind.Box)] [TestCase(RuneKind.Lake, RuneKind.Lake)]
        [TestCase(RuneKind.None, RuneKind.Lake)] [TestCase(RuneKind.Box, RuneKind.None)]
        [TestCase(RuneKind.Open, RuneKind.Lake)]
        public void InvalidObjectPairDoesNotFreezeOrExchange(RuneKind first, RuneKind second)
        {
            var initial = Target(); initial[7] = first; initial[8] = second;
            var board = new RuneBoardModel(initial, Target(), Array.Empty<RuneKind>());
            using (var session = new SliceSession(8, 960, 9, board))
            {
                Assert.That(session.TryCompile(), Is.False);
                Assert.That(board.Frozen, Is.False);
                Assert.That((session.BoxSeal, session.LakeSeal, session.ExchangeCount), Is.EqualTo((1, 0, 0)));
            }
        }

        [TestCase(0, RuneKind.Day)] [TestCase(4, RuneKind.Open)] [TestCase(6, RuneKind.None)]
        public void ReversingObjectsDoesNotRelaxOtherSlots(int slot, RuneKind wrong)
        {
            var initial = Target(); initial[7] = RuneKind.Lake; initial[8] = RuneKind.Box; initial[slot] = wrong;
            Assert.That(new RuneBoardModel(initial, Target(), Array.Empty<RuneKind>()).TryFreeze(), Is.False);
        }

        [Test] public void OtherProgramsDoNotGainUnorderedOperandMatching()
        {
            var target = Target(); target[4] = RuneKind.Ring;
            var initial = (RuneKind[])target.Clone(); initial[7] = RuneKind.Lake; initial[8] = RuneKind.Box;
            Assert.That(new RuneBoardModel(initial, target, Array.Empty<RuneKind>()).MatchesTarget(), Is.False);
        }

        [Test] public void SlotAndTrayOperationsConserveFiniteRunes()
        {
            var start = Target(); start[4] = RuneKind.None;
            var board = new RuneBoardModel(start, Target(), new[] { RuneKind.Exchange });
            Assert.That(board.PickFromTray(RuneKind.Exchange), Is.True);
            Assert.That(board.PickFromTray(RuneKind.Exchange), Is.False);
            board.ClickSlot(4);
            Assert.That(board.At(4), Is.EqualTo(RuneKind.Exchange));
            Assert.That(board.Held, Is.EqualTo(RuneKind.None));
            Assert.That(board.TryFreeze(), Is.True);
        }
    }
}
