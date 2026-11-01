using System;
namespace Demon.Slice.Domain
{
    // Post-D3 MOTION pair. Aim changes trigger an exchange, never a per-frame swap.
    public sealed class StableBeamMotion
    {
        public double RequiredSeconds { get; }
        public double StableSeconds { get; private set; }
        public bool Locked { get; private set; }
        public int DemonMotion { get; private set; } = 1;
        public int VaneMotion { get; private set; }
        public int ExchangeCount { get; private set; }
        public StableBeamMotion(double requiredSeconds)
        {
            if (double.IsNaN(requiredSeconds) || double.IsInfinity(requiredSeconds) || requiredSeconds <= 0)
                throw new ArgumentOutOfRangeException(nameof(requiredSeconds));
            RequiredSeconds = requiredSeconds;
        }
        public void Advance(bool eligible, double seconds)
        {
            if (double.IsNaN(seconds) || double.IsInfinity(seconds) || seconds < 0) throw new ArgumentOutOfRangeException(nameof(seconds));
            if (!eligible) { Release(); return; }
            if (Locked) return;
            StableSeconds = Math.Min(RequiredSeconds, StableSeconds + seconds);
            if (StableSeconds + .000001 < RequiredSeconds) return;
            Swap(); Locked = true;
        }
        public void Release()
        {
            StableSeconds = 0;
            if (!Locked) return;
            Swap(); Locked = false;
        }
        void Swap()
        {
            if (!AttributeOperations.TryExchange(DemonMotion, 1, VaneMotion, 1, out int demon, out int vane))
                throw new InvalidOperationException("MOTION pair exceeded authored capacity.");
            DemonMotion = demon; VaneMotion = vane; ExchangeCount++;
        }
    }
}
