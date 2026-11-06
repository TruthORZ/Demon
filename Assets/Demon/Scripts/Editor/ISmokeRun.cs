namespace Demon.Slice.Editor
{
    // One lifecycle for every bounded Editor verification, including interruption cleanup.
    public interface ISmokeRun
    {
        void Tick();
        void Cleanup();
    }
}
