using System;
using Demon.Slice.Domain;
using NUnit.Framework;

namespace Demon.Slice.Tests
{
    public class FinaleTimeWindowTests
    {
        const double Midnight = 8 * 1440;
        [Test] public void NoInteractionLeavesNaturalMidnightUntouched()
        {
            var window = new FinaleTimeWindow(Midnight, 1);
            Assert.That(window.Limit(Midnight - 2, Midnight + 10), Is.EqualTo(Midnight + 10));
        }
        [Test] public void LongPuzzleAndReadingStayBeforeMidnight()
        {
            var window = new FinaleTimeWindow(Midnight, 1);
            window.Reserve(Midnight - 30);
            double now = window.Limit(Midnight - 30, Midnight + 10000);
            Assert.That(now, Is.EqualTo(Midnight - 1));
            Assert.That(window.Limit(now, now + 10000), Is.EqualTo(now));
        }
        [Test] public void VeryLateInteractionAndReadingNeverReverseTime()
        {
            var window = new FinaleTimeWindow(Midnight, 1);
            double now = Midnight - .01;
            Assert.That(window.Reserve(now), Is.True);
            Assert.That(window.Limit(now, now + 100), Is.EqualTo(now));
            Assert.That(window.AlignForCountdown(now), Is.EqualTo(now));
        }
        [Test] public void ImmediateReadingCloseAlignsForwardButDoesNotExecuteMidnight()
        {
            var window = new FinaleTimeWindow(Midnight, 1);
            window.Reserve(Midnight - 120);
            Assert.That(window.AlignForCountdown(Midnight - 120), Is.EqualTo(Midnight - 1));
        }
        [Test] public void ExplicitSleepReleasesReservation()
        {
            var window = new FinaleTimeWindow(Midnight, 1);
            window.Reserve(Midnight - 2); window.Release();
            Assert.That(window.Limit(Midnight - 2, Midnight + 480), Is.EqualTo(Midnight + 480));
        }
        [Test] public void CannotReserveAfterMidnightOrAcceptReverseRequest()
        {
            var window = new FinaleTimeWindow(Midnight, 1);
            Assert.That(window.Reserve(Midnight), Is.False);
            Assert.Throws<InvalidOperationException>(() => window.AlignForCountdown(Midnight));
            Assert.Throws<ArgumentOutOfRangeException>(() => window.Limit(10, 9));
        }
    }
}
