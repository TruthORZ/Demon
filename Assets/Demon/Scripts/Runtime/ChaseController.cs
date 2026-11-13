using System;
using Demon.Slice.Domain;
using UnityEngine;
using UnityEngine.AI;
namespace Demon.Slice
{
    public sealed partial class ChaseController : MonoBehaviour
    {
        public enum ChasePhase { Dormant, Pursuing, Frozen, Retreating, Waiting, Ended }
        [SerializeField] ChaseDefinition definition;
        [SerializeField] NavMeshAgent agent;
        [SerializeField] Transform[] retreatPoints;
        [SerializeField] AudioSource footstepSource;
        [SerializeField] AudioClip footstepClip;
        readonly RaycastHit[] hits = new RaycastHit[64];
        NavMeshPath candidatePath;
        GameSession session;
        PlayerController player;
        BeamTether tether;
        ISceneFeedback feedback;
        float replanAt, stepAt, missingNavSeconds;
        bool retreating, tracking;
        float unseenSeconds, laughAt;
        CompletePlaythrough main;
        [SerializeField] AudioSource misleadingVoice;
        [SerializeField] AudioClip laughClip;
        [SerializeField] AudioSource proximityVoice;
        bool heardNearby;
        float nextScream;

        public bool Tracking => tracking;
        [Tooltip("One outdoor spawn per pursuit morning, starting on the first motion day.")]
        [SerializeField] Transform[] morningSpawns;
        [Header("Return after prolonged absence (seconds and metres)")]
        [SerializeField, Min(1)] float absenceSeconds = 90;
        [SerializeField, Min(1)] float absenceDistance = 48;
        [SerializeField] Vector2 returnRange = new Vector2(20, 32);
        float absentFor;
        int lastSpawnDay;
        Vector3 authoredPosition;
        Quaternion authoredRotation;
        public ClosePassGate Gate { get; private set; }
        public ChasePhase Phase { get; private set; }
        public NavMeshAgent Agent => agent;
        public ChaseDefinition Definition => definition;
        // Presentation subscribes to the actual chase step; navigation has no decal dependency.
        public event Action<Vector3, Vector3> Stepped;
        public event Action PursuitReset;
        public bool HasNavigation => agent && agent.isActiveAndEnabled && agent.isOnNavMesh;
        public void Configure(ChaseDefinition data, NavMeshAgent actor, Transform[] points, AudioSource audio, AudioClip clip)
        { definition = data; agent = actor; retreatPoints = points; footstepSource = audio; footstepClip = clip; }
        public void Initialize(GameSession rules, PlayerController actor, BeamTether motion, ISceneFeedback messages)
        {
            if (!definition || !agent || retreatPoints == null || retreatPoints.Length < 2 || !footstepSource || !footstepClip || !motion || !actor || rules == null || messages == null)
                throw new InvalidOperationException("Chase requires saved settings, agent, retreat points, footsteps and T06 motion owner.");
            foreach (var point in retreatPoints) if (!point) throw new InvalidOperationException("Missing retreat point.");
            if (candidatePath == null)
            {
                candidatePath = new NavMeshPath(); // Native navigation objects require the main-thread initialization phase.
                authoredPosition = agent.transform.position; authoredRotation = agent.transform.rotation;
            }
            else
            {
                Stop();
                if (HasNavigation) agent.Warp(authoredPosition); else agent.transform.position = authoredPosition;
                agent.transform.rotation = authoredRotation;
            }
            replanAt = stepAt = missingNavSeconds = unseenSeconds = laughAt = 0; tracking = false; main = (messages as GameSceneController)?.Playthrough; retreating = false; Phase = ChasePhase.Dormant;
            if (misleadingVoice) misleadingVoice.Stop();
            if (proximityVoice) proximityVoice.Stop();
            absentFor = 0; lastSpawnDay = 0; heardNearby = false; nextScream = 0;
            session = rules; player = actor; tether = motion; feedback = messages; Gate = new ClosePassGate(definition.ClosePassMetres, definition.ClosePassCooldownSeconds);
            agent.speed = definition.MetresPerSecond; agent.acceleration = definition.AccelerationMetresPerSecondSquared; agent.angularSpeed = definition.AngularDegreesPerSecond;
            PursuitReset?.Invoke();
        }
        internal void RefreshMorningSpawn(float wakeHour)
        {
            int day = session.Clock.Day;
            int index = day - tether.MotionDay;
            if (day == lastSpawnDay || index < 0 || morningSpawns == null || index >= morningSpawns.Length ||
                session.Clock.MinuteOfDay < wakeHour * 60 || session.LakeSeal != 0) return;

            absentFor = 0; lastSpawnDay = day;
            Transform marker = morningSpawns[index];
            if (!marker || !NavMesh.SamplePosition(marker.position, out var ground, 2f, agent.areaMask) || main.InsideCabin(ground.position))
                throw new InvalidOperationException($"Day {day} needs an outdoor morning spawn on the enemy NavMesh.");
            Stop();
            if (agent.isActiveAndEnabled)
            {
                if (!agent.Warp(ground.position)) throw new InvalidOperationException($"Could not place enemy at the day {day} spawn.");
                agent.ResetPath();
            }
            else agent.transform.position = ground.position;
            agent.transform.rotation = marker.rotation;
            tracking = retreating = heardNearby = false;
            unseenSeconds = nextScream = replanAt = stepAt = 0;
            if (proximityVoice) proximityVoice.Stop();
            if (misleadingVoice) misleadingVoice.Stop();
            Gate.Restore(0, Gate.PassCount);
            Phase = ChasePhase.Waiting;
            PursuitReset?.Invoke();
        }

        void Update()
        {
            if (Gate == null || player.MenuOpen) return;
            Gate.Advance(Time.deltaTime);
            if (session.Descended) { Phase = ChasePhase.Ended; Stop(); return; }
            if (!tether.ProxyCanTravel || session.LakeSeal != 0) { absentFor = 0; Phase = ChasePhase.Dormant; Stop(); return; }
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
            if (main) UpdateAbsence(Time.deltaTime);
            if (main && !UpdateDetection()) return;
            if (!tether.DemonCanMove) { Phase = ChasePhase.Frozen; Stop(); return; }
            agent.isStopped = false;
            float distance = Vector3.Distance(agent.transform.position, player.transform.position);
            if (main && proximityVoice && proximityVoice.clip)
            {
                float range = main.Definition.PursuitNoticeMetres;

                if (!retreating && !heardNearby && distance <= range && Time.time >= nextScream && !proximityVoice.isPlaying)
                {
                    proximityVoice.Play();
                    heardNearby = true;
                    nextScream = Time.time + Mathf.Max(8, proximityVoice.clip.length);
                }
            }
            if (Gate.TryPass(distance, HasSightToPlayer(), true))
            {
                bool interrupted = player.InterruptPickup();
                feedback.SetMessage(interrupted ? definition.InterruptedMessage : definition.NearPassMessage);
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
                    if (NavMesh.SamplePosition(player.transform.position, out var goal, definition.TargetProjectionMetres, agent.areaMask) && (!main || !main.InsideCabin(goal.position))) agent.SetDestination(goal.position);
                    else Stop();
                }
            }
            bool moving = !agent.isStopped && agent.velocity.sqrMagnitude > .05f;
            if (footstepSource.loop)
            {
                bool audible = moving && Phase == ChasePhase.Pursuing;
                if (audible && !footstepSource.isPlaying) footstepSource.Play();
                else if (!audible && footstepSource.isPlaying) footstepSource.Stop();
            }
            if (moving && Time.time >= stepAt)
            {
                if (!footstepSource.loop) footstepSource.PlayOneShot(footstepClip);
                stepAt = Time.time + definition.FootstepSeconds;
                Stepped?.Invoke(agent.transform.position, agent.velocity.normalized);
            }
        }
        void UpdateAbsence(float dt)
        {
            var center = player.transform.position;
            if (main.InsideCabin(center) || !tether.DemonCanMove || retreating || Gate.Remaining > 0 ||
                Vector3.Distance(center, agent.transform.position) <= absenceDistance)
            { absentFor = 0; return; }
            if (main.BlocksClock || player.MenuOpen || !player.Exploring || dt <= 0) return;
            absentFor += dt;
            if (absentFor < absenceSeconds) return;
            absentFor = Mathf.Max(0, absenceSeconds - 4); // Retry later if the player is near a navigation boundary.
            float min = Mathf.Max(definition.ClosePassMetres + 2, returnRange.x), max = Mathf.Max(min, returnRange.y);
            if (!NavMesh.SamplePosition(center, out var playerGround, 3, agent.areaMask)) return;
            float bearing = UnityEngine.Random.Range(0f, 360f);
            for (int i = 0; i < 24; i++)
            {
                var direction = Quaternion.Euler(0, bearing + i * 137.5f, 0) * Vector3.forward;
                var candidate = center + direction * UnityEngine.Random.Range(min, max);
                if (!NavMesh.SamplePosition(candidate, out var ground, 4, agent.areaMask) || main.InsideCabin(ground.position)) continue;
                float range = Vector3.Distance(center, ground.position);
                if (range < min || range > max ||
                    !NavMesh.CalculatePath(ground.position, playerGround.position, agent.areaMask, candidatePath) ||
                    candidatePath.status != NavMeshPathStatus.PathComplete || !agent.Warp(ground.position)) continue;
                agent.ResetPath(); agent.velocity = Vector3.zero;
                absentFor = replanAt = unseenSeconds = 0; tracking = heardNearby = false;
                PursuitReset?.Invoke();
                return;
            }
        }

        bool UpdateDetection()
        {
            var plan = main.Definition;
            agent.speed = plan.ChaseSpeeds[Mathf.Clamp(session.Clock.Day - 3, 0, plan.ChaseSpeeds.Length - 1)];
            if (main.InsideCabin(agent.transform.position))
            {
                Stop();
                if (NavMesh.SamplePosition(main.CabinExit, out var outside, 3, agent.areaMask) && !main.InsideCabin(outside.position)) agent.Warp(outside.position);
                tracking = false; Phase = ChasePhase.Waiting; return false;
            }
            if (main.InsideCabin(player.transform.position)) { heardNearby = false; tracking = false; retreating = false; unseenSeconds = 0; Phase = ChasePhase.Waiting; Stop(); return false; }
            float distance = Vector3.Distance(agent.transform.position, player.transform.position);
            if (distance > plan.PursuitNoticeMetres * 1.3f) heardNearby = false;
            bool sight = HasSightToPlayer();
            if (tracking)
            {
                unseenSeconds = sight ? 0 : unseenSeconds + Time.deltaTime;
                if (distance > plan.LoseMetres || unseenSeconds >= plan.LostSightSeconds) tracking = false;
            }
            else if (distance <= plan.DetectionMetres && sight) { tracking = true; unseenSeconds = 0; }
            if (!tracking) { Phase = ChasePhase.Waiting; Stop(); return false; }
            if (session.Clock.Day >= 5 && misleadingVoice && laughClip && Time.time >= laughAt)
            {
                laughAt = Time.time + plan.LaughIntervalSeconds;
                // A separate emitter may disagree with the real target; chimes still sample the real target.
                Vector3 offset = Vector3.Cross(Vector3.up, (player.transform.position - agent.transform.position).normalized) * plan.MisleadingOffsetMetres;
                misleadingVoice.transform.position = agent.transform.position + offset;
                misleadingVoice.PlayOneShot(laughClip);
            }
            return true;
        }
        bool ChooseRetreat()
        {
            Vector3 best = default; float score = -1;
            foreach (var point in retreatPoints)
            {
                if (main && main.InsideCabin(point.position)) continue;
                float distance = Vector3.Distance(point.position, player.transform.position);
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
            Vector3 offset = player.transform.position + Vector3.up * .9f - origin;
            int count = Physics.RaycastNonAlloc(origin, offset.normalized, hits, offset.magnitude, ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                var collider = hits[i].collider;
                if (collider.transform.IsChildOf(player.transform) || collider.transform.IsChildOf(agent.transform)) continue;
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
