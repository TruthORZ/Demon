using System;
using Demon.Slice.Domain;
using NUnit.Framework;
namespace Demon.Slice.Tests
{
    public sealed class BearingCipherTests
    {
        [TestCase(0, 1, 0)] [TestCase(1, 0, 3)] [TestCase(0, -1, 5)] [TestCase(-1, 0, 8)]
        public void CardinalOffsetsUseClockwiseNearestSector(double east, double north, int sector) => Assert.AreEqual(sector, BearingCipher.Sector(east, north));
        [Test] public void SectorWrapsAroundNorthAndIgnoresDistance()
        {
            Assert.AreEqual(0, BearingCipher.Sector(-.1, 1)); Assert.AreEqual(0, BearingCipher.Sector(.1, 1));
            Assert.AreEqual(BearingCipher.Sector(2, 3), BearingCipher.Sector(20, 30));
        }
        [Test] public void InvalidLandmarkAndAmbiguousDigitsFailClearly()
        {
            Assert.Throws<ArgumentException>(() => BearingCipher.Sector(0, 0));
            Assert.Throws<ArgumentException>(() => BearingCipher.Sector(double.NaN, 1));
            Assert.Throws<ArgumentException>(() => BearingCipher.ValidateDigits(new int[10]));
            Assert.Throws<ArgumentException>(() => BearingCipher.ValidateDigits(new[] { 0, 1 }));
        }
        [Test] public void WrongCodeCanRetryAndDigitsWrap()
        {
            var model = new ThreeDigitLock(new[] { 1, 2, 3 });
            Assert.IsFalse(model.TryUnlock()); for (int i = 0; i < 10; i++) model.Cycle(0);
            Assert.AreEqual(0, model.At(0)); model.Cycle(0); model.Cycle(1); model.Cycle(1);
            for (int i = 0; i < 3; i++) model.Cycle(2);
            Assert.IsTrue(model.TryUnlock()); Assert.IsFalse(model.TryUnlock()); model.Cycle(0); Assert.AreEqual(1, model.At(0));
        }
        [Test] public void AcquisitionRequiresUnlockAndOccursOnce()
        {
            int[] code = { 0, 0, 0 }; var model = new ThreeDigitLock(code); code[0] = 9;
            Assert.IsFalse(model.TryAcquire()); Assert.IsTrue(model.TryUnlock());
            Assert.IsTrue(model.TryAcquire()); Assert.IsFalse(model.TryAcquire()); Assert.IsTrue(model.Acquired);
        }
        [Test] public void AcquiredTabletCanBeFoundBeforeItsScheduledDayWithoutUnlockingOtherTablets()
        {
            var progress = new DiscoveryProgress(new[] { 2, 3, 4, 5, 6, 7 });
            Assert.IsFalse(progress.Available(5, 1)); Assert.IsTrue(progress.Acquire(5));
            Assert.IsTrue(progress.Available(5, 1)); Assert.IsFalse(progress.Discover(5, 1)); Assert.IsFalse(progress.Acquire(5));
            Assert.AreEqual(1, progress.FoundCount); Assert.IsFalse(progress.Available(4, 1));
        }
    }
}
