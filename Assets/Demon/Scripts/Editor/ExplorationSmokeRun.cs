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
    // Drives the existing Input Actions and CharacterController; navigation only supplies walking corners.
    public sealed class ExplorationSmokeRun
    {
        readonly string operation; readonly Action<bool,string> finish;
        readonly double started=EditorApplication.timeSinceStartup;
        SliceSceneController world; ExplorationFlow flow; Keyboard keyboard; Mouse mouse;
        InputSettings.BackgroundBehavior oldBackground; InputSettings.EditorInputBehaviorInPlayMode oldInput;
        bool oldRun,finished; int stage,observation,trailIndex,corner,t01Reads,lastFrame=-1; double since,lastSteer,walkStarted;
        Vector3[] corners; Vector3 startPosition; float walked; Vector3 previous;
        bool Late=>operation=="exploration-late"; bool Tuning=>operation=="exploration-tuning";
        public ExplorationSmokeRun(string op,Action<bool,string> callback){operation=op;finish=callback;}
        static void Require(bool value,string message){if(!value)throw new InvalidOperationException(message);}
        public void Tick()
        {
            if(finished||!Application.isPlaying)return;
            try
            {
                if(EditorApplication.timeSinceStartup-started>240)throw new TimeoutException("Exploration stage "+stage+", position "+(world?world.Player.transform.position.ToString():"missing"));
                if(!world)world=UnityEngine.Object.FindAnyObjectByType<SliceSceneController>();
                if(world&&!world.enabled)throw new InvalidOperationException("World disabled; inspect initialization error.");
                if(!world||world.Session==null||!world.Exploration||world.Exploration.Log==null)return;
                if(lastFrame==Time.frameCount)return;lastFrame=Time.frameCount;
                flow=world.Exploration;double now=Time.timeAsDouble;
                if(stage==0)
                {
                    Require(world.Session.Clock.Day==(Late?2:1)&&flow.Log.Count==0&&flow.TrailVisible==Late&&flow.FirstTablet.gameObject.activeInHierarchy==Late,"Saved start/day gate failed.");
                    Require(!world.Tether.MotionReady&&!world.Tether.TargetVisible&&world.Session.ExchangeCount==0,"D1/D2 motion/SEAL leaked.");
                    Require(Math.Abs(world.Settings.WalkMetresPerSecond-(Tuning?2.7f:3.3f))<.001f&&flow.Definition.Radio.Normal.StartsWith("【保存参数检查】")==Tuning,"Saved tuning ignored.");
                    if(operation=="exploration-skip"){world.Sleep();Next(70,now);return;}
                    oldBackground=InputSystem.settings.backgroundBehavior;oldInput=InputSystem.settings.editorInputBehaviorInPlayMode;oldRun=Application.runInBackground;
                    InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;Application.runInBackground=true;
                    keyboard=InputSystem.AddDevice<Keyboard>();mouse=InputSystem.AddDevice<Mouse>();InputSystem.onAfterUpdate+=Current;Current();Cursor.lockState=CursorLockMode.Locked;
                    startPosition=previous=world.Player.transform.position;WalkTo(flow.Observations[0].Approach.position);Next(1,now);
                }
                else if(stage==1&&Walk(now)){Aim(flow.Observations[observation/2].AimTarget.position);Next(2,now);}
                else if(stage==2&&now-since>.3){Capture("observation-approach-"+observation);Keys(Key.E);Next(3,now);}
                else if(stage==3&&now-since>.3)
                {
                    Keys();var site=flow.Observations[observation/2];Require(world.Player.Reading,"E observation failed: "+site.Site+", prompt="+world.Player.FocusPrompt+", position="+world.Player.transform.position+", camera="+world.Player.ViewCamera.transform.position+" forward="+world.Player.ViewCamera.transform.forward+", target="+site.AimTarget.position+", hits="+string.Join(";",Physics.RaycastAll(world.Player.ViewCamera.transform.position,world.Player.ViewCamera.transform.forward,world.Settings.InteractionMetres).OrderBy(h=>h.distance).Select(h=>h.collider.name+":"+h.distance+":"+h.collider.GetComponentInParent<SliceInteractable>())));
                    Require(flow.Log.Count==(Late?0:observation/2+1)&&world.Session.Clock.Day==(Late?2:1)&&world.Session.ExchangeCount==0,"Duplicate/day/SEAL record failed.");
                    string expected=site.Site==Domain.ObservationSite.Radio?flow.Definition.Radio.Normal:site.Site==Domain.ObservationSite.WindVane?flow.Definition.WindVane.Normal:flow.Definition.Lake.Normal;
                    Require(world.ReadingText.Contains(expected)&&world.ReadingText.Contains("观察时刻：D"+(Late?2:1)),"Saved text/time snapshot missing.");Next(4,now);
                }
                else if(stage==4&&now-since>.65){Capture("observation-"+observation);Keys(Key.Escape);Next(5,now);}
                else if(stage==5&&now-since>.15){Keys();Next(6,now);}
                else if(stage==6&&now-since>.8)
                {
                    Require(world.Player.CanUseTools,"Esc failed to return to walking.");observation++;
                    if(observation<(Late?2:6)){WalkTo(flow.Observations[observation/2].Approach.position);Next(1,now);}
                    else{if(!Late)world.Sleep();Next(10,now);}
                }
                else if(stage==10&&now-since>.4)
                {
                    Require(world.Session.Clock.Day==2&&flow.Log.Count==(Late?0:3)&&flow.TrailVisible&&flow.FirstTablet.gameObject.activeInHierarchy&&!world.Tether.TargetVisible&&!world.Tether.MotionReady,"D2 footprints/T01 or inspection persistence failed.");
                    WalkTo(flow.TrailWaypoints[0].position);Next(11,now);
                }
                else if(stage==11&&Walk(now))
                {
                    if(trailIndex==2){Aim(flow.TrailWaypoints[3].position);Capture("human-trail");}
                    trailIndex++;if(trailIndex<flow.TrailWaypoints.Length)WalkTo(flow.TrailWaypoints[trailIndex].position);
                    else{Aim(flow.FirstTablet.transform.position);Next(12,now);}
                }
                else if(stage==12&&now-since>.35){Keys(Key.E);Next(13,now);}
                else if(stage==13&&now-since>.3)
                {
                    Keys();Require(world.Player.Reading&&world.Campaign.Progress.Found(0)&&world.Campaign.Progress.FoundCount==1&&!world.Tether.MotionReady&&world.Session.ExchangeCount==0,"Physical cave T01 read/discovery/date failed: "+world.Player.FocusPrompt);Next(14,now);
                }
                else if(stage==14&&now-since>.8){Capture("cave-t01");Keys(Key.Escape);Next(15,now);}
                else if(stage==15&&now-since>.15){Keys();Next(16,now);}
                else if(stage==16&&now-since>.8)
                {
                    Require(world.Player.CanUseTools,"T01 Esc return failed.");
                    if(t01Reads++==0){Aim(flow.FirstTablet.transform.position);Next(12,now);}
                    else{WalkTo(flow.ReturnPoint.position);Next(17,now);}
                }
                else if(stage==17&&Walk(now))
                {
                    Require(world.Player.transform.position.z>-5&&world.Player.transform.position.y>-.2f&&walked>45,"Camp/cave physical roundtrip missing or fell through ground.");Capture("camp-return");world.Sleep();Next(18,now);
                }
                else if(stage==18&&now-since>.6)
                {
                    Require(world.Session.Clock.Day==3&&world.Tether.MotionReady&&world.Tether.TargetVisible&&world.Chase.Agent.isOnNavMesh,"D3 existing motion/chase on expanded native mesh failed.");
                    var path=new NavMeshPath();Require(world.Chase.Agent.CalculatePath(flow.TrailWaypoints.Last().position,path)&&path.status==NavMeshPathStatus.PathComplete,"D3 chase cannot reach cave.");
                    if(!Late&&!Tuning)
                    {
                        // Only after proving the physical return, move the test target to the cave to exercise production pursuit.
                        var body=world.Player.GetComponent<CharacterController>();body.enabled=false;world.Player.transform.position=flow.TrailWaypoints.Last().position;body.enabled=true;Next(19,now);
                    }
                    else Complete(true,Result());
                }
                else if(stage==19)
                {
                    if(Vector3.Distance(world.Chase.Agent.transform.position,world.Player.transform.position)<2.2f)Complete(true,Result()+" Production NavMeshAgent pursued the test target through the forest gate into the cave.");
                    else if(now-since>25)throw new InvalidOperationException("Native chase failed to enter cave: "+world.Chase.Agent.transform.position+", "+world.Chase.Phase);
                }
                else if(stage==70&&now-since>.4)
                {
                    Require(flow.Log.Count==0&&world.Session.ExchangeCount==0,"Skipping created inspections or SEAL.");
                    if(world.Session.Clock.Day<9){world.Sleep();Next(70,now);}else{Require(world.Session.Descended&&world.Consequences.Rule.SoundExchangeCount==1&&world.Consequences.Rule.LightTransferCount==1,"Skip blocked scheduled ending.");Complete(true,"Zero inspections/discoveries can sleep D1 through D9; D2 trail and existing scheduled effects/ending remain optional.");}
                }
            }
            catch(Exception e){Complete(false,"Stage "+stage+", observation "+observation+", trail "+trailIndex+": "+e);}
        }
        string Result() => (Late?"Saved D2 start: real observation remains uncounted for D1; ":"Saved D1: three unique optional observations and duplicate reads with real WASD/E/Esc; ")+"D2 human trail, physical cave T01 read and repeat without duplicate discovery, physical camp return, no early MOTION/SEAL, D3 native path connectivity; tuning="+Tuning+", physical metres="+walked+", "+operation;
        void WalkTo(Vector3 target)
        {
            Vector3 position=world.Player.transform.position;
            // Agent bake allows 0.75m steps; the 0.3m player must walk around the 0.4m sleeping bag.
            // These fixture waypoints exercise the open camp aisle instead of treating the agent path as a player route.
            Vector3[] stops=stage==0?new[]{new Vector3(-6,.04f,-4.5f),target}:
                stage==6&&observation/2==1&&position.x<-6?new[]{new Vector3(-6,.04f,-4.5f),flow.ReturnPoint.position,target}:new[]{target};
            var joined=new System.Collections.Generic.List<Vector3>();
            foreach(var stop in stops)
            {
                var path=new NavMeshPath();Require(NavMesh.SamplePosition(position,out var from,1.5f,NavMesh.AllAreas)&&NavMesh.SamplePosition(stop,out var to,1.5f,NavMesh.AllAreas)&&NavMesh.CalculatePath(from.position,to.position,NavMesh.AllAreas,path)&&path.status==NavMeshPathStatus.PathComplete,"No complete authored walking path to "+stop);
                joined.AddRange(joined.Count==0?path.corners:path.corners.Skip(1));position=path.corners.Last();
            }
            corners=joined.ToArray();corner=corners.Length>1?1:0;lastSteer=-100;walkStarted=Time.timeAsDouble;Keys();
        }
        bool Walk(double now)
        {
            var position=world.Player.transform.position;
            if(now-walkStarted>25){ Capture("walking-blocked"); throw new InvalidOperationException("Walking blocked at "+position+", corner "+corner+"/"+corners.Length+" target="+corners[corner]+", path="+string.Join(";",corners.Select(p=>p.ToString()))+", yaw="+world.Player.transform.eulerAngles.y+", nearby="+string.Join(";",Physics.OverlapSphere(position+Vector3.up,.7f).Select(c=>c.name+" bounds="+c.bounds))); }
            walked+=Vector3.Distance(position,previous);previous=position;
            Vector3 delta=corners[corner]-position;delta.y=0;
            if(delta.magnitude<.2f){corner++;if(corner>=corners.Length){Keys();return true;}lastSteer=-100;delta=corners[corner]-position;delta.y=0;}
            if(now-lastSteer>.15){Aim(position+delta+Vector3.up*world.Player.ViewCamera.transform.localPosition.y);lastSteer=now;}
            Keys(Key.W);return false;
        }
        void Aim(Vector3 point)
        {
            var cam=world.Player.ViewCamera.transform;var desired=Quaternion.LookRotation(point-cam.position).eulerAngles;
            float yaw=Mathf.DeltaAngle(world.Player.transform.eulerAngles.y,desired.y),pitch=Mathf.DeltaAngle(0,cam.localEulerAngles.x),targetPitch=Mathf.DeltaAngle(0,desired.x);
            InputSystem.QueueStateEvent(mouse,new MouseState{delta=new Vector2(yaw/world.Settings.MouseSensitivity,(pitch-targetPitch)/world.Settings.MouseSensitivity)});
        }
        void Keys(params Key[] keys)=>InputSystem.QueueStateEvent(keyboard,new KeyboardState(keys));
        void Current(){keyboard?.MakeCurrent();mouse?.MakeCurrent();}
        void Next(int value,double now){stage=value;since=now;}
        void Capture(string name)=>ScreenCapture.CaptureScreenshot(Path.GetFullPath("Temp/DemonSliceReports/"+operation+"-"+name+".png"));
        public void Cleanup(){if(keyboard==null)return;InputSystem.onAfterUpdate-=Current;InputSystem.RemoveDevice(keyboard);InputSystem.RemoveDevice(mouse);InputSystem.settings.backgroundBehavior=oldBackground;InputSystem.settings.editorInputBehaviorInPlayMode=oldInput;Application.runInBackground=oldRun;keyboard=null;mouse=null;}
        void Complete(bool success,string message){finished=true;Cleanup();finish(success,message);EditorApplication.delayCall+=EditorApplication.ExitPlaymode;}
    }
}