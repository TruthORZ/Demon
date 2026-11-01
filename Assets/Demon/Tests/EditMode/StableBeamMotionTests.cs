using System;
using Demon.Slice.Domain;
using NUnit.Framework;
namespace Demon.Slice.Tests
{
    public sealed class StableBeamMotionTests
    {
        [Test] public void ShortAimDoesNotExchangeAndBreakResetsProgress()
        {
            var rule = new StableBeamMotion(1); rule.Advance(true, .7); Assert.IsFalse(rule.Locked);
            rule.Advance(false, .1); Assert.AreEqual(0, rule.StableSeconds);
            rule.Advance(true, .4); Assert.AreEqual(0, rule.ExchangeCount);
        }
        [Test] public void StableHoldExchangesOnceAndContinuousFramesCannotSwapBack()
        {
            var rule = new StableBeamMotion(1); rule.Advance(true, .4); rule.Advance(true, .6);
            Assert.IsTrue(rule.Locked); Assert.AreEqual((0, 1, 1), (rule.DemonMotion, rule.VaneMotion, rule.ExchangeCount));
            for (int i = 0; i < 100; i++) rule.Advance(true, .02);
            Assert.AreEqual((0, 1, 1), (rule.DemonMotion, rule.VaneMotion, rule.ExchangeCount));
        }
        [Test] public void LossExchangesBackExactlyOnce()
        {
            var rule = new StableBeamMotion(.5); rule.Advance(true, 2); rule.Advance(false, 0);
            for (int i = 0; i < 30; i++) rule.Advance(false, .1);
            Assert.AreEqual((1, 0, 2), (rule.DemonMotion, rule.VaneMotion, rule.ExchangeCount)); Assert.IsFalse(rule.Locked);
        }
        [Test] public void ReleaseOnDisableRestoresAndNextUseRequiresFullHold()
        {
            var rule = new StableBeamMotion(1); rule.Advance(true, 1); rule.Release(); rule.Release();
            rule.Advance(true, .3); Assert.IsFalse(rule.Locked); Assert.AreEqual(2, rule.ExchangeCount);
            rule.Advance(true, .7); Assert.IsTrue(rule.Locked); Assert.AreEqual(3, rule.ExchangeCount);
        }
        [Test] public void RepeatedCyclesConserveOneUnitOfMotion()
        {
            var rule = new StableBeamMotion(.2);
            for (int i = 0; i < 50; i++)
            { rule.Advance(true, .2); Assert.AreEqual(1, rule.DemonMotion + rule.VaneMotion); rule.Release(); }
            Assert.AreEqual((1, 0, 100), (rule.DemonMotion, rule.VaneMotion, rule.ExchangeCount));
        }
        [Test] public void InvalidTimeCannotMutateRule()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new StableBeamMotion(0));
            var rule = new StableBeamMotion(1);
            Assert.Throws<ArgumentOutOfRangeException>(() => rule.Advance(true, -.1));
            Assert.Throws<ArgumentOutOfRangeException>(() => rule.Advance(true, double.NaN));
            Assert.AreEqual(0, rule.ExchangeCount);
        }
    }
}
