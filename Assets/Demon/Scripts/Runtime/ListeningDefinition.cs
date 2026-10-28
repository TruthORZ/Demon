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
        [TextArea(3, 8)] public string Instructions = "T05 · 风铃监听\n中心放 RING，下中放 CHIME，Enter 固化。\nON ENTER：进入区域才触发，停留不会不断响。\n听音色辨认区域；听不到不代表安全。";
        [TextArea(2, 5)] public string ActivatedText = "监听已启用，日期没有变化。\n沿右后方雪径观察雪痕经过两处风铃，再绕到木墙后听。\n风铃响度取决于你离风铃多远，沉默不代表安全。";
    }
}
