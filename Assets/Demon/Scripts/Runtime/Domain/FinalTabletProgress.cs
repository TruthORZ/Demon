namespace Demon.Slice.Domain
{
    // Session-local acquisition and placement; never owns time, rune compilation or SEAL.
    public sealed class FinalTabletProgress
    {
        public bool Acquired { get; private set; }
        public bool Placed { get; private set; }
        public bool Collect() { if (Acquired) return false; Acquired = true; return true; }
        public bool Place() { if (!Acquired || Placed) return false; Placed = true; return true; }
    }
}
