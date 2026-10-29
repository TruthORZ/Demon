using System;
using System.IO;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace Demon.Slice.Editor
{
    public static class FinalTabletAuthoring
    {
        public const string ScenePath = "Assets/Demon/Scenes/DemonRetrieval.unity";
        const string SettingsPath = "Assets/Demon/Data/RetrievalSettings.asset";
        const string NavPath = "Assets/Demon/Data/RetrievalNavigation.asset";
        static void Editable()
        { if (EditorApplication.isPlayingOrWillChangePlaymode || SceneManager.GetActiveScene().isDirty) throw new InvalidOperationException("Leave Play and save current human edits before retrieval authoring."); }
        public static string Create()
        {
            Editable();
            if (File.Exists(ScenePath) || File.Exists(SettingsPath) || File.Exists(NavPath)) throw new InvalidOperationException("T07 outputs already exist; preserve saved authoring.");
            EditorSceneManager.OpenScene(NineDayAuthoring.ScenePath);
            var world = UnityEngine.Object.FindAnyObjectByType<SliceSceneController>();
            if (!world.Chase || !world.Finale || world.Retrieval || GameObject.Find("T07Retrieval_Authored")) throw new InvalidOperationException("Expected accepted chase source without T07 retrieval.");
            var root = new GameObject("T07Retrieval_Authored").transform;
            var stone = AssetDatabase.LoadAssetAtPath<Material>("Assets/Demon/Art/Materials/Stone.mat");
            var paper = AssetDatabase.LoadAssetAtPath<Material>("Assets/Demon/Art/Materials/Paper.mat");
            var font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Demon/UI/WindowsChinese.fontsettings");
            var owner = world.gameObject.AddComponent<FinalTabletRetrieval>(); world.ConfigureRetrieval(owner);
            var pickupRoot = new GameObject("LakeT07_HoldE").transform; pickupRoot.SetParent(root, false); pickupRoot.position = new Vector3(8.4f, 0, 4.2f);
            Primitive("PermanentLakeStand", root, new Vector3(8.4f, .35f, 4.2f), new Vector3(.8f, .7f, .8f), stone);
            var visual = new GameObject("ItemVisual").transform; visual.SetParent(pickupRoot, false);
            Primitive("T07Stone", visual, new Vector3(0, .82f, 0), new Vector3(.65f, .16f, .45f), paper);
            Primitive("InteractionFace", visual, new Vector3(0, 1.35f, -.3f), new Vector3(1.2f, .5f, .12f), stone);
            Label("T07Label", visual, new Vector3(0, 1.35f, -.37f), "T07 最后的石板\n按住 E 拾取", font, .014f);
            var view = new GameObject("PickupView_Editable").transform; view.SetParent(pickupRoot, false); view.localPosition = new Vector3(0, 1.12f, -1.6f); view.LookAt(pickupRoot.position + Vector3.up * .82f);
            var pickup = pickupRoot.gameObject.AddComponent<ThreatPickup>(); pickup.Configure(world, view, visual.gameObject);
            var ps = new SerializedObject(pickup); ps.FindProperty("itemName").stringValue = "T07"; ps.FindProperty("holdSeconds").floatValue = 1.2f; ps.ApplyModifiedProperties();
            var dock = new GameObject("ShedPlacementDock").transform; dock.SetParent(root, false); dock.position = new Vector3(-5.3f, 0, 1); dock.rotation = Quaternion.Euler(0, 90, 0);
            var empty = new GameObject("EmptyDockVisual").transform; empty.SetParent(dock, false);
            Primitive("EmptyStand", dock, new Vector3(0, .35f, .1f), new Vector3(1.5f, .7f, .7f), stone);
            Primitive("InteractionFace", empty, new Vector3(0, 1.35f, -.45f), new Vector3(1.4f, .55f, .12f), stone);
            Label("DockLabel", empty, new Vector3(0, 1.35f, -.52f), "T07 放置台\n带回石板后按 E", font, .014f);
            var dockUse = dock.gameObject.AddComponent<FinalTabletDock>(); dockUse.Configure(owner); owner.Configure(pickup, empty.gameObject);
            // Keep the original serialized board, slot IDs, line graph and prefab connection.
            var tablet = world.Tablet.transform; tablet.SetPositionAndRotation(new Vector3(-5.3f, 1.5f, 1), Quaternion.Euler(0, 90, 0)); tablet.localScale = Vector3.one * .6f; PrefabUtility.RecordPrefabInstancePropertyModifications(tablet); AlignLightInOpenScene(world); FrameBoard(world);
            var hud = UnityEngine.Object.FindAnyObjectByType<SliceHud>();
            var status = new GameObject("T07Status", typeof(RectTransform), typeof(Text)).GetComponent<Text>(); status.transform.SetParent(hud.transform, false);
            var rect = status.rectTransform; rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.one; rect.anchoredPosition = new Vector2(-30, -26); rect.sizeDelta = new Vector2(600, 150);
            status.font = font; status.fontSize = 24; status.alignment = TextAnchor.UpperRight; status.color = Color.white; status.raycastTarget = false;
            var outline = status.gameObject.AddComponent<Outline>(); outline.effectColor = new Color(0, 0, 0, .8f); outline.effectDistance = new Vector2(1, -1); hud.ConfigureFinalTabletStatus(status);
            var binding = hud.GetComponent<SliceFontBinding>(); binding.Configure(font, hud.GetComponentsInChildren<Text>(true), UnityEngine.Object.FindObjectsByType<TextMesh>(FindObjectsInactive.Include)); EditorUtility.SetDirty(binding);
            var definition = world.Campaign.Definition; definition.Days[7].Objective = "T07在指定夜间出现在湖岸。按住E拾取，带回工具棚西侧放置台补全；也可直接睡到第九天。"; EditorUtility.SetDirty(definition);
            world.Settings.Instructions += "\nD8夜间：湖岸按住E取T07，回棚西侧按E放置。"; EditorUtility.SetDirty(world.Settings);
            // Include late-visible geometry conservatively, without changing any old navigation asset.
            var surface = UnityEngine.Object.FindAnyObjectByType<NavMeshSurface>(); world.Tablet.gameObject.SetActive(true); pickupRoot.gameObject.SetActive(true);
            surface.BuildNavMesh(); if (!surface.navMeshData) throw new InvalidOperationException("T07 navigation bake failed."); AssetDatabase.CreateAsset(surface.navMeshData, NavPath); EditorUtility.SetDirty(surface);
            world.Tablet.gameObject.SetActive(false); pickupRoot.gameObject.SetActive(false);
            EditorUtility.SetDirty(world); EditorUtility.SetDirty(owner); EditorUtility.SetDirty(pickup); EditorUtility.SetDirty(dockUse); EditorUtility.SetDirty(hud); Save();
            var quick = UnityEngine.Object.Instantiate(world.Settings); quick.name = "RetrievalSettings"; quick.StartDay = 8; quick.StartHour = 21.9f; quick.InitialLakeSealed = false; AssetDatabase.CreateAsset(quick, SettingsPath); AssetDatabase.SaveAssets();
            if (!AssetDatabase.CopyAsset(NineDayAuthoring.ScenePath, ScenePath)) throw new InvalidOperationException("Could not save retrieval quick scene.");
            EditorSceneManager.OpenScene(ScenePath); world = UnityEngine.Object.FindAnyObjectByType<SliceSceneController>();
            var ws = new SerializedObject(world); ws.FindProperty("settings").objectReferenceValue = AssetDatabase.LoadAssetAtPath<SliceSettings>(SettingsPath); ws.ApplyModifiedProperties();
            var body = world.Player.GetComponent<CharacterController>(); body.enabled = false; world.Player.transform.SetPositionAndRotation(new Vector3(-8, .04f, -2.4f), Quaternion.identity); body.enabled = true; PrefabUtility.RecordPrefabInstancePropertyModifications(world.Player.transform);
            Save(); EditorSceneManager.OpenScene(ScenePath); return Validate();
        }
        static GameObject Primitive(string name, Transform parent, Vector3 pos, Vector3 scale, Material material)
        { var go = GameObject.CreatePrimitive(PrimitiveType.Cube); go.name = name; go.transform.SetParent(parent, false); go.transform.localPosition = pos; go.transform.localScale = scale; go.GetComponent<Renderer>().sharedMaterial = material; return go; }
        static void Label(string name, Transform parent, Vector3 pos, string text, Font font, float size)
        { var go = new GameObject(name).AddComponent<TextMesh>(); go.transform.SetParent(parent, false); go.transform.localPosition = pos; go.font = font; go.fontSize = 64; go.characterSize = size; go.anchor = TextAnchor.MiddleCenter; go.alignment = TextAlignment.Center; go.text = text; }
        static void AlignLightInOpenScene(SliceSceneController world)
        {
            var sequence = new SerializedObject(world.Finale); var pulse = sequence.FindProperty("tabletPulse").objectReferenceValue as Light;
            if (!pulse) throw new InvalidOperationException("Missing authored midnight pulse.");
            pulse.transform.SetParent(world.Tablet.transform, false); pulse.transform.localPosition = Vector3.back * .5f; EditorUtility.SetDirty(pulse.transform);
        }
        static void FrameBoard(SliceSceneController world)
        {
            var view = world.Tablet.CloseView; var local = view.localPosition; local.x = -.45f; local.z = -2.4f; view.localPosition = local;
            view.LookAt(world.Tablet.transform.TransformPoint(new Vector3(.45f, -.08f, 0))); PrefabUtility.RecordPrefabInstancePropertyModifications(view);
            // Labels remain within their original collider panels, clear of the authored shed post.
            foreach (var item in world.Tablet.GetComponentsInChildren<SliceRuneTrayItem>(true))
            { var label = item.Label.transform; var position = label.localPosition; position.x = 1.9f; label.localPosition = position; PrefabUtility.RecordPrefabInstancePropertyModifications(label); }
            var title = world.Tablet.transform.Find("TrayTitle"); var titlePosition = title.localPosition; titlePosition.x = 1.9f; title.localPosition = titlePosition; PrefabUtility.RecordPrefabInstancePropertyModifications(title);
        }
        public static string RepairView()
        {
            Editable(); foreach (var path in new[] { NineDayAuthoring.ScenePath, ScenePath })
            { EditorSceneManager.OpenScene(path); var world = UnityEngine.Object.FindAnyObjectByType<SliceSceneController>(); AlignLightInOpenScene(world); FrameBoard(world); Save(); }
            return Validate();
        }
        public static string AlignLighting()
        {
            Editable(); foreach (var path in new[] { NineDayAuthoring.ScenePath, ScenePath })
            { EditorSceneManager.OpenScene(path); AlignLightInOpenScene(UnityEngine.Object.FindAnyObjectByType<SliceSceneController>()); Save(); }
            return Validate();
        }
        static void Save() { EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); EditorSceneManager.SaveScene(SceneManager.GetActiveScene()); AssetDatabase.SaveAssets(); }
        public static void OpenQuick() { Editable(); EditorSceneManager.OpenScene(ScenePath); }
        public static string RepairQuickSettings()
        {
            OpenQuick(); var world = UnityEngine.Object.FindAnyObjectByType<SliceSceneController>();
            if (world.Settings) return Validate();
            var saved = AssetDatabase.LoadAssetAtPath<SliceSettings>(SettingsPath); if (!saved) throw new InvalidOperationException("Saved quick settings missing.");
            var so = new SerializedObject(world); so.FindProperty("settings").objectReferenceValue = saved; so.ApplyModifiedProperties(); EditorUtility.SetDirty(world); Save();
            EditorSceneManager.OpenScene(ScenePath); return Validate();
        }
        public static string Validate()
        {
            var world = UnityEngine.Object.FindAnyObjectByType<SliceSceneController>(); var surface = UnityEngine.Object.FindAnyObjectByType<NavMeshSurface>();
            if (!world || !world.Settings || !world.Player || !world.Tablet || !world.Campaign || !world.Finale || !world.Retrieval || !world.Retrieval.Pickup || !world.Chase || !surface || !surface.navMeshData || AssetDatabase.GetAssetPath(surface.navMeshData) != NavPath) throw new InvalidOperationException("Missing saved T07 pickup/dock/navigation composition.");
            var dock = UnityEngine.Object.FindAnyObjectByType<FinalTabletDock>(); if (!dock || dock.Retrieval != world.Retrieval || world.Tablet.gameObject.activeSelf || world.Retrieval.Pickup.gameObject.activeSelf) throw new InvalidOperationException("Unexpected initial T07 visibility or dock reference.");
            if (Vector3.Distance(world.Tablet.transform.position, new Vector3(-5.3f, 1.5f, 1)) > .01f) throw new InvalidOperationException("Original T07 board not saved in shed.");
            var pulse = new SerializedObject(world.Finale).FindProperty("tabletPulse").objectReferenceValue as Light;
            if (!pulse || !pulse.transform.IsChildOf(world.Tablet.transform)) throw new InvalidOperationException("Midnight pulse must follow the saved T07 board.");
            return "Saved lake pickup, shelf placement gate, original board IDs/prefab, shared native navigation, HUD and D8 quick settings reloaded and verified.";
        }
        const string Tune = "Demon.Retrieval.Restore";
        public static void BeginTuning()
        {
            var pickup = UnityEngine.Object.FindAnyObjectByType<SliceSceneController>().Retrieval.Pickup;
            SessionState.SetFloat(Tune + "hold", pickup.HoldSeconds); SessionState.SetBool(Tune, true); SetHold(pickup, 1.8f); Save();
        }
        public static void RestoreTuning()
        {
            if (!SessionState.GetBool(Tune, false)) return;
            SetHold(UnityEngine.Object.FindAnyObjectByType<SliceSceneController>().Retrieval.Pickup, SessionState.GetFloat(Tune + "hold", 1.2f)); Save(); SessionState.SetBool(Tune, false); Debug.Log("DEMON_RETRIEVAL_TUNING_RESTORED");
        }
        static void SetHold(ThreatPickup pickup, float hold) { var so = new SerializedObject(pickup); so.FindProperty("holdSeconds").floatValue = hold; so.ApplyModifiedProperties(); EditorUtility.SetDirty(pickup); }
        public static string UpdateOperandWording()
        {
            Editable(); int changed = 0;
            foreach (var guid in AssetDatabase.FindAssets("t:SliceSettings", new[] { "Assets/Demon/Data" }))
            {
                var data = AssetDatabase.LoadAssetAtPath<SliceSettings>(AssetDatabase.GUIDToAssetPath(guid));
                if (!data || string.IsNullOrEmpty(data.BoxText)) continue;
                string text = data.BoxText.Replace("从湖的槽位回到DAY", "从末端参数槽回到DAY");
                if (text == data.BoxText) continue;
                data.BoxText = text; EditorUtility.SetDirty(data); changed++;
            }
            AssetDatabase.SaveAssets(); return "Updated terminal-operand wording in " + changed + " saved settings assets; no layout/GUID changes.";
        }
        public static string Build()
        {
            Editable(); Directory.CreateDirectory("Builds/WindowsRetrieval");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = new[] { ScenePath }, locationPathName = "Builds/WindowsRetrieval/DemonRetrieval.exe", target = BuildTarget.StandaloneWindows64, options = BuildOptions.Development });
            if (report.summary.result != BuildResult.Succeeded) throw new InvalidOperationException("Retrieval Windows build failed."); return "T07 Windows64 build succeeded, " + report.summary.totalSize + " bytes.";
        }
    }
}
