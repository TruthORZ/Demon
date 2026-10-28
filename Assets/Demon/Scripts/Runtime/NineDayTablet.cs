using UnityEngine;

namespace Demon.Slice
{
    public sealed class NineDayTablet : SliceInteractable
    {
        [SerializeField] NineDayFlow flow;
        [SerializeField] int index;
        [SerializeField] Transform closeView;
        public int Index => index;
        public override string Prompt => (index == 4 ? "查看监听石板记录 T" : "查看已固化石板 T") + (index + 1).ToString("00");
        public void Configure(NineDayFlow owner, int number, Transform anchor)
        { flow = owner; index = number; closeView = anchor; }
        public override void Use(SlicePlayer player) => flow.ReadTablet(index, closeView);
    }
}
