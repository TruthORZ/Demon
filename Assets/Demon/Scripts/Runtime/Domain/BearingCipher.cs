using System;
namespace Demon.Slice.Domain
{
    public static class BearingCipher
    {
        // Ten equal sectors; north is zero, east increases clockwise. A half-sector rounds clockwise.
        public static int Sector(double east, double north)
        {
            if (double.IsNaN(east) || double.IsNaN(north) || double.IsInfinity(east) || double.IsInfinity(north) || east * east + north * north < .000001)
                throw new ArgumentException("A landmark needs a finite horizontal offset from the wheel.");
            double angle = (Math.Atan2(east, north) * 180 / Math.PI + 360) % 360;
            return (int)Math.Floor((angle + 18) / 36) % 10;
        }
        public static void ValidateDigits(int[] digits)
        {
            if (digits == null || digits.Length != 10) throw new ArgumentException("The wheel needs ten digits.");
            bool[] used = new bool[10];
            foreach (int digit in digits)
            {
                if (digit < 0 || digit > 9 || used[digit]) throw new ArgumentException("Wheel digits must be a permutation of 0 through 9.");
                used[digit] = true;
            }
        }
    }
    public sealed class ThreeDigitLock
    {
        readonly int[] answer;
        readonly int[] input = new int[3];
        public bool Unlocked { get; private set; }
        public bool Acquired { get; private set; }
        public ThreeDigitLock(int[] code)
        {
            if (code == null || code.Length != 3) throw new ArgumentException("A three-digit code is required.");
            foreach (int digit in code) if (digit < 0 || digit > 9) throw new ArgumentOutOfRangeException(nameof(code));
            answer = (int[])code.Clone();
        }
        public int At(int index) => input[index];
        public void Cycle(int index)
        {
            if (index < 0 || index > 2) throw new ArgumentOutOfRangeException(nameof(index));
            if (!Unlocked) input[index] = (input[index] + 1) % 10;
        }
        public bool TryUnlock()
        {
            if (Unlocked) return false;
            for (int i = 0; i < 3; i++) if (input[i] != answer[i]) return false;
            Unlocked = true; return true;
        }
        public bool TryAcquire()
        {
            if (!Unlocked || Acquired) return false;
            Acquired = true; return true;
        }
    }
}
