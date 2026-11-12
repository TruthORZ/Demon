using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Demon.Slice.Domain;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Demon.Slice.Editor
{
    // Frame-separated engine/input checks, distinct from route replay and real user acceptance.
    public sealed class CompletePlaythroughIntegrationRun : ISmokeRun
    {
        readonly Action<bool, string> finish;
        readonly double started = EditorApplication.timeSinceStartup;
        int stage, frame;
        double since, minute;
        Keyboard keyboard; Mouse mouse;
        InputSettings.BackgroundBehavior oldBackground;
        InputSettings.EditorInputBehaviorInPlayMode oldInput;
        GameSceneController world;
        CompletePlaythrough flow;
        bool done, devices;
        Quaternion sun, savedVane;
        Vector3 savedDestination;
        Vector2 slotClickPoint;
        InputActionAsset uiActions;
        InputDevice[] previousUIDevices;
        public CompletePlaythroughIntegrationRun(Action<bool, string> completed) { finish = completed; }
        void Next(int value) { stage = value; frame = Time.frameCount; since = EditorApplication.timeSinceStartup; }
        bool Ready => Time.frameCount > frame + 3 && EditorApplication.timeSinceStartup - since > .3;
        public void Tick()
        {
            if (done || !Application.isPlaying) return;
            EditorApplication.QueuePlayerLoopUpdate();
            try
            {
                if (EditorApplication.timeSinceStartup - started > 150) throw new Exception("Integration timeout at stage " + stage);
                if (stage != 0 && !Ready) return;
                if (stage == 0)
                {
                    world = UnityEngine.Object.FindAnyObjectByType<GameSceneController>(); if (!world || world.Session == null || Time.frameCount < 4) return;
                    flow = world.Playthrough; flow.BeginDebugReplay();
                    world.Restart(); world.Session.Clock.AdvanceTo(world.Session.Clock.NextMidnight);
                    Require(flow.Failed, "Plain restart did not fail incomplete D1."); flow.RetryDay();
                    Require(!flow.Failed && world.Session.Clock.Day == 1, "Plain R restart did not seed a retry checkpoint.");
                    oldBackground = InputSystem.settings.backgroundBehavior; oldInput = InputSystem.settings.editorInputBehaviorInPlayMode;
                    InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
                    InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
                    keyboard = InputSystem.AddDevice<Keyboard>(); mouse = InputSystem.AddDevice<Mouse>(); devices = true; InputSystem.onAfterUpdate += MakeCurrent; MakeCurrent();
                    uiActions = (EventSystem.current.currentInputModule as InputSystemUIInputModule)?.actionsAsset;
                    Require(uiActions, "Missing authored UI input module/actions."); previousUIDevices = uiActions.devices?.ToArray();
                    uiActions.devices = new InputDevice[] { keyboard, mouse }; // Keep hardware motion from replacing the test pointer.
                    var note = world.gameObject.scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<StoryPickup>(true)).First(x => x.EntryId == "note-a");
                    Require(CompletePlaythroughAuthoring.TryStandpoint(note, world, out var feet, out var aim), "Critical paper A has no clear standing/raycast position.");
                    MovePlayer(feet);
                    var desired = Quaternion.LookRotation(aim - world.Player.ViewCamera.transform.position);
                    float yaw = Mathf.DeltaAngle(world.Player.transform.eulerAngles.y, desired.eulerAngles.y);
                    float pitch = Mathf.DeltaAngle(0, desired.eulerAngles.x);
                    float current = Mathf.DeltaAngle(0, world.Player.ViewCamera.transform.localEulerAngles.x);
                    InputSystem.QueueDeltaStateEvent(mouse.delta, new Vector2(yaw, current - pitch) / world.Settings.MouseSensitivity);
                    Next(100);
                }
                else if (stage == 100)
                { Press(Key.E); Next(101); }
                else if (stage == 101)
                {
                    Require(flow.Records.Contains("note-a") && world.Player.Reading, "The actual E did not collect readable paper A.");
                    Press(); world.Player.ReturnToExploration(); flow.Collect("flashlight", world.Player.transform);
                    Press(Key.Tab); Next(1);
                }
                else if (stage == 1)
                {
                    Require(flow.BlocksClock && world.Player.MenuOpen && Time.timeScale == 0, "Tab did not open/pause journal.");
                    minute = world.Session.Clock.AbsoluteMinutes; Press();
                    ScreenCapture.CaptureScreenshot(Path.GetFullPath("Temp/DemonSliceReports/complete-journal.png"));
                    var button = flow.GetComponentInParent<GameSceneController>().Playthrough;
                    var close = new SerializedObject(button).FindProperty("journalClose").objectReferenceValue as Button;
                    var hits = new List<RaycastResult>(); EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = RectTransformUtility.WorldToScreenPoint(null, close.transform.position) }, hits);
                    Require(hits.Count > 0 && hits[0].gameObject.GetComponentInParent<Button>() == close, "Journal close button is not pointer accessible.");
                    var inventory = (JournalInventory)new SerializedObject(flow).FindProperty("journalInventory").objectReferenceValue;
                    Require(inventory.AuthoredSlots.Count >= 24 && inventory.ItemCount == 2, "Grid did not keep one slot per collected item.");
                    if (SessionState.GetBool("Demon.Complete.Editability", false))
                        Require(Mathf.Abs(inventory.GetComponentInChildren<GridLayoutGroup>().spacing.x - SessionState.GetFloat("Demon.Complete.GridExpectedX", -1)) < .001f, "Saved grid spacing was overwritten at runtime.");
                    var paper = inventory.AuthoredSlots.Single(x => x.ItemId == "note-a");
                    var rect = paper.Button.GetComponent<RectTransform>();
                    Vector2 point = RectTransformUtility.WorldToScreenPoint(null, rect.position);
                    hits.Clear(); EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = point }, hits);
                    Require(hits.Count > 0 && hits[0].gameObject.GetComponentInParent<JournalSlot>() == paper, "Paper slot is not pointer reachable.");
                    slotClickPoint = point; InputSystem.QueueStateEvent(mouse, new MouseState { position = point }); Next(200);
                }
                else if (stage == 200)
                { InputSystem.QueueStateEvent(mouse, new MouseState { position = slotClickPoint }.WithButton(MouseButton.Left)); Next(201); }
                else if (stage == 201)
                { InputSystem.QueueStateEvent(mouse, new MouseState { position = slotClickPoint }); Next(202); }
                else if (stage == 202)
                {
                    var inventory = (JournalInventory)new SerializedObject(flow).FindProperty("journalInventory").objectReferenceValue;
                    Require(inventory.SelectedId == "note-a" && inventory.DetailText == flow.Definition.Entry("note-a").Body, "Paper click failed; selection=" + inventory.SelectedId + "; point=" + mouse.position.ReadValue() + "; cursor=" + Cursor.lockState + "; UI=" + EventSystem.current.currentInputModule);
                    Require(!world.Tether.LampSwitchedOn, "Inventory click toggled the flashlight.");
                    Require(world.Session.Clock.AbsoluteMinutes == minute, "Slot interaction leaked paused world time.");
                    ScreenCapture.CaptureScreenshot(Path.GetFullPath("Temp/DemonSliceReports/complete-journal-grid.png"));
                    var overflow = Enumerable.Range(0, 30).Select(i => new JournalInventory.Item("probe-" + i, "测试" + i, "正文", "纸条")).ToList();
                    overflow.Add(overflow[0]); inventory.Show(overflow);
                    Require(inventory.ItemCount == 30 && inventory.GetComponentsInChildren<JournalSlot>().Count(x => x.ItemId != null) == 30, "Grid overflow lost an item or duplicated a record.");
                    inventory.Show(Array.Empty<JournalInventory.Item>());
                    Require(inventory.ItemCount == 0 && inventory.AuthoredSlots.All(x => x.gameObject.activeSelf && x.ItemId == null), "Empty inventory did not retain its authored cells.");
                    inventory.Show(new[] { new JournalInventory.Item("note-a", flow.Definition.Entry("note-a").Title, flow.Definition.Entry("note-a").Body, "纸条"), new JournalInventory.Item("flashlight", "手电筒", flow.Definition.Entry("flashlight").Body, "道具") });
                    var close = (Button)new SerializedObject(flow).FindProperty("journalClose").objectReferenceValue;
                    slotClickPoint = RectTransformUtility.WorldToScreenPoint(null, close.transform.position);
                    InputSystem.QueueStateEvent(mouse, new MouseState { position = slotClickPoint }); Next(211);
                }
                else if (stage == 211)
                { InputSystem.QueueStateEvent(mouse, new MouseState { position = slotClickPoint }.WithButton(MouseButton.Left)); Next(212); }
                else if (stage == 212)
                { InputSystem.QueueStateEvent(mouse, new MouseState { position = slotClickPoint }); Next(213); }
                else if (stage == 213)
                {
                    Require(!world.Player.MenuOpen && !world.Tether.LampSwitchedOn, "Close-button click leaked into the lamp switch.");
                    Press(Key.Tab); Next(214);
                }
                else if (stage == 214)
                { Require(world.Player.MenuOpen, "Journal could not reopen."); minute = world.Session.Clock.AbsoluteMinutes; Press(); Next(2); }
                else if (stage == 2)
                { Require(world.Session.Clock.AbsoluteMinutes == minute, "Journal leaked world time."); Press(Key.Escape); Next(3); }
                else if (stage == 3)
                {
                    Require(!flow.BlocksClock && !world.Player.MenuOpen && Time.timeScale == 1, "Esc failed to restore journal pause."); Press();
                    InputSystem.QueueStateEvent(mouse, new MouseState().WithButton(MouseButton.Left)); Next(204);
                }
                else if (stage == 204)
                { Require(world.Tether.LightVisible, "Left click did not switch the flashlight on."); InputSystem.QueueStateEvent(mouse, new MouseState()); Next(4); }
                else if (stage == 4)
                {
                    Require(world.Tether.LightVisible && !world.Tether.Rule.Locked && world.Tether.Rule.ExchangeCount == 0, "Ordinary lamp failed or granted T06 motion.");
                    InputSystem.QueueStateEvent(mouse, new MouseState().WithButton(MouseButton.Right)); Next(205);
                }
                else if (stage == 205)
                { Require(world.Tether.LightVisible, "Right click changed the latched lamp."); InputSystem.QueueStateEvent(mouse, new MouseState().WithButton(MouseButton.Left)); Next(206); }
                else if (stage == 206)
                { Require(!world.Tether.LightVisible, "Second left click did not switch the lamp off."); InputSystem.QueueStateEvent(mouse, new MouseState()); Next(207); }
                else if (stage == 207)
                { InputSystem.QueueStateEvent(mouse, new MouseState().WithButton(MouseButton.Left)); Next(208); }
                else if (stage == 208)
                {
                    InputSystem.QueueStateEvent(mouse, new MouseState());
                    Require(world.Tether.LampSwitchedOn, "Lamp switch did not latch."); flow.CaptureDayStart(); Next(209);
                }
                else if (stage == 209)
                { InputSystem.QueueStateEvent(mouse, new MouseState().WithButton(MouseButton.Left)); Next(210); }
                else if (stage == 210)
                {
                    Require(!world.Tether.LampSwitchedOn, "Snapshot probe failed to turn off the lamp."); InputSystem.QueueStateEvent(mouse, new MouseState());
                    world.Session.Clock.AdvanceTo(world.Session.Clock.NextMidnight); flow.RetryDay();
                    Require(world.Tether.LampSwitchedOn && flow.Records.Contains("note-a"), "Day retry lost the lamp switch or collected paper.");
                    var finale = new SerializedObject(world.Finale); Require(!finale.FindProperty("secondHand").objectReferenceValue, "Main clock second hand remains.");
                    world.Restart(); CompletedDayReplay.Run(world, 3, false); Next(5);
                }
                else if (stage == 5)
                {
                    var saved = new SerializedObject(flow); var safe = (BoxCollider)saved.FindProperty("cabinSafeZone").objectReferenceValue;
                    MovePlayer(safe.transform.TransformPoint(safe.center));
                    Require(flow.InsideCabin(world.Player.transform.position), "Cabin safe bounds are invalid.");
                    var door = world.gameObject.scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<CabinDoorInteraction>(true)).First(); door.Use(world.Player);
                    Next(6);
                }
                else if (stage == 6)
                {
                    var door = world.gameObject.scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<CabinDoorInteraction>(true)).First();
                    Require(door.Capture().opened, "Main-route door did not open.");
                    var link = (Behaviour)new SerializedObject(door).FindProperty("doorwayNavigation").objectReferenceValue; Require(!link.enabled, "Open door exposed enemy cabin navigation.");
                    Require(!world.Chase.Tracking && world.Chase.Phase == ChaseController.ChasePhase.Waiting, "Chase did not protect cabin occupant.");
                    Require(!flow.InsideCabin(world.Chase.Agent.transform.position), "Demon remained inside cabin.");
                    Require(Mathf.Abs(world.Chase.Agent.speed - flow.Definition.ChaseSpeeds[0]) < .001f, "D3 speed mismatch.");
                    world.Exploration.Observations.Single(x => x.Site == ObservationSite.WindVane && !x.IsForest).Use(world.Player);
                    Next(30);
                }
                else if (stage == 30)
                {
                    if (EditorApplication.timeSinceStartup - since < flow.Definition.NoticeSeconds + 1) return;
                    Require(world.Player.Reading, "Notice probe lost its observation page.");
                    world.Player.ReturnToExploration(); Next(31);
                }
                else if (stage == 31)
                {
                    var notice = (Text)new SerializedObject(flow).FindProperty("noticeText").objectReferenceValue;
                    Require(notice.gameObject.activeSelf && notice.text.Contains("现在明明有风"), "Observation notice expired while hidden by the reading page.");
                    var agent = world.Chase.Agent;
                    Require(NavMesh.SamplePosition(agent.transform.position + Vector3.forward * 5, out var goal, 4, agent.areaMask), "No saved destination probe point.");
                    Require(agent.SetDestination(goal.position), "Destination probe failed."); Next(20);
                }
                else if (stage == 20)
                {
                    if (world.Chase.Agent.pathPending) return;
                    Require(world.Chase.Agent.hasPath, "Destination probe did not produce a path.");
                    savedDestination = world.Chase.Agent.destination; savedVane = world.Tether.VaneBlade.localRotation; flow.CaptureDayStart();
                    world.Chase.Agent.ResetPath(); world.Tether.VaneBlade.Rotate(Vector3.up, 70);
                    world.Session.Clock.AdvanceTo(world.Session.Clock.NextMidnight); Require(flow.Failed, "Snapshot probe did not fail its day.");
                    flow.RetryDay();
                    Require(Vector3.Distance(world.Chase.Agent.destination, savedDestination) < .05f, "Retry lost the actual navigation destination.");
                    Require(Quaternion.Angle(world.Tether.VaneBlade.localRotation, savedVane) < .01f, "Retry lost the actual vane rotation.");
                    world.Restart(); CompletedDayReplay.Run(world, 7, false); Next(7);
                }
                else if (stage == 7)
                {
                    Require(world.Presentation.PermanentNight && world.Consequences.WhiteRimVisible, "D7 lost light/rim failed.");
                    sun = world.Presentation.Sun.transform.rotation;
                    world.Session.Clock.AdvanceBy(120); Next(8);
                }
                else if (stage == 8)
                {
                    Require(Quaternion.Angle(sun, world.Presentation.Sun.transform.rotation) < .01f, "Permanent-night sun continued moving.");
                    var before = world.Cipher.Wheel.ReadCode(); world.Session.Clock.AdvanceBy(120); var after = world.Cipher.Wheel.ReadCode();
                    Require(before.SequenceEqual(after), "Solar motion changed fixed cipher answer.");
                    world.Restart(); CompletedDayReplay.Run(world, 8, false); Next(9);
                }
                else if (stage == 9)
                {
                    Require(world.Chase.Agent.speed > world.Settings.SprintMetresPerSecond, "D8 speed must exceed sprint.");
                    world.Exploration.HandoverRecord.Use(world.Player); world.Player.ReturnToExploration();
                    var safe = (BoxCollider)new SerializedObject(flow).FindProperty("cabinSafeZone").objectReferenceValue;
                    MovePlayer(safe.transform.TransformPoint(safe.center)); Press(Key.T); Next(10);
                }
                else if (stage == 10)
                { Require(flow.Waiting, "Actual T input did not start handover wait."); Press(); Next(11); }
                else if (stage == 11)
                {
                    if (flow.Waiting) return;
                    Require(world.Campaign.FinalAvailable && flow.Tasks.Contains("wait"), "Wait did not reach real night availability.");
                    var pickup = world.Retrieval.Pickup; pickup.Use(world.Player); world.Player.ReturnToExploration();
                    world.Session.Clock.AdvanceTo(world.Finale.LimitNaturalTime(world.Session.Clock.NextMidnight + 300));
                    Require(world.Session.Clock.Day == 8 && !flow.Failed, "Reserved T07 window did not protect midnight.");
                    ScreenCapture.CaptureScreenshot(Path.GetFullPath("Temp/DemonSliceReports/complete-night.png"));
                    File.WriteAllText("Temp/DemonSliceReports/complete-integration-evidence.txt", "Actual E-key paper A from a clear physical standing position; central observation notice survives a long reading page; plain R/restart failure and retry; navigation destination/vane rotation snapshot restore; Tab/Esc journal pause/pointer raycast; ordinary lamp without motion; D3 speed and open-door cabin exclusion; D7 fixed sun/white rim/cipher; D8 faster than sprint; T-key waiting and reserved T07 window passed.");
                    world.Player.EnterInspectionView(world.Finale.ClockView); Next(12);
                }
                else if (stage == 12)
                {
                    ScreenCapture.CaptureScreenshot(Path.GetFullPath("Temp/DemonSliceReports/complete-clock-twohands.png"));
                    done = true; finish(true, "Left-click latch/right-click ignored/retry switch; actual slot click/close isolation, empty and 30-item overflow; saved grid tuning; two-hand clock plus main-route integration passed."); EditorApplication.ExitPlaymode();
                }
            }
            catch (Exception error) { done = true; finish(false, error.ToString()); EditorApplication.ExitPlaymode(); }
        }
        void Press(params Key[] keys) => InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
        void MakeCurrent() { keyboard?.MakeCurrent(); mouse?.MakeCurrent(); }
        void MovePlayer(Vector3 point)
        { var body = world.Player.GetComponent<CharacterController>(); body.enabled = false; world.Player.transform.position = point; body.enabled = true; world.Player.ReturnToExploration(); }
        public void Cleanup()
        {
            if (devices)
            { if (uiActions) uiActions.devices = previousUIDevices; InputSystem.onAfterUpdate -= MakeCurrent; InputSystem.RemoveDevice(keyboard); InputSystem.RemoveDevice(mouse); InputSystem.settings.backgroundBehavior = oldBackground; InputSystem.settings.editorInputBehaviorInPlayMode = oldInput; }
            Time.timeScale = 1; AudioListener.pause = false;
        }
        static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    }
}
