using UnityEngine;

namespace Demon.Slice
{
    public sealed class NineDayTablet : Interactable
    {
        [SerializeField] NineDayFlow flow;
        [SerializeField] int index;
        [SerializeField] Transform closeView;
        [SerializeField] RuneTablet editableTablet;
        public void ConfigureEditable(RuneTablet tablet) => editableTablet = tablet;
        [SerializeField] bool collectPhysicalTablet;
        public bool CollectPhysicalTablet => collectPhysicalTablet;
        public Transform Observation => closeView;
        public int Index => index;
        public override string Prompt => editableTablet ? editableTablet.Prompt : (index == 4 ? "Examine listening tablet T" : "Examine tablet T") + (index + 1).ToString("00");
        public void Configure(NineDayFlow owner, int number, Transform anchor)
        { flow = owner; index = number; closeView = anchor; }
        public override void Use(PlayerController player)
        {
            if (editableTablet) editableTablet.Use(player);
            else flow.ReadTablet(index, closeView);
        }
    }
}
