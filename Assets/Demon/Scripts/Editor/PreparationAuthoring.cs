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
    public static class PreparationAuthoring
    {
        public const string ScenePath = "Assets/Demon/Scenes/DemonPreparation.unity";
        const string SettingsPath = "Assets/Demon/Data/PreparationSettings.asset";
        const string NavigationPath = "Assets/Demon/Data/PreparationNavigation.asset";
        const string Probe = "Demon.Preparation.Restore";
        static void Editable()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || SceneManager.GetActiveScene().isDirty)
                throw new InvalidOperationException("Leave Play and save human scene edits before D7–D8 authoring.");
        }
        public static string Create()
        {
            Editable();
            foreach (string path in new[] { ScenePath, SettingsPath, NavigationPath })
                if (File.Exists(path)) throw new InvalidOperationException("Preserving existing D7–D8 output: " + path);
            EditorSceneManager.OpenScene(NineDayAuthoring.ScenePath);
            var world = UnityEngine.Object.FindAnyObjectByType<GameSceneController>();
            if (!world || !world.Exploration.HasSignalSearch || world.Exploration.HasFinalPreparation || GameObject.Find("Preparation_Authored"))
                throw new InvalidOperationException("Expected accepted D1–D6 scene without a preparation extension.");
            var root = new GameObject("Preparation_Authored").transform;
            var font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Demon/UI/WindowsChinese.fontsettings");
            var solar = Record(root, world.Exploration, PreparationRecordKind.Solar, "SolarObservation_Editable",
                new Vector3(-5.1f, 1.1f, -2.7f), new Vector3(-5.1f, .04f, -4.5f), "日出观察记录", font);
            var handover = Record(root, world.Exploration, PreparationRecordKind.Handover, "HandoverDesk_Editable",
                new Vector3(-4, 1.1f, -4.4f), new Vector3(-4, .04f, -6.2f), "值守交接记录", font);
            var practice = Anchor("BeamPracticePoint_Editable", root, world.Tether.ObservationPoint.position);
            practice.rotation = world.Tether.ObservationPoint.rotation;
            world.Exploration.ConfigureFinalPreparation(solar, handover, practice);
            EditorUtility.SetDirty(world.Exploration);
            SetInitialObjectives();
            // Persist newly added text defaults; the saved definition, not a generator, owns subsequent edits.
            EditorUtility.SetDirty(world.Exploration.Definition);
            var hud = UnityEngine.Object.FindAnyObjectByType<GameHud>();
            var binding = hud.GetComponent<SceneFontBinding>();
            binding.Configure(font, hud.GetComponentsInChildren<Text>(true), UnityEngine.Object.FindObjectsByType<TextMesh>(FindObjectsInactive.Include));
            EditorUtility.SetDirty(binding);
            var surface = UnityEngine.Object.FindAnyObjectByType<NavMeshSurface>();
            var late = UnityEngine.Object.FindObjectsByType<NineDayTablet>(FindObjectsInactive.Include).Select(t => t.gameObject)
                .Concat(new[] { world.Tablet.gameObject, world.Retrieval.Pickup.gameObject, solar.gameObject, handover.gameObject }).Distinct().ToArray();
            var active = late.Select(o => o.activeSelf).ToArray();
            foreach (var go in late) go.SetActive(true);
            surface.RemoveData(); surface.navMeshData = null; surface.BuildNavMesh();
            if (!surface.navMeshData) throw new InvalidOperationException("Preparation native navigation bake failed.");
            AssetDatabase.CreateAsset(surface.navMeshData, NavigationPath); EditorUtility.SetDirty(surface);
            for (int i = 0; i < late.Length; i++) late[i].SetActive(active[i]);
            Save(); Validate();
            var quick = UnityEngine.Object.Instantiate(world.Settings); quick.name = "PreparationSettings"; quick.StartDay = 7; quick.StartHour = 8;
            AssetDatabase.CreateAsset(quick, SettingsPath); AssetDatabase.SaveAssets();
            if (!AssetDatabase.CopyAsset(NineDayAuthoring.ScenePath, ScenePath)) throw new InvalidOperationException("Could not copy D7–D8 quick scene.");
            EditorSceneManager.OpenScene(ScenePath); world = UnityEngine.Object.FindAnyObjectByType<GameSceneController>();
            var serialized = new SerializedObject(world); serialized.FindProperty("settings").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameSettings>(SettingsPath); serialized.ApplyModifiedProperties();
            Save(); EditorSceneManager.OpenScene(ScenePath); return Validate();
        }
        static PreparationRecord Record(Transform parent, ExplorationFlow owner, PreparationRecordKind kind, string name, Vector3 position, Vector3 stand, string text, Font font)
        {
            var root = new GameObject(name).transform; root.SetParent(parent, false); root.position = position;
            var board = GameObject.CreatePrimitive(PrimitiveType.Cube); board.name = "RecordFace"; board.transform.SetParent(root, false);
            board.transform.localScale = new Vector3(1.25f, .8f, .08f);
            board.GetComponent<Renderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Demon/Art/Materials/Paper.mat");
            var post = GameObject.CreatePrimitive(PrimitiveType.Cube); post.name = "WoodStand"; post.transform.SetParent(root, false);
            post.transform.localPosition = new Vector3(0, -.55f, .03f); post.transform.localScale = new Vector3(.1f, 1.1f, .1f);
            post.GetComponent<Renderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Demon/Art/Materials/BoxWood.mat");
            var label = new GameObject("RecordTitle").AddComponent<TextMesh>(); label.transform.SetParent(root, false); label.transform.localPosition = new Vector3(0, 0, -.047f);
            label.font = font; label.fontSize = 64; label.characterSize = .018f; label.anchor = TextAnchor.MiddleCenter; label.alignment = TextAlignment.Center;
            label.color = new Color(.12f, .14f, .16f); label.text = text + "\nE 查看"; label.GetComponent<MeshRenderer>().sharedMaterial = font.material;
            var approach = Anchor("Approach_Editable", root, stand);
            var view = Anchor("ReadView_Editable", root, stand + Vector3.up * 1.5f); view.LookAt(position);
            var record = root.gameObject.AddComponent<PreparationRecord>(); record.Configure(owner, kind, view, board.transform, approach, "查看" + text);
            return record;
        }
        static Transform Anchor(string name, Transform parent, Vector3 position)
        { var t = new GameObject(name).transform; t.SetParent(parent, false); t.position = position; return t; }
        static void Save()
        { EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); EditorSceneManager.SaveScene(SceneManager.GetActiveScene()); AssetDatabase.SaveAssets(); }
        public static void OpenQuick() { Editable(); EditorSceneManager.OpenScene(ScenePath); }
        static void SetInitialObjectives()
        {
            // Shared daily text must also make sense in older standalone tool scenes without these records.
            var plan = AssetDatabase.LoadAssetAtPath<NineDayDefinition>("Assets/Demon/Data/NineDayDefinition.asset");
            plan.Days[6].Objective = "预期日出后仍无日光，太阳只剩白边，湖中持续发光。对照太阳符号与轮盘解 A、取 T06，在湖岸观察锁定与恢复。";
            plan.Days[7].Objective = "原定今天交班，仍未见接班人员。核对已有线索和工具，指定夜间到湖岸取回 T07、带回工具棚西侧放置补全。仍可直接睡觉。";
            EditorUtility.SetDirty(plan); AssetDatabase.SaveAssets();
        }
        public static string Validate()
        {
            var world = UnityEngine.Object.FindAnyObjectByType<GameSceneController>(); var surface = UnityEngine.Object.FindAnyObjectByType<NavMeshSurface>();
            if (!world || !world.Settings || !world.Exploration.HasFinalPreparation || !surface || AssetDatabase.GetAssetPath(surface.navMeshData) != NavigationPath || !world.Cipher.Wheel.ReadCode().SequenceEqual(new[] { 3, 4, 2 }))
                throw new InvalidOperationException("Invalid saved preparation composition/settings/navigation/cipher.");
            var flow = world.Exploration;
            foreach (var record in new[] { flow.SolarRecord, flow.HandoverRecord })
            {
                if (!record || record.Owner != flow || !record.AimTarget || !record.CloseView || !record.Approach || !record.GetComponentInChildren<Collider>())
                    throw new InvalidOperationException("Incomplete saved preparation record.");
                Path(flow.ReturnPoint.position, record.Approach.position);
            }
            Path(flow.SolarRecord.Approach.position, flow.PracticePoint.position);
            Path(flow.HandoverRecord.Approach.position, flow.ReturnPoint.position);
            foreach (var end in new[] { flow.TrailWaypoints.Last(), flow.ForestRoute.Last(), flow.RitualRoute.Last(), flow.LakeRoute.Last(), flow.ListeningRoute.Last() }) Path(flow.ReturnPoint.position, end.position);
            Path(flow.ReturnPoint.position, new Vector3(8.4f, .04f, 2.5f));
            if (world.Listening.Chimes.Any(c => c.MonitoredTarget != world.Chase.Agent.transform)) throw new InvalidOperationException("Preserve the sole production chase target.");
            return "Saved D7 solar / D8 handover records and editable views, original cipher342/tools/chase, all D1–D8 native walking paths reloaded.";
        }
        static void Path(Vector3 from, Vector3 to)
        { var path = new NavMeshPath(); if (!NavMesh.SamplePosition(from, out var a, 1, NavMesh.AllAreas) || !NavMesh.SamplePosition(to, out var b, 1, NavMesh.AllAreas) || !NavMesh.CalculatePath(a.position, b.position, NavMesh.AllAreas, path) || path.status != NavMeshPathStatus.PathComplete) throw new InvalidOperationException("Preparation route incomplete: " + from + " -> " + to); }
        public static void BeginProbe(string operation)
        {
            if (SessionState.GetBool(Probe, false)) throw new InvalidOperationException("Restore the previous preparation probe first.");
            var world = UnityEngine.Object.FindAnyObjectByType<GameSceneController>();
            SessionState.SetInt(Probe + "day", world.Settings.StartDay); SessionState.SetFloat(Probe + "hour", world.Settings.StartHour);
            SessionState.SetFloat(Probe + "speed", world.Settings.WalkMetresPerSecond); SessionState.SetString(Probe + "record", world.Exploration.Definition.HandoverRecord);
            SessionState.SetVector3(Probe + "view", world.Exploration.HandoverRecord.CloseView.position); SessionState.SetBool(Probe, true);
            if (operation == "prep-late") world.Settings.StartDay = 8;
            if (operation == "prep-boundary") { world.Settings.StartDay = 6; world.Settings.StartHour = 23.99f; }
            if (operation == "prep-first-search") world.Settings.StartDay = 1;
            if (operation == "prep-early") world.Settings.StartDay = 1;
            if (operation == "prep-old-search") world.Settings.StartDay = 3;
            if (operation == "prep-signal-search") world.Settings.StartDay = 5;
            if (operation == "prep-night") { world.Settings.StartDay = 8; world.Settings.StartHour = 21.9f; }
            if (operation == "prep-tuning")
            {
                world.Settings.WalkMetresPerSecond = 2.8f; world.Exploration.Definition.HandoverRecord = "【保存参数检查】\n" + world.Exploration.Definition.HandoverRecord;
                world.Exploration.HandoverRecord.CloseView.position += Vector3.right * .08f; EditorUtility.SetDirty(world.Exploration.Definition);
            }
            EditorUtility.SetDirty(world.Settings); Save();
        }
        public static void RestoreProbe()
        {
            if (!SessionState.GetBool(Probe, false)) return;
            var world = UnityEngine.Object.FindAnyObjectByType<GameSceneController>();
            world.Settings.StartDay = SessionState.GetInt(Probe + "day", 7); world.Settings.StartHour = SessionState.GetFloat(Probe + "hour", 8);
            world.Settings.WalkMetresPerSecond = SessionState.GetFloat(Probe + "speed", 3.3f); world.Exploration.Definition.HandoverRecord = SessionState.GetString(Probe + "record", "");
            world.Exploration.HandoverRecord.CloseView.position = SessionState.GetVector3(Probe + "view", Vector3.zero);
            EditorUtility.SetDirty(world.Settings); EditorUtility.SetDirty(world.Exploration.Definition); Save(); SessionState.SetBool(Probe, false); Debug.Log("DEMON_PREPARATION_PROBE_RESTORED");
        }
        public static string Build()
        {
            Editable(); Directory.CreateDirectory("Builds/WindowsPreparation");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = new[] { ScenePath }, locationPathName = "Builds/WindowsPreparation/DemonPreparation.exe", target = BuildTarget.StandaloneWindows64, options = BuildOptions.Development });
            if (report.summary.result != BuildResult.Succeeded) throw new InvalidOperationException("D7–D8 Windows build failed.");
            return "D7–D8 preparation Windows64 succeeded, " + report.summary.totalSize + " bytes.";
        }
    }
}
