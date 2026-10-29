using System;
using Demon.Slice.Domain;
using NUnit.Framework;
namespace Demon.Slice.Tests
{
    public sealed class PursuitRuleTests
    {
        [Test] public void FrozenOrOccludedThreatCannotPass()
        {
            var gate = new ClosePassGate(2, 3); Assert.IsFalse(gate.TryPass(1, true, false)); Assert.IsFalse(gate.TryPass(1, false, true)); Assert.IsFalse(gate.TryPass(3, true, true)); Assert.AreEqual(0, gate.PassCount);
        }
        [Test] public void ClosePassIsDeduplicatedUntilCooldownEnds()
        {
            var gate = new ClosePassGate(2, 3); Assert.IsTrue(gate.TryPass(1, true, true));
            for (int i = 0; i < 10; i++) Assert.IsFalse(gate.TryPass(1, true, true));
            gate.Advance(2.9); Assert.IsFalse(gate.TryPass(1, true, true)); gate.Advance(.2); Assert.IsTrue(gate.TryPass(1, true, true)); Assert.AreEqual(2, gate.PassCount);
        }
        [Test] public void InterruptedPickupMustRetryButDoesNotAffectAnotherCompletedItem()
        {
            var first = new PickupAttempt(1); first.Begin(); Assert.IsTrue(first.Advance(1, true));
            var second = new PickupAttempt(1); second.Begin(); second.Advance(.8, true); Assert.IsTrue(second.Cancel(true));
            Assert.IsFalse(second.Completed); Assert.AreEqual(0, second.Elapsed); Assert.AreEqual(1, second.InterruptCount); Assert.IsTrue(first.Completed);
            Assert.IsTrue(second.Begin()); Assert.IsFalse(second.Advance(.5, true)); Assert.IsTrue(second.Advance(.5, true));
        }
        [Test] public void ReleasingInputCancelsAndCompletionOccursOnlyOnce()
        {
            var pickup = new PickupAttempt(.5); pickup.Begin(); pickup.Advance(.2, false); Assert.IsFalse(pickup.Active); Assert.AreEqual(0, pickup.Elapsed);
            pickup.Begin(); Assert.IsTrue(pickup.Advance(.5, true)); Assert.IsFalse(pickup.Advance(2, true)); Assert.IsFalse(pickup.Begin()); Assert.IsFalse(pickup.Cancel(true)); Assert.AreEqual(1, pickup.Progress);
        }
        [Test] public void InvalidDurationsFailBeforeStateChanges()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ClosePassGate(0, 1)); Assert.Throws<ArgumentOutOfRangeException>(() => new PickupAttempt(0));
            var pickup = new PickupAttempt(1); pickup.Begin(); Assert.Throws<ArgumentOutOfRangeException>(() => pickup.Advance(-1, true)); Assert.AreEqual(0, pickup.Elapsed);
        }
    }
}
