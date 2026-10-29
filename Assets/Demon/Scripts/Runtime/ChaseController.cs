using System;
using Demon.Slice.Domain;
using UnityEngine;
using UnityEngine.AI;
namespace Demon.Slice
{
    public sealed class ChaseController : MonoBehaviour
    {
        public enum ChasePhase { Dormant, Pursuing, Frozen, Retreating, Waiting, Ended }
        [SerializeField] ChaseDefinition definition;
        [SerializeField] NavMeshAgent agent;
        [SerializeField] Transform[] retreatPoints;
        [SerializeField] AudioSource footstepSource;
        [SerializeField] AudioClip footstepClip;
        readonly RaycastHit[] hits = new RaycastHit[64];
        NavMeshPath candidatePath;
        SliceSceneController world;
        float replanAt, stepAt, missingNavSeconds;
        bool retreating;
        public ClosePassGate Gate { get; private set; }
        public ChasePhase Phase { get; private set; }
        public NavMeshAgent Agent => agent;
        public ChaseDefinition Definition => definition;
        public bool HasNavigation => agent && agent.isActiveAndEnabled && agent.isOnNavMesh;
        public void Configure(ChaseDefinition data, NavMeshAgent actor, Transform[] points, AudioSource audio, AudioClip clip)
        { definition = data; agent = actor; retreatPoints = points; footstepSource = audio; footstepClip = clip; }
        public void Initialize(SliceSceneController controller)
        {
            if (!definition || !agent || retreatPoints == null || retreatPoints.Length < 2 || !footstepSource || !footstepClip || !controller.Tether)
                throw new InvalidOperationException("Chase requires saved settings, agent, retreat points, footsteps and T06 motion owner.");
            foreach (var point in retreatPoints) if (!point) throw new InvalidOperationException("Missing retreat point.");
            candidatePath = new NavMeshPath(); // Native navigation objects require the main-thread initialization phase.
            world = controller; Gate = new ClosePassGate(definition.ClosePassMetres, definition.ClosePassCooldownSeconds);
            agent.speed = definition.MetresPerSecond; agent.acceleration = definition.AccelerationMetresPerSecondSquared; agent.angularSpeed = definition.AngularDegreesPerSecond;
        }
        void Update()
        {
            if (Gate == null) return;
            Gate.Advance(Time.deltaTime);
            if (world.Session.Descended) { Phase = ChasePhase.Ended; Stop(); return; }
            if (!world.Tether.ProxyCanTravel || world.Session.LakeSeal != 0) { Phase = ChasePhase.Dormant; Stop(); return; }
            if (!agent.isActiveAndEnabled) return; // T06/date presentation enables the authored target root.
            if (!agent.isOnNavMesh)
            {
                if (NavMesh.SamplePosition(agent.transform.position, out var start, 2, agent.areaMask)) agent.Warp(start.position);
                if (!agent.isOnNavMesh)
                {
                    missingNavSeconds += Time.deltaTime;
                    if (missingNavSeconds > 1) { Debug.LogError("Authored chase agent cannot bind to the saved NavMesh. Re-bake the scene surface.", this); enabled = false; }
                    return;
                }
            }
            missingNavSeconds = 0;
            if (!world.Tether.DemonCanMove) { Phase = ChasePhase.Frozen; Stop(); return; }
            agent.isStopped = false;
            float distance = Vector3.Distance(agent.transform.position, world.Player.transform.position);
            if (Gate.TryPass(distance, HasSightToPlayer(), true))
            {
                bool interrupted = world.Player.InterruptPickup();
                world.SetMessage(interrupted ? definition.InterruptedMessage : definition.NearPassMessage);
                retreating = ChooseRetreat(); replanAt = 0;
            }
            if (retreating)
            {
                Phase = ChasePhase.Retreating;
                if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + .25f)
                { retreating = false; agent.ResetPath(); }
            }
            else if (Gate.Remaining > 0) { Phase = ChasePhase.Waiting; Stop(); }
            else
            {
                Phase = ChasePhase.Pursuing;
                if (Time.time >= replanAt)
                {
                    replanAt = Time.time + definition.ReplanSeconds;
                    if (NavMesh.SamplePosition(world.Player.transform.position, out var goal, definition.TargetProjectionMetres, agent.areaMask)) agent.SetDestination(goal.position);
                    else Stop();
                }
            }
            if (!agent.isStopped && agent.velocity.sqrMagnitude > .05f && Time.time >= stepAt)
            { footstepSource.PlayOneShot(footstepClip); stepAt = Time.time + definition.FootstepSeconds; }
        }
        bool ChooseRetreat()
        {
            Vector3 best = default; float score = -1;
            foreach (var point in retreatPoints)
            {
                float distance = Vector3.Distance(point.position, world.Player.transform.position);
                if (distance < definition.RetreatMinimumMetres || distance <= score || !NavMesh.SamplePosition(point.position, out var goal, 1, agent.areaMask)) continue;
                if (!agent.CalculatePath(goal.position, candidatePath) || candidatePath.status != NavMeshPathStatus.PathComplete) continue;
                best = goal.position; score = distance;
            }
            if (score < 0) { agent.ResetPath(); return false; }
            agent.SetDestination(best); return true;
        }
        public bool HasSightToPlayer()
        {
            Vector3 origin = agent.transform.position + Vector3.up * .65f;
            Vector3 offset = world.Player.transform.position + Vector3.up * .9f - origin;
            int count = Physics.RaycastNonAlloc(origin, offset.normalized, hits, offset.magnitude, ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                var collider = hits[i].collider;
                if (collider.transform.IsChildOf(world.Player.transform) || collider.transform.IsChildOf(agent.transform)) continue;
                if (hits[i].distance < offset.magnitude - .02f) return false;
            }
            return true;
        }
        void Stop()
        {
            if (HasNavigation) { agent.isStopped = true; agent.velocity = Vector3.zero; }
            if (footstepSource) footstepSource.Stop();
        }
        void OnDisable() => Stop();
    }
}
