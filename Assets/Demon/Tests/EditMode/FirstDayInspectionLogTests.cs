using System;
using Demon.Slice.Domain;
using NUnit.Framework;
namespace Demon.Slice.Tests
{
    public sealed class FirstDayInspectionLogTests
    {
        [Test] public void FirstDayThreeDistinctSitesCountOnce()
        { var log = new FirstDayInspectionLog(); foreach (ObservationSite site in Enum.GetValues(typeof(ObservationSite))) { Assert.That(log.Record(1, site), Is.True); Assert.That(log.Record(1, site), Is.False); Assert.That(log.Contains(site), Is.True); } Assert.That(log.Count, Is.EqualTo(3)); }
        [TestCase(2)] [TestCase(9)] public void SkippedFirstDayCannotBeBackfilled(int day)
        { var log = new FirstDayInspectionLog(); Assert.That(log.Record(day, ObservationSite.Radio), Is.False); Assert.That(log.Count, Is.Zero); }
        [Test] public void LaterObservationsPreserveExistingFirstDayRecord()
        { var log = new FirstDayInspectionLog(); log.Record(1, ObservationSite.Lake); log.Record(2, ObservationSite.Radio); Assert.That(log.Contains(ObservationSite.Lake), Is.True); Assert.That(log.Contains(ObservationSite.Radio), Is.False); Assert.That(log.Count, Is.EqualTo(1)); }
        [Test] public void InvalidSiteAndDateDoNotMutateLog()
        { var log = new FirstDayInspectionLog(); Assert.Throws<ArgumentOutOfRangeException>(() => log.Record(0, ObservationSite.Radio)); Assert.Throws<ArgumentOutOfRangeException>(() => log.Record(1, (ObservationSite)3)); Assert.That(log.Count, Is.Zero); }
    }
}
