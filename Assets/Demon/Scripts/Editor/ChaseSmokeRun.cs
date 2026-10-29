using System;
using System.IO;
using System.Linq;
using Demon.Slice.Domain;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
namespace Demon.Slice.Editor
{
    public sealed class ChaseSmokeRun
    {
        readonly string operation;
        readonly Action<bool, string> finish;
        readonly double started = EditorApplication.timeSinceStartup;
        SliceSceneController world;
        ChaseController chase;
        ThreatPickup pickup;
        Collider wall;
        Keyboard keyboard; Mouse mouse;
        InputSettings.BackgroundBehavior oldBackground;
        InputSettings.EditorInputBehaviorInPlayMode oldInput;
        bool finished, pickupCaptured;
        int stage, steps, passBefore;
        double since;
        float maxLateral;
        Vector3 frozen, playerAtPickup;
        Quaternion vane;
        public ChaseSmokeRun(string operation, Action<bool, string> finish) { this.operation = operation; this.finish = finish; }
        static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
        public void Tick()
        {
            if (finished || !Application.isPlaying) return;
            try
            {
                double now = Time.timeAsDouble;
                if (EditorApplication.timeSinceStartup - started > 160) throw new TimeoutException("Chase smoke timed out at stage " + stage);
                if (!world) world = UnityEngine.Object.FindAnyObjectByType<SliceSceneController>();
                if (!world || world.Session == null || !world.Chase || world.Chase.Gate == null) return;
                chase = world.Chase;
                if (!pickup) pickup = UnityEngine.Object.FindAnyObjectByType<ThreatPickup>();
                if (!pickup || pickup.Attempt == null) return;
                if (chase.HasNavigation && wall)
                {
                    var bounds = wall.bounds; bounds.Expand(-.08f);
                    Require(!bounds.Contains(chase.Agent.transform.position + Vector3.up), "Agent crossed solid practice wall.");
                }
                if (stage == 0)
                {
                    Require(world.Session.Clock.Day == 7 && world.Session.LakeSeal == 0, "Practice must begin at D7 with open lake.");
                    wall = GameObject.Find("ChasePractice_Authored/PracticeRoutingWall").GetComponent<Collider>();
                    if (!chase.HasNavigation) return;
                    Require(chase.Definition.MetresPerSecond == (operation == "chase-tuning" ? 2.2f : 3.6f), "Saved chase speed ignored.");
                    Require(Math.Abs(pickup.HoldSeconds - (operation == "chase-tuning" ? 1.8f : 1.2f)) < .001f, "Saved pickup hold ignored.");
                    oldBackground = InputSystem.settings.backgroundBehavior; oldInput = InputSystem.settings.editorInputBehaviorInPlayMode;
                    InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
                    InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
                    keyboard = InputSystem.AddDevice<Keyboard>(); mouse = InputSystem.AddDevice<Mouse>(); InputSystem.onAfterUpdate += Current; Current();
                    chase.enabled = false; PlacePlayer(new Vector3(0, .04f, -4.85f)); Warp(new Vector3(0, 0, -6.2f)); chase.enabled = true; Next(100, now);
                }
                else if (stage == 100 && now - since > .2)
                {
                    Require(!chase.HasSightToPlayer() && chase.Gate.PassCount == 0, "Close pass went through the solid wall.");
                    chase.enabled = false; PlacePlayer(new Vector3(0, .04f, -2.8f)); Warp(new Vector3(0, 0, -8)); chase.enabled = true; Next(1, now);
                }
                else if (stage == 1)
                {
                    maxLateral = Mathf.Max(maxLateral, Mathf.Abs(chase.Agent.transform.position.x));
                    if (now - since > .65)
                    {
                        Require(chase.HasNavigation && chase.Agent.hasPath && chase.Agent.path.status == NavMeshPathStatus.PathComplete && chase.Agent.path.corners.Length >= 2, "Saved NavMesh did not produce an obstacle route.");
                        PlacePlayer(new Vector3(6, .04f, -2.8f)); Next(2, now);
                    }
                }
                else if (stage == 2 && now - since > .4)
                {
                    maxLateral = Mathf.Max(maxLateral, Mathf.Abs(chase.Agent.transform.position.x));
                    Require(chase.Agent.destination.x > 5 && chase.Gate.PassCount == 0, "Player movement did not replan destination, or early pass occurred.");
                    Require(maxLateral > .6f, "Agent did not deviate around the wall."); Capture("route"); chase.enabled = false;
                    PlacePlayer(new Vector3(3, .04f, -2.8f)); Warp(new Vector3(3, 0, -7.5f));
                    var box = world.Cipher; var answer = box.Wheel.ReadCode(); for (int i = 0; i < 3; i++) while (box.Model.At(i) != answer[i]) box.Model.Cycle(i);
                    Require(box.TryConfirm(), "Preparing actual A unlock failed."); box.Use(world.Player);
                    var board = world.Listening.Board; board.PickFromTray(RuneKind.Ring); board.ClickSlot(4); board.PickFromTray(RuneKind.Chime); board.ClickSlot(7); Require(world.Listening.TryCompile(), "Preparing actual T05 failed.");
                    // T05 exits its own near view; reopen T06 to exercise physical Esc cleanup.
                    box.Use(world.Player); InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Escape)); Next(3, now);
                }
                else if (stage == 3 && now - since > .8)
                {
                    InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                    Require(world.Player.CanUseTools && world.Tether.Equipped, "Reading did not return to exploration.");
                    PlacePlayer(new Vector3(1.8f, .04f, -3.7f)); PointAt(pickup.transform.position + new Vector3(0, 1.5f, -.35f), false);
                    InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E)); Next(30, now);
                }
                else if (stage == 30 && now - since > .2)
                {
                    Require(world.Player.IsPickingUp && pickup.Attempt.Progress > 0, "Actual held E did not start pickup.");
                    InputSystem.QueueStateEvent(keyboard, new KeyboardState()); Next(31, now);
                }
                else if (stage == 31 && now - since > .8)
                {
                    Require(!pickup.Attempt.Active && !pickup.Attempt.Completed && pickup.Attempt.Elapsed == 0 && world.Player.CanUseTools, "Release did not cancel pickup/view.");
                    PointAt(pickup.transform.position + new Vector3(0, 1.5f, -.35f), false); InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E)); Next(32, now);
                }
                else if (stage == 32 && now - since > .2)
                {
                    Require(world.Player.IsPickingUp, "Retry for Escape did not start."); InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E, Key.Escape)); Next(33, now);
                }
                else if (stage == 33 && now - since > .8)
                {
                    Require(!pickup.Attempt.Active && !pickup.Attempt.Completed && world.Player.CanUseTools, "Escape did not cancel pickup/view.");
                    InputSystem.QueueStateEvent(keyboard, new KeyboardState()); Next(34, now);
                }
                else if (stage == 34 && now - since > .1)
                {
                    PointAt(pickup.transform.position + new Vector3(0, 1.5f, -.35f), false); InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E)); Next(35, now);
                }
                else if (stage == 35 && now - since > .2)
                {
                    Require(world.Player.IsPickingUp, "Retry for disable did not start."); pickup.enabled = false;
                    Require(!world.Player.IsPickingUp && !pickup.Attempt.Active && !pickup.Attempt.Completed && pickup.Attempt.InterruptCount == 0, "Disabled pickup retained attempt/view.");
                    pickup.enabled = true; InputSystem.QueueStateEvent(keyboard, new KeyboardState()); Next(36, now);
                }
                else if (stage == 36 && now - since > .8)
                {
                    Require(world.Player.CanUseTools, "Disabled pickup left close view active."); PlacePlayer(new Vector3(3, .04f, -2.8f)); Warp(new Vector3(3, 0, -7.5f));
                    chase.enabled = true; PointAt(world.Tether.AimRegion.position, true); Next(4, now);
                }
                else if (stage == 4 && now - since > world.Tether.Definition.StableSeconds + .25)
                {
                    Require(world.Tether.Rule.Locked && chase.Phase == ChaseController.ChasePhase.Frozen && chase.Agent.isStopped, "Physical right mouse did not freeze navigation.");
                    frozen = chase.Agent.transform.position; vane = world.Tether.VaneBlade.localRotation; steps = world.Listening.Chimes.Sum(c => c.RingCount); Capture("frozen"); Next(5, now);
                }
                else if (stage == 5 && now - since > .8)
                {
                    Require(Vector3.Distance(frozen, chase.Agent.transform.position) < .02f && Quaternion.Angle(vane, world.Tether.VaneBlade.localRotation) > 10, "Frozen agent drifted or vane failed to rotate.");
                    Require(world.Listening.Chimes.Sum(c => c.RingCount) == steps, "Frozen actor repeatedly rang chimes.");
                    PlacePlayer(new Vector3(1.8f, .04f, -3.7f)); playerAtPickup = world.Player.transform.position;
                    PointAt(pickup.transform.position + new Vector3(0, 1.5f, -.35f), true); InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E)); passBefore = chase.Gate.PassCount; Next(6, now);
                }
                else if (stage == 6)
                {
                    if (world.Player.IsPickingUp) { Require(!world.Tether.BeamOn, "Pickup kept flashlight active."); if (!pickupCaptured) { Capture("pickup"); pickupCaptured = true; } }
                    if (pickup.Attempt.InterruptCount > 0)
                    {
                        Require(!world.Player.IsPickingUp && !pickup.Attempt.Completed && pickup.Attempt.Elapsed == 0 && chase.Gate.PassCount == passBefore + 1, "Close pass did not cancel only the active pickup.");
                        Require(world.Cipher.Acquired && world.Cipher.Model.Unlocked && world.Listening.Board.Frozen && !world.HasReadBox && world.Session.ExchangeCount == 0, "Interruption erased completed progress or triggered B/T07.");
                        Require(Vector2.Distance(new Vector2(playerAtPickup.x, playerAtPickup.z), new Vector2(world.Player.transform.position.x, world.Player.transform.position.z)) < .02f, "Close pass moved/teleported the player.");
                        InputSystem.QueueStateEvent(keyboard, new KeyboardState()); InputSystem.QueueStateEvent(mouse, new MouseState()); Capture("interrupted"); Next(7, now);
                    }
                    else if (now - since > 4) throw new InvalidOperationException("Actual E pickup was not interrupted; picking=" + world.Player.IsPickingUp + ", progress=" + pickup.Attempt.Progress + ", phase=" + chase.Phase + ", passes=" + chase.Gate.PassCount);
                }
                else if (stage == 7 && now - since > 1)
                {
                    Require(chase.Gate.PassCount == passBefore + 1 && chase.Gate.Remaining > 0, "Close-pass cooldown repeated before retreat completed.");
                    // A slower authored speed can still be rounding an obstacle after one second.
                    if (Vector3.Distance(chase.Agent.transform.position, world.Player.transform.position) <= chase.Definition.ClosePassMetres)
                    {
                        Require(now - since < 3, "Retreat never left close range: position=" + chase.Agent.transform.position + ", destination=" + chase.Agent.destination + ", phase=" + chase.Phase);
                        return;
                    }
                    Require(world.Player.CanUseTools, "Interrupted pickup left player trapped in close view.");
                    PointAt(pickup.transform.position + new Vector3(0, 1.5f, -.35f), false); InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E)); Next(8, now);
                }
                else if (stage == 8)
                {
                    if (pickup.Attempt.Completed)
                    {
                        Require(pickup.CollectionCount == 1 && pickup.Attempt.InterruptCount == 1, "Retry did not collect once.");
                        InputSystem.QueueStateEvent(keyboard, new KeyboardState()); Next(9, now);
                    }
                    else if (now - since > pickup.HoldSeconds + 1) throw new InvalidOperationException("Retry failed during retreat window; phase=" + chase.Phase);
                }
                else if (stage == 9 && now - since > .8)
                {
                    pickup.Use(world.Player); Require(pickup.CollectionCount == 1 && !world.Player.IsPickingUp, "Completed item was collected again.");
                    Require(world.Cipher.Wheel.ReadCode().SequenceEqual(new[] { 3, 4, 2 }) && world.Session.Clock.Day == 7 && world.Session.BoxSeal == 1 && world.Session.LakeSeal == 0 && !world.Finale.TimeReserved, "Chase/pickup changed cipher/date/SEAL/finale.");
                    Capture("collected"); chase.enabled = false; frozen = chase.Agent.transform.position; Next(10, now);
                }
                else if (stage == 10 && now - since > .3)
                {
                    Require(Vector3.Distance(frozen, chase.Agent.transform.position) < .02f && chase.Agent.isStopped, "Disabled controller kept moving agent: drift=" + Vector3.Distance(frozen, chase.Agent.transform.position) + ", stopped=" + chase.Agent.isStopped + ", velocity=" + chase.Agent.velocity); chase.enabled = true; Next(11, now);
                }
                else if (stage == 11 && now - since > 1)
                {
                    for (int i = 0; i < 2; i++) world.Sleep(); Next(12, now);
                }
                else if (stage == 12 && now - since > .3)
                {
                    Require(world.Session.Descended && chase.Phase == ChaseController.ChasePhase.Ended && pickup.Attempt.Completed && world.Session.ExchangeCount == 0 && world.Session.Clock.Day == 9, "Terminal sleep/navigation cleanup failed.");
                    Complete(true, "Saved NavMesh routes around actual wall, moving-player destination replans, physical right-mouse freezes Agent/vane exchange and chime dedup, physical E release/Escape/item-disable cancel, held E interrupted once then retried/collected once, retreat cooldown and completed progress retained, disable/end cleanup and no SEAL/date/cipher/T07 leak passed. Tuning=" + (operation == "chase-tuning"));
                }
            }
            catch (Exception e) { Complete(false, "Stage " + stage + ": " + e); }
        }
        void PlacePlayer(Vector3 point)
        { var body = world.Player.GetComponent<CharacterController>(); body.enabled = false; world.Player.transform.SetPositionAndRotation(point, Quaternion.identity); body.enabled = true; Cursor.lockState = CursorLockMode.Locked; }
        void Warp(Vector3 point)
        {
            Require(NavMesh.SamplePosition(point, out var hit, 1.5f, NavMesh.AllAreas) && chase.Agent.Warp(hit.position), "Saved NavMesh cannot place test agent."); chase.Agent.ResetPath();
        }
        void PointAt(Vector3 point, bool right)
        {
            var camera = world.Player.ViewCamera.transform; Vector3 desired = Quaternion.LookRotation(point - camera.position).eulerAngles;
            float yaw = Mathf.DeltaAngle(world.Player.transform.eulerAngles.y, desired.y); float pitch = Mathf.DeltaAngle(0, camera.localEulerAngles.x); float goalPitch = Mathf.DeltaAngle(0, desired.x);
            InputSystem.QueueStateEvent(mouse, new MouseState { delta = new Vector2(yaw / world.Settings.MouseSensitivity, (pitch - goalPitch) / world.Settings.MouseSensitivity), buttons = (ushort)(right ? 2 : 0) });
        }
        void Next(int value, double now) { stage = value; since = now; }
        void Current() { keyboard?.MakeCurrent(); mouse?.MakeCurrent(); }
        void Capture(string label) => ScreenCapture.CaptureScreenshot(Path.GetFullPath("Temp/DemonSliceReports/" + operation + "-" + label + ".png"));
        public void Cleanup()
        {
            if (keyboard == null) return; InputSystem.onAfterUpdate -= Current; InputSystem.RemoveDevice(keyboard); InputSystem.RemoveDevice(mouse); InputSystem.settings.backgroundBehavior = oldBackground; InputSystem.settings.editorInputBehaviorInPlayMode = oldInput; keyboard = null; mouse = null;
        }
        void Complete(bool success, string message) { finished = true; Cleanup(); finish(success, message); EditorApplication.delayCall += EditorApplication.ExitPlaymode; }
    }
}
