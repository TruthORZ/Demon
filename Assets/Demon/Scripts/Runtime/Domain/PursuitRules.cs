using System;
namespace Demon.Slice.Domain
{
    public sealed class ClosePassGate
    {
        public double Distance { get; }
        public double CooldownSeconds { get; }
        public double Remaining { get; private set; }
        public int PassCount { get; private set; }
        public ClosePassGate(double distance, double cooldown)
        {
            if (!Valid(distance) || !Valid(cooldown)) throw new ArgumentOutOfRangeException(nameof(distance));
            Distance = distance; CooldownSeconds = cooldown;
        }
        static bool Valid(double value) => !double.IsNaN(value) && !double.IsInfinity(value) && value > 0;
        public void Advance(double seconds)
        {
            if (double.IsNaN(seconds) || double.IsInfinity(seconds) || seconds < 0) throw new ArgumentOutOfRangeException(nameof(seconds));
            Remaining = Math.Max(0, Remaining - seconds);
        }
        public bool TryPass(double distance, bool lineOfSight, bool hasMotion)
        {
            if (double.IsNaN(distance) || double.IsInfinity(distance) || distance < 0) throw new ArgumentOutOfRangeException(nameof(distance));
            if (distance > Distance || !lineOfSight || !hasMotion || Remaining > 0) return false;
            Remaining = CooldownSeconds; PassCount++; return true;
        }
    }
    public sealed class PickupAttempt
    {
        public double RequiredSeconds { get; }
        public double Elapsed { get; private set; }
        public bool Active { get; private set; }
        public bool Completed { get; private set; }
        public int InterruptCount { get; private set; }
        public double Progress => Completed ? 1 : Elapsed / RequiredSeconds;
        public PickupAttempt(double requiredSeconds)
        {
            if (double.IsNaN(requiredSeconds) || double.IsInfinity(requiredSeconds) || requiredSeconds <= 0) throw new ArgumentOutOfRangeException(nameof(requiredSeconds));
            RequiredSeconds = requiredSeconds;
        }
        public bool Begin()
        {
            if (Completed || Active) return false;
            Active = true; Elapsed = 0; return true;
        }
        public bool Advance(double seconds, bool held)
        {
            if (double.IsNaN(seconds) || double.IsInfinity(seconds) || seconds < 0) throw new ArgumentOutOfRangeException(nameof(seconds));
            if (!Active || Completed) return false;
            if (!held) { Cancel(false); return false; }
            Elapsed = Math.Min(RequiredSeconds, Elapsed + seconds);
            if (Elapsed + .000001 < RequiredSeconds) return false;
            Completed = true; Active = false; return true;
        }
        public bool Cancel(bool interrupted)
        {
            if (Completed || !Active) return false;
            if (interrupted) InterruptCount++;
            Active = false; Elapsed = 0; return true;
        }
    }
}
