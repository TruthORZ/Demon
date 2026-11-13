using Demon.Slice.Domain;
using UnityEngine;

namespace Demon.Slice
{
    [CreateAssetMenu(menuName = "Demon/Listening Tablet Definition")]
    public sealed class ListeningDefinition : ScriptableObject
    {
        [Header("T05 teaching layout - authored placeholder, not a general interpreter")]
        public RuneKind[] Initial = { RuneKind.On, RuneKind.Enter, RuneKind.Demon,
            RuneKind.None, RuneKind.None, RuneKind.None, RuneKind.Region, RuneKind.None, RuneKind.None };
        public RuneKind[] Target = { RuneKind.On, RuneKind.Enter, RuneKind.Demon,
            RuneKind.None, RuneKind.Ring, RuneKind.None, RuneKind.Region, RuneKind.Chime, RuneKind.None };
        public RuneKind[] LooseRunes = { RuneKind.Ring, RuneKind.Chime, RuneKind.Open };
        [TextArea(3, 8)] public string Instructions = "Fit the missing stones. Press Enter to set them.";
        [TextArea(2, 5)] public string ActivatedText = "The bells are listening. Silence does not mean the woods are empty.";
    }
}
