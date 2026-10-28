using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
namespace Demon.Slice.Editor
{
    public sealed class CipherSmokeRun
    {
        readonly string operation;
        readonly Action<bool, string> finish;
        readonly double started = EditorApplication.timeSinceStartup;
        SliceSceneController world;
        CipherBox box;
        Keyboard keyboard; Mouse mouse;
        InputSettings.BackgroundBehavior oldBackground;
        InputSettings.EditorInputBehaviorInPlayMode oldInput;
        bool finished;
        int stage, click;
        double since;
        Transform[] clickTargets;
        int[] code;
        Vector3 startPosition;
        public CipherSmokeRun(string operation, Action<bool, string> finish) { this.operation = operation; this.finish = finish; }
        static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
        public void Tick()
        {
            if (finished || !Application.isPlaying) return;
            try
            {
                double now = Time.timeAsDouble;
                if (EditorApplication.timeSinceStartup - started > 100) throw new TimeoutException("Cipher smoke timed out at stage " + stage);
                if (!world) world = UnityEngine.Object.FindAnyObjectByType<SliceSceneController>();
                if (!world || world.Session == null || !world.Cipher || world.Cipher.Model == null) return;
                box = world.Cipher;
                if (stage == 0)
                {
                    Require(world.Session.Clock.Day == 1 && !box.Acquired && !box.Model.Unlocked, "Quick state must permit D1 early acquisition.");
                    world.Campaign.ReadTablet(5, box.CloseView);
                    Require(!world.Player.Reading && world.Campaign.Progress.FoundCount == 0, "Old T06 bypassed box acquisition.");
                    code = box.Wheel.ReadCode();
                    Require(code.SequenceEqual(operation == "cipher-tuning" ? new[] { 5, 4, 2 } : new[] { 3, 4, 2 }), "Saved wheel mapping did not drive code.");
                    var blade = GameObject.Find("Cipher_Authored/WindVane_FixedPosition/RotatingBlade_OrientationIsNotCode").transform;
                    blade.Rotate(0, 137, 0); Require(code.SequenceEqual(box.Wheel.ReadCode()), "Vane blade orientation changed fixed-position answer.");
                    oldBackground = InputSystem.settings.backgroundBehavior; oldInput = InputSystem.settings.editorInputBehaviorInPlayMode;
                    InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
                    InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
                    keyboard = InputSystem.AddDevice<Keyboard>(); mouse = InputSystem.AddDevice<Mouse>(); InputSystem.onAfterUpdate += Current; Current(); Cursor.lockState = CursorLockMode.Locked;
                    startPosition = world.Player.transform.position; Key(KeyCode.W); Next(1, now);
                }
                else if (stage == 1 && now - since > .25)
                {
                    Require(world.Player.transform.position.z > startPosition.z + .15f, "W did not move player toward box A.");
                    Key(KeyCode.E); Next(2, now);
                }
                else if (stage == 2 && now - since > .8)
                {
                    Release(); Require(world.Player.InspectingLock && Vector3.Distance(world.Player.ViewCamera.transform.position, box.CloseView.position) < .03f, "E failed to enter saved A lock view.");
                    Key(KeyCode.Return); Next(3, now);
                }
                else if (stage == 3 && now - since > .3)
                {
                    Release(); Require(!box.Model.Unlocked && world.Player.InspectingLock && !world.HasReadBox && !world.Finale.TimeReserved, "Wrong code changed final state or locked out retry.");
                    var digit = box.GetComponentsInChildren<CipherDigit>().Single(d => d.Index == 0); Click(digit.transform, true); Next(4, now);
                }
                else if (stage == 4 && now - since > .2)
                {
                    InputSystem.QueueStateEvent(mouse, new MouseState()); Require(box.Model.At(0) == 1, "Physical digit click failed."); Key(KeyCode.Escape); Next(5, now);
                }
                else if (stage == 5 && now - since > .7) { Release(); Key(KeyCode.E); Next(6, now); }
                else if (stage == 6 && now - since > .7)
                {
                    Release(); Require(world.Player.InspectingLock && box.Model.At(0) == 1, "Esc/reenter did not preserve input.");
                    var buttons = box.GetComponentsInChildren<CipherDigit>();
                    clickTargets = Enumerable.Range(0, 3).SelectMany(i => Enumerable.Repeat(buttons.Single(d => d.Index == i).transform, (code[i] - box.Model.At(i) + 10) % 10)).ToArray(); Next(7, now);
                }
                else if (stage == 7 && now - since > .14)
                {
                    if (click < clickTargets.Length * 2) { Click(clickTargets[click / 2], click % 2 == 0); click++; since = now; }
                    else { Capture("lock"); Next(70, now); }
                }
                else if (stage == 70 && now - since > .5) { Key(KeyCode.Return); Next(8, now); }
                else if (stage == 8 && now - since > .8)
                {
                    Release(); Require(box.Model.Unlocked && !box.Acquired && world.Campaign.Progress.FoundCount == 0, "Unlock must precede separate T06 pickup.");
                    Key(KeyCode.E); Next(9, now);
                }
                else if (stage == 9 && now - since > .8)
                {
                    Release(); Require(box.Acquired && world.Player.Reading && !world.HasReadBox && world.Campaign.Progress.FoundCount == 1 && world.Campaign.Progress.Found(5), "D1 E pickup failed or was counted as B reading.");
                    Require(world.Session.Clock.Day == 1 && world.Session.BoxSeal == 1 && world.Session.LakeSeal == 1 && world.Session.ExchangeCount == 0 && !world.Finale.TimeReserved, "Mechanical A changed rune/time/SEAL state.");
                    Capture("t06"); Next(90, now);
                }
                else if (stage == 90 && now - since > .5) { Key(KeyCode.Escape); Next(10, now); }
                else if (stage == 10 && now - since > .7)
                {
                    Release(); var body = world.Player.GetComponent<CharacterController>(); body.enabled = false;
                    world.Player.transform.SetPositionAndRotation(box.Wheel.transform.position + new Vector3(0, -.81f, -2), Quaternion.identity); body.enabled = true;
                    Cursor.lockState = CursorLockMode.Locked; InputSystem.QueueStateEvent(mouse, new MouseState { delta = new Vector2(0, -300) }); Next(11, now);
                }
                else if (stage == 11 && now - since > .3) { Key(KeyCode.E); Next(12, now); }
                else if (stage == 12 && now - since > .9)
                {
                    Release(); Require(Vector3.Distance(world.Player.ViewCamera.transform.position, box.Wheel.CloseView.position) < .03f && !world.Player.Reading && !world.Player.InspectingLock, "E failed to inspect authored wheel.");
                    var labels = box.Wheel.GetComponentsInChildren<TextMesh>().Where(t => t.name.StartsWith("DirectionDigit_")).ToArray();
                    Require(labels.Length == 10 && labels.All(t => t.text == box.Wheel.Definition.DigitsClockwise[int.Parse(t.name.Substring(15))].ToString()), "Wheel labels disagree with saved mapping.");
                    Capture("wheel"); Key(KeyCode.Escape); Next(13, now);
                }
                else if (stage == 13 && now - since > .7)
                {
                    Release(); box.Use(world.Player); box.Use(world.Player); Require(world.Campaign.Progress.FoundCount == 1, "Repeated T06 reading duplicated discovery.");
                    world.Player.ExitCloseView(); for (int i = 0; i < 8 && !world.Session.Descended; i++) world.Sleep();
                    Require(world.Session.Descended && world.Session.Clock.Day == 9 && world.Campaign.Progress.FoundCount == 1 && world.Session.ExchangeCount == 0, "Early acquisition broke sleeping to D9 or fabricated T07.");
                    Complete(true, "D1 physical W/E lock/wheel, digit raycasts, wrong retry, Esc input persistence, correct unlock then separate T06 pickup, repeated discovery, blade-orientation invariance, no B/SEAL/time/finale mutation, saved mapping/view and sleep-to-D9 passed. Tuning=" + (operation == "cipher-tuning"));
                }
            }
            catch (Exception e) { Complete(false, "Stage " + stage + ": " + e); }
        }
        void Next(int next, double now) { stage = next; since = now; }
        void Key(KeyCode key)
        {
            var mapped = key == KeyCode.W ? UnityEngine.InputSystem.Key.W : key == KeyCode.E ? UnityEngine.InputSystem.Key.E : key == KeyCode.Return ? UnityEngine.InputSystem.Key.Enter : UnityEngine.InputSystem.Key.Escape;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(mapped));
        }
        void Release() => InputSystem.QueueStateEvent(keyboard, new KeyboardState());
        void Current() { keyboard?.MakeCurrent(); mouse?.MakeCurrent(); }
        void Click(Transform target, bool down)
        {
            Vector3 screen = world.Player.ViewCamera.WorldToScreenPoint(target.position);
            Require(screen.z > 0 && screen.x > 0 && screen.x < Screen.width && screen.y > 0 && screen.y < Screen.height, "Lock button outside authored view.");
            var hit = Physics.RaycastAll(world.Player.ViewCamera.ScreenPointToRay(screen), 20).Where(h => !h.collider.transform.IsChildOf(world.Player.transform)).OrderBy(h => h.distance).FirstOrDefault();
            Require(hit.collider && hit.collider.transform.IsChildOf(target), "Digit ray blocked by " + (hit.collider ? hit.collider.name : "nothing"));
            InputSystem.QueueStateEvent(mouse, new MouseState { position = new Vector2(screen.x, screen.y), buttons = (ushort)(down ? 1 : 0) });
        }
        void Capture(string name) => ScreenCapture.CaptureScreenshot(Path.GetFullPath("Temp/DemonSliceReports/" + operation + "-" + name + ".png"));
        public void Cleanup()
        {
            if (keyboard == null) return; InputSystem.onAfterUpdate -= Current; InputSystem.RemoveDevice(keyboard); InputSystem.RemoveDevice(mouse);
            InputSystem.settings.backgroundBehavior = oldBackground; InputSystem.settings.editorInputBehaviorInPlayMode = oldInput; keyboard = null; mouse = null;
        }
        void Complete(bool success, string message) { finished = true; Cleanup(); finish(success, message); EditorApplication.delayCall += EditorApplication.ExitPlaymode; }
    }
}
