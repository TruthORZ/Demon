using Demon.Slice.Domain;
using UnityEngine;

namespace Demon.Slice
{
    public sealed partial class PlayerController
    {
        internal struct Pose { public Vector3 position; public Quaternion rotation; public float pitch; }
        internal Pose CapturePose() => new Pose { position = transform.position, rotation = transform.rotation, pitch = pitch };
        internal void RestorePose(Pose saved)
        {
            ReturnToExploration(); bool active = body.enabled; body.enabled = false;
            transform.SetPositionAndRotation(saved.position, saved.rotation); body.enabled = active;
            pitch = saved.pitch; viewCamera.transform.localRotation = Quaternion.Euler(pitch, 0, 0);
        }
    }
    public sealed partial class CipherBox
    {
        internal void Restore(ThreeDigitLock.Snapshot saved)
        { model.Restore(saved); tabletVisual.SetActive(!model.Acquired); lidPivot.localRotation = restingLid * Quaternion.Euler(model.Unlocked ? openLidEuler : Vector3.zero); RefreshDigits(); }
    }
    public sealed partial class ExplorationFlow
    {
        internal bool[] CaptureMemory() => new[] { solarRead, handoverRead, sawBeamLock, sawBeamRelease,
            Log.Contains(ObservationSite.Radio), Log.Contains(ObservationSite.WindVane), Log.Contains(ObservationSite.Lake) };
        internal void RestoreMemory(bool[] saved)
        {
            solarRead = saved[0]; handoverRead = saved[1]; sawBeamLock = saved[2]; sawBeamRelease = saved[3];
            Log = new FirstDayInspectionLog(); for (int i = 0; i < 3; i++) if (saved[i + 4]) Log.Record(1, (ObservationSite)i);
            RefreshTrails();
        }
    }
    public sealed partial class ListeningChime
    {
        internal struct State { public int count; public bool inside; public float swing; }
        internal State Capture() => new State { count = RingCount, inside = entry.Inside, swing = swingRemaining };
        internal void Restore(State saved)
        {
            RingCount = saved.count; swingRemaining = saved.swing;
            entry.SetEnabled(false, false); entry.SetEnabled(tool.Active, saved.inside); source.Stop();
        }
    }
    public sealed partial class ChaseController
    {
        internal struct State
        { public Vector3 position, destination, velocity; public bool hasPath, stopped; public Quaternion rotation; public float replan, step; public double cooldown; public int passes; public bool retreat, tracking; public float unseen, laugh; public ChasePhase phase; }
        internal State Capture() => new State { position = agent.transform.position, rotation = agent.transform.rotation,
            destination = HasNavigation && agent.hasPath ? agent.destination : agent.transform.position, hasPath = HasNavigation && agent.hasPath, stopped = !HasNavigation || agent.isStopped, velocity = HasNavigation ? agent.velocity : Vector3.zero,
            replan = Mathf.Max(0, replanAt - Time.time), step = Mathf.Max(0, stepAt - Time.time), cooldown = Gate.Remaining,
            passes = Gate.PassCount, retreat = retreating, phase = Phase, tracking = tracking, unseen = unseenSeconds, laugh = Mathf.Max(0, laughAt - Time.time) };
        internal void Restore(State saved)
        {
            Stop(); if (HasNavigation) agent.Warp(saved.position); else agent.transform.position = saved.position;
            agent.transform.rotation = saved.rotation; Gate.Restore(saved.cooldown, saved.passes);
            replanAt = Time.time + saved.replan; stepAt = Time.time + saved.step; retreating = saved.retreat; Phase = saved.phase; tracking = saved.tracking; unseenSeconds = saved.unseen; laughAt = Time.time + saved.laugh;
            if (HasNavigation && saved.hasPath) { agent.isStopped = saved.stopped; agent.SetDestination(saved.destination); agent.velocity = saved.velocity; }
            else if (retreating) ChooseRetreat();
        }
    }
    public sealed partial class WorldPresentation
    {
        internal bool CaptureBox() => boxOpened;
        internal void RestoreBox(bool opened)
        { boxOpened = opened; lidPivot.localRotation = authoredLidRotation * Quaternion.Euler(opened && world.Session.BoxSeal == 0 ? openLidEuler : Vector3.zero); Refresh(); }
    }
    public sealed partial class FinalTabletRetrieval
    {
        internal void Restore(bool acquired, bool placed, int interruptions)
        { Progress.Restore(acquired, placed); pickup.Attempt.Restore(acquired, interruptions); RefreshPresentation(); }
    }
    public sealed partial class WorldAttributeConsequences
    {
        internal int[] CapturePlayback() => new[] { radioBroadcast.timeSamples, radioStatic.timeSamples, demonBroadcast.timeSamples, parkedSample };
        internal void RestorePlayback(int[] saved)
        {
            var sources = new[] { radioBroadcast, radioStatic, demonBroadcast };
            for (int i = 0; i < 3; i++) if (sources[i].clip) sources[i].timeSamples = Mathf.Clamp(saved[i], 0, sources[i].clip.samples - 1);
            parkedSample = saved[3];
        }
    }
    internal sealed class DayCheckpoint
    {
        internal GameSession.Snapshot session;
        internal PlayerController.Pose player;
        internal bool[] discovered, exploration;
        internal string[] tasks, records, cues;
        internal int day;
        internal ThreeDigitLock.Snapshot cipher;
        internal RuneBoardModel.Snapshot listening;
        internal StableBeamMotion.Snapshot beam;
        internal ListeningChime.State[] chimes;
        internal ChaseController.State chase;
        internal bool acquired, placed, boxOpened, flashlight, waited;
        internal int interruptions;
        internal Quaternion vane;
        internal int[] broadcast;
        internal CabinDoorInteraction.State[] doors;
    }
}
