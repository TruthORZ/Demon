using System;
using System.IO;
using System.Linq;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Demon.Slice.Editor
{
    public static class SignalSearchAuthoring
    {
        public const string ScenePath = "Assets/Demon/Scenes/DemonSignalSearch.unity";
        const string SettingsPath = "Assets/Demon/Data/SignalSearchSettings.asset";
        const string NavigationPath = "Assets/Demon/Data/SignalSearchNavigation.asset";
        const string Probe = "Demon.SignalSearch.Restore";
        static void Editable()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || SceneManager.GetActiveScene().isDirty)
                throw new InvalidOperationException("Leave Play and save human scene edits before signal-search authoring.");
        }
        public static string Create()
        {
            Editable();
            foreach (string path in new[] { ScenePath, SettingsPath, NavigationPath })
                if (File.Exists(path)) throw new InvalidOperationException("Preserving existing signal-search output: " + path);
            EditorSceneManager.OpenScene(NineDayAuthoring.ScenePath);
            var world = UnityEngine.Object.FindAnyObjectByType<SliceSceneController>();
            if (!world || !world.Exploration || !world.Exploration.HasInvestigation || world.Exploration.HasSignalSearch || GameObject.Find("SignalSearch_Authored"))
                throw new InvalidOperationException("Expected accepted D1–D4 composition without signal-search extension.");
            var tablets = UnityEngine.Object.FindObjectsByType<NineDayTablet>(FindObjectsInactive.Include);
            var fourth = tablets.Single(t => t.Index == 3); var fifth = tablets.Single(t => t.Index == 4);
            var root = new GameObject("SignalSearch_Authored").transform;
            var lake = new GameObject("LakeRecordSite_Editable").transform; lake.SetParent(root, false);
            Move(fourth.transform, lake, new Vector3(4.5f, 1.2f, 4.6f), Quaternion.identity);
            var lakeView = fourth.transform.Find("ReadView");
            if (!lakeView) throw new InvalidOperationException("Original T04 ReadView missing.");
            lakeView.position = new Vector3(4.5f, 1.55f, 2.3f); lakeView.LookAt(fourth.transform.position + Vector3.up * .15f);
            var listeningSite = new GameObject("ForestListeningSite_Editable").transform; listeningSite.SetParent(root, false);
            Move(fifth.transform, listeningSite, new Vector3(12, 1.2f, -12), Quaternion.Euler(0, 90, 0));
            // Original board, slots, tray and close-view follow their original parent; no rebinding or duplicates.
            if (!world.Listening.Tablet.transform.IsChildOf(fifth.transform)) throw new InvalidOperationException("Original T05 board is not owned by original T05 identity.");
            var chimes = world.Listening.Chimes;
            if (chimes.Length != 2 || world.Chase.Agent.transform != chimes[0].MonitoredTarget || chimes[0].MonitoredTarget != chimes[1].MonitoredTarget)
                throw new InvalidOperationException("Preserve the original two chimes and sole chase target.");
            Move(chimes[0].transform, listeningSite, new Vector3(3.4f, .15f, -14.3f), Quaternion.identity);
            Move(chimes[1].transform, listeningSite, new Vector3(9.5f, .15f, -14), Quaternion.identity);
            var wall = GameObject.Find("Listening_Authored/ListeningOccluder");
            var instruction = GameObject.Find("Listening_Authored/OccluderInstruction").GetComponent<TextMesh>();
            if (!wall || !instruction) throw new InvalidOperationException("Original listening wall/instruction missing.");
            Move(wall.transform, listeningSite, new Vector3(8, 1.35f, -15.7f), Quaternion.identity);
            Move(instruction.transform, listeningSite, new Vector3(8, 1.6f, -15.81f), Quaternion.identity);
            instruction.text = "绕木墙听高低音\n沉默不代表安全"; EditorUtility.SetDirty(instruction);
            var legacyView = GameObject.Find("Listening_Authored/OccludedView").transform;
            Move(legacyView, listeningSite, new Vector3(8, 1.65f, -17), Quaternion.identity);
            var visible = Anchor("VisibleListeningPoint_Editable", root, new Vector3(9.5f, .04f, -15), new Vector3(9.5f, .15f, -14));
            var hidden = Anchor("HiddenListeningPoint_Editable", root, new Vector3(8, .04f, -17), new Vector3(5.8f, .15f, -14));
            var lakeRoute = Route("LakeRoute_Editable", root, new[] { new Vector3(0,.04f,-4), new Vector3(3,.04f,0), new Vector3(4.5f,.04f,2.5f) });
            var toolRoute = Route("ListeningRoute_Editable", root, new[] { new Vector3(2.8f,.04f,-7), new Vector3(3,.04f,-9.5f), new Vector3(3,.04f,-12), new Vector3(5,.04f,-15), new Vector3(8.5f,.04f,-12), new Vector3(9,.04f,-12), new Vector3(9.8f,.04f,-12) });
            world.Exploration.ConfigureSignalSearch(fourth, fifth, lakeRoute, toolRoute, visible, hidden); EditorUtility.SetDirty(world.Exploration);
            var font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Demon/UI/WindowsChinese.fontsettings");
            Primitive("OldReceiverPost", listeningSite, new Vector3(12.2f, .65f, -14.8f), new Vector3(.12f, 1.3f, .12f), Mat("BoxWood"));
            Label("StationName", root, new Vector3(12, 2.5f, -12), "旧监听站 · T05", font, true);
            root.Find("StationName").rotation = Quaternion.Euler(0, 90, 0);
            Label("LakesideName", lake, new Vector3(4.5f, 2.45f, 4.6f), "湖岸遗留记录", font, false);
            Primitive("EastForkPost", root, new Vector3(4.7f,.7f,-12), new Vector3(.08f,1.4f,.08f), Mat("BoxWood"));
            Primitive("EastForkFace", root, new Vector3(4.7f,1.3f,-12), new Vector3(1.7f,.45f,.1f), Mat("BoxWood"));
            Label("EastForkName", root, new Vector3(4.7f,1.3f,-11.94f), "← 旧监听站", font, true);
            var plan = world.Campaign.Definition;
            plan.Days[4].Objective = "电台只剩静电。循湖中原广播到湖岸，寻找T04。可自由返营或睡觉。";
            plan.Days[5].Objective = "沿南侧林径向东找T05；补全后看雪痕经过风铃，再绕木墙听高低音。沉默不代表安全。";
            // Retain the unresolved T04 signature in design records, not in player-facing implementation notes.
            plan.Tablets[3].Record = "【T04 · 封印四 · 已固化】\nIF DAY == 9\n\n第九天零点，湖中的投影将获得实体。\n\n边缘残缺，实体来自何处已无法辨认。日期刻痕仍然清晰。\n发现或放下这块石板，不会改变它预告的时刻。";
            EditorUtility.SetDirty(plan); EditorUtility.SetDirty(world.Exploration.Definition);
            var hud = UnityEngine.Object.FindAnyObjectByType<SliceHud>(); var binding = hud.GetComponent<SliceFontBinding>();
            binding.Configure(font, hud.GetComponentsInChildren<Text>(true), UnityEngine.Object.FindObjectsByType<TextMesh>(FindObjectsInactive.Include)); EditorUtility.SetDirty(binding);
            var surface = UnityEngine.Object.FindAnyObjectByType<NavMeshSurface>();
            var late = tablets.Select(t => t.gameObject).Concat(new[] { world.Tablet.gameObject, world.Retrieval.Pickup.gameObject }).Distinct().ToArray();
            bool[] active = late.Select(o => o.activeSelf).ToArray(); foreach (var go in late) go.SetActive(true);
            surface.RemoveData(); surface.navMeshData = null; surface.BuildNavMesh();
            if (!surface.navMeshData) throw new InvalidOperationException("Signal-search native bake failed.");
            AssetDatabase.CreateAsset(surface.navMeshData, NavigationPath); EditorUtility.SetDirty(surface);
            for (int i = 0; i < late.Length; i++) late[i].SetActive(active[i]);
            Save(); Validate();
            return CreateQuick(world);
        }
        static string CreateQuick(SliceSceneController world)
        {
            var quick = UnityEngine.Object.Instantiate(world.Settings); quick.name = "SignalSearchSettings"; quick.StartDay = 5; quick.StartHour = 8;
            AssetDatabase.CreateAsset(quick, SettingsPath); AssetDatabase.SaveAssets();
            if (!AssetDatabase.CopyAsset(NineDayAuthoring.ScenePath, ScenePath)) throw new InvalidOperationException("Could not save signal-search quick scene.");
            EditorSceneManager.OpenScene(ScenePath); world = UnityEngine.Object.FindAnyObjectByType<SliceSceneController>();
            var so = new SerializedObject(world); so.FindProperty("settings").objectReferenceValue = AssetDatabase.LoadAssetAtPath<SliceSettings>(SettingsPath); so.ApplyModifiedProperties();
            Save(); EditorSceneManager.OpenScene(ScenePath); return Validate();
        }
        public static string FinishLayout()
        {
            Editable();
            if (File.Exists(ScenePath) || File.Exists(SettingsPath)) throw new InvalidOperationException("FinishLayout only recovers initial failed creation; edit existing saved scenes normally.");
            EditorSceneManager.OpenScene(NineDayAuthoring.ScenePath);
            var world = UnityEngine.Object.FindAnyObjectByType<SliceSceneController>(); var flow = world.Exploration;
            if (!flow.HasSignalSearch || Vector3.Distance(flow.FifthTablet.transform.position,new Vector3(14,1.2f,-14.5f))>.01f) throw new InvalidOperationException("Expected initial candidate T05; preserving other manual layout.");
            flow.FifthTablet.transform.position = new Vector3(12,1.2f,-12); flow.FifthTablet.transform.rotation = Quaternion.Euler(0,90,0); EditorUtility.SetDirty(flow.FifthTablet.transform);
            flow.ListeningRoute[5].position = new Vector3(9,.04f,-12); flow.ListeningRoute[6].position = new Vector3(9.8f,.04f,-12);
            var root = GameObject.Find("SignalSearch_Authored").transform;
            root.Find("StationName").position = new Vector3(12,2.5f,-12); root.Find("StationName").rotation = Quaternion.Euler(0,90,0);
            root.Find("ForestListeningSite_Editable/OldReceiverPost").position = new Vector3(12.2f,.65f,-14.8f);
            RebakeOwnedNavigation(world);
            Save(); EditorSceneManager.OpenScene(NineDayAuthoring.ScenePath); Validate();
            return CreateQuick(UnityEngine.Object.FindAnyObjectByType<SliceSceneController>());
        }
        static void RebakeOwnedNavigation(SliceSceneController world)
        {
            var surface = UnityEngine.Object.FindAnyObjectByType<NavMeshSurface>();
            var original = surface.navMeshData;
            if (AssetDatabase.GetAssetPath(original)!=NavigationPath) throw new InvalidOperationException("Refusing to overwrite another navigation asset.");
            var late = UnityEngine.Object.FindObjectsByType<NineDayTablet>(FindObjectsInactive.Include).Select(t=>t.gameObject).Concat(new[]{world.Tablet.gameObject,world.Retrieval.Pickup.gameObject}).Distinct().ToArray();
            var active=late.Select(o=>o.activeSelf).ToArray(); foreach(var go in late)go.SetActive(true);
            surface.BuildNavMesh(); var built=surface.navMeshData; surface.RemoveData(); EditorUtility.CopySerialized(built,original); surface.navMeshData=original; surface.AddData(); UnityEngine.Object.DestroyImmediate(built); EditorUtility.SetDirty(original); EditorUtility.SetDirty(surface);
            for(int i=0;i<late.Length;i++)late[i].SetActive(active[i]);
        }
        public static string AlignLowChime()
        {
            Editable();
            foreach(string path in new[]{NineDayAuthoring.ScenePath,ScenePath})
            {
                EditorSceneManager.OpenScene(path); var world=UnityEngine.Object.FindAnyObjectByType<SliceSceneController>(); var low=world.Listening.Chimes[0];
                if(Vector3.Distance(low.transform.position,new Vector3(5.4f,.15f,-14.6f))>.01f && Vector3.Distance(low.transform.position,new Vector3(4.4f,.15f,-15))>.01f) throw new InvalidOperationException("Preserving manually moved low chime.");
                low.transform.position=new Vector3(3.4f,.15f,-14.3f); EditorUtility.SetDirty(low.transform);
                if(path==NineDayAuthoring.ScenePath) RebakeOwnedNavigation(world);
                Save(); EditorSceneManager.OpenScene(path); Validate();
            }
            return "Original low chime aligned to actual west-of-wall chase path, original radius/speed retained, saved navigation GUID preserved.";
        }
        static void Move(Transform item, Transform parent, Vector3 position, Quaternion rotation)
        { item.SetParent(parent, true); item.position = position; item.rotation = rotation; EditorUtility.SetDirty(item); }
        static Transform Anchor(string name, Transform parent, Vector3 position, Vector3 lookAt)
        { var t = new GameObject(name).transform; t.SetParent(parent, false); t.position = position; t.LookAt(lookAt); return t; }
        static Transform[] Route(string name, Transform parent, Vector3[] points)
        { var root = new GameObject(name).transform; root.SetParent(parent, false); return points.Select((p,i) => Anchor("Node_" + i, root, p, p + Vector3.forward)).ToArray(); }
        static Material Mat(string name) => AssetDatabase.LoadAssetAtPath<Material>("Assets/Demon/Art/Materials/" + name + ".mat");
        static void Primitive(string name, Transform parent, Vector3 position, Vector3 scale, Material material)
        { var go = GameObject.CreatePrimitive(PrimitiveType.Cube); go.name = name; go.transform.SetParent(parent, false); go.transform.position = position; go.transform.localScale = scale; go.GetComponent<Renderer>().sharedMaterial = material; }
        static void Label(string name, Transform parent, Vector3 position, string text, Font font, bool north)
        { var label = new GameObject(name).AddComponent<TextMesh>(); label.transform.SetParent(parent, false); label.transform.position = position; label.transform.rotation = north ? Quaternion.Euler(0,180,0) : Quaternion.identity; label.font = font; label.fontSize = 64; label.characterSize = .018f; label.anchor = TextAnchor.MiddleCenter; label.alignment = TextAlignment.Center; label.text = text; }
        static void Save() { EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); EditorSceneManager.SaveScene(SceneManager.GetActiveScene()); AssetDatabase.SaveAssets(); }
        public static void OpenQuick() { Editable(); EditorSceneManager.OpenScene(ScenePath); }
        public static string RepairSettings()
        {
            OpenQuick(); var world=UnityEngine.Object.FindAnyObjectByType<SliceSceneController>();
            var settings=AssetDatabase.LoadAssetAtPath<SliceSettings>(SettingsPath);
            if(world.Settings || !settings || settings.StartDay!=5 || Math.Abs(settings.StartHour-8)>.001f) throw new InvalidOperationException("Repair only fills the missing initial quick-settings reference.");
            var so=new SerializedObject(world); so.FindProperty("settings").objectReferenceValue=settings; so.ApplyModifiedProperties(); Save();
            // Initial boundary attempt failed before reading/backing up or changing any setting.
            SessionState.SetBool(Probe,false); EditorSceneManager.OpenScene(ScenePath); return Validate();
        }
        public static string Validate()
        {
            var world = UnityEngine.Object.FindAnyObjectByType<SliceSceneController>(); var surface = UnityEngine.Object.FindAnyObjectByType<NavMeshSurface>();
            if (!world || !world.Settings || !world.Exploration.HasSignalSearch || !surface || AssetDatabase.GetAssetPath(surface.navMeshData) != NavigationPath || !world.Cipher.Wheel.ReadCode().SequenceEqual(new[] {3,4,2})) throw new InvalidOperationException("Invalid saved signal-search settings/composition/navigation/cipher.");
            var flow = world.Exploration;
            if (flow.FourthTablet.Index != 3 || flow.FifthTablet.Index != 4 || !world.Listening.Tablet.transform.IsChildOf(flow.FifthTablet.transform) || world.Listening.Chimes.Any(c => c.MonitoredTarget != world.Chase.Agent.transform)) throw new InvalidOperationException("Original tablet/chime references changed.");
            Path(flow.ReturnPoint.position, flow.LakeRoute.Last().position); Path(flow.ReturnPoint.position, flow.ListeningRoute.Last().position);
            Path(flow.ListeningRoute.Last().position, flow.VisibleListeningPoint.position); Path(flow.VisibleListeningPoint.position, flow.HiddenListeningPoint.position); Path(flow.HiddenListeningPoint.position, flow.ReturnPoint.position);
            Path(flow.ReturnPoint.position,flow.TrailWaypoints.Last().position); Path(flow.ReturnPoint.position,flow.ForestRoute.Last().position); Path(flow.ReturnPoint.position,flow.RitualRoute.Last().position); Path(flow.ReturnPoint.position,new Vector3(8.4f,.04f,2.5f));
            return "Saved original lakeside T04 / forest T05, original two spatial chimes and chase target, cipher342 and full native roundtrip paths reloaded.";
        }
        static void Path(Vector3 from, Vector3 to)
        { var p = new NavMeshPath(); if (!NavMesh.SamplePosition(from, out var a, 1, NavMesh.AllAreas) || !NavMesh.SamplePosition(to, out var b, 1, NavMesh.AllAreas) || !NavMesh.CalculatePath(a.position,b.position,NavMesh.AllAreas,p) || p.status != NavMeshPathStatus.PathComplete) throw new InvalidOperationException("Signal-search route incomplete: " + from + " -> " + to); }
        public static void BeginProbe(string op)
        {
            if (SessionState.GetBool(Probe, false)) throw new InvalidOperationException("Restore previous signal-search probe first.");
            var world = UnityEngine.Object.FindAnyObjectByType<SliceSceneController>();
            if (!world || !world.Settings) throw new InvalidOperationException("Saved signal-search settings reference missing.");
            SessionState.SetBool(Probe,true); SessionState.SetInt(Probe+"day",world.Settings.StartDay); SessionState.SetFloat(Probe+"hour",world.Settings.StartHour); SessionState.SetFloat(Probe+"speed",world.Settings.WalkMetresPerSecond); SessionState.SetString(Probe+"guide",world.Exploration.Definition.Day5Guide);
            if (op == "signal-late") world.Settings.StartDay = 6;
            if (op == "signal-old-search") world.Settings.StartDay = 3;
            if (op == "signal-first-search") world.Settings.StartDay = 1;
            if (op == "signal-boundary") { world.Settings.StartDay = 4; world.Settings.StartHour = 23.99f; }
            if (op == "signal-tuning") { world.Settings.WalkMetresPerSecond = 2.8f; world.Exploration.Definition.Day5Guide = "【保存参数检查】" + world.Exploration.Definition.Day5Guide; EditorUtility.SetDirty(world.Exploration.Definition); }
            EditorUtility.SetDirty(world.Settings); Save();
        }
        public static void RestoreProbe()
        {
            if (!SessionState.GetBool(Probe,false)) return;
            var world = UnityEngine.Object.FindAnyObjectByType<SliceSceneController>();
            world.Settings.StartDay = SessionState.GetInt(Probe+"day",5); world.Settings.StartHour = SessionState.GetFloat(Probe+"hour",8); world.Settings.WalkMetresPerSecond = SessionState.GetFloat(Probe+"speed",3.3f); world.Exploration.Definition.Day5Guide = SessionState.GetString(Probe+"guide",world.Exploration.Definition.Day5Guide);
            EditorUtility.SetDirty(world.Settings); EditorUtility.SetDirty(world.Exploration.Definition); Save(); SessionState.SetBool(Probe,false); Debug.Log("DEMON_SIGNAL_PROBE_RESTORED");
        }
        public static string Build()
        { Editable(); Directory.CreateDirectory("Builds/WindowsSignalSearch"); var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {scenes=new[]{ScenePath},locationPathName="Builds/WindowsSignalSearch/DemonSignalSearch.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development}); if(report.summary.result!=BuildResult.Succeeded) throw new InvalidOperationException("Signal-search Windows build failed."); return "Signal-search Windows64 succeeded, " + report.summary.totalSize + " bytes."; }
    }
}
