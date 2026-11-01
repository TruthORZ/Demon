using System;
using Demon.Slice.Domain;
using NUnit.Framework;

namespace Demon.Slice.Tests
{
    public class NineDayRuleTests
    {
        [Test] public void UnavailableTabletCannotBeDiscoveredAndRepeatedReadingDoesNotDuplicate()
        {
            var progress = new DiscoveryProgress(new[] { 2, 3, 4, 5, 6, 7 });
            Assert.That(progress.Discover(0, 1), Is.False);
            Assert.That(progress.Discover(0, 2), Is.True);
            Assert.That(progress.Discover(0, 7), Is.False);
            Assert.That(progress.FoundCount, Is.EqualTo(1));
            Assert.That(progress.Found(0), Is.True);
        }
        [Test] public void DiscoveryScheduleCopiesAuthoredArray()
        {
            var days = new[] { 2, 3, 4, 5, 6, 7 };
            var progress = new DiscoveryProgress(days); days[0] = 1;
            Assert.That(progress.Available(0, 1), Is.False);
        }
        [Test] public void ClosedFirstNightCanOpenWithoutTouchingBoxSeal()
        {
            using var session = Session();
            Assert.That(session.LakeSeal, Is.EqualTo(1));
            session.OpenLake(); session.OpenLake();
            Assert.That((session.BoxSeal, session.LakeSeal, session.ExchangeCount), Is.EqualTo((1, 0, 0)));
        }
        [Test] public void BeforeDayEightTargetCannotFreezeButCanRetryOnDayEight()
        {
            using var session = Session();
            Assert.That(session.TryCompile(), Is.False);
            Assert.That(session.Board.Frozen, Is.False);
            session.OpenLake(); session.Clock.AdvanceTo(7 * 1440 + 480);
            Assert.That(session.TryCompile(), Is.True);
            Assert.That((session.BoxSeal, session.LakeSeal), Is.EqualTo((0, 1)));
            session.Clock.AdvanceTo(8 * 1440);
            Assert.That(session.Descended, Is.True);
            Assert.That(session.ExchangeCount, Is.EqualTo(2));
        }
        [Test] public void SkippingEveryDiscoveryStillEndsAtNinthMidnight()
        {
            using var session = Session(); session.OpenLake();
            for (int i = 0; i < 8; i++) session.SleepToNextDay(480);
            Assert.That(session.Descended, Is.True);
            Assert.That((session.Clock.Day, session.Clock.MinuteOfDay), Is.EqualTo((9, 0d)));
        }
        static SliceSession Session()
        {
            var target = new[] { RuneKind.Nine, RuneKind.GreaterEqual, RuneKind.Day, RuneKind.None,
                RuneKind.Exchange, RuneKind.None, RuneKind.Seal, RuneKind.Box, RuneKind.Lake };
            return new SliceSession(1, 480, 9, new RuneBoardModel(target, target, Array.Empty<RuneKind>()), true, 8);
        }
    }
}
