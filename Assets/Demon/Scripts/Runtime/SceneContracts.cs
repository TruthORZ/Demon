using Demon.Slice.Domain;
using UnityEngine;

namespace Demon.Slice
{
    // Consumer-sized runtime contracts. Serialized scene references remain on the composition root.
    public interface ISceneFeedback
    {
        void SetMessage(string text);
        void ReadDocument(string text, Transform anchor);
    }

    public interface IPlayerSession
    {
        GameSession Session { get; }
        GameSettings Settings { get; }
        bool EndingPresented { get; }
        bool OnReadingClosed();
    }

    public interface IHudState
    {
        string Message { get; }
        string ReadingText { get; }
        bool EndingPresented { get; }
    }

    public interface ITabletProgram
    {
        RuneBoardModel Board { get; }
        bool BeginInteraction();
        bool TryCompile();
    }

    // Every close-up puzzle supports the same input operations; the player does not dispatch by puzzle type.
    public interface ICloseViewInteraction
    {
        bool Confirm();
        void Click(Collider target);
    }
}
