using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Demon.Slice.Editor
{
    // Physical keyboard/mouse movement through saved routes. The production chase remains enabled.
    public sealed class InvestigationSmokeRun
    {
        readonly string operation;
        readonly Action<bool, string> finish;
        readonly double started = EditorApplication.timeSinceStartup;
        SliceSceneController world;
        ExplorationFlow flow;
        Keyboard keyboard; Mouse mouse;
        InputSettings.BackgroundBehavior oldBackground;
        InputSettings.EditorInputBehaviorInPlayMode oldInput;
        bool oldRun, finished, sawAgentMove;
        int stage, routeIndex, corner, lastFrame = -1, repeat, afterRead, expectedFound;
        double since, lastSteer, walkStarted;
        float metres;
        Vector3 previous;
        Vector3[] corners;
        NineDayTablet readingTablet;
        bool Late => operation == "investigation-late" || operation == "investigation-review";
        bool Tuning => operation == "investigation-tuning";
        public InvestigationSmokeRun(string op, Action<bool, string> callback) { operation = op; finish = callback; }
        static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
        public void Tick()
        {
            if (finished || !Application.isPlaying) return;
            try
            {
                if (EditorApplication.timeSinceStartup - started > (operation == "investigation-review" ? 600 : 210)) throw new TimeoutException("Investigation stage " + stage);
                if (!world) world = UnityEngine.Object.FindAnyObjectByType<SliceSceneController>();
                if (!world || world.Session == null || !world.Exploration || world.Exploration.Log == null) return;
                if (lastFrame == Time.frameCount) return; lastFrame = Time.frameCount;
                flow = world.Exploration; double now = Time.timeAsDouble;
                if (world.Chase.HasNavigation && world.Chase.Agent.velocity.sqrMagnitude > .1f) sawAgentMove = true;
                Require(world.Session.ExchangeCount == 0 && flow.Log.Count == 0, "Search fabricated SEAL/D1 completion.");
                if (stage == 0)
                {
                    Require(flow.HasInvestigation && world.Campaign.Progress.FoundCount == 0 && !world.Cipher.Acquired, "Saved route or clean discovery/tool state missing.");
                    Require(Math.Abs(world.Settings.WalkMetresPerSecond - (Tuning ? 2.8f : 3.3f)) < .001f && flow.Definition.Day3Guide.StartsWith("【保存参数检查】") == Tuning, "Saved tuning ignored.");
                    if (operation == "investigation-boundary")
                    {
                        Require(world.Session.Clock.Day == 2 && !flow.LargeTrailVisible && !flow.SecondTablet.gameObject.activeInHierarchy && !flow.ThirdTablet.gameObject.activeInHierarchy && !world.Tether.MotionReady, "Large traces/T02/T03 appeared before D3.");
                        Next(80, now); return;
                    }
                    Require(world.Session.Clock.Day == (Late ? 4 : 3) && flow.LargeTrailVisible && flow.SecondTablet.gameObject.activeInHierarchy && flow.ThirdTablet.gameObject.activeInHierarchy == Late, "D3/D4 display dates failed.");
                    Require(world.Tether.MotionReady && world.Consequences.Rule.SoundExchangeCount == 0 && world.Consequences.Rule.LightTransferCount == 0, "Search changed capability dates.");
                    if (operation == "investigation-skip") { world.Sleep(); Next(90, now); return; }
                    SetupInput(); previous = world.Player.transform.position;
                    Capture("start-guide");
                    if (Late) { routeIndex = 0; WalkTo(flow.RitualRoute[0].position); Next(21, now); }
                    else { Require(flow.Status == flow.Definition.Day3Guide, "D3 saved guide ignored."); WalkTo(flow.Observations[1].Approach.position); Next(1, now); }
                }
                else if (stage == 1 && Walk(now)) { Aim(flow.Observations[1].AimTarget.position); Next(2, now); }
                else if (stage == 2 && now - since > .3) { Keys(Key.E); Next(3, now); }
                else if (stage == 3 && now - since > .3)
                {
                    Keys(); Require(world.Player.Reading && world.ReadingText.Contains(flow.Definition.WindVane.Changed) && world.Session.Clock.Day == 3, "Physical stopped-vane observation failed: " + world.Player.FocusPrompt); Next(4, now);
                }
                else if (stage == 4 && now - since > .7) { Capture("stopped-vane"); Next(7, now); }
                else if (stage == 7 && now - since > .25) { Keys(Key.Escape); Next(5, now); }
                else if (stage == 5 && now - since > .15) { Keys(); Next(6, now); }
                else if (stage == 6 && now - since > .8)
                { Require(world.Player.CanUseTools, "Vane Esc failed."); routeIndex = 0; WalkTo(flow.ForestRoute[0].position); Next(10, now); }
                else if (stage == 10 && Walk(now))
                {
                    if (routeIndex == 2) { Aim(flow.ForestRoute[3].position); Capture("large-residual-trail"); }
                    routeIndex++;
                    if (routeIndex < flow.ForestRoute.Length) WalkTo(flow.ForestRoute[routeIndex].position);
                    else BeginRead(flow.SecondTablet, 1, 15, now);
                }
                else if (stage == 15)
                {
                    Require(world.Session.Clock.Day == 3 && !flow.ThirdTablet.gameObject.activeInHierarchy && flow.Status == flow.Definition.Day3FoundGuide && world.Consequences.Rule.RadioSound == 1 && world.Consequences.Rule.SunLight == 1, "T02 advanced D4/5/7 or missed status.");
                    if (sawAgentMove && world.Chase.Gate.PassCount > 0) { WalkTo(flow.ReturnPoint.position); Next(16, now); }
                    else if (now - since > 20) throw new InvalidOperationException("Existing chase never reached the route: " + world.Chase.Agent.transform.position + ", " + world.Chase.Phase);
                }
                else if (stage == 16 && Walk(now)) { Capture("day3-return"); Next(17, now); }
                else if (stage == 17 && now - since > .25) { world.Sleep(); Next(20, now); }
                else if (stage == 20 && now - since > .4)
                {
                    Require(world.Session.Clock.Day == 4 && flow.ThirdTablet.gameObject.activeInHierarchy && flow.Status == flow.Definition.Day4Guide && world.Consequences.Rule.SoundExchangeCount == 0 && world.Consequences.Rule.LightTransferCount == 0, "D4 site/date failed.");
                    routeIndex = 0; WalkTo(flow.RitualRoute[0].position); Next(21, now);
                }
                else if (stage == 21 && Walk(now))
                {
                    routeIndex++;
                    if (routeIndex < flow.RitualRoute.Length) WalkTo(flow.RitualRoute[routeIndex].position);
                    else BeginRead(flow.ThirdTablet, Late ? 1 : 2, 25, now);
                }
                else if (stage == 25)
                {
                    Require(world.Session.Clock.Day == 4 && flow.Status == flow.Definition.Day4FoundGuide && world.Consequences.Rule.SoundExchangeCount == 0 && world.Consequences.Rule.LightTransferCount == 0, "T03 changed capabilities early.");
                    if (Late) Require(!world.Campaign.Progress.Found(1), "Late T03 search required/fabricated T02 discovery.");
                    WalkTo(flow.ReturnPoint.position); Next(26, now);
                }
                else if (stage == 26 && Walk(now))
                {
                    Require(world.Player.transform.position.z > -5 && world.Player.transform.position.y > -.2f && metres > (Late ? 35 : 65), "Physical search roundtrip missing.");
                    Capture("day4-return"); Next(27, now);
                }
                else if (stage == 27 && now - since > .25) { world.Sleep(); Next(30, now); }
                else if (stage == 30 && now - since > .5)
                {
                    Require(world.Session.Clock.Day == 5 && world.Consequences.Rule.SoundExchangeCount == 1 && world.Consequences.Rule.LightTransferCount == 0 && flow.ShowStatus == flow.HasSignalSearch, "D5 fixed SOUND or configured guide boundary failed.");
                    world.Sleep(); world.Sleep(); Next(31, now);
                }
                else if (stage == 31 && now - since > .5)
                {
                    Require(world.Session.Clock.Day == 7 && world.Consequences.Rule.SoundExchangeCount == 1 && world.Consequences.Rule.LightTransferCount == 1 && world.Presentation.PermanentNight, "D7 fixed LIGHT failed.");
                    Complete(true, "Real WASD/E/Esc " + (Late ? "D4 T03 without T02" : "D3 stopped vane / residual-trail T02 / existing chase close pass, D4 outside-cave T03") + ", repeated reads deduplicate, physical camp return, no D1 backfill or early SOUND/LIGHT/SEAL, sleep to D5/D7 schedules, saved tuning=" + Tuning + ", physical metres=" + metres + ", closePasses=" + world.Chase.Gate.PassCount);
                }
                else if (stage == 50 && now - since > .35) { Keys(Key.E); Next(51, now); }
                else if (stage == 51 && now - since > .3)
                {
                    Keys(); Require(world.Player.Reading && world.Campaign.Progress.Found(readingTablet.Index) && world.Campaign.Progress.FoundCount == expectedFound, "Physical tablet read/repeat failed: T" + (readingTablet.Index + 1) + ", prompt=" + world.Player.FocusPrompt + ", position=" + world.Player.transform.position);
                    Require(world.ReadingText.Contains(readingTablet.Index == 1 ? "D5 00:00" : "D7 00:00"), "Original date record changed.");
                    var overlay = UnityEngine.Object.FindAnyObjectByType<SliceHud>().GetComponentsInChildren<UnityEngine.UI.Text>(true).Single(t => t.name == "RecordText");
                    Debug.Log("INVESTIGATION_FONT_DIAGNOSTIC shader=" + overlay.materialForRendering.shader.name + ", default=" + overlay.defaultMaterial.shader.name + ", canvas=" + overlay.canvas.renderMode + ", texture=" + overlay.mainTexture.width + "x" + overlay.mainTexture.height + ", font=" + overlay.font.name + ", size=" + overlay.fontSize + ", chars=" + overlay.cachedTextGenerator.characterCount + ", vertices=" + overlay.cachedTextGenerator.vertexCount + ", culled=" + overlay.canvasRenderer.cull);
 Next(52, now);
                }
                else if (stage == 52 && operation == "investigation-review")
                {
                    if (!File.Exists("Temp/DemonReadingReview.ready"))
                    {
                        Capture("manual-reading");
                        File.WriteAllText("Temp/DemonReadingReview.ready", "T03 D4 reading open; confirm live UI, not an automated pass.");
                    }
                    if (File.Exists("Temp/DemonReadingReview.stop"))
                    {
                        File.Delete("Temp/DemonReadingReview.stop");
                        Complete(true, "Bounded T03 live-reading preview closed after user feedback; visual verdict recorded separately.");
                    }
                }
                else if (stage == 52 && now - since > 1.5) { Capture("tablet-" + readingTablet.Index + "-read-" + repeat); Next(55, now); }
                else if (stage == 55 && now - since > 2) { Keys(Key.Escape); Next(53, now); }
                else if (stage == 53 && now - since > .15) { Keys(); Next(54, now); }
                else if (stage == 54 && now - since > .8)
                {
                    Require(world.Player.CanUseTools, "Tablet Esc failed.");
                    if (repeat++ == 0) { Aim(readingTablet.transform.position); Next(50, now); }
                    else Next(afterRead, now);
                }
                else if (stage == 80 && world.Session.Clock.Day == 3 && now - since > 1)
                {
                    Require(flow.LargeTrailVisible && flow.SecondTablet.gameObject.activeInHierarchy && !flow.ThirdTablet.gameObject.activeInHierarchy && world.Tether.MotionReady && world.Tether.TargetVisible && world.Chase.HasNavigation && world.Consequences.Rule.DemonSound == 0 && world.Consequences.Rule.SunLight == 1 && world.Campaign.Progress.FoundCount == 0, "Natural D3 boundary required discovery or leaked other dates.");
                    Capture("natural-day3"); Complete(true, "Saved D2 23:59.4 naturally crosses D3: residual traces/T02/MOTION/chase appear, T03/SOUND/LIGHT remain dated; zero discoveries.");
                }
                else if (stage == 90 && now - since > .4)
                {
                    int day = world.Session.Clock.Day;
                    Require(world.Campaign.Progress.FoundCount == 0 && world.Consequences.Rule.SoundExchangeCount == (day >= 5 ? 1 : 0) && world.Consequences.Rule.LightTransferCount == (day >= 7 ? 1 : 0), "Skipping search blocked/duplicated dated attributes.");
                    if (day < 9) { world.Sleep(); Next(90, now); }
                    else { Require(world.Session.Descended && !flow.ShowStatus && !flow.LargeTrailVisible && !flow.TrailVisible, "Sleep ending left exploration state active."); Complete(true, "D3 through D9 sleep-only: zero discoveries/checks/SEAL exchanges, fixed SOUND/LIGHT once, terminal trail cleanup."); }
                }
            }
            catch (Exception exception) { Complete(false, "Stage " + stage + ", route=" + routeIndex + ": " + exception); }
        }
        void BeginRead(NineDayTablet tablet, int count, int next, double now)
        { readingTablet = tablet; expectedFound = count; afterRead = next; repeat = 0; Aim(tablet.transform.position); Next(50, now); }
        void WalkTo(Vector3 target)
        {
            var path = new NavMeshPath();
            Require(NavMesh.SamplePosition(world.Player.transform.position, out var from, 1.5f, NavMesh.AllAreas) && NavMesh.SamplePosition(target, out var to, 1.5f, NavMesh.AllAreas) && NavMesh.CalculatePath(from.position, to.position, NavMesh.AllAreas, path) && path.status == NavMeshPathStatus.PathComplete, "No complete player fixture path to " + target);
            corners = path.corners; corner = corners.Length > 1 ? 1 : 0; lastSteer = -100; walkStarted = Time.timeAsDouble; Keys();
        }
        bool Walk(double now)
        {
            var position = world.Player.transform.position;
            if (now - walkStarted > 25) throw new InvalidOperationException("Walking blocked at " + position + ", corner=" + corners[corner] + ", route=" + string.Join(";", corners.Select(p => p.ToString())));
            metres += Vector3.Distance(position, previous); previous = position;
            Vector3 delta = corners[corner] - position; delta.y = 0;
            if (delta.magnitude < .2f) { corner++; if (corner >= corners.Length) { Keys(); return true; } lastSteer = -100; delta = corners[corner] - position; delta.y = 0; }
            if (now - lastSteer > .15) { Aim(position + delta + Vector3.up * world.Player.ViewCamera.transform.localPosition.y); lastSteer = now; }
            Keys(Key.W); return false;
        }
        void Aim(Vector3 point)
        {
            var cam = world.Player.ViewCamera.transform; var desired = Quaternion.LookRotation(point - cam.position).eulerAngles;
            float yaw = Mathf.DeltaAngle(world.Player.transform.eulerAngles.y, desired.y), pitch = Mathf.DeltaAngle(0, cam.localEulerAngles.x), targetPitch = Mathf.DeltaAngle(0, desired.x);
            InputSystem.QueueStateEvent(mouse, new MouseState { delta = new Vector2(yaw / world.Settings.MouseSensitivity, (pitch - targetPitch) / world.Settings.MouseSensitivity) });
        }
        void SetupInput()
        {
            oldBackground = InputSystem.settings.backgroundBehavior; oldInput = InputSystem.settings.editorInputBehaviorInPlayMode; oldRun = Application.runInBackground;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus; InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView; Application.runInBackground = true;
            keyboard = InputSystem.AddDevice<Keyboard>(); mouse = InputSystem.AddDevice<Mouse>(); InputSystem.onAfterUpdate += Current; Current(); Cursor.lockState = CursorLockMode.Locked;
        }
        void Keys(params Key[] keys) => InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
        void Current() { keyboard?.MakeCurrent(); mouse?.MakeCurrent(); }
        void Next(int value, double time) { stage = value; since = time; }
        void Capture(string name)
        {
            // Background Editor Game views can render less often than the player loop.
            // Keep reading open after this asynchronous request and explicitly request a view repaint.
            Canvas.ForceUpdateCanvases();
            foreach (var window in Resources.FindObjectsOfTypeAll<EditorWindow>())
                if (window.GetType().Name == "GameView") window.Repaint();
            ScreenCapture.CaptureScreenshot(Path.GetFullPath("Temp/DemonSliceReports/" + operation + "-" + name + ".png"));
        }
        public void Cleanup()
        {
            if (keyboard == null) return;
            InputSystem.onAfterUpdate -= Current; InputSystem.RemoveDevice(keyboard); InputSystem.RemoveDevice(mouse); InputSystem.settings.backgroundBehavior = oldBackground; InputSystem.settings.editorInputBehaviorInPlayMode = oldInput; Application.runInBackground = oldRun; keyboard = null; mouse = null;
        }
        void Complete(bool success, string message) { finished = true; Cleanup(); finish(success, message); EditorApplication.delayCall += EditorApplication.ExitPlaymode; }
    }
}
