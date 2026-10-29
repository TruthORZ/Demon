using System;
using Demon.Slice.Domain;
using NUnit.Framework;
namespace Demon.Slice.Tests
{
    public sealed class ScheduledWorldAttributesTests
    {
        [Test] public void BeforeSoundDateKeepsRadioAndSun()
        { var rule = new ScheduledWorldAttributes(5, 7); rule.AdvanceToDay(4); Assert.That((rule.RadioSound, rule.DemonSound, rule.SunLight, rule.DemonLight), Is.EqualTo((1, 0, 1, 0))); Assert.That((rule.SoundExchangeCount, rule.LightTransferCount), Is.EqualTo((0, 0))); }
        [Test] public void SoundSwapsOnceAndDoesNotGrantLightEarly()
        { var rule = new ScheduledWorldAttributes(5, 7); rule.AdvanceToDay(5); for (int i = 0; i < 100; i++) rule.AdvanceToDay(6); Assert.That((rule.RadioSound, rule.DemonSound, rule.SunLight, rule.DemonLight), Is.EqualTo((0, 1, 1, 0))); Assert.That((rule.SoundExchangeCount, rule.LightTransferCount), Is.EqualTo((1, 0))); }
        [TestCase(7)] [TestCase(8)] [TestCase(9)] public void LateStartCatchesUpBothProgramsExactlyOnce(int day)
        { var rule = new ScheduledWorldAttributes(5, 7); rule.AdvanceToDay(day); rule.AdvanceToDay(day); Assert.That((rule.RadioSound, rule.DemonSound, rule.SunLight, rule.DemonLight), Is.EqualTo((0, 1, 0, 1))); Assert.That((rule.SoundExchangeCount, rule.LightTransferCount), Is.EqualTo((1, 1))); }
        [Test] public void MidnightSequenceAndSkippedDatesAgree()
        { var gradual = new ScheduledWorldAttributes(5, 7); for (int day = 1; day <= 9; day++) gradual.AdvanceToDay(day); var skipped = new ScheduledWorldAttributes(5, 7); skipped.AdvanceToDay(9); Assert.That((gradual.RadioSound, gradual.DemonSound, gradual.SunLight, gradual.DemonLight), Is.EqualTo((skipped.RadioSound, skipped.DemonSound, skipped.SunLight, skipped.DemonLight))); Assert.That((gradual.SoundExchangeCount, gradual.LightTransferCount), Is.EqualTo((1, 1))); }
        [Test] public void UsesAuthoredDatesAndRejectsReverseTime()
        { var rule = new ScheduledWorldAttributes(4, 6); rule.AdvanceToDay(4); Assert.That(rule.DemonSound, Is.EqualTo(1)); Assert.That(rule.DemonLight, Is.Zero); rule.AdvanceToDay(6); Assert.That(rule.DemonLight, Is.EqualTo(1)); Assert.Throws<ArgumentOutOfRangeException>(() => rule.AdvanceToDay(5)); Assert.Throws<ArgumentOutOfRangeException>(() => new ScheduledWorldAttributes(0, 7)); }
    }
}
