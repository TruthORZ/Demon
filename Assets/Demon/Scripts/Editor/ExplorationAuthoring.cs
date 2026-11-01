using System;
using System.IO;
using System.Linq;
using Demon.Slice.Domain;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
namespace Demon.Slice.Editor
{
    public static class ExplorationAuthoring
    {
        public const string ScenePath = "Assets/Demon/Scenes/DemonExploration.unity";
        const string DefinitionPath = "Assets/Demon/Data/ExplorationDefinition.asset";
        const string SettingsPath = "Assets/Demon/Data/ExplorationSettings.asset";
        const string NavigationPath = "Assets/Demon/Data/ExplorationNavigation.asset";
        static readonly string[] Materials = { "PineNeedle", "HumanFootprint" };
        static void Editable() { if (EditorApplication.isPlayingOrWillChangePlaymode || SceneManager.GetActiveScene().isDirty) throw new InvalidOperationException("Leave Play and save current human edits before exploration authoring."); }
        public static string Create()
        {
            Editable(); foreach(string path in new[] { ScenePath, DefinitionPath, SettingsPath, NavigationPath }) if(File.Exists(path)) throw new InvalidOperationException("Exploration output exists; edit saved assets: " + path);
            foreach(string name in Materials) if(File.Exists("Assets/Demon/Art/Materials/" + name + ".mat")) throw new InvalidOperationException("Preserving existing exploration material.");
            MaterialAsset("PineNeedle", new Color(.08f, .15f, .12f)); MaterialAsset("HumanFootprint", new Color(.14f, .2f, .24f));
            AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<ExplorationDefinition>(), DefinitionPath);
            EditorSceneManager.OpenScene(NineDayAuthoring.ScenePath); var world = UnityEngine.Object.FindAnyObjectByType<SliceSceneController>();
            if (!world.Consequences || !world.Retrieval || world.Exploration || GameObject.Find("Exploration_Authored")) throw new InvalidOperationException("Expected accepted nine-day source without exploration.");
            var root = new GameObject("Exploration_Authored").transform; var flow = world.gameObject.AddComponent<ExplorationFlow>(); world.ConfigureExploration(flow);
            var font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Demon/UI/WindowsChinese.fontsettings");
            var boundary = GameObject.Find("Environment/SouthBoundary").GetComponent<BoxCollider>();
            if(Vector3.Distance(boundary.size, new Vector3(26,3,.3f)) > .01f || boundary.center.sqrMagnitude > .001f) throw new InvalidOperationException("Unexpected authored south boundary; align manual layout before migration.");
            boundary.size = new Vector3(14,3,.3f); boundary.center = new Vector3(-6,0,0); EditorUtility.SetDirty(boundary);
            Barrier("SouthBoundary_RightSegment", root, new Vector3(9,1,-10), new Vector3(8,3,.3f));
            Primitive("SnowForestGround", root, new Vector3(5.5f,-.15f,-19), new Vector3(23,.3f,18), Mat("Snow"));
            Barrier("ForestWestBoundary",root,new Vector3(-6,1,-19),new Vector3(.3f,3,18)); Barrier("ForestEastBoundary",root,new Vector3(17,1,-19),new Vector3(.3f,3,18));
            Barrier("ForestSouthBoundary",root,new Vector3(5.5f,1,-28),new Vector3(23,3,.3f)); Barrier("ForestNorthReturnBoundary",root,new Vector3(15,1,-10),new Vector3(4,3,.3f));
            var trees = new GameObject("ForestTrees_Editable").transform; trees.SetParent(root,false);
            Vector3[] treePositions = { new Vector3(-2,0,-12),new Vector3(7,0,-13),new Vector3(0,0,-17),new Vector3(11,0,-16),new Vector3(3,0,-21),new Vector3(15,0,-20),new Vector3(-2,0,-25),new Vector3(4,0,-26),new Vector3(16,0,-26) };
            for(int i=0;i<treePositions.Length;i++)
            {
                var tree=new GameObject("Pine_"+i).transform;tree.SetParent(trees,false);tree.localPosition=treePositions[i];
                Primitive("Trunk",tree,new Vector3(0,1.5f,0),new Vector3(.25f,3,.25f),Mat("BoxWood"));
                for(int j=0;j<3;j++)Primitive("Needles_"+j,tree,new Vector3(0,2+j*.7f,0),new Vector3(2.8f-j*.6f,1.8f,2.8f-j*.6f),Mat("PineNeedle"),false,PrimitiveType.Sphere);
            }
            var cave=new GameObject("Cave_Editable").transform;cave.SetParent(root,false);cave.localPosition=new Vector3(10,0,-23);
            Primitive("WestRock",cave,new Vector3(-3.7f,1.5f,0),new Vector3(.5f,3,6.5f),Mat("Stone")); Primitive("EastRock",cave,new Vector3(3.7f,1.5f,0),new Vector3(.5f,3,6.5f),Mat("Stone"));
            Primitive("BackRock",cave,new Vector3(0,1.5f,-3),new Vector3(7.6f,3,.5f),Mat("Stone")); Primitive("RoofRock",cave,new Vector3(0,3.1f,0),new Vector3(8,.4f,6.5f),Mat("Stone"));
            foreach(float x in new[]{-2.6f,2.6f})Primitive("EntranceRock",cave,new Vector3(x,1.0f,3),new Vector3(2,2.4f,1.5f),Mat("Stone"),true,PrimitiveType.Sphere);
            var lamp=new GameObject("CaveWorkLamp_Replaceable").AddComponent<Light>();lamp.transform.SetParent(cave,false);lamp.transform.localPosition=new Vector3(0,2,-.5f);lamp.type=LightType.Point;lamp.range=6;lamp.intensity=1.5f;lamp.color=new Color(1,.73f,.4f);
            for(int i=0;i<12;i++){float a=i*2*Mathf.PI/12;var mark=Primitive("OldRitualEtching_"+i,cave,new Vector3(Mathf.Sin(a)*1.2f,.015f,-1.6f+Mathf.Cos(a)*1.2f),new Vector3(.3f,.025f,.04f),Mat("HumanFootprint"),false);mark.transform.localRotation=Quaternion.Euler(0,i*30,0);}
            var first=UnityEngine.Object.FindObjectsByType<NineDayTablet>(FindObjectsInactive.Include,FindObjectsSortMode.None).Single(t=>t.Index==0);
            first.transform.SetParent(cave,true);first.transform.localPosition=new Vector3(0,1.2f,-1.6f);first.transform.localRotation=Quaternion.Euler(0,180,0);EditorUtility.SetDirty(first.transform);
            var trail=new GameObject("HumanTrail_D2_Editable").transform;trail.SetParent(root,false);
            var routeRoot=new GameObject("TrailRoute_Editable").transform;routeRoot.SetParent(root,false);
            Vector3[] route={new Vector3(2.8f,.04f,-7),new Vector3(3,.04f,-9.5f),new Vector3(3,.04f,-12.5f),new Vector3(5,.04f,-16),new Vector3(8,.04f,-18.5f),new Vector3(10,.04f,-22.3f)};
            var nodes=new Transform[route.Length];for(int i=0;i<route.Length;i++)nodes[i]=Anchor("Trail_"+i,routeRoot,route[i],route[i]+Vector3.back);
            int foot=0;for(int segment=0;segment<route.Length-1;segment++)
            {
                Vector3 delta=route[segment+1]-route[segment];float length=delta.magnitude;Vector3 direction=delta/length;Vector3 right=Vector3.Cross(Vector3.up,direction);
                for(float distance=0;distance<length;distance+=.65f)
                {var print=Primitive("SmallHumanFoot_"+foot,trail,route[segment]+direction*distance+right*(foot%2==0?-.12f:.12f),new Vector3(.10f,.018f,.24f),Mat("HumanFootprint"),false,PrimitiveType.Sphere);print.transform.localRotation=Quaternion.LookRotation(direction);print.GetComponent<Renderer>().shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;foot++;}
            }
            var sign=new GameObject("ForestPathSign").transform;sign.SetParent(root,false);sign.localPosition=new Vector3(1,0,-8.2f);
            Primitive("Post",sign,new Vector3(0,.8f,0),new Vector3(.09f,1.6f,.09f),Mat("BoxWood"));Primitive("Face",sign,new Vector3(0,1.5f,0),new Vector3(1.6f,.5f,.12f),Mat("BoxWood"));
            var signText=Label("PathName",sign,new Vector3(0,1.5f,.07f),"林间小径",font,.025f);signText.transform.localRotation=Quaternion.Euler(0,180,0);
            var points=new StationObservation[3];var landmarks=world.Cipher.Wheel.Landmarks;
            var radioView=GameObject.Find("Consequences_Authored/RadioView").transform;
            points[0]=landmarks[0].gameObject.AddComponent<StationObservation>();points[0].Configure(flow,ObservationSite.Radio,radioView,landmarks[0].Find("RadioBody"),Anchor("RadioApproach",root,new Vector3(-9.1f,.04f,-2.4f),landmarks[0].position+Vector3.up));
            var vaneTarget=Primitive("ObservationFace",landmarks[2],new Vector3(0,1.2f,-.2f),new Vector3(.8f,.4f,.08f),Mat("BoxWood"));
            Label("ObservationLabel",landmarks[2],new Vector3(0,1.2f,-.25f),"观察风向标",font,.012f);
            points[1]=landmarks[2].gameObject.AddComponent<StationObservation>();points[1].Configure(flow,ObservationSite.WindVane,Anchor("WindVaneView",root,landmarks[2].position+new Vector3(0,1.65f,-2.2f),landmarks[2].position+Vector3.up*1.5f),vaneTarget.transform,Anchor("WindVaneApproach",root,landmarks[2].position+new Vector3(0,.04f,-2.1f),landmarks[2].position+Vector3.up*1.2f));
            var lake=new GameObject("LakeWatchPoint").transform;lake.SetParent(root,false);lake.localPosition=new Vector3(1.5f,0,4.3f);
            Primitive("Post",lake,new Vector3(0,.7f,0),new Vector3(.08f,1.4f,.08f),Mat("BoxWood"));var lakeTarget=Primitive("ObservationFace",lake,new Vector3(0,1.2f,0),new Vector3(.9f,.45f,.09f),Mat("BoxWood"));Label("ObservationLabel",lake,new Vector3(0,1.2f,-.05f),"湖岸观察点",font,.012f);
            points[2]=lake.gameObject.AddComponent<StationObservation>();points[2].Configure(flow,ObservationSite.Lake,GameObject.Find("Consequences_Authored/LakeView").transform,lakeTarget.transform,Anchor("LakeApproach",root,new Vector3(1.5f,.04f,2.1f),lake.position+Vector3.up*1.2f));
            var home=Anchor("CampReturn",root,new Vector3(0,.04f,-4),Vector3.forward);flow.Configure(AssetDatabase.LoadAssetAtPath<ExplorationDefinition>(DefinitionPath),points,trail.gameObject,nodes,first,home);
            var hud=UnityEngine.Object.FindAnyObjectByType<SliceHud>();var status=new GameObject("ExplorationStatus",typeof(RectTransform),typeof(Text)).GetComponent<Text>();status.transform.SetParent(hud.transform,false);
            var rect=status.rectTransform;rect.anchorMin=rect.anchorMax=rect.pivot=Vector2.one;rect.anchoredPosition=new Vector2(-30,-26);rect.sizeDelta=new Vector2(600,150);status.font=font;status.fontSize=24;status.alignment=TextAnchor.UpperRight;status.color=Color.white;status.raycastTarget=false;var outline=status.gameObject.AddComponent<Outline>();outline.effectColor=new Color(0,0,0,.8f);outline.effectDistance=new Vector2(1,-1);hud.ConfigureExplorationStatus(status);EditorUtility.SetDirty(hud);
            var binding=hud.GetComponent<SliceFontBinding>();binding.Configure(font,hud.GetComponentsInChildren<Text>(true),UnityEngine.Object.FindObjectsByType<TextMesh>(FindObjectsInactive.Include));EditorUtility.SetDirty(binding);
            var plan=world.Campaign.Definition;plan.Days[0].Objective="可选检查电台、风向标和湖岸，熟悉正常状态。完成与否都可用睡袋到次日。";plan.Days[1].Objective="南侧林间小径出现小脚印，沿痕迹去山洞寻找T01。尚未到恶魔获得运动的日期。";EditorUtility.SetDirty(plan);
            FitSnowColliders();var surface=UnityEngine.Object.FindAnyObjectByType<NavMeshSurface>();surface.center=new Vector3(2,1,-11);surface.size=new Vector3(30,8,34);
            bool firstActive=first.gameObject.activeSelf;bool finalActive=world.Tablet.gameObject.activeSelf;bool pickupActive=world.Retrieval.Pickup.gameObject.activeSelf;
            first.gameObject.SetActive(true);world.Tablet.gameObject.SetActive(true);world.Retrieval.Pickup.gameObject.SetActive(true);surface.BuildNavMesh();if(!surface.navMeshData)throw new InvalidOperationException("Expanded navigation bake failed.");AssetDatabase.CreateAsset(surface.navMeshData,NavigationPath);EditorUtility.SetDirty(surface);
            first.gameObject.SetActive(firstActive);world.Tablet.gameObject.SetActive(finalActive);world.Retrieval.Pickup.gameObject.SetActive(pickupActive);trail.gameObject.SetActive(false);EditorUtility.SetDirty(world);EditorUtility.SetDirty(flow);Save();
            var quick=UnityEngine.Object.Instantiate(world.Settings);quick.name="ExplorationSettings";quick.StartDay=1;quick.StartHour=8;quick.InitialLakeSealed=true;AssetDatabase.CreateAsset(quick,SettingsPath);AssetDatabase.SaveAssets();
            if(!AssetDatabase.CopyAsset(NineDayAuthoring.ScenePath,ScenePath))throw new InvalidOperationException("Could not save exploration scene.");EditorSceneManager.OpenScene(ScenePath);world=UnityEngine.Object.FindAnyObjectByType<SliceSceneController>();var so=new SerializedObject(world);so.FindProperty("settings").objectReferenceValue=AssetDatabase.LoadAssetAtPath<SliceSettings>(SettingsPath);so.ApplyModifiedProperties();Save();EditorSceneManager.OpenScene(ScenePath);return Validate();
        }
        static GameObject Primitive(string name,Transform parent,Vector3 position,Vector3 scale,Material material,bool collision=true,PrimitiveType type=PrimitiveType.Cube)
        {var go=GameObject.CreatePrimitive(type);go.name=name;go.transform.SetParent(parent,false);go.transform.localPosition=position;go.transform.localScale=scale;go.GetComponent<Renderer>().sharedMaterial=material;if(!collision)UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());return go;}
        static void Barrier(string name,Transform parent,Vector3 position,Vector3 size){var node=new GameObject(name).transform;node.SetParent(parent,false);node.localPosition=position;node.gameObject.AddComponent<BoxCollider>().size=size;}
        static Transform Anchor(string name,Transform parent,Vector3 position,Vector3 lookAt){var node=new GameObject(name).transform;node.SetParent(parent,false);node.position=position;node.LookAt(lookAt);return node;}
        static TextMesh Label(string name,Transform parent,Vector3 position,string text,Font font,float size){var label=new GameObject(name).AddComponent<TextMesh>();label.transform.SetParent(parent,false);label.transform.localPosition=position;label.font=font;label.fontSize=64;label.characterSize=size;label.anchor=TextAnchor.MiddleCenter;label.alignment=TextAlignment.Center;label.text=text;return label;}
        static Material Mat(string name)=>AssetDatabase.LoadAssetAtPath<Material>("Assets/Demon/Art/Materials/"+name+".mat");
        static void MaterialAsset(string name,Color color){var shader=Shader.Find("Universal Render Pipeline/Lit");if(!shader)throw new InvalidOperationException("Installed URP Lit shader missing.");var mat=new Material(shader);mat.name=name;mat.SetColor("_BaseColor",color);AssetDatabase.CreateAsset(mat,"Assets/Demon/Art/Materials/"+name+".mat");}
        static void Save(){EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());EditorSceneManager.SaveScene(SceneManager.GetActiveScene());AssetDatabase.SaveAssets();}
        static void FitSnowColliders()
        {
            for(int i=0;i<12;i++)
            {
                var snow=GameObject.Find("Environment/SnowBank_"+i);if(!snow)throw new InvalidOperationException("Missing original snowbank "+i);
                var sphere=snow.GetComponent<SphereCollider>();if(!sphere)throw new InvalidOperationException("Original snowbank collider missing.");
                var mesh=snow.GetComponent<MeshCollider>();if(!mesh)mesh=snow.AddComponent<MeshCollider>();mesh.sharedMesh=snow.GetComponent<MeshFilter>().sharedMesh;sphere.enabled=false;EditorUtility.SetDirty(sphere);EditorUtility.SetDirty(mesh);
            }
        }
        public static string RepairSnowCollision()
        {
            Editable();EditorSceneManager.OpenScene(NineDayAuthoring.ScenePath);FitSnowColliders();
            var world=UnityEngine.Object.FindAnyObjectByType<SliceSceneController>();var surface=UnityEngine.Object.FindAnyObjectByType<NavMeshSurface>();var saved=AssetDatabase.LoadAssetAtPath<UnityEngine.AI.NavMeshData>(NavigationPath);
            bool first=world.Exploration.FirstTablet.gameObject.activeSelf,final=world.Tablet.gameObject.activeSelf,pickup=world.Retrieval.Pickup.gameObject.activeSelf;
            world.Exploration.FirstTablet.gameObject.SetActive(true);world.Tablet.gameObject.SetActive(true);world.Retrieval.Pickup.gameObject.SetActive(true);
            surface.BuildNavMesh();var baked=surface.navMeshData;if(!baked||baked==saved)throw new InvalidOperationException("Expected separate native rebake output.");
            surface.RemoveData();EditorUtility.CopySerialized(baked,saved);surface.navMeshData=saved;surface.AddData();UnityEngine.Object.DestroyImmediate(baked);EditorUtility.SetDirty(saved);EditorUtility.SetDirty(surface);
            world.Exploration.FirstTablet.gameObject.SetActive(first);world.Tablet.gameObject.SetActive(final);world.Retrieval.Pickup.gameObject.SetActive(pickup);Save();Validate();
            EditorSceneManager.OpenScene(ScenePath);FitSnowColliders();Save();return Validate()+" Snow mesh colliders match authored ellipsoids; original sphere components preserved disabled; new navigation GUID preserved.";
        }
        public static string RepairRadioApproach()
        {
            Editable();foreach(var path in new[]{NineDayAuthoring.ScenePath,ScenePath})
            {
                EditorSceneManager.OpenScene(path);var world=UnityEngine.Object.FindAnyObjectByType<SliceSceneController>();
                var stand=world.Exploration.Observations[0].Approach;stand.position=new Vector3(-9.1f,.04f,-2.4f);EditorUtility.SetDirty(stand);Save();
            }
            return "Saved radio approach clear of BoxA lock occlusion; facilities and navigation unchanged.";
        }
        public static void OpenQuick(){Editable();EditorSceneManager.OpenScene(ScenePath);}
        public static string Validate()
        {
            var world=UnityEngine.Object.FindAnyObjectByType<SliceSceneController>();var surface=UnityEngine.Object.FindAnyObjectByType<NavMeshSurface>();
            if(!world||!world.Settings||!world.Player||!world.Exploration||!world.Exploration.Definition||!world.Consequences||!surface||AssetDatabase.GetAssetPath(surface.navMeshData)!=NavigationPath)throw new InvalidOperationException("Exploration composition/navigation/settings missing.");
            if(world.Exploration.Observations.Length!=3||world.Exploration.FirstTablet.Index!=0||world.Exploration.FirstTablet.transform.position.z>-20||!world.Cipher.Wheel.ReadCode().SequenceEqual(new[]{3,4,2}))throw new InvalidOperationException("Original T01/cipher/observation migration failed.");
            RequirePath(new Vector3(0,0,-4),world.Exploration.TrailWaypoints.Last().position);RequirePath(world.Exploration.TrailWaypoints.Last().position,new Vector3(0,0,-4));
            return "Saved optional observations, D2 human trail/cave original T01, expanded native navigation roundtrip and unchanged cipher342 reloaded.";
        }
        static void RequirePath(Vector3 from,Vector3 to){var path=new UnityEngine.AI.NavMeshPath();if(!UnityEngine.AI.NavMesh.SamplePosition(from,out var start,1,UnityEngine.AI.NavMesh.AllAreas)||!UnityEngine.AI.NavMesh.SamplePosition(to,out var end,1,UnityEngine.AI.NavMesh.AllAreas)||!UnityEngine.AI.NavMesh.CalculatePath(start.position,end.position,UnityEngine.AI.NavMesh.AllAreas,path)||path.status!=UnityEngine.AI.NavMeshPathStatus.PathComplete)throw new InvalidOperationException("Camp/cave navigation is not completely connected.");}
        const string Tune="Demon.Exploration.Restore";
        public static void BeginProbe(bool late,bool tune)
        {
            if(SessionState.GetBool(Tune,false))throw new InvalidOperationException("Restore previous exploration probe first.");var world=UnityEngine.Object.FindAnyObjectByType<SliceSceneController>();SessionState.SetBool(Tune,true);SessionState.SetFloat(Tune+"day",world.Settings.StartDay);SessionState.SetFloat(Tune+"hour",world.Settings.StartHour);SessionState.SetFloat(Tune+"speed",world.Settings.WalkMetresPerSecond);SessionState.SetString(Tune+"text",world.Exploration.Definition.Radio.Normal);
            if(late){world.Settings.StartDay=2;world.Settings.StartHour=8;}if(tune){world.Settings.WalkMetresPerSecond=2.7f;world.Exploration.Definition.Radio.Normal="【保存参数检查】"+world.Exploration.Definition.Radio.Normal;EditorUtility.SetDirty(world.Exploration.Definition);}EditorUtility.SetDirty(world.Settings);Save();
        }
        public static void RestoreProbe()
        {
            if(!SessionState.GetBool(Tune,false))return;var world=UnityEngine.Object.FindAnyObjectByType<SliceSceneController>();world.Settings.StartDay=(int)SessionState.GetFloat(Tune+"day",1);world.Settings.StartHour=SessionState.GetFloat(Tune+"hour",8);world.Settings.WalkMetresPerSecond=SessionState.GetFloat(Tune+"speed",3.3f);world.Exploration.Definition.Radio.Normal=SessionState.GetString(Tune+"text",world.Exploration.Definition.Radio.Normal);EditorUtility.SetDirty(world.Settings);EditorUtility.SetDirty(world.Exploration.Definition);Save();SessionState.SetBool(Tune,false);Debug.Log("DEMON_EXPLORATION_PROBE_RESTORED");
        }
        public static string Build(){Editable();Directory.CreateDirectory("Builds/WindowsExploration");var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{ScenePath},locationPathName="Builds/WindowsExploration/DemonExploration.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});if(report.summary.result!=BuildResult.Succeeded)throw new InvalidOperationException("Exploration Windows build failed.");return "Exploration Windows64 succeeded, "+report.summary.totalSize+" bytes.";}
    }
}
