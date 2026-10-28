using System;

namespace Demon.Slice.Domain
{
    // An authored pacing window, not an operation performed by the rune program.
    // Only normal frame advancement is limited; an explicit sleep still crosses midnight.
    public sealed class FinaleTimeWindow
    {
        public double Midnight { get; }
        public double HoldAt { get; }
        public bool Reserved { get; private set; }

        public FinaleTimeWindow(double midnight, double minutesToReserve)
        {
            if (double.IsNaN(midnight) || double.IsInfinity(midnight) || midnight <= 0 ||
                double.IsNaN(minutesToReserve) || double.IsInfinity(minutesToReserve) ||
                minutesToReserve <= 0 || minutesToReserve >= GameClock.MinutesPerDay)
                throw new ArgumentOutOfRangeException(nameof(minutesToReserve));
            Midnight = midnight;
            HoldAt = midnight - minutesToReserve;
        }

        public bool Reserve(double now)
        {
            if (now >= Midnight) return false;
            Reserved = true;
            return true;
        }

        public double Limit(double now, double requested)
        {
            if (double.IsNaN(requested) || double.IsInfinity(requested) || requested < now)
                throw new ArgumentOutOfRangeException(nameof(requested));
            return Reserved ? Math.Max(now, Math.Min(requested, HoldAt)) : requested;
        }

        public double AlignForCountdown(double now)
        {
            if (!Reserved || now >= Midnight) throw new InvalidOperationException("No reserved midnight remains.");
            return Math.Max(now, HoldAt);
        }

        public void Release() => Reserved = false;
    }
}
