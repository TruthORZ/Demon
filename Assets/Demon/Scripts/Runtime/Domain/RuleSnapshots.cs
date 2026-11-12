using System;
using System.Collections.Generic;

namespace Demon.Slice.Domain
{
    // Explicit session snapshots. Copy mutable arrays/collections in both directions;
    // no scene references, reflection, synthetic completion or backwards clock writes.
    public sealed partial class RuneBoardModel
    {
        public sealed class Snapshot
        {
            internal RuneKind[] slots, target, compiled;
            internal Dictionary<RuneKind, int> supply;
            internal RuneKind held;
        }
        public Snapshot Capture() => new Snapshot { slots = (RuneKind[])slots.Clone(), target = (RuneKind[])target.Clone(),
            compiled = compiled == null ? null : (RuneKind[])compiled.Clone(), held = Held, supply = new Dictionary<RuneKind, int>(supply) };
        public void Restore(Snapshot state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            for (int i = 0; i < target.Length; i++) if (target[i] != state.target[i]) throw new InvalidOperationException("Board definition changed during session.");
            Array.Copy(state.slots, slots, slots.Length); supply.Clear();
            foreach (var pair in state.supply) supply.Add(pair.Key, pair.Value);
            Held = state.held; compiled = state.compiled == null ? null : (RuneKind[])state.compiled.Clone(); Changed?.Invoke();
        }
    }
    public sealed partial class GameSession
    {
        public sealed class Snapshot
        {
            internal int day, finalDay, earliest, box, lake, count;
            internal double minute;
            internal bool body, descended, pending;
            internal int[] exchanged;
            internal RuneBoardModel.Snapshot board;
        }
        public Snapshot Capture()
        {
            var days = new int[exchangedDays.Count]; exchangedDays.CopyTo(days);
            return new Snapshot { day = Clock.Day, minute = Clock.MinuteOfDay, finalDay = finalDay, earliest = earliestCompileDay,
                box = BoxSeal, lake = LakeSeal, count = ExchangeCount, body = HasBody, descended = Descended,
                pending = LoopPending, exchanged = days, board = Board.Capture() };
        }
        public static GameSession FromSnapshot(Snapshot state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            var board = new RuneBoardModel(state.board.slots, state.board.target, null); board.Restore(state.board);
            var model = new GameSession(state.day, state.minute, state.finalDay, board, state.lake == 1, state.earliest);
            model.BoxSeal = state.box; model.LakeSeal = state.lake; model.ExchangeCount = state.count;
            model.HasBody = state.body; model.Descended = state.descended; model.LoopPending = state.pending;
            model.exchangedDays.Clear(); foreach (int day in state.exchanged) model.exchangedDays.Add(day);
            if (state.descended) model.Clock.Stop();
            return model;
        }
    }
    public sealed partial class DiscoveryProgress
    {
        public bool[] Capture() => (bool[])found.Clone();
        public void Restore(bool[] state)
        { if (state == null || state.Length != found.Length) throw new ArgumentException("Invalid discoveries."); Array.Copy(state, found, found.Length); FoundCount = 0; foreach (bool item in found) if (item) FoundCount++; }
    }
    public sealed partial class ThreeDigitLock
    {
        public sealed class Snapshot { internal int[] input; internal bool unlocked, acquired; }
        public Snapshot Capture() => new Snapshot { input = (int[])input.Clone(), unlocked = Unlocked, acquired = Acquired };
        public void Restore(Snapshot state) { Array.Copy(state.input, input, 3); Unlocked = state.unlocked; Acquired = state.acquired; }
    }
    public sealed partial class StableBeamMotion
    {
        public sealed class Snapshot { internal double stable; internal bool locked; internal int demon, vane, count; }
        public Snapshot Capture() => new Snapshot { stable = StableSeconds, locked = Locked, demon = DemonMotion, vane = VaneMotion, count = ExchangeCount };
        public void Restore(Snapshot state) { StableSeconds = state.stable; Locked = state.locked; DemonMotion = state.demon; VaneMotion = state.vane; ExchangeCount = state.count; }
    }
    public sealed partial class FinalTabletProgress
    {
        public void Restore(bool acquired, bool placed)
        { if (placed && !acquired) throw new ArgumentException("Placed T07 must be acquired."); Acquired = acquired; Placed = placed; }
    }
    public sealed partial class ClosePassGate
    {
        public void Restore(double remaining, int count) { Remaining = remaining; PassCount = count; }
    }
    public sealed partial class PickupAttempt
    {
        public void Restore(bool complete, int interruptions)
        { Active = false; Completed = complete; Elapsed = complete ? RequiredSeconds : 0; InterruptCount = interruptions; }
    }
}
