using System;
using System.IO;
using System.Linq;
using Demon.Slice.Domain;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
namespace Demon.Slice.Editor
{
    public sealed class BeamSmokeRun
    {
        readonly string operation;
        readonly Action<bool, string> finish;
        readonly double started = EditorApplication.timeSinceStartup;
        SliceSceneController world;
        BeamTether beam;
        ListeningTrailProxy proxy;
        Keyboard keyboard; Mouse mouse;
        InputSettings.BackgroundBehavior oldBackground;
        InputSettings.EditorInputBehaviorInPlayMode oldInput;
        int stage, ringAtLock;
        double since;
        bool finished;
        Vector3 frozenPosition;
        Quaternion vaneRotation;
        public BeamSmokeRun(string operation, Action<bool, string> finish) { this.operation = operation; this.finish = finish; }
        static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
        double Hold => beam.Definition.StableSeconds;
        public void Tick()
        {
            if (finished || !Application.isPlaying) return;
            try
            {
                double now = Time.timeAsDouble;
                if (EditorApplication.timeSinceStartup - started > 160) throw new TimeoutException("Beam smoke timed out at stage " + stage);
                if (!world) world = UnityEngine.Object.FindAnyObjectByType<SliceSceneController>();
                if (!world || world.Session == null || !world.Tether || world.Tether.Rule == null) return;
                beam = world.Tether;
                if (stage == 0)
                {
                    proxy = UnityEngine.Object.FindAnyObjectByType<ListeningTrailProxy>(); proxy.enabled = false;
                    oldBackground = InputSystem.settings.backgroundBehavior; oldInput = InputSystem.settings.editorInputBehaviorInPlayMode;
                    InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
                    InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
                    keyboard = InputSystem.AddDevice<Keyboard>(); mouse = InputSystem.AddDevice<Mouse>(); InputSystem.onAfterUpdate += Current; Current();
                    PlacePlayer(new Vector3(5.3f, .04f, -3)); proxy.Target.position = new Vector3(5.3f, .15f, -7);
                    Require(!beam.Equipped, "T06 must be obtained from the real box.");
                    if (operation == "beam-tuning") Require(Math.Abs(Hold - .4) < .001 && Math.Abs(beam.Definition.RangeMetres - 6) < .001 && Math.Abs(beam.Definition.RegionRadiusMetres - 1.1) < .001, "Saved beam parameters ignored.");
                    PointAt(beam.AimRegion.position); Next(1, now);
                }
                else if (stage == 1 && now - since > .5)
                {
                    Require(!beam.BeamOn && beam.Rule.ExchangeCount == 0, "Unacquired tool exchanged motion."); Release();
                    var box = world.Cipher; var answer = box.Wheel.ReadCode();
                    for (int i = 0; i < 3; i++) while (box.Model.At(i) != answer[i]) box.Model.Cycle(i);
                    Require(box.TryConfirm(), "Box A preparation failed."); box.Use(world.Player);
                    Require(beam.Equipped && world.Player.Reading && !world.HasReadBox, "Real A pickup failed to equip T06.");
                    PointAt(beam.AimRegion.position); Next(2, now);
                }
                else if (stage == 2 && now - since > .4)
                {
                    Require(!beam.BeamOn && !beam.Rule.Locked, "Reading did not suppress tool input."); Release();
                    InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Escape)); Next(3, now);
                }
                else if (stage == 3 && now - since > .8)
                {
                    InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                    if (operation == "beam-early")
                    {
                        Require(world.Session.Clock.Day == 1 && !beam.MotionReady && !beam.TargetVisible && !beam.DemonCanMove && beam.VaneCanMove, "D1 motion boundary violated or early pickup blocked.");
                        vaneRotation = beam.VaneBlade.localRotation; PointAt(beam.AimRegion.position); Next(40, now); return;
                    }
                    proxy.Target.position = new Vector3(2, .15f, -7);
                    var board = world.Listening.Board; board.PickFromTray(RuneKind.Ring); board.ClickSlot(4); board.PickFromTray(RuneKind.Chime); board.ClickSlot(7);
                    Require(world.Listening.TryCompile(), "T05 preparation failed.");
                    proxy.Target.position = new Vector3(5.3f, .15f, -7); PointAt(beam.AimRegion.position); Next(4, now);
                }
                else if (stage == 4 && now - since > Hold * .4)
                {
                    Require(!beam.Rule.Locked && beam.Rule.ExchangeCount == 0 && beam.ClearAim && beam.BeamOn, "Short stable hold exchanged, or real mouse failed to aim.");
                    Release(); Next(5, now);
                }
                else if (stage == 5 && now - since > .2)
                {
                    Require(beam.Rule.StableSeconds == 0, "Interrupted hold kept partial progress."); PointAt(beam.AimRegion.position); Next(6, now);
                }
                else if (stage == 6 && now - since > Hold + .25)
                {
                    Require(beam.Rule.Locked && beam.Rule.ExchangeCount == 1 && !beam.DemonCanMove && beam.VaneCanMove, "Stable aim did not exchange once.");
                    ringAtLock = world.Listening.Chimes[0].RingCount; Require(ringAtLock == 1, "T05 entry was not observed before immobilization.");
                    frozenPosition = proxy.Target.position; vaneRotation = beam.VaneBlade.localRotation; proxy.enabled = true; Next(7, now);
                }
                else if (stage == 7 && now - since > 1.2)
                {
                    Require(beam.Rule.ExchangeCount == 1 && Vector3.Distance(proxy.Target.position, frozenPosition) < .001f && Quaternion.Angle(vaneRotation, beam.VaneBlade.localRotation) > 10, "Locked actor moved or vane failed to receive motion.");
                    Require(world.Listening.Chimes[0].RingCount == ringAtLock, "Stationary target repeatedly rang T05."); Capture("locked"); Next(70, now);
                }
                else if (stage == 70 && now - since > .3)
                { InputSystem.QueueStateEvent(mouse, new MouseState { delta = new Vector2(600, 0), buttons = 2 }); Next(8, now); }
                else if (stage == 8 && now - since > 2.2)
                {
                    Require(!beam.Rule.Locked && beam.Rule.ExchangeCount == 2 && beam.DemonCanMove && !beam.VaneCanMove && Vector3.Distance(proxy.Target.position, frozenPosition) > .1f, "Turning away did not restore motion.");
                    vaneRotation = beam.VaneBlade.localRotation; proxy.enabled = false; Next(9, now);
                }
                else if (stage == 9 && now - since > .5)
                {
                    Require(Quaternion.Angle(vaneRotation, beam.VaneBlade.localRotation) < .01f, "Vane kept moving after restoration.");
                    proxy.Target.position = new Vector3(5.3f, .15f, -7); PointAt(beam.AimRegion.position); Next(10, now);
                }
                else if (stage == 10 && now - since > Hold + .2)
                {
                    Require(beam.Rule.Locked && beam.Rule.ExchangeCount == 3, "Reacquisition failed.");
                    proxy.Target.position = new Vector3(5.3f, .15f, -3 - beam.Definition.RangeMetres - 3); PointAt(beam.AimRegion.position); Next(11, now);
                }
                else if (stage == 11 && now - since > .25)
                {
                    Require(!beam.Rule.Locked && beam.Rule.ExchangeCount == 4 && !beam.ClearAim, "Over-range target kept lock.");
                    proxy.Target.position = new Vector3(5.3f, .15f, -7); PointAt(beam.AimRegion.position); Next(12, now);
                }
                else if (stage == 12 && now - since > Hold + .2)
                {
                    Require(beam.Rule.Locked && beam.Rule.ExchangeCount == 5, "Second reacquisition failed.");
                    PlacePlayer(new Vector3(7.5f, .04f, -9.45f)); proxy.Target.position = new Vector3(7.5f, .15f, -7); PointAt(beam.AimRegion.position); Next(13, now);
                }
                else if (stage == 13 && now - since > .3)
                {
                    Require(!beam.Rule.Locked && beam.Rule.ExchangeCount == 6 && !beam.ClearAim, "Real wooden occluder did not release lock."); Capture("occluded"); Next(130, now);
                }
                else if (stage == 130 && now - since > .3)
                { PlacePlayer(new Vector3(5.3f, .04f, -3)); proxy.Target.position = new Vector3(5.3f, .15f, -7); PointAt(beam.AimRegion.position); Next(14, now); }
                else if (stage == 14 && now - since > Hold + .2)
                {
                    Require(beam.Rule.Locked && beam.Rule.ExchangeCount == 7, "Post-occlusion reacquisition failed."); world.Cipher.Wheel.Use(world.Player); Next(15, now);
                }
                else if (stage == 15 && now - since > .3)
                {
                    Require(!beam.Rule.Locked && beam.Rule.ExchangeCount == 8 && !beam.BeamOn, "Near inspection did not release tether."); Release(); world.Player.ExitCloseView(); Next(16, now);
                }
                else if (stage == 16 && now - since > .8) { PointAt(beam.AimRegion.position); Next(17, now); }
                else if (stage == 17 && now - since > Hold + .2)
                {
                    Require(beam.Rule.Locked && beam.Rule.ExchangeCount == 9, "Reacquisition before lifecycle check failed.");
                    beam.enabled = false; Require(beam.Rule.ExchangeCount == 10 && beam.DemonCanMove && !beam.LightVisible, "Disable leaked a motion exchange or flashlight."); Release(); Next(18, now);
                }
                else if (stage == 18 && now - since > .2) { beam.enabled = true; PointAt(beam.AimRegion.position); Next(19, now); }
                else if (stage == 19 && now - since > Hold + .2)
                { Require(beam.Rule.Locked && beam.Rule.ExchangeCount == 11, "Enable did not permit a fresh stable hold."); Release(); Next(20, now); }
                else if (stage == 20 && now - since > .2)
                {
                    Require(!beam.Rule.Locked && beam.Rule.ExchangeCount == 12 && !beam.BeamOn, "Right-button release did not restore.");
                    Require(world.Session.Clock.Day == 7 && world.Session.ExchangeCount == 0 && world.Session.BoxSeal == 1 && world.Session.LakeSeal == 0 && !world.Finale.TimeReserved && world.Cipher.Wheel.ReadCode().SequenceEqual(new[] { 3, 4, 2 }), "MOTION leaked into SEAL/time/cipher/finale.");
                    if (operation != "beam-tuning")
                    { PlacePlayer(beam.ObservationPoint.position); proxy.Target.position = new Vector3(2, .15f, -7); PointAt(beam.AimRegion.position); Next(21, now); }
                    else { PointAt(beam.AimRegion.position); Next(23, now); }
                }
                else if (stage == 21 && now - since > Hold + .3)
                { Require(beam.Rule.Locked && beam.Rule.ExchangeCount == 13, "Saved observation point could not see/lock visible snow region."); Capture("observation"); Next(22, now); }
                else if (stage == 22 && now - since > .4) { world.Sleep(); Next(24, now); }
                else if (stage == 23 && now - since > Hold + .2) { Require(beam.Rule.Locked, "Tuned lock failed before terminal sleep."); world.Sleep(); Next(24, now); }
                else if (stage == 24 && now - since > .3)
                {
                    if (!world.Session.Descended) { world.Sleep(); since = now; return; }
                    Require(!beam.Rule.Locked && beam.Rule.ExchangeCount == 14 && !beam.BeamOn && world.Session.Clock.Day == 9 && world.Session.ExchangeCount == 0, "Sleep/ending leaked held motion or advanced T07.");
                    Complete(true, "Physical right-mouse stable/partial hold, actor freeze/vane motion, T05 stationary dedup, turn/range/wood occlusion/right-release/nearview/disable/sleep restoration, saved settings and observer, no SEAL/time/cipher/finale mutation passed. Exchanges=14; tuning=" + (operation == "beam-tuning"));
                }
                else if (stage == 40 && now - since > .6)
                {
                    Require(!beam.Rule.Locked && beam.Rule.ExchangeCount == 0 && !beam.TargetVisible && Quaternion.Angle(vaneRotation, beam.VaneBlade.localRotation) > 10, "Early T06 invented demon motion or stopped pre-D3 vane.");
                    world.Session.Clock.AdvanceTo(2 * 1440 + 600); Release(); Next(41, now);
                }
                else if (stage == 41 && now - since > .25)
                {
                    Require(beam.Equipped && beam.MotionReady && beam.TargetVisible && beam.DemonCanMove && !beam.VaneCanMove, "D3 boundary failed or hidden D7 gate exists.");
                    PointAt(beam.AimRegion.position); Next(42, now);
                }
                else if (stage == 42 && now - since > Hold + .2)
                {
                    Require(beam.Rule.Locked && beam.Rule.ExchangeCount == 1, "Early acquired T06 could not work on D3."); Release(); Next(43, now);
                }
                else if (stage == 43 && now - since > .25)
                { Require(!beam.Rule.Locked && beam.Rule.ExchangeCount == 2, "Early boundary lock did not restore."); Complete(true, "Real A acquisition on D1, no pre-D3 demon motion/lock, pre-D3 vane rotation, D3 visible region/motion switch and actual right-mouse T06 lock before D7 passed."); }
            }
            catch (Exception e) { Complete(false, "Stage " + stage + ": " + e); }
        }
        void PlacePlayer(Vector3 point)
        {
            var body = world.Player.GetComponent<CharacterController>(); body.enabled = false; world.Player.transform.SetPositionAndRotation(point, Quaternion.identity); body.enabled = true; Cursor.lockState = CursorLockMode.Locked;
        }
        void PointAt(Vector3 point)
        {
            var camera = world.Player.ViewCamera.transform; Vector3 desired = Quaternion.LookRotation(point - camera.position).eulerAngles;
            float pitch = Mathf.DeltaAngle(0, camera.localEulerAngles.x); float targetPitch = Mathf.DeltaAngle(0, desired.x);
            float yaw = Mathf.DeltaAngle(world.Player.transform.eulerAngles.y, desired.y);
            InputSystem.QueueStateEvent(mouse, new MouseState { delta = new Vector2(yaw / world.Settings.MouseSensitivity, (pitch - targetPitch) / world.Settings.MouseSensitivity), buttons = 2 });
        }
        void Release() => InputSystem.QueueStateEvent(mouse, new MouseState());
        void Current() { keyboard?.MakeCurrent(); mouse?.MakeCurrent(); }
        void Next(int value, double now) { stage = value; since = now; }
        void Capture(string name) => ScreenCapture.CaptureScreenshot(Path.GetFullPath("Temp/DemonSliceReports/" + operation + "-" + name + ".png"));
        public void Cleanup()
        {
            if (keyboard == null) return; InputSystem.onAfterUpdate -= Current; InputSystem.RemoveDevice(keyboard); InputSystem.RemoveDevice(mouse);
            InputSystem.settings.backgroundBehavior = oldBackground; InputSystem.settings.editorInputBehaviorInPlayMode = oldInput; keyboard = null; mouse = null;
        }
        void Complete(bool success, string message) { finished = true; Cleanup(); finish(success, message); EditorApplication.delayCall += EditorApplication.ExitPlaymode; }
    }
}
