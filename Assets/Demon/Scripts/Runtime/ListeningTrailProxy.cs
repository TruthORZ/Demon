using System;
using UnityEngine;

namespace Demon.Slice
{
    // Explicit teaching proxy only. Actual pursuit will supply another monitored Transform.
    public sealed class ListeningTrailProxy : MonoBehaviour
    {
        [SerializeField] Transform target;
        [SerializeField] Transform[] waypoints;
        [SerializeField, Min(.1f)] float metresPerSecond = 2;
        [SerializeField, Min(0)] float pauseSeconds = 1.5f;
        [SerializeField] SliceSceneController world;
        [SerializeField] ListeningTool tool;
        [SerializeField] BeamTether tether;
        public void ConfigureTether(BeamTether beam) => tether = beam;
        int next;
        float remaining;
        public Transform Target => target;
        public float MetresPerSecond => metresPerSecond;
        public void Configure(Transform actor, Transform[] path, SliceSceneController controller, ListeningTool listener)
        { target = actor; waypoints = path; world = controller; tool = listener; }
        void Start()
        {
            if (!target || !world || !tool || waypoints == null || waypoints.Length < 2)
                throw new InvalidOperationException("Listening teaching proxy requires a saved target, route and composition.");
            foreach (var point in waypoints) if (!point) throw new InvalidOperationException("Missing listening proxy waypoint.");
        }
        void Update()
        {
            if (!world || world.Session == null || world.Session.Descended) return;
            if (tether ? !tether.ProxyCanTravel || !tether.DemonCanMove : !tool.Active) return;
            if (remaining > 0) { remaining -= Time.deltaTime; return; }
            target.position = Vector3.MoveTowards(target.position, waypoints[next].position, metresPerSecond * Time.deltaTime);
            if ((target.position - waypoints[next].position).sqrMagnitude < .0001f)
            { next = (next + 1) % waypoints.Length; remaining = pauseSeconds; }
        }
    }
}
