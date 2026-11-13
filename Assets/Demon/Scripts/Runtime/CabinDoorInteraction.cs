using UnityEngine;
using UnityEngine.Events;

namespace Demon.Slice
{
    public sealed class CabinDoorInteraction : Interactable
    {
        [SerializeField] UnityEvent toggle = new UnityEvent();
        [SerializeField] Behaviour doorwayNavigation;
        [SerializeField] bool excludeEnemyNavigation;
        [SerializeField] bool mainRouteDoor;
        [SerializeField] Vector3 mainRouteOpenEuler = new Vector3(0, 90, 0);
        [SerializeField, Min(.1f)] float mainRouteSpeed = 8;
        [SerializeField] AudioSource mainRouteAudio;
        [SerializeField] AudioClip openClip, closeClip;
        Quaternion closedRotation;
        bool initialized, opened;
        public struct State { public Quaternion rotation; public bool opened; }
        public State Capture() => new State { rotation = transform.localRotation, opened = opened };
        public void Restore(State state) { EnsureInitialized(); opened = state.opened; transform.localRotation = state.rotation; if (mainRouteAudio) mainRouteAudio.Stop(); }
        public void ResetForSession() { EnsureInitialized(); Restore(new State { rotation = closedRotation }); }
        void EnsureInitialized() { if (initialized) return; initialized = true; closedRotation = transform.localRotation; }

        public override string Prompt => "Open or close the cabin door";
        public UnityEvent Toggle => toggle;

        void Awake()
        {
            EnsureInitialized();
            if (doorwayNavigation) doorwayNavigation.enabled = false;
        }

        void LateUpdate()
        {
            if (mainRouteDoor)
                transform.localRotation = Quaternion.Slerp(transform.localRotation, closedRotation * Quaternion.Euler(opened ? mainRouteOpenEuler : Vector3.zero), 1 - Mathf.Exp(-Time.deltaTime * mainRouteSpeed));
            if (!doorwayNavigation) return;
            bool passable = !excludeEnemyNavigation && Quaternion.Angle(closedRotation, transform.localRotation) >= 65f;
            if (doorwayNavigation.enabled != passable) doorwayNavigation.enabled = passable;
        }

        public override void Use(PlayerController player)
        {
            if (!mainRouteDoor) { toggle.Invoke(); return; }
            opened = !opened;
            AudioClip clip = opened ? openClip : closeClip;
            if (mainRouteAudio && clip) mainRouteAudio.PlayOneShot(clip);
        }
    }
}
