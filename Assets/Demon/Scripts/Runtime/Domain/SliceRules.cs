using System;
using System.Collections.Generic;

namespace Demon.Slice.Domain
{
    // Value 5 is reserved for the retired time rune. Preserve serialized values of SEAL/BOX/LAKE.
    public enum RuneKind { None = 0, Nine = 1, GreaterEqual = 2, Day = 3, Exchange = 4,
        Seal = 6, Box = 7, Lake = 8, Open = 9 }

    public static class AttributeOperations
    {
        public static bool TryExchange(int a, int capacityA, int b, int capacityB, out int nextA, out int nextB)
        {
            nextA = a;
            nextB = b;
            if (a < 0 || b < 0 || a > capacityA || b > capacityB || b > capacityA || a > capacityB)
                return false;
            nextA = b;
            nextB = a;
            return true;
        }

        public static bool TryTransfer(int source, int destination, int capacity, out int nextSource, out int nextDestination)
        {
            nextSource = source;
            nextDestination = destination;
            if (source < 1 || destination < 0 || destination >= capacity)
                return false;
            nextSource = source - 1;
            nextDestination = destination + 1;
            return true;
        }
    }

    public sealed class GameClock
    {
        public const int MinutesPerDay = 1440;
        public event Action<int> Midnight;
        public double AbsoluteMinutes { get; private set; }
        public int Day => (int)Math.Floor(AbsoluteMinutes / MinutesPerDay) + 1;
        public double MinuteOfDay => AbsoluteMinutes - (Day - 1) * MinutesPerDay;
        public double NextMidnight => Day * (double)MinutesPerDay;
        public bool Stopped { get; private set; }

        public GameClock(int day, double minute)
        {
            if (day < 1 || minute < 0 || minute >= MinutesPerDay)
                throw new ArgumentOutOfRangeException(nameof(day));
            AbsoluteMinutes = (day - 1) * MinutesPerDay + minute;
        }

        public void AdvanceBy(double minutes) => AdvanceTo(AbsoluteMinutes + minutes);

        public void AdvanceTo(double target)
        {
            if (double.IsNaN(target) || double.IsInfinity(target) || target < AbsoluteMinutes)
                throw new ArgumentOutOfRangeException(nameof(target));
            if (Stopped) return;
            while (NextMidnight <= target)
            {
                AbsoluteMinutes = NextMidnight;
                Midnight?.Invoke(Day);
                // A midnight listener may end the game. Never continue a sleep/loop jump into D10.
                if (Stopped) return;
            }
            AbsoluteMinutes = target;
        }

        public void Stop() => Stopped = true;
    }

    public sealed class RuneBoardModel
    {
        readonly RuneKind[] slots;
        readonly RuneKind[] target;
        readonly Dictionary<RuneKind, int> supply = new Dictionary<RuneKind, int>();
        RuneKind[] compiled;
        public event Action Changed;
        public RuneKind Held { get; private set; }
        public bool Frozen => compiled != null;
        public int Count => slots.Length;

        public RuneBoardModel(RuneKind[] initial, RuneKind[] expected, RuneKind[] loose)
        {
            if (initial == null || expected == null || initial.Length != 9 || expected.Length != 9)
                throw new ArgumentException("The authored tablet must have nine slots.");
            slots = (RuneKind[])initial.Clone();
            target = (RuneKind[])expected.Clone();
            foreach (var rune in loose ?? Array.Empty<RuneKind>())
                Return(rune);
        }

        public RuneKind At(int slot) => slots[slot];
        public RuneKind CompiledAt(int slot) => compiled == null ? RuneKind.None : compiled[slot];
        public int Available(RuneKind rune) => supply.TryGetValue(rune, out int count) ? count : 0;

        void Return(RuneKind rune)
        {
            if (rune == RuneKind.None) return;
            supply[rune] = Available(rune) + 1;
        }

        public bool PickFromTray(RuneKind rune)
        {
            if (Frozen || Available(rune) < 1) return false;
            Return(Held);
            Held = rune;
            supply[rune]--;
            Changed?.Invoke();
            return true;
        }

        public bool ClickSlot(int index)
        {
            if (index < 0 || index >= slots.Length) return false;
            if (Frozen)
            {
                // Surface runes can be removed, but the immutable compiled snapshot remains.
                if (slots[index] == RuneKind.None) return false;
                Return(slots[index]);
                slots[index] = RuneKind.None;
            }
            else
            {
                RuneKind displaced = slots[index];
                slots[index] = Held;
                Held = displaced;
            }
            Changed?.Invoke();
            return true;
        }

        public bool MatchesTarget()
        {
            for (int i = 0; i < slots.Length; i++)
                if (slots[i] != target[i]) return false;
            return true;
        }

        public bool TryFreeze()
        {
            if (Frozen || !MatchesTarget()) return false;
            compiled = (RuneKind[])slots.Clone();
            Return(Held);
            Held = RuneKind.None;
            Changed?.Invoke();
            return true;
        }
    }

    public sealed class SliceSession : IDisposable
    {
        readonly HashSet<int> exchangedDays = new HashSet<int>();
        readonly int finalDay;
        readonly int earliestCompileDay;
        public event Action Changed;
        public GameClock Clock { get; }
        public RuneBoardModel Board { get; }
        public int BoxSeal { get; private set; } = 1;
        public int LakeSeal { get; private set; }
        public bool HasBody { get; private set; }
        public bool Descended { get; private set; }
        public bool LoopPending { get; private set; }
        public int ExchangeCount { get; private set; }

        public SliceSession(int startDay, double minute, int finalDay, RuneBoardModel board,
            bool initialLakeSealed = false, int earliestCompileDay = 1)
        {
            this.finalDay = finalDay;
            this.earliestCompileDay = earliestCompileDay;
            LakeSeal = initialLakeSealed ? 1 : 0;
            Board = board ?? throw new ArgumentNullException(nameof(board));
            Clock = new GameClock(startDay, minute);
            Clock.Midnight += OnMidnight;
            if (startDay >= finalDay)
            {
                HasBody = true;
                CheckDescent();
            }
        }

        public bool TryCompile()
        {
            if (Descended || Clock.Day < earliestCompileDay || Clock.Day > finalDay || !Board.TryFreeze()) return false;
            LoopPending = true;
            ExchangeToday();
            return true;
        }

        void OnMidnight(int day)
        {
            // T04 is represented by a scheduled state in this slice, not a newly invented body source.
            if (day >= finalDay) HasBody = true;
            if (LoopPending && Board.Frozen && day <= finalDay)
                ExchangeToday();
            else
                CheckDescent();
            if (day > finalDay) LoopPending = false;
            Changed?.Invoke();
        }

        public void OpenLake()
        {
            if (Descended || LakeSeal == 0) return;
            LakeSeal = 0;
            CheckDescent();
            Changed?.Invoke();
        }

        void ExchangeToday()
        {
            if (Descended || !exchangedDays.Add(Clock.Day)) return;
            if (!AttributeOperations.TryExchange(BoxSeal, 1, LakeSeal, 1, out int box, out int lake))
                throw new InvalidOperationException("Authored SEAL state exceeded capacity.");
            BoxSeal = box;
            LakeSeal = lake;
            ExchangeCount++;
            CheckDescent();
            Changed?.Invoke();
        }

        void CheckDescent()
        {
            if (Descended || !HasBody || LakeSeal != 0) return;
            Descended = true;
            LoopPending = false;
            Clock.Stop();
        }

        public void SleepToNextDay(double wakeMinute)
        {
            if (wakeMinute < 0 || wakeMinute >= GameClock.MinutesPerDay)
                throw new ArgumentOutOfRangeException(nameof(wakeMinute));
            Clock.AdvanceTo(Clock.NextMidnight + wakeMinute);
        }

        public void Dispose() => Clock.Midnight -= OnMidnight;
    }
}
