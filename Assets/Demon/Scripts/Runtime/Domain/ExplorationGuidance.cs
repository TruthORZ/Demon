using System;
using System.Collections.Generic;

namespace Demon.Slice.Domain
{
    public enum ExplorationGuideKey
    {
        Day1, Day2, Found, Day3, Day3Found, Day4, Day4Found, Day5, Day5Found,
        Day6, Day6Found, Day6Active, Day7, Day7Cipher, Day7Beam, Day7Locked,
        Day7Ready, Day8, Day8Tool, Day8Listening, Day8Ready
    }
    public enum ExplorationGuideCondition
    {
        Always, FirstFound, SecondFound, ThirdFound, FourthFound, FifthFound,
        ListeningActive, SolarUnread, HandoverUnread, ToolMissing, BeamLocked, BeamReleased, ListeningInactive
    }
    [Serializable]
    public sealed class ExplorationGuideRule
    {
        // Day 0 is the fallback; rules are checked in authored order. These rules only select text.
        public int Day;
        public bool RequiresPreparation;
        public ExplorationGuideCondition Condition;
        public ExplorationGuideKey Key;
        public ExplorationGuideRule(int day, ExplorationGuideCondition condition, ExplorationGuideKey key, bool preparation = false)
        { Day = day; Condition = condition; Key = key; RequiresPreparation = preparation; }
    }
    public struct ExplorationGuideState
    {
        public int Day;
        public bool HasPreparation, FirstFound, SecondFound, ThirdFound, FourthFound, FifthFound;
        public bool ListeningActive, SolarRead, HandoverRead, ToolAcquired, BeamLocked, BeamReleased;
    }
    public static class ExplorationGuidance
    {
        public static ExplorationGuideKey Select(IReadOnlyList<ExplorationGuideRule> rules, ExplorationGuideState state)
        {
            if (rules == null || rules.Count == 0) throw new ArgumentException("Exploration needs authored guide rules.");
            for (int i = 0; i < rules.Count; i++)
            {
                var rule = rules[i];
                if ((rule.Day == 0 || rule.Day == state.Day) && (!rule.RequiresPreparation || state.HasPreparation) && Matches(rule.Condition, state))
                    return rule.Key;
            }
            throw new InvalidOperationException("No exploration guide rule matched; author a fallback.");
        }
        public static void Validate(IReadOnlyList<ExplorationGuideRule> rules)
        {
            if (rules == null || rules.Count == 0) throw new ArgumentException("Exploration needs authored guide rules.");
            bool fallback = false;
            foreach (var rule in rules)
            {
                if (rule == null || rule.Day < 0 || rule.Day > 9 || !Enum.IsDefined(typeof(ExplorationGuideKey), rule.Key) || !Enum.IsDefined(typeof(ExplorationGuideCondition), rule.Condition))
                    throw new ArgumentException("Invalid exploration guide rule.");
                fallback |= rule.Day == 0 && !rule.RequiresPreparation && rule.Condition == ExplorationGuideCondition.Always;
            }
            if (!fallback) throw new ArgumentException("Author an unconditional Day 0 fallback.");
        }
        static bool Matches(ExplorationGuideCondition condition, ExplorationGuideState s)
        {
            switch (condition)
            {
                case ExplorationGuideCondition.Always: return true;
                case ExplorationGuideCondition.FirstFound: return s.FirstFound;
                case ExplorationGuideCondition.SecondFound: return s.SecondFound;
                case ExplorationGuideCondition.ThirdFound: return s.ThirdFound;
                case ExplorationGuideCondition.FourthFound: return s.FourthFound;
                case ExplorationGuideCondition.FifthFound: return s.FifthFound;
                case ExplorationGuideCondition.ListeningActive: return s.ListeningActive;
                case ExplorationGuideCondition.SolarUnread: return !s.SolarRead;
                case ExplorationGuideCondition.HandoverUnread: return !s.HandoverRead;
                case ExplorationGuideCondition.ToolMissing: return !s.ToolAcquired;
                case ExplorationGuideCondition.BeamLocked: return s.BeamLocked;
                case ExplorationGuideCondition.BeamReleased: return s.BeamReleased;
                case ExplorationGuideCondition.ListeningInactive: return !s.ListeningActive;
                default: throw new ArgumentOutOfRangeException(nameof(condition));
            }
        }
        public static ExplorationGuideRule[] DefaultRules() => new[]
        {
            new ExplorationGuideRule(1, ExplorationGuideCondition.Always, ExplorationGuideKey.Day1),
            new ExplorationGuideRule(2, ExplorationGuideCondition.FirstFound, ExplorationGuideKey.Found),
            new ExplorationGuideRule(2, ExplorationGuideCondition.Always, ExplorationGuideKey.Day2),
            new ExplorationGuideRule(3, ExplorationGuideCondition.SecondFound, ExplorationGuideKey.Day3Found),
            new ExplorationGuideRule(3, ExplorationGuideCondition.Always, ExplorationGuideKey.Day3),
            new ExplorationGuideRule(4, ExplorationGuideCondition.ThirdFound, ExplorationGuideKey.Day4Found),
            new ExplorationGuideRule(4, ExplorationGuideCondition.Always, ExplorationGuideKey.Day4),
            new ExplorationGuideRule(5, ExplorationGuideCondition.FourthFound, ExplorationGuideKey.Day5Found),
            new ExplorationGuideRule(5, ExplorationGuideCondition.Always, ExplorationGuideKey.Day5),
            new ExplorationGuideRule(7, ExplorationGuideCondition.SolarUnread, ExplorationGuideKey.Day7, true),
            new ExplorationGuideRule(7, ExplorationGuideCondition.ToolMissing, ExplorationGuideKey.Day7Cipher, true),
            new ExplorationGuideRule(7, ExplorationGuideCondition.BeamLocked, ExplorationGuideKey.Day7Locked, true),
            new ExplorationGuideRule(7, ExplorationGuideCondition.BeamReleased, ExplorationGuideKey.Day7Ready, true),
            new ExplorationGuideRule(7, ExplorationGuideCondition.Always, ExplorationGuideKey.Day7Beam, true),
            new ExplorationGuideRule(8, ExplorationGuideCondition.HandoverUnread, ExplorationGuideKey.Day8, true),
            new ExplorationGuideRule(8, ExplorationGuideCondition.ToolMissing, ExplorationGuideKey.Day8Tool, true),
            new ExplorationGuideRule(8, ExplorationGuideCondition.ListeningInactive, ExplorationGuideKey.Day8Listening, true),
            new ExplorationGuideRule(8, ExplorationGuideCondition.Always, ExplorationGuideKey.Day8Ready, true),
            new ExplorationGuideRule(0, ExplorationGuideCondition.ListeningActive, ExplorationGuideKey.Day6Active),
            new ExplorationGuideRule(0, ExplorationGuideCondition.FifthFound, ExplorationGuideKey.Day6Found),
            new ExplorationGuideRule(0, ExplorationGuideCondition.Always, ExplorationGuideKey.Day6)
        };
    }
}
