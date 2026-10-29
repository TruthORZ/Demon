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
    // Fixture uses real player input. Chase, sensor sampling and scheduled effects remain production code.
    public sealed class SignalSearchSmokeRun
    {
        readonly string operation;
        readonly Action<bool,string> finish;
        readonly double started = EditorApplication.timeSinceStartup;
        SliceSceneController world; ExplorationFlow flow;
        Keyboard keyboard; Mouse mouse;
        InputSettings.BackgroundBehavior oldBackground;
        InputSettings.EditorInputBehaviorInPlayMode oldInput;
        bool oldRun, finished, sawVisible, heardOccluded, sawAgent;
        int stage, routeIndex, corner, lastFrame = -1, repeat, click;
        double since, lastSteer, walkStarted, before;
        float metres; Vector3 previous; Vector3[] corners; Transform[] targets;
        readonly int[] rings = new int[2];
        bool Late => operation == "signal-late";
        bool Tuning => operation == "signal-tuning";
        public SignalSearchSmokeRun(string op, Action<bool,string> callback) { operation = op; finish = callback; }
        static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
        public void Tick()
        {
            if (finished || !Application.isPlaying) return;
            try
            {
                if (EditorApplication.timeSinceStartup - started > 240) throw new TimeoutException("Signal-search stage " + stage);
                if (!world) world = UnityEngine.Object.FindAnyObjectByType<SliceSceneController>();
                if (!world || world.Session == null || !world.Exploration || world.Exploration.Log == null) return;
                if (lastFrame == Time.frameCount) return; lastFrame = Time.frameCount;
                flow = world.Exploration; double now = Time.timeAsDouble;
                Require(world.Session.ExchangeCount == 0 && flow.Log.Count == 0, "Search fabricated SEAL or D1 checks.");
                if (world.Chase.HasNavigation && world.Chase.Agent.velocity.sqrMagnitude > .1f) sawAgent = true;
                if (world.Listening.Active) ObserveRings();
                if (stage == 0)
                {
                    Require(flow.HasSignalSearch && world.Campaign.Progress.FoundCount == 0 && !world.Listening.Active && !world.Cipher.Acquired, "Missing clean saved search state.");
                    Require(Math.Abs(world.Settings.WalkMetresPerSecond - (Tuning ? 2.8f : 3.3f)) < .001f && flow.Definition.Day5Guide.StartsWith("【保存参数检查】") == Tuning, "Saved tuning ignored.");
                    if (operation == "signal-boundary")
                    { Require(world.Session.Clock.Day == 4 && !flow.FourthTablet.gameObject.activeInHierarchy && !flow.FifthTablet.gameObject.activeInHierarchy && world.Consequences.Rule.RadioSound == 1, "D5/6 appeared early."); Next(80,now); return; }
                    Require(world.Session.Clock.Day == (Late ? 6 : 5) && flow.FourthTablet.gameObject.activeInHierarchy && flow.FifthTablet.gameObject.activeInHierarchy == Late, "Saved D5/D6 visibility failed.");
                    Require(world.Consequences.Rule.SoundExchangeCount == 1 && world.Consequences.Rule.LightTransferCount == 0 && world.Consequences.RadioStatic.isPlaying && !world.Consequences.RadioBroadcast.isPlaying && world.Consequences.DemonBroadcast.isPlaying, "D5 broadcast/static transfer failed.");
                    if (operation == "signal-skip") { world.Sleep(); Next(90,now); return; }
                    SetupInput(); previous = world.Player.transform.position; Capture("start-guide");
                    if (Late) BeginToolRoute(now);
                    else { Require(flow.Status == flow.Definition.Day5Guide,"D5 guide missing."); WalkTo(new Vector3(-6,.04f,-4.5f)); Next(1,now); }
                }
                else if (stage == 1 && Walk(now)) { WalkTo(flow.Observations[0].Approach.position); Next(2,now); }
                else if (stage == 2 && Walk(now)) { Aim(flow.Observations[0].AimTarget.position); Next(3,now); }
                else if (stage == 3 && now-since>.3) { Keys(Key.E); Next(4,now); }
                else if (stage == 4 && now-since>.5)
                { Keys(); Require(world.Player.Reading && world.ReadingText.Contains(flow.Definition.Radio.Changed),"Physical static-radio observation failed: " + world.Player.FocusPrompt); Capture("radio-static"); Next(5,now); }
                else if (stage == 5 && now-since>.5) { Keys(Key.Escape); Next(6,now); }
                else if (stage == 6 && now-since>.2) { Keys(); Next(7,now); }
                else if (stage == 7 && now-since>.8) { WalkTo(new Vector3(-6,.04f,-4.5f)); Next(8,now); }
                else if (stage == 8 && Walk(now)) { routeIndex=0; WalkTo(flow.LakeRoute[0].position); Next(10,now); }
                else if (stage == 10 && Walk(now))
                { if (++routeIndex<flow.LakeRoute.Length) WalkTo(flow.LakeRoute[routeIndex].position); else { Aim(flow.FourthTablet.transform.position); repeat=0; Next(11,now); } }
                else if (stage == 11 && now-since>.3) { Keys(Key.E); Next(12,now); }
                else if (stage == 12 && now-since>.6)
                { Keys(); Require(world.Player.Reading && world.Campaign.Progress.FoundCount==1 && world.Campaign.Progress.Found(3) && world.ReadingText.Contains("D9 00:00") && flow.Status==flow.Definition.Day5FoundGuide,"Physical T04 read/repeat/status failed: " + world.Player.FocusPrompt); Capture("lake-t04-"+repeat); Next(13,now); }
                else if (stage == 13 && now-since>.7) { Keys(Key.Escape); Next(14,now); }
                else if (stage == 14 && now-since>.2) { Keys(); Next(15,now); }
                else if (stage == 15 && now-since>.8)
                { if (repeat++==0) { Aim(flow.FourthTablet.transform.position); Next(11,now); } else { WalkTo(flow.ReturnPoint.position); Next(16,now); } }
                else if (stage == 16 && Walk(now)) { Require(world.Session.Clock.Day==5 && !flow.FifthTablet.gameObject.activeInHierarchy,"T04 unlocked T05 early."); Capture("day5-return"); Next(17,now); }
                else if (stage == 17 && now-since>.3) { world.Sleep(); Next(18,now); }
                else if (stage == 18 && now-since>.5)
                { Require(world.Session.Clock.Day==6 && flow.FifthTablet.gameObject.activeInHierarchy && flow.Status==flow.Definition.Day6Guide,"D6 availability/guide failed."); BeginToolRoute(now); }
                else if (stage == 20 && Walk(now))
                { if (++routeIndex<flow.ListeningRoute.Length) WalkTo(flow.ListeningRoute[routeIndex].position); else { Aim(flow.FifthTablet.transform.position); Next(21,now); } }
                else if (stage == 21 && now-since>.4) { Keys(Key.E); Next(22,now); }
                else if (stage == 22 && now-since>.9)
                {
                    Keys(); Require(world.Player.InspectingTablet && world.Player.ActiveTablet==world.Listening.Tablet && world.Campaign.Progress.FoundCount==(Late?1:2) && flow.Status==flow.Definition.Day6FoundGuide,"Physical forest T05 interaction failed: " + world.Player.FocusPrompt);
                    if (Late) Require(!world.Campaign.Progress.Found(3),"T05 required/fabricated T04 discovery.");
                    var tray=world.Listening.Tablet.GetComponentsInChildren<SliceRuneTrayItem>(); var slots=world.Listening.Tablet.GetComponentsInChildren<SliceRuneSlot>();
                    targets=new[]{tray.Single(t=>t.Rune==RuneKind.Open).transform,slots.Single(s=>s.Index==4).transform}; click=0; Next(23,now);
                }
                else if (stage == 23 && now-since>.16)
                { if(click<targets.Length*2) { Click(targets[click/2],click%2==0); click++; since=now; } else { Keys(Key.Enter); Next(24,now); } }
                else if (stage == 24 && now-since>.4)
                {
                    Keys(); Require(!world.Listening.Active && !world.Session.Board.Frozen && world.Session.Clock.Day==6,"Wrong T05 attempt changed rules.");
                    var tray=world.Listening.Tablet.GetComponentsInChildren<SliceRuneTrayItem>(); var slots=world.Listening.Tablet.GetComponentsInChildren<SliceRuneSlot>();
                    targets=new[]{tray.Single(t=>t.Rune==RuneKind.Ring).transform,slots.Single(s=>s.Index==4).transform,tray.Single(t=>t.Rune==RuneKind.Chime).transform,slots.Single(s=>s.Index==7).transform}; click=0; Next(25,now);
                }
                else if (stage == 25 && now-since>.16)
                { if(click<targets.Length*2) { Click(targets[click/2],click%2==0); click++; since=now; } else { Require(world.Listening.Board.MatchesTarget(),"Mouse did not fill T05."); Capture("t05-complete-layout"); before=world.Session.Clock.AbsoluteMinutes; Next(26,now); } }
                else if (stage == 26 && now-since>.5) { Keys(Key.Enter); Next(27,now); }
                else if (stage == 27 && now-since>.8)
                {
                    Keys(); Require(world.Listening.Active && !world.Session.Board.Frozen && !world.Finale.TimeReserved && world.Session.Clock.Day==6 && world.Session.Clock.AbsoluteMinutes-before<10 && flow.Status==flow.Definition.Day6ActiveGuide,"T05 activation changed T07/date or guide failed.");
                    WalkTo(new Vector3(12,.04f,-12)); Next(28,now);
                }
                else if (stage == 28 && Walk(now)) { WalkTo(new Vector3(12,.04f,-14.8f)); Next(29,now); }
                else if (stage == 29 && Walk(now)) { WalkTo(flow.VisibleListeningPoint.position); Next(30,now); }
                else if (stage == 30 && Walk(now)) { Next(31,now); }
                else if (stage == 31)
                {
                    Aim(world.Chase.Agent.transform.position+Vector3.up*.3f);
                    if(sawVisible) { Capture("visible-snow-entry"); Next(32,now); }
                    else if(now-since>35) throw new InvalidOperationException("No real visible chase entry at listening point: "+world.Chase.Agent.transform.position+" rings="+string.Join(",",rings));
                }
                else if (stage == 32 && now-since>.4) { WalkTo(flow.HiddenListeningPoint.position); Next(40,now); }
                else if (stage == 40 && Walk(now)) { heardOccluded=false; Aim(world.Listening.Chimes[0].transform.position+Vector3.up*.3f); Next(41,now); }
                else if (stage == 41)
                {
                    if(now-lastSteer>3) { lastSteer=now; Debug.Log("SIGNAL_HIDDEN player="+world.Player.transform.position+", agent="+world.Chase.Agent.transform.position+", destination="+world.Chase.Agent.destination+", remaining="+world.Chase.Agent.remainingDistance+", path="+world.Chase.Agent.pathStatus+", phase="+world.Chase.Phase+", passes="+world.Chase.Gate.PassCount); }
                    if(heardOccluded) { Capture("occluded-real-entry"); Next(42,now); }
                    else if(now-since>40) throw new InvalidOperationException("No real occluded chase entry: "+world.Chase.Agent.transform.position+" phase="+world.Chase.Phase+" rings="+string.Join(",",rings));
                }
                else if (stage == 42 && now-since>.7) { WalkTo(flow.ReturnPoint.position); Next(50,now); }
                else if (stage == 50 && Walk(now))
                {
                    Require(sawAgent && sawVisible && heardOccluded && world.Consequences.Rule.SoundExchangeCount==1 && world.Consequences.Rule.LightTransferCount==0 && world.Session.BoxSeal==1 && world.Session.LakeSeal==0 && !world.Cipher.Acquired,"Search/listening altered independent state.");
                    Capture("day6-return"); Next(51,now);
                }
                else if (stage == 51 && now-since>.4) { world.Sleep(); Next(52,now); }
                else if (stage == 52 && now-since>.5)
                {
                    Require(world.Session.Clock.Day==7 && !flow.ShowStatus && world.Consequences.Rule.LightTransferCount==1 && world.Presentation.PermanentNight,"D7 schedule/guide retirement failed.");
                    Complete(true,"Real WASD/E/Esc "+(Late?"D6 original T05 without prior T04":"static radio / lakeside original T04 repeat / forest original T05")+" mouse wrong+correct compile, sole chase target visible then wall-occluded spatial chime entry, no forced discovery/SEAL/time, physical return; late="+Late+", tuning="+Tuning+", metres="+metres+", rings="+string.Join(",",rings)+", closePasses="+world.Chase.Gate.PassCount);
                }
                else if(stage==80 && world.Session.Clock.Day==5 && now-since>1)
                { Require(flow.FourthTablet.gameObject.activeInHierarchy && !flow.FifthTablet.gameObject.activeInHierarchy && world.Campaign.Progress.FoundCount==0 && world.Consequences.Rule.SoundExchangeCount==1 && world.Consequences.Rule.LightTransferCount==0 && !world.Consequences.RadioBroadcast.isPlaying && world.Consequences.RadioStatic.isPlaying && world.Consequences.DemonBroadcast.isPlaying,"Natural D5 boundary failed."); world.Sleep(); Next(81,now); }
                else if(stage==81 && now-since>.5)
                { Require(world.Session.Clock.Day==6 && flow.FifthTablet.gameObject.activeInHierarchy && !world.Listening.Active && world.Campaign.Progress.FoundCount==0,"D6 required a search/read prerequisite."); Complete(true,"Natural D4→D5 broadcast/static and T04, D6 original T05 availability without any discovery; no early LIGHT/tool activation."); }
                else if(stage==90 && now-since>.4)
                {
                    int day=world.Session.Clock.Day; Require(world.Campaign.Progress.FoundCount==0 && !world.Listening.Active && world.Consequences.Rule.SoundExchangeCount==1 && world.Consequences.Rule.LightTransferCount==(day>=7?1:0),"Skip blocked or duplicated date effects.");
                    if(day<9) { world.Sleep(); Next(90,now); } else { Require(world.Session.Descended && !flow.ShowStatus && !flow.TrailVisible && !flow.LargeTrailVisible,"Terminal cleanup failed."); Complete(true,"D5→D9 sleep-only, zero discoveries/checks/tool/SEAL, SOUND/LIGHT exactly once, normal descent and cleanup."); }
                }
            }
            catch(Exception e) { Complete(false,"Stage "+stage+", route="+routeIndex+": "+e); }
        }
        void BeginToolRoute(double now) { routeIndex=0; WalkTo(flow.ListeningRoute[0].position); Next(20,now); }
        void ObserveRings()
        {
            var sensors=world.Listening.Chimes;
            for(int i=0;i<sensors.Length;i++)
            {
                if(sensors[i].RingCount<=rings[i]) continue;
                rings[i]=sensors[i].RingCount;
                Require(sensors[i].Source.isPlaying && sensors[i].Source.spatialBlend==1 && sensors[i].MonitoredTarget==world.Chase.Agent.transform,"Ring lacks actual spatial playback/original chase target.");
                var cam=world.Player.ViewCamera; Vector3 target=sensors[i].MonitoredTarget.position+Vector3.up*.3f;
                Vector3 delta=target-cam.transform.position;
                var hit=Physics.RaycastAll(cam.transform.position,delta.normalized,delta.magnitude,~0,QueryTriggerInteraction.Ignore)
                    .Where(h=>!h.collider.transform.IsChildOf(world.Player.transform) && !h.collider.transform.IsChildOf(sensors[i].MonitoredTarget))
                    .OrderBy(h=>h.distance).FirstOrDefault();
                bool blocked=hit.collider;
                Vector3 screen=cam.WorldToViewportPoint(target);
                if(stage>=28 && !blocked && screen.z>0 && screen.x>0 && screen.x<1 && screen.y>0 && screen.y<1) sawVisible=true;
                if(stage==41 && blocked && hit.collider.name=="ListeningOccluder") heardOccluded=true;
                Debug.Log("SIGNAL_REAL_ENTRY stage="+stage+", chime="+i+", target="+target+", occluder="+(blocked?hit.collider.name:"none"));
            }
        }
        void Click(Transform target,bool down)
        {
            Vector3 screen=world.Player.ViewCamera.WorldToScreenPoint(target.position);
            Require(screen.z>0 && screen.x>0 && screen.x<Screen.width && screen.y>0 && screen.y<Screen.height,"T05 click target outside camera.");
            var hit=Physics.RaycastAll(world.Player.ViewCamera.ScreenPointToRay(screen),20).Where(h=>!h.collider.transform.IsChildOf(world.Player.transform)).OrderBy(h=>h.distance).FirstOrDefault();
            Require(hit.collider && hit.collider.transform.IsChildOf(target),"T05 click blocked by "+(hit.collider?hit.collider.name:"nothing"));
            InputSystem.QueueStateEvent(mouse,new MouseState{position=new Vector2(screen.x,screen.y),buttons=(ushort)(down?1:0)});
        }
        void WalkTo(Vector3 target)
        {
            var path=new NavMeshPath();
            Require(NavMesh.SamplePosition(world.Player.transform.position,out var from,1.5f,NavMesh.AllAreas) && NavMesh.SamplePosition(target,out var to,1.5f,NavMesh.AllAreas) && NavMesh.CalculatePath(from.position,to.position,NavMesh.AllAreas,path) && path.status==NavMeshPathStatus.PathComplete,"No complete player fixture path to "+target);
            corners=path.corners; corner=corners.Length>1?1:0; lastSteer=-100; walkStarted=Time.timeAsDouble; Keys();
        }
        bool Walk(double now)
        {
            var position=world.Player.transform.position;
            if(now-walkStarted>30) throw new InvalidOperationException("Walking blocked at "+position+", corner="+corners[corner]);
            metres+=Vector3.Distance(position,previous); previous=position;
            Vector3 delta=corners[corner]-position; delta.y=0;
            if(delta.magnitude<.2f) { if(++corner>=corners.Length) { Keys(); return true; } lastSteer=-100; delta=corners[corner]-position; delta.y=0; }
            if(now-lastSteer>.15) { Aim(position+delta+Vector3.up*world.Player.ViewCamera.transform.localPosition.y); lastSteer=now; }
            Keys(Key.W); return false;
        }
        void Aim(Vector3 point)
        {
            var cam=world.Player.ViewCamera.transform; var desired=Quaternion.LookRotation(point-cam.position).eulerAngles;
            float yaw=Mathf.DeltaAngle(world.Player.transform.eulerAngles.y,desired.y), pitch=Mathf.DeltaAngle(0,cam.localEulerAngles.x), targetPitch=Mathf.DeltaAngle(0,desired.x);
            InputSystem.QueueStateEvent(mouse,new MouseState{delta=new Vector2(yaw/world.Settings.MouseSensitivity,(pitch-targetPitch)/world.Settings.MouseSensitivity)});
        }
        void SetupInput()
        {
            oldBackground=InputSystem.settings.backgroundBehavior; oldInput=InputSystem.settings.editorInputBehaviorInPlayMode; oldRun=Application.runInBackground;
            InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus; InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView; Application.runInBackground=true;
            keyboard=InputSystem.AddDevice<Keyboard>(); mouse=InputSystem.AddDevice<Mouse>(); InputSystem.onAfterUpdate+=Current; Current(); Cursor.lockState=CursorLockMode.Locked;
        }
        void Keys(params Key[] keys)=>InputSystem.QueueStateEvent(keyboard,new KeyboardState(keys));
        void Current() { keyboard?.MakeCurrent(); mouse?.MakeCurrent(); }
        void Next(int value,double time) { stage=value; since=time; }
        void Capture(string name)
        { Canvas.ForceUpdateCanvases(); foreach(var window in Resources.FindObjectsOfTypeAll<EditorWindow>()) if(window.GetType().Name=="GameView") window.Repaint(); ScreenCapture.CaptureScreenshot(Path.GetFullPath("Temp/DemonSliceReports/"+operation+"-"+name+".png")); }
        public void Cleanup()
        { if(keyboard==null)return; InputSystem.onAfterUpdate-=Current; InputSystem.RemoveDevice(keyboard); InputSystem.RemoveDevice(mouse); InputSystem.settings.backgroundBehavior=oldBackground; InputSystem.settings.editorInputBehaviorInPlayMode=oldInput; Application.runInBackground=oldRun; keyboard=null; mouse=null; }
        void Complete(bool success,string message) { finished=true; Cleanup(); finish(success,message); EditorApplication.delayCall+=EditorApplication.ExitPlaymode; }
    }
}
