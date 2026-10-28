using System;

namespace Demon.Slice.Domain
{
    public sealed class DiscoveryProgress
    {
        readonly int[] days;
        readonly bool[] found;
        public int FoundCount { get; private set; }
        public DiscoveryProgress(int[] discoveryDays)
        {
            if (discoveryDays == null || discoveryDays.Length != 6)
                throw new ArgumentException("Six fixed tablets are required.");
            days = (int[])discoveryDays.Clone();
            foreach (int day in days) if (day < 1) throw new ArgumentOutOfRangeException(nameof(discoveryDays));
            found = new bool[days.Length];
        }
        public bool Available(int index, int day) => day >= days[index];
        public bool Found(int index) => found[index];
        public bool Discover(int index, int day)
        {
            if (!Available(index, day) || found[index]) return false;
            found[index] = true; FoundCount++; return true;
        }
    }
}
