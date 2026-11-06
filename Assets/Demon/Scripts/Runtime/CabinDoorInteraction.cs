using UnityEngine;
using UnityEngine.Events;

namespace Demon.Slice
{
    public sealed class CabinDoorInteraction : Interactable
    {
        [SerializeField] UnityEvent toggle = new UnityEvent();
        [SerializeField] Behaviour doorwayNavigation;
        Quaternion closedRotation;

        public override string Prompt => "开关木屋门";
        public UnityEvent Toggle => toggle;

        void Awake()
        {
            closedRotation = transform.localRotation;
            if (doorwayNavigation) doorwayNavigation.enabled = false;
        }

        void LateUpdate()
        {
            if (!doorwayNavigation) return;
            bool passable = Quaternion.Angle(closedRotation, transform.localRotation) >= 65f;
            if (doorwayNavigation.enabled != passable) doorwayNavigation.enabled = passable;
        }

        public override void Use(PlayerController player) => toggle.Invoke();
    }
}
