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
    public static class ChaseAuthoring
    {
        public const string ScenePath = "Assets/Demon/Scenes/DemonChase.unity";
        const string DataPath = "Assets/Demon/Data/ChaseDefinition.asset";
        const string SettingsPath = "Assets/Demon/Data/ChaseSettings.asset";
        static void Editable()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || SceneManager.GetActiveScene().isDirty) throw new InvalidOperationException("Leave Play and save active edits before chase authoring.");
        }
        public static string Create()
        {
            Editable();
            if (File.Exists(ScenePath) || File.Exists(DataPath) || File.Exists(SettingsPath) || File.Exists("Assets/Demon/Data/ChaseNavigation.asset") || File.Exists("Assets/Demon/Data/ChasePracticeNavigation.asset")) throw new InvalidOperationException("Chase outputs already exist; preserve saved authoring.");
            EditorSceneManager.OpenScene(NineDayAuthoring.ScenePath);
            var world = UnityEngine.Object.FindAnyObjectByType<SliceSceneController>();
            if (!world.Tether || world.Chase || GameObject.Find("Chase_Authored")) throw new InvalidOperationException("Unexpected chase source composition.");
            var proxy = UnityEngine.Object.FindAnyObjectByType<ListeningTrailProxy>();
            if (!proxy) throw new InvalidOperationException("Original proxy must remain available for audit.");
            var data = ScriptableObject.CreateInstance<ChaseDefinition>(); AssetDatabase.CreateAsset(data, DataPath);
            Sound(); AssetDatabase.ImportAsset("Assets/Demon/Audio/PrototypeSnowStep.wav", ImportAssetOptions.ForceSynchronousImport);
            var root = new GameObject("Chase_Authored").transform;
            var surface = root.gameObject.AddComponent<NavMeshSurface>(); surface.collectObjects = CollectObjects.Volume; surface.center = new Vector3(0, 1, -2); surface.size = new Vector3(26, 6, 16); surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders; surface.overrideVoxelSize = true; surface.voxelSize = .08f; surface.minRegionArea = .3f;
            var playerFilter = world.Player.gameObject.AddComponent<NavMeshModifier>(); playerFilter.ignoreFromBuild = true; playerFilter.applyToChildren = true;
            var agent = proxy.Target.gameObject.AddComponent<NavMeshAgent>(); agent.enabled = false; agent.agentTypeID = 0; agent.radius = .45f; agent.height = 1.6f; agent.baseOffset = .15f; agent.stoppingDistance = .8f; agent.autoBraking = true; agent.updateRotation = true;
            proxy.enabled = false; EditorUtility.SetDirty(proxy);
            var audio = new GameObject("SpatialSnowSteps").AddComponent<AudioSource>(); audio.transform.SetParent(agent.transform, false); audio.playOnAwake = false; audio.spatialBlend = 1; audio.minDistance = 1; audio.maxDistance = 14; audio.rolloffMode = AudioRolloffMode.Linear; audio.dopplerLevel = 0; audio.volume = .6f;
            var points = new Transform[4]; Vector3[] positions = { new Vector3(8, .15f, 3), new Vector3(-8, .15f, -7), new Vector3(9, .15f, -5), new Vector3(-7, .15f, 3) };
            var anchors = new GameObject("RetreatPoints_Editable").transform; anchors.SetParent(root, false);
            for (int i = 0; i < points.Length; i++) { points[i] = new GameObject("Retreat_" + i).transform; points[i].SetParent(anchors, false); points[i].position = positions[i]; }
            var chase = world.gameObject.AddComponent<ChaseController>(); chase.Configure(AssetDatabase.LoadAssetAtPath<ChaseDefinition>(DataPath), agent, points, audio, AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Demon/Audio/PrototypeSnowStep.wav")); world.ConfigureChase(chase);
            EditorUtility.SetDirty(world); EditorUtility.SetDirty(chase); PrefabUtility.RecordPrefabInstancePropertyModifications(playerFilter);
            // Date-activated tablet colliders are conservatively included in the saved navigation footprint.
            var late = world.Tablet.gameObject; bool lateWasActive = late.activeSelf; late.SetActive(true);
            Bake(surface, "Assets/Demon/Data/ChaseNavigation.asset"); late.SetActive(lateWasActive); agent.enabled = true; Save();
            var settings = UnityEngine.Object.Instantiate(world.Settings); settings.name = "ChaseSettings"; settings.StartDay = 7; settings.StartHour = 10; settings.InitialLakeSealed = false; AssetDatabase.CreateAsset(settings, SettingsPath); AssetDatabase.SaveAssets();
            if (!AssetDatabase.CopyAsset(NineDayAuthoring.ScenePath, ScenePath)) throw new InvalidOperationException("Could not save chase practice scene.");
            EditorSceneManager.OpenScene(ScenePath); world = UnityEngine.Object.FindAnyObjectByType<SliceSceneController>();
            var serialized = new SerializedObject(world); serialized.FindProperty("settings").objectReferenceValue = AssetDatabase.LoadAssetAtPath<SliceSettings>(SettingsPath); serialized.ApplyModifiedProperties();
            var practice = new GameObject("ChasePractice_Authored").transform;
            var stone = AssetDatabase.LoadAssetAtPath<Material>("Assets/Demon/Art/Materials/Stone.mat"); var paper = AssetDatabase.LoadAssetAtPath<Material>("Assets/Demon/Art/Materials/Paper.mat"); var wood = AssetDatabase.LoadAssetAtPath<Material>("Assets/Demon/Art/Materials/BoxWood.mat"); var font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Demon/UI/WindowsChinese.fontsettings");
            Primitive("PracticeRoutingWall", practice, new Vector3(0, 1.1f, -5.5f), new Vector3(3, 2.2f, .4f), wood);
            var pickupRoot = new GameObject("PracticePickup").transform; pickupRoot.SetParent(practice, false); pickupRoot.position = new Vector3(1.8f, 0, -1.5f);
            Primitive("Stand", pickupRoot, new Vector3(0, .4f, 0), new Vector3(.8f, .8f, .8f), stone);
            var visual = new GameObject("ItemVisual").transform; visual.SetParent(pickupRoot, false);
            Primitive("Fragment", visual, new Vector3(0, .9f, 0), new Vector3(.5f, .12f, .35f), paper);
            Primitive("InteractionSign", visual, new Vector3(0, 1.5f, -.35f), new Vector3(1.2f, .5f, .12f), stone);
            Label("PickupLabel", visual, new Vector3(0, 1.5f, -.42f), "按住 E 拾取\n练习碎片", font, .014f);
            var view = new GameObject("PickupView").transform; view.SetParent(pickupRoot, false); view.localPosition = new Vector3(0, 1.2f, -1.6f); view.LookAt(pickupRoot.position + Vector3.up * .9f);
            pickupRoot.gameObject.AddComponent<ThreatPickup>().Configure(world, view, visual.gameObject);
            Label("PracticeInstructions", practice, new Vector3(4.7f, 2, -.3f), "牵制与拾取练习\n松开光束后伸手；被逼近会打断\n已完成的解谜和收集保留", font, .018f);
            var binding = UnityEngine.Object.FindAnyObjectByType<SliceHud>().GetComponent<SliceFontBinding>(); binding.Configure(font, binding.GetComponentsInChildren<Text>(true), UnityEngine.Object.FindObjectsByType<TextMesh>(FindObjectsInactive.Include)); EditorUtility.SetDirty(binding);
            var body = world.Player.GetComponent<CharacterController>(); body.enabled = false; world.Player.transform.SetPositionAndRotation(new Vector3(1.8f, .04f, -3.7f), Quaternion.identity); body.enabled = true; PrefabUtility.RecordPrefabInstancePropertyModifications(world.Player.transform);
            surface = UnityEngine.Object.FindAnyObjectByType<NavMeshSurface>(); surface.RemoveData(); surface.navMeshData = null;
            Bake(surface, "Assets/Demon/Data/ChasePracticeNavigation.asset"); Save(); EditorSceneManager.OpenScene(ScenePath); return Validate();
        }
        static void Bake(NavMeshSurface surface, string path)
        {
            surface.BuildNavMesh(); if (!surface.navMeshData) throw new InvalidOperationException("Official NavMesh build produced no data.");
            AssetDatabase.CreateAsset(surface.navMeshData, path); EditorUtility.SetDirty(surface); AssetDatabase.SaveAssets();
        }
        static GameObject Primitive(string name, Transform parent, Vector3 pos, Vector3 scale, Material material)
        { var go = GameObject.CreatePrimitive(PrimitiveType.Cube); go.name = name; go.transform.SetParent(parent, false); go.transform.localPosition = pos; go.transform.localScale = scale; go.GetComponent<Renderer>().sharedMaterial = material; return go; }
        static void Label(string name, Transform parent, Vector3 pos, string text, Font font, float size)
        { var label = new GameObject(name).AddComponent<TextMesh>(); label.transform.SetParent(parent, false); label.transform.localPosition = pos; label.font = font; label.fontSize = 64; label.characterSize = size; label.anchor = TextAnchor.MiddleCenter; label.alignment = TextAlignment.Center; label.text = text; }
        static void Sound()
        {
            const string path = "Assets/Demon/Audio/PrototypeSnowStep.wav"; if (File.Exists(path)) throw new InvalidOperationException("Preserving existing snow step clip.");
            const int rate = 22050; int count = rate / 5; var random = new System.Random(9);
            using (var file = new BinaryWriter(File.Create(path)))
            {
                file.Write(System.Text.Encoding.ASCII.GetBytes("RIFF")); file.Write(36 + count * 2); file.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt ")); file.Write(16); file.Write((short)1); file.Write((short)1); file.Write(rate); file.Write(rate * 2); file.Write((short)2); file.Write((short)16); file.Write(System.Text.Encoding.ASCII.GetBytes("data")); file.Write(count * 2);
                double smooth = 0; for (int i = 0; i < count; i++) { smooth = smooth * .7 + (random.NextDouble() * 2 - 1) * .3; double t = (double)i / rate; file.Write((short)(smooth * Math.Exp(-t * 19) * Math.Min(1, t * 120) * 24000)); }
            }
        }
        static void Save() { EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); EditorSceneManager.SaveScene(SceneManager.GetActiveScene()); AssetDatabase.SaveAssets(); }
        public static void OpenQuick() { Editable(); EditorSceneManager.OpenScene(ScenePath); }
        public static string Validate()
        {
            var world = UnityEngine.Object.FindAnyObjectByType<SliceSceneController>(); var surface = UnityEngine.Object.FindAnyObjectByType<NavMeshSurface>();
            if (!world || !world.Chase || !world.Chase.Agent || !surface || !surface.navMeshData || !EditorUtility.IsPersistent(surface.navMeshData)) throw new InvalidOperationException("Missing persisted chase navigation/composition.");
            var proxy = world.Chase.Agent.GetComponentInParent<ListeningTrailProxy>();
            if (proxy && proxy.enabled) throw new InvalidOperationException("Two movement owners are enabled.");
            if (!NavMesh.SamplePosition(world.Chase.Agent.transform.position, out var hit, 2, NavMesh.AllAreas)) throw new InvalidOperationException("Authored actor has no walkable start.");
            return "Saved official navigation data/agent, single movement owner, retreat anchors and chase composition reloaded and verified.";
        }
        const string Tune = "Demon.Chase.Restore";
        public static void BeginTuning()
        {
            var world = UnityEngine.Object.FindAnyObjectByType<SliceSceneController>(); var data = world.Chase.Definition; var pickup = UnityEngine.Object.FindAnyObjectByType<ThreatPickup>();
            SessionState.SetFloat(Tune + "speed", data.MetresPerSecond); SessionState.SetFloat(Tune + "hold", pickup.HoldSeconds); SessionState.SetBool(Tune, true);
            data.MetresPerSecond = 2.2f; EditorUtility.SetDirty(data); SetFloat(pickup, "holdSeconds", 1.8f); Save();
        }
        public static void RestoreTuning()
        {
            if (!SessionState.GetBool(Tune, false)) return;
            var data = AssetDatabase.LoadAssetAtPath<ChaseDefinition>(DataPath); data.MetresPerSecond = SessionState.GetFloat(Tune + "speed", 3.6f); EditorUtility.SetDirty(data);
            SetFloat(UnityEngine.Object.FindAnyObjectByType<ThreatPickup>(), "holdSeconds", SessionState.GetFloat(Tune + "hold", 1.2f)); Save(); SessionState.SetBool(Tune, false); Debug.Log("DEMON_CHASE_TUNING_RESTORED");
        }
        static void SetFloat(UnityEngine.Object obj, string field, float value) { var data = new SerializedObject(obj); data.FindProperty(field).floatValue = value; data.ApplyModifiedProperties(); EditorUtility.SetDirty(obj); }
        public static string Build()
        {
            Editable(); Directory.CreateDirectory("Builds/WindowsChase");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = new[] { ScenePath }, locationPathName = "Builds/WindowsChase/DemonChase.exe", target = BuildTarget.StandaloneWindows64, options = BuildOptions.Development });
            if (report.summary.result != BuildResult.Succeeded) throw new InvalidOperationException("Chase Windows build failed."); return "Chase Windows64 build succeeded, " + report.summary.totalSize + " bytes.";
        }
    }
}
