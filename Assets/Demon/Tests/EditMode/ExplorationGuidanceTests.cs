using System;
using Demon.Slice.Domain;
using NUnit.Framework;

namespace Demon.Slice.Tests
{
    public sealed class ExplorationGuidanceTests
    {
        [Test] public void DefaultRulesPreserveEveryOriginalDateAndPriorityCombination()
        {
            var rules = ExplorationGuidance.DefaultRules();
            ExplorationGuidance.Validate(rules);
            for (int day = 1; day <= 9; day++)
                for (int bits = 0; bits < 4096; bits++)
                {
                    var s = new ExplorationGuideState { Day = day,
                        FirstFound = (bits & 1) != 0, SecondFound = (bits & 2) != 0, ThirdFound = (bits & 4) != 0,
                        FourthFound = (bits & 8) != 0, FifthFound = (bits & 16) != 0, HasPreparation = (bits & 32) != 0,
                        SolarRead = (bits & 64) != 0, HandoverRead = (bits & 128) != 0, ToolAcquired = (bits & 256) != 0,
                        BeamLocked = (bits & 512) != 0, BeamReleased = (bits & 1024) != 0, ListeningActive = (bits & 2048) != 0 };
                    Assert.That(ExplorationGuidance.Select(rules, s), Is.EqualTo(Legacy(s)), "D" + day + " flags=" + bits);
                }
        }
        // Characterization of the pre-refactor flow, including its old standalone-scene fallback.
        static ExplorationGuideKey Legacy(ExplorationGuideState s)
        {
            if (s.Day == 1) return ExplorationGuideKey.Day1;
            if (s.Day == 2) return s.FirstFound ? ExplorationGuideKey.Found : ExplorationGuideKey.Day2;
            if (s.Day == 3) return s.SecondFound ? ExplorationGuideKey.Day3Found : ExplorationGuideKey.Day3;
            if (s.Day == 4) return s.ThirdFound ? ExplorationGuideKey.Day4Found : ExplorationGuideKey.Day4;
            if (s.Day == 5) return s.FourthFound ? ExplorationGuideKey.Day5Found : ExplorationGuideKey.Day5;
            if (s.HasPreparation && s.Day == 7)
            {
                if (!s.SolarRead) return ExplorationGuideKey.Day7;
                if (!s.ToolAcquired) return ExplorationGuideKey.Day7Cipher;
                if (s.BeamLocked) return ExplorationGuideKey.Day7Locked;
                return s.BeamReleased ? ExplorationGuideKey.Day7Ready : ExplorationGuideKey.Day7Beam;
            }
            if (s.HasPreparation && s.Day == 8)
            {
                if (!s.HandoverRead) return ExplorationGuideKey.Day8;
                if (!s.ToolAcquired) return ExplorationGuideKey.Day8Tool;
                return !s.ListeningActive ? ExplorationGuideKey.Day8Listening : ExplorationGuideKey.Day8Ready;
            }
            return s.ListeningActive ? ExplorationGuideKey.Day6Active : s.FifthFound ? ExplorationGuideKey.Day6Found : ExplorationGuideKey.Day6;
        }
        [Test] public void AuthoredOrderAndDayOverrideRequireNoGameplayMutation()
        {
            var rules = new[] {
                new ExplorationGuideRule(4, ExplorationGuideCondition.ToolMissing, ExplorationGuideKey.Day7Cipher),
                new ExplorationGuideRule(0, ExplorationGuideCondition.Always, ExplorationGuideKey.Day4) };
            ExplorationGuidance.Validate(rules);
            var s = new ExplorationGuideState { Day = 4 };
            Assert.That(ExplorationGuidance.Select(rules, s), Is.EqualTo(ExplorationGuideKey.Day7Cipher));
            s.ToolAcquired = true;
            Assert.That(ExplorationGuidance.Select(rules, s), Is.EqualTo(ExplorationGuideKey.Day4));
            Assert.That(s.Day, Is.EqualTo(4));
        }
        [Test] public void InvalidRulesFailBeforePresentationStarts()
        {
            Assert.Throws<ArgumentException>(() => ExplorationGuidance.Validate(Array.Empty<ExplorationGuideRule>()));
            Assert.Throws<ArgumentException>(() => ExplorationGuidance.Validate(new[] { new ExplorationGuideRule(10, ExplorationGuideCondition.Always, ExplorationGuideKey.Day1) }));
            Assert.Throws<ArgumentException>(() => ExplorationGuidance.Validate(new[] { new ExplorationGuideRule(1, ExplorationGuideCondition.Always, ExplorationGuideKey.Day1) }));
            var rules = ExplorationGuidance.DefaultRules(); rules[0].Condition = (ExplorationGuideCondition)999;
            Assert.Throws<ArgumentException>(() => ExplorationGuidance.Validate(rules));
        }
    }
}
