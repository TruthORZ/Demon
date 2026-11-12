using Demon.Slice;
using UnityEngine;

namespace StandaloneTerrainFootprints
{
    /// <summary>Receives real demon chase steps, synchronized with its snow-step audio.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TerrainFootprintEmitter))]
    public sealed class DemonFootprints : MonoBehaviour
    {
        [SerializeField] ChaseController chase;
        [SerializeField] TerrainFootprintEmitter emitter;
        [Tooltip("Ignore tiny movement between audible steps to avoid stacking prints at a stop.")]
        [SerializeField, Min(0)] float minimumTravel = 0.15f;
        bool nextLeft = true;
        bool hasPrevious;
        Vector3 previous;
        public ChaseController Chase => chase;
        public TerrainFootprintEmitter Emitter => emitter;

        public void Configure(ChaseController controller, TerrainFootprintEmitter output)
        { chase = controller; emitter = output; }

        void OnEnable()
        {
            if (!chase || !emitter || emitter.automaticSteps)
            {
                Debug.LogError("Demon footprints require Chase + Emitter references and Automatic Steps OFF (the chase owns step timing).", this);
                enabled = false;
                return;
            }
            chase.Stepped += OnStep;
            chase.PursuitReset += ResetTrail;
            ResetTrail();
        }

        void OnDisable()
        {
            if (!chase) return;
            chase.Stepped -= OnStep;
            chase.PursuitReset -= ResetTrail;
        }

        void ResetTrail()
        {
            hasPrevious = false;
            nextLeft = true;
            if (emitter) emitter.ClearFootprints();
        }

        void OnStep(Vector3 position, Vector3 forward)
        {
            if (!emitter.isActiveAndEnabled || !chase.HasNavigation || chase.Agent.isOnOffMeshLink) return;
            if (hasPrevious)
            {
                float travel = Vector3.Distance(previous, position);
                if (travel < minimumTravel) return;
                // A navigation warp must not leave a connecting trail or a spurious arrival stamp.
                if (travel > emitter.teleportDistance) { previous = position; return; }
            }
            previous = position; hasPrevious = true;
            forward = Vector3.ProjectOnPlane(forward, Vector3.up).normalized;
            if (forward.sqrMagnitude < 0.001f) return;
            Vector3 right = Vector3.Cross(Vector3.up, forward);
            if (emitter.StampAt(position + right * (nextLeft ? -emitter.lateralOffset : emitter.lateralOffset), forward, nextLeft))
                nextLeft = !nextLeft;
        }
    }
}
