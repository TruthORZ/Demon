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
    public sealed class PrototypeSmokeRun : ISmokeRun
    {
        readonly string id, operation, reportDirectory;
        readonly Action<bool, string> finish;
        readonly double start = EditorApplication.timeSinceStartup;
        GameSceneController root;
        int stage;
        int firstFrame;
        double stageTime;
        bool captured;
        bool finished;
        Vector3 startingPosition;
        Transform[] clickTargets;
        int inputStep;
        double initialMinutes;
        float initialGameTime;
        InputSettings.BackgroundBehavior oldBackground;
        InputSettings.EditorInputBehaviorInPlayMode oldEditorInput;
        Keyboard testKeyboard;
        Mouse testMouse;

        public PrototypeSmokeRun(string reportId, string op, string directory, Action<bool, string> callback)
        { id = reportId; operation = op; reportDirectory = directory; finish = callback; }

        void Require(bool condition, string message)
        { if (!condition) throw new InvalidOperationException(message); }

        public void Tick()
        {
            if (finished) return;
            try
            {
                double now = EditorApplication.timeSinceStartup;
                if (now - start > 120) throw new TimeoutException("Play Mode did not finish within 120 seconds.");
                if (!Application.isPlaying) return;
                if (!root) root = UnityEngine.Object.FindAnyObjectByType<GameSceneController>();
                if (!root || root.Session == null) return;
                if (stage == 0)
                {
                    firstFrame = Time.frameCount;
                    initialMinutes = root.Session.Clock.AbsoluteMinutes;
                    initialGameTime = Time.time;
                    Require(root.Session.Clock.Day == 8 && root.Session.BoxSeal == 1 && root.Session.LakeSeal == 0, "Wrong authored initial state.");
                    Capture("start");
                    if (operation == "smoke-input")
                    {
                        oldBackground = InputSystem.settings.backgroundBehavior;
                        oldEditorInput = InputSystem.settings.editorInputBehaviorInPlayMode;
                        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
                        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
                        testKeyboard = InputSystem.AddDevice<Keyboard>("DemonSmokeKeyboard");
                        testMouse = InputSystem.AddDevice<Mouse>("DemonSmokeMouse");
                        InputSystem.onAfterUpdate += MakeTestDevicesCurrent;
                        Require(Keyboard.current != null && Mouse.current != null, "Keyboard/mouse device missing.");
                        startingPosition = root.Player.transform.position;
                        Cursor.lockState = CursorLockMode.Locked;
                        InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(Key.W));
                        stageTime = now; stage = 10; return;
                    }
                    root.Tablet.Use(root.Player);
                    Require(root.Player.InspectingTablet, "Close view did not acquire input ownership.");
                    stageTime = now; stage = 1;
                }
                else if (stage == 1 && now - stageTime > 2)
                {
                    Require(Time.frameCount > firstFrame + 5, "Game frames are not advancing.");
                    if (operation == "smoke-editability")
                    {
                        double rate = (root.Session.Clock.AbsoluteMinutes - initialMinutes) / (Time.time - initialGameTime);
                        Require(Math.Abs(rate - 2) < .1, "Saved day-length tuning was ignored.");
                        Require(Vector3.Distance(root.Player.ViewCamera.transform.position, root.Tablet.CloseView.position) < .03f,
                            "Saved camera tuning was ignored.");
                        float hour = (float)root.Session.Clock.MinuteOfDay / 60;
                        float expected = Mathf.Lerp(root.Settings.NightSunIntensity, .37f, Mathf.Clamp01(Mathf.Sin((hour - 6) * Mathf.PI / 12)));
                        Require(Mathf.Abs(RenderSettings.sun.intensity - expected) < .01f, "Saved sunlight tuning was ignored.");
                        finish(true, "Saved day length, camera anchor and sunlight edits respected in Play Mode; restore on exit.");
                        finished = true;
                        EditorApplication.delayCall += EditorApplication.ExitPlaymode;
                        return;
                    }
                    if (!captured) { Capture("tablet"); captured = true; stageTime = now; return; }
                    captured = false;
                    if (operation == "smoke-sleep")
                    {
                        root.Player.ExitCloseView(); root.Sleep();
                    }
                    else
                    {
                        Require(!root.TryCompile(), "An incomplete tablet unexpectedly compiled.");
                        var board = root.Session.Board;
                        board.PickFromTray(RuneKind.Exchange); board.ClickSlot(4);
                        board.PickFromTray(RuneKind.Box); board.ClickSlot(7);
                        board.PickFromTray(RuneKind.Lake); board.ClickSlot(8);
                        Require(root.TryCompile(), "Target tablet did not compile.");
                        Require(root.Session.ExchangeCount == 1 && root.Session.BoxSeal == 0 && root.Session.LakeSeal == 1, "First exchange failed.");
                        board.ClickSlot(4);
                        Require(board.CompiledAt(4) == RuneKind.Exchange, "Surface removal erased the program.");
                        UnityEngine.Object.FindAnyObjectByType<RecordBox>().Use(root.Player);
                        Require(root.Player.Reading && root.HasReadBox, "Box text did not open.");
                    }
                    stage = 2; stageTime = now;
                }
                else if (stage == 2)
                {
                    if (!captured && now - stageTime > 2)
                    {
                        Capture("record"); captured = true;
                    }
                    if (captured && now - stageTime > 3 && root.Player.Reading) root.Player.ExitCloseView();
                    if (root.Session.Descended)
                    {
                        Require(root.Session.Clock.Day == 9 && root.Session.Clock.MinuteOfDay == 0, "Terminal time leaked beyond D9 midnight.");
                        Require(root.Session.HasBody && root.Session.LakeSeal == 0, "Descent state is inconsistent.");
                        Require(root.Session.ExchangeCount == (operation == "smoke-sleep" ? 0 : 2), "Wrong exchange count.");
                        stageTime = now; stage = 3;
                    }
                }
                else if (stage == 3 && now - stageTime > 2)
                {
                    Capture("ending");
                    finish(true, $"Play Mode {operation}: frames {firstFrame}->{Time.frameCount}, D9 midnight, exchanges={root.Session.ExchangeCount}; close view, target, box and terminal states verified.");
                    finished = true;
                    EditorApplication.delayCall += EditorApplication.ExitPlaymode;
                }
                else if (stage == 10 && now - stageTime > 1)
                {
                    InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(Key.E));
                    Require(root.Player.transform.position.z > startingPosition.z + .5f, "W did not move the CharacterController through Input System: position=" + root.Player.transform.position + ", cursor=" + Cursor.lockState);
                    stage = 11; stageTime = now;
                }
                else if (stage == 11 && now - stageTime > 1)
                {
                    InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState());
                    Require(root.Player.InspectingTablet, "Physical E press did not enter close view.");
                    Require(Vector3.Distance(root.Player.ViewCamera.transform.position, root.Tablet.CloseView.position) < .03f,
                        "Authored close camera anchor was not respected.");
                    var trays = root.Tablet.GetComponentsInChildren<RuneTrayItem>();
                    var slots = root.Tablet.GetComponentsInChildren<RuneSlot>();
                    clickTargets = new[] {
                        trays.First(t => t.Rune == RuneKind.Exchange).transform, slots.First(s => s.Index == 4).transform,
                        trays.First(t => t.Rune == RuneKind.Box).transform, slots.First(s => s.Index == 7).transform,
                        trays.First(t => t.Rune == RuneKind.Lake).transform, slots.First(s => s.Index == 8).transform };
                    stage = 12; stageTime = now;
                }
                else if (stage == 12 && now - stageTime > .2)
                {
                    if (inputStep >= clickTargets.Length * 2)
                    {
                        Require(root.Session.Board.MatchesTarget() && !root.Session.Board.Frozen,
                            "Mouse raycasts did not place the target layout, or it froze before confirmation.");
                        Capture("input-layout");
                        InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(Key.Enter));
                        stage = 13; stageTime = now;
                    }
                    else
                    {
                        Vector3 screen = root.Player.ViewCamera.WorldToScreenPoint(clickTargets[inputStep / 2].position);
                        Require(screen.z > 0 && screen.x > 0 && screen.x < Screen.width && screen.y > 0 && screen.y < Screen.height,
                            "A selectable authored rune is outside the close camera view.");
                        Ray clickRay = root.Player.ViewCamera.ScreenPointToRay(screen);
                        var clickHit = Physics.RaycastAll(clickRay, 20).Where(h => !h.collider.transform.IsChildOf(root.Player.transform))
                            .OrderBy(h => h.distance).FirstOrDefault();
                        Require(clickHit.collider && clickHit.collider.transform.IsChildOf(clickTargets[inputStep / 2]),
                            "Authored target is blocked: " + clickTargets[inputStep / 2].name + " hit " + (clickHit.collider ? clickHit.collider.name : "nothing"));
                        InputSystem.QueueStateEvent(testMouse, new MouseState {
                            position = new Vector2(screen.x, screen.y), buttons = (ushort)(inputStep % 2 == 0 ? 1 : 0) });
                        inputStep++; stageTime = now;
                    }
                }
                else if (stage == 13 && now - stageTime > .6)
                {
                    InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState());
                    Require(root.Session.Board.Frozen && root.Session.ExchangeCount == 1,
                        "Enter did not confirm exactly one exchange.");
                    stage = 14; stageTime = now;
                }
                else if (stage == 14 && now - stageTime > 1)
                {
                    var box = UnityEngine.Object.FindAnyObjectByType<RecordBox>();
                    var body = root.Player.GetComponent<CharacterController>();
                    body.enabled = false;
                    root.Player.transform.SetPositionAndRotation(box.transform.position + Vector3.back * 2, Quaternion.identity);
                    body.enabled = true;
                    // Approach is level; looking down is required to aim at the waist-high box.
                    InputSystem.QueueStateEvent(testMouse, new MouseState { delta = new Vector2(0, -250) });
                    stage = 15; stageTime = now;
                }
                else if (stage == 15 && now - stageTime > .5)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    InputSystem.QueueStateEvent(testKeyboard, new KeyboardState(Key.E));
                    stage = 16; stageTime = now;
                }
                else if (stage == 16 && now - stageTime > 1)
                {
                    Require(root.Player.Reading && root.HasReadBox, "E on the saved box instance did not open its record; focus=" + root.Player.FocusPrompt);
                    Capture("box-reading");
                    InputSystem.QueueStateEvent(testKeyboard, new KeyboardState());
                    stage = 17; stageTime = now;
                }
                else if (stage == 17 && now - stageTime > .5)
                {
                    InputSystem.QueueStateEvent(testKeyboard, new KeyboardState(Key.Escape));
                    stage = 18; stageTime = now;
                }
                else if (stage == 18 && now - stageTime > 1)
                {
                    Require(!root.Player.Reading, "Escape did not leave the box record.");
                    finish(true, "W/E, six rune raycasts, Enter, physical E on saved box, reading and Escape passed.");
                    finished = true;
                    RestoreInput();
                    EditorApplication.delayCall += EditorApplication.ExitPlaymode;
                }
            }
            catch (Exception exception)
            {
                finish(false, exception.ToString());
                finished = true;
                RestoreInput();
                EditorApplication.delayCall += EditorApplication.ExitPlaymode;
            }
        }

        public void Cleanup() => RestoreInput();
        void RestoreInput()
        {
            if (testKeyboard == null) return;
            InputSystem.onAfterUpdate -= MakeTestDevicesCurrent;
            InputSystem.RemoveDevice(testKeyboard);
            InputSystem.RemoveDevice(testMouse);
            InputSystem.settings.backgroundBehavior = oldBackground;
            InputSystem.settings.editorInputBehaviorInPlayMode = oldEditorInput;
            testKeyboard = null; testMouse = null;
        }

        void MakeTestDevicesCurrent()
        {
            testKeyboard.MakeCurrent();
            testMouse.MakeCurrent();
        }

        void Capture(string label)
        {
            Directory.CreateDirectory(reportDirectory);
            ScreenCapture.CaptureScreenshot(Path.GetFullPath(Path.Combine(reportDirectory, id + "-" + label + ".png")));
        }
    }
}
