namespace Demon.Slice.Domain
{
    // Enabling establishes occupancy; it does not invent an ENTER event.
    public sealed class RegionEntryLatch
    {
        public bool Enabled { get; private set; }
        public bool Inside { get; private set; }
        public void SetEnabled(bool enabled, bool inside)
        {
            if (Enabled == enabled) return;
            Enabled = enabled;
            Inside = enabled && inside;
        }
        public bool Sample(bool inside)
        {
            if (!Enabled) return false;
            bool entered = inside && !Inside;
            Inside = inside;
            return entered;
        }
    }
}
