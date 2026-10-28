using Demon.Slice.Domain;
using NUnit.Framework;

namespace Demon.Slice.Tests
{
    public sealed class RegionEntryLatchTests
    {
        [Test] public void DisabledDoesNotReportOrTrackEntries()
        {
            var latch = new RegionEntryLatch();
            Assert.IsFalse(latch.Sample(true)); Assert.IsFalse(latch.Inside);
        }
        [Test] public void EnterStayExitReenterReportsOnlyTwoEntries()
        {
            var latch = new RegionEntryLatch(); latch.SetEnabled(true, false);
            Assert.IsTrue(latch.Sample(true));
            for (int i = 0; i < 100; i++) Assert.IsFalse(latch.Sample(true));
            Assert.IsFalse(latch.Sample(false)); Assert.IsTrue(latch.Sample(true));
        }
        [Test] public void EnablingInsideEstablishesBaselineWithoutSyntheticEntry()
        {
            var latch = new RegionEntryLatch(); latch.SetEnabled(true, true);
            Assert.IsFalse(latch.Sample(true)); Assert.IsFalse(latch.Sample(false)); Assert.IsTrue(latch.Sample(true));
        }
        [Test] public void RepeatedEnableDoesNotSwallowRealEntry()
        {
            var latch = new RegionEntryLatch(); latch.SetEnabled(true, false);
            latch.SetEnabled(true, true); Assert.IsTrue(latch.Sample(true));
        }
        [Test] public void DisableAndReenableInsideWaitsForNewEntry()
        {
            var latch = new RegionEntryLatch(); latch.SetEnabled(true, false); Assert.IsTrue(latch.Sample(true));
            latch.SetEnabled(false, false); Assert.IsFalse(latch.Sample(true));
            latch.SetEnabled(true, true); Assert.IsFalse(latch.Sample(true));
            latch.Sample(false); Assert.IsTrue(latch.Sample(true));
        }
        [Test] public void SeparateRegionsDoNotConsumeEachOthersEvents()
        {
            var a = new RegionEntryLatch(); var b = new RegionEntryLatch();
            a.SetEnabled(true, false); b.SetEnabled(true, false);
            Assert.IsTrue(a.Sample(true)); Assert.IsTrue(b.Sample(true));
            Assert.IsFalse(a.Sample(true)); b.Sample(false); Assert.IsTrue(b.Sample(true));
        }
        [Test] public void ListeningAndFinalBoardSnapshotsRemainIndependent()
        {
            RuneKind[] initial = { RuneKind.On, RuneKind.Enter, RuneKind.Demon, RuneKind.None, RuneKind.None, RuneKind.None, RuneKind.Region, RuneKind.None, RuneKind.None };
            RuneKind[] target = (RuneKind[])initial.Clone(); target[4] = RuneKind.Ring; target[7] = RuneKind.Chime;
            var listening = new RuneBoardModel(initial, target, new[] { RuneKind.Ring, RuneKind.Chime });
            var final = new RuneBoardModel(new RuneKind[9], new RuneKind[9], null);
            listening.PickFromTray(RuneKind.Ring); listening.ClickSlot(4);
            listening.PickFromTray(RuneKind.Chime); listening.ClickSlot(7);
            Assert.IsTrue(listening.TryFreeze()); listening.ClickSlot(4);
            Assert.AreEqual(RuneKind.Ring, listening.CompiledAt(4)); Assert.IsFalse(final.Frozen);
            Assert.AreEqual(RuneKind.None, initial[4]);
        }
    }
}
