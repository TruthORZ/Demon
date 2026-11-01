using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using Demon.Slice.Domain;
namespace Demon.Slice.Editor
{
    public sealed class ConsequencesSmokeRun
    {
        readonly string operation;
        readonly Action<bool, string> finish;
        readonly double started = EditorApplication.timeSinceStartup;
        readonly float[] samples = new float[512];
        SliceSceneController world;
        WorldAttributeConsequences effects;
        Keyboard keyboard; Mouse mouse;
        InputSettings.BackgroundBehavior oldBackground;
        InputSettings.EditorInputBehaviorInPlayMode oldInput;
        bool oldRun, finished, sawReturn;
        int stage;
        double since;
        float radioRms, demonRms;
        bool Tuning => operation == "consequences-tuning";
        public ConsequencesSmokeRun(string op, Action<bool, string> callback) { operation = op; finish = callback; }
        static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
        public void Tick()
        {
            if (finished || !Application.isPlaying) return;
            try
            {
                if (EditorApplication.timeSinceStartup - started > 160) throw new TimeoutException("Attribute smoke timed out at " + stage);
                if (!world) world = UnityEngine.Object.FindAnyObjectByType<SliceSceneController>();
                if (world && !world.enabled) throw new InvalidOperationException("World disabled; inspect saved references.");
                if (!world || world.Session == null || !world.Consequences || world.Consequences.Rule == null) return;
                effects = world.Consequences; var rule = effects.Rule; double now = Time.timeAsDouble;
                if (stage == 0)
                {
                    Require(world.Cipher.Wheel.ReadCode().SequenceEqual(new[] { 3, 4, 2 }), "Sun appearance changed cipher342.");
                    Require(effects.RadioBroadcast.spatialBlend == 1 && effects.DemonBroadcast.spatialBlend == 1 && Vector3.Distance(effects.RadioBroadcast.transform.position, effects.DemonBroadcast.transform.position) > 10, "Broadcast endpoints are not distinct saved spatial sources.");
                    Require(effects.RadioBroadcast.clip == effects.BroadcastClip && effects.DemonBroadcast.clip == effects.BroadcastClip, "Single transferable clip ignored.");
                    Require(Math.Abs(effects.RadioBroadcast.volume - (Tuning ? .12f : .35f)) < .001f && Math.Abs(world.Presentation.SunriseHour - (Tuning ? 7.5f : 6)) < .001f, "Saved gain/sunrise ignored.");
                    Require((effects.BroadcastClip.name == "PrototypeChimeHigh") == Tuning, "Saved replacement broadcast clip ignored.");
                    world.Chase.enabled = false;
                    if (operation == "consequences-late") { Require(world.Session.Clock.Day == 8, "Late probe start missing."); Next(80, now); return; }
                    Require(world.Session.Clock.Day == 4 && rule.RadioSound == 1 && rule.DemonSound == 0 && rule.LightTransferCount == 0 && !world.Finale.LakeGlowVisible, "Pre-D5 attributes leaked.");
                    if (operation == "consequences-skip") { world.Sleep(); Next(70, now); return; }
                    oldBackground = InputSystem.settings.backgroundBehavior; oldInput = InputSystem.settings.editorInputBehaviorInPlayMode; oldRun = Application.runInBackground;
                    InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus; InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
                    keyboard = InputSystem.AddDevice<Keyboard>(); mouse = InputSystem.AddDevice<Mouse>(); InputSystem.onAfterUpdate += Current; Current();
                    world.Player.EnterInspectionView(View("RadioView")); Next(1, now);
                }
                else if (stage == 1)
                {
                    radioRms = Mathf.Max(radioRms, ReadRms(effects.RadioBroadcast));
                    Require(!effects.DemonBroadcast.isPlaying && effects.RadioStatic.isPlaying, "Early demon voice or missing native static bed.");
                    if (now - since > .8 && radioRms > .000001f)
                    { Require(effects.RadioBroadcast.isPlaying && rule.SoundExchangeCount == 0, "Radio not playing before D5."); Capture("radio-before5"); Next(2, now); }
                    else if (now - since > 4) throw new InvalidOperationException("Native radio output is silent: playing=" + effects.RadioBroadcast.isPlaying + ", virtual=" + effects.RadioBroadcast.isVirtual + ", dsp=" + AudioSettings.dspTime + ", listenerPause=" + AudioListener.pause);
                }
                else if (stage == 2 && now - since > .35)
                { world.Session.Clock.AdvanceTo(4 * 1440 - .1); Next(3, now); }
                else if (stage == 3 && now - since > .45)
                {
                    Require(world.Session.Clock.Day == 5 && rule.SoundExchangeCount == 1 && rule.LightTransferCount == 0 && rule.RadioSound == 0 && rule.DemonSound == 1 && rule.SunLight == 1, "Natural D5 exchange/capacity failed.");
                    Require(!effects.RadioBroadcast.isPlaying && effects.DemonBroadcast.isPlaying && effects.RadioStatic.isPlaying && !world.Finale.LakeGlowVisible, "D5 voice route/static or premature LIGHT failed.");
                    world.Player.EnterInspectionView(View("LakeView")); Next(4, now);
                }
                else if (stage == 4)
                {
                    demonRms = Mathf.Max(demonRms, ReadRms(effects.DemonBroadcast));
                    if (now - since > .8 && demonRms > .000001f) { Capture("lake-after5"); Next(5, now); }
                    else if (now - since > 4) throw new InvalidOperationException("Transferred native lake output is silent.");
                }
                else if (stage == 5 && now - since > .35)
                { world.Session.Clock.AdvanceTo(5 * 1440 + 720); world.Player.EnterInspectionView(View("SolarView")); Next(6, now); }
                else if (stage == 6 && now - since > .8)
                {
                    Require(!world.Presentation.PermanentNight && world.Presentation.Sun.intensity > 1 && !effects.WhiteRimVisible && effects.SolarDisk.gameObject.activeSelf && rule.LightTransferCount == 0, "D6 normal noon solar baseline failed.");
                    Capture("day6-noon"); Next(7, now);
                }
                else if (stage == 7 && now - since > .35)
                { world.Session.Clock.AdvanceTo(6 * 1440 - .1); Next(8, now); }
                else if (stage == 8 && now - since > .45)
                {
                    Require(world.Session.Clock.Day == 7 && rule.SunLight == 0 && rule.DemonLight == 1 && rule.LightTransferCount == 1 && rule.SoundExchangeCount == 1 && world.Presentation.Sun.intensity == 0 && world.Presentation.PermanentNight && !effects.WhiteRimVisible && world.Finale.LakeGlowVisible, "Natural D7 transfer/first lake light/night boundary failed.");
                    world.Session.Clock.AdvanceTo(6 * 1440 + world.Presentation.SunriseHour * 60 + 1); Next(9, now);
                }
                else if (stage == 9 && now - since > .8)
                {
                    Require(effects.WhiteRimVisible && effects.SolarDisk.gameObject.activeSelf && world.Presentation.Sun.intensity == 0 && ColorDistance(RenderSettings.fogColor, world.Settings.NightFog) < .01f, "Authored sunrise restored daylight or omitted white rim.");
                    Capture("day7-rim"); Next(10, now);
                }
                else if (stage == 10 && now - since > .35)
                {
                    world.Session.Clock.AdvanceTo(6 * 1440 + 720);
                    Require(world.Presentation.PermanentNight, "Noon restored sunlight after transfer.");
                    var box = world.Cipher; var code = box.Wheel.ReadCode(); for (int i = 0; i < 3; i++) while (box.Model.At(i) != code[i]) box.Model.Cycle(i);
                    Require(box.TryConfirm(), "A fixture unlock failed."); box.Use(world.Player);
                    var board = world.Listening.Board; board.PickFromTray(RuneKind.Ring); board.ClickSlot(4); board.PickFromTray(RuneKind.Chime); board.ClickSlot(7); Require(world.Listening.TryCompile(), "T05 fixture failed.");
                    box.Use(world.Player); Keys(Key.Escape); Next(11, now);
                }
                else if (stage == 11 && now - since > .8)
                {
                    Keys(); Require(world.Player.CanUseTools, "T06 reading retained near view.");
                    PlacePlayer(new Vector3(3, .04f, -2.8f)); Require(NavMesh.SamplePosition(new Vector3(3, 0, -7.5f), out var hit, 1.5f, NavMesh.AllAreas) && world.Chase.Agent.Warp(hit.position), "Saved nav fixture unavailable."); world.Chase.Agent.ResetPath();
                    world.Chase.enabled = true; PointAt(world.Tether.AimRegion.position); Next(12, now);
                }
                else if (stage == 12 && now - since > world.Tether.Definition.StableSeconds + .3)
                {
                    Require(world.Tether.Rule.Locked && world.Chase.Agent.isStopped && effects.DemonBroadcast.isPlaying && world.Presentation.Sun.intensity == 0 && world.Session.ExchangeCount == 0, "Physical MOTION freeze affected SOUND/LIGHT/SEAL."); Next(13, now);
                }
                else if (stage == 13 && now - since > .5)
                { Require(effects.DemonBroadcast.isPlaying, "Frozen motion stopped demon voice."); effects.enabled = false; Next(14, now); }
                else if (stage == 14 && now - since > .3)
                {
                    Require(!effects.RadioBroadcast.isPlaying && !effects.RadioStatic.isPlaying && !effects.DemonBroadcast.isPlaying && world.Presentation.PermanentNight && rule.SoundExchangeCount == 1 && rule.LightTransferCount == 1, "Disabled presentation leaked audio or reversed attributes.");
                    effects.enabled = true; Next(15, now);
                }
                else if (stage == 15 && now - since > .5)
                {
                    Require(effects.DemonBroadcast.isPlaying && effects.RadioStatic.isPlaying && !effects.RadioBroadcast.isPlaying && rule.SoundExchangeCount == 1 && rule.LightTransferCount == 1, "Re-enable replayed rules or failed to resume routing.");
                    InputSystem.QueueStateEvent(mouse, new MouseState()); Capture("frozen-voice"); Next(16, now);
                }
                else if (stage == 16 && now - since > .3)
                {
                    world.Chase.enabled = false; world.Player.ExitCloseView(); world.Session.Clock.AdvanceTo(7 * 1440 + world.Campaign.Definition.FinalTabletHour * 60 + 1);
                    var retrieval = world.Retrieval; retrieval.RefreshPresentation(); retrieval.Pickup.Use(world.Player); retrieval.Pickup.TickHeld(true, retrieval.Pickup.HoldSeconds + .01f); Require(retrieval.Place(world.Player), "T07 fixture placement failed.");
                    var board = world.Session.Board; board.PickFromTray(RuneKind.Exchange); board.ClickSlot(4); board.PickFromTray(RuneKind.Lake); board.ClickSlot(7); board.PickFromTray(RuneKind.Box); board.ClickSlot(8);
                    Require(world.TryCompile() && world.Session.ExchangeCount == 1, "Accepted reversed T07 failed with new attributes."); Next(17, now);
                }
                else if (stage == 17 && now - since > .4)
                {
                    Require(!world.Finale.LakeGlowVisible && !effects.DemonBroadcast.isPlaying && rule.DemonLight == 1 && rule.DemonSound == 1 && world.Presentation.PermanentNight, "Lake seal did not conceal presentation independently of attributes.");
                    world.ReadBox(); Require(world.Player.Reading, "B record fixture failed."); world.Player.ExitCloseView(); Next(18, now);
                }
                else if (stage == 18)
                {
                    if (world.Session.Descended && !world.EndingPresented && effects.DemonBroadcast.isPlaying && world.Finale.LakeGlowVisible) sawReturn = true;
                    if (world.EndingPresented && !effects.DemonBroadcast.isPlaying && !effects.RadioStatic.isPlaying)
                    {
                        Require(sawReturn && world.Session.ExchangeCount == 2 && world.Session.Clock.Day == 9 && rule.SoundExchangeCount == 1 && rule.LightTransferCount == 1 && world.Presentation.PermanentNight, "D9 return/end cleanup or one-shot rule ledger failed.");
                        Complete(true, "Native nonzero radio/lake audio output and shared clip transfer at natural D5, persistent static, D6 noon versus natural D7 light loss/rim/night, real right-mouse MOTION independence, disable/resume, reversed T07 seal conceals voice/glow without undoing attributes, D9 opens/resumes then terminal audio cleanup passed. radioRMS=" + radioRms + ", lakeRMS=" + demonRms + ", " + operation);
                    }
                }
                else if (stage == 70 && now - since > .4)
                {
                    int day = world.Session.Clock.Day;
                    Require(rule.SoundExchangeCount == 1 && rule.LightTransferCount == (day >= 7 ? 1 : 0), "Sleep skipped or repeated attribute dates.");
                    if (day < 9) { world.Sleep(); Next(70, now); }
                    else { Require(world.Session.Descended && world.Session.ExchangeCount == 0 && !effects.DemonBroadcast.isPlaying && !effects.RadioStatic.isPlaying, "Sleep-only terminal cleanup failed."); Complete(true, "D4->D9 explicit sleep crosses D5/D7 once without discovery, T07 or SEAL exchange; terminal audio stopped."); }
                }
                else if (stage == 80 && now - since > .5)
                {
                    Require(rule.SoundExchangeCount == 1 && rule.LightTransferCount == 1 && rule.RadioSound == 0 && rule.DemonSound == 1 && effects.DemonBroadcast.isPlaying && !effects.RadioBroadcast.isPlaying && world.Presentation.Sun.intensity == 0 && effects.WhiteRimVisible, "Late D8 scene load did not reconstruct scheduled attributes.");
                    Capture("late-start"); world.Sleep(); Next(81, now);
                }
                else if (stage == 81 && now - since > .5)
                { Require(world.Session.Descended && rule.SoundExchangeCount == 1 && rule.LightTransferCount == 1 && !effects.DemonBroadcast.isPlaying && !effects.RadioStatic.isPlaying, "Late-start end cleanup failed."); Complete(true, "Saved D8 noon start catches up SOUND/LIGHT once, permanent night and rim/audio routing, explicit sleep and terminal cleanup passed."); }
            }
            catch (Exception e) { Complete(false, "Stage " + stage + ": " + e); }
        }
        Transform View(string name) => GameObject.Find("Consequences_Authored/" + name).transform;
        float ReadRms(AudioSource source) { source.GetOutputData(samples, 0); double sum = 0; foreach (float value in samples) sum += value * value; return (float)Math.Sqrt(sum / samples.Length); }
        static float ColorDistance(Color a, Color b) => Vector4.Distance(a, b);
        void PlacePlayer(Vector3 point) { var body = world.Player.GetComponent<CharacterController>(); body.enabled = false; world.Player.transform.SetPositionAndRotation(point, Quaternion.identity); body.enabled = true; Cursor.lockState = CursorLockMode.Locked; }
        void PointAt(Vector3 point)
        {
            var cam = world.Player.ViewCamera.transform; Vector3 desired = Quaternion.LookRotation(point - cam.position).eulerAngles;
            float yaw = Mathf.DeltaAngle(world.Player.transform.eulerAngles.y, desired.y), pitch = Mathf.DeltaAngle(0, cam.localEulerAngles.x), targetPitch = Mathf.DeltaAngle(0, desired.x);
            InputSystem.QueueStateEvent(mouse, new MouseState { delta = new Vector2(yaw / world.Settings.MouseSensitivity, (pitch - targetPitch) / world.Settings.MouseSensitivity), buttons = 2 });
        }
        void Keys(params Key[] keys) => InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
        void Current() { keyboard?.MakeCurrent(); mouse?.MakeCurrent(); }
        void Next(int value, double time) { stage = value; since = time; }
        void Capture(string name) => ScreenCapture.CaptureScreenshot(Path.GetFullPath("Temp/DemonSliceReports/" + operation + "-" + name + ".png"));
        public void Cleanup()
        {
            if (keyboard == null) return; InputSystem.onAfterUpdate -= Current; InputSystem.RemoveDevice(keyboard); InputSystem.RemoveDevice(mouse);
            InputSystem.settings.backgroundBehavior = oldBackground; InputSystem.settings.editorInputBehaviorInPlayMode = oldInput; Application.runInBackground = oldRun; keyboard = null; mouse = null;
        }
        void Complete(bool success, string message) { finished = true; Cleanup(); finish(success, message); EditorApplication.delayCall += EditorApplication.ExitPlaymode; }
    }
}
