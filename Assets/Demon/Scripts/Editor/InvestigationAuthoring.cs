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
    public static class InvestigationAuthoring
    {
        public const string ScenePath = "Assets/Demon/Scenes/DemonInvestigation.unity";
        const string SettingsPath = "Assets/Demon/Data/InvestigationSettings.asset";
        const string NavigationPath = "Assets/Demon/Data/InvestigationNavigation.asset";
        const string TraceMaterialPath = "Assets/Demon/Art/Materials/OldSnowTrace.mat";
        const string Probe = "Demon.Investigation.Restore";
        static void Editable()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || SceneManager.GetActiveScene().isDirty)
                throw new InvalidOperationException("Leave Play and save existing human edits before investigation authoring.");
        }
        public static string Create()
        {
            Editable();
            foreach (string path in new[] { ScenePath, SettingsPath, NavigationPath, TraceMaterialPath })
                if (File.Exists(path)) throw new InvalidOperationException("Preserving existing investigation output: " + path);
            EditorSceneManager.OpenScene(NineDayAuthoring.ScenePath);
            var world = UnityEngine.Object.FindAnyObjectByType<SliceSceneController>();
            if (!world || !world.Exploration || world.Exploration.HasInvestigation || GameObject.Find("Investigation_Authored"))
                throw new InvalidOperationException("Expected the accepted D1–D2 main composition without D3–D4 extension.");
            AlignChaseHint(); PolishRecords();
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (!shader) throw new InvalidOperationException("Installed URP Lit shader missing.");
            var traceMaterial = new Material(shader) { name = "OldSnowTrace" };
            traceMaterial.SetColor("_BaseColor", new Color(.12f, .16f, .2f));
            AssetDatabase.CreateAsset(traceMaterial, TraceMaterialPath);
            var root = new GameObject("Investigation_Authored").transform;
            var forestSite = new GameObject("ForestTabletSite_Editable").transform;
            forestSite.SetParent(root, false);
            var ritual = new GameObject("OutsideCaveRitual_Editable").transform;
            ritual.SetParent(root, false);
            var tablets = UnityEngine.Object.FindObjectsByType<NineDayTablet>(FindObjectsInactive.Include);
            var second = tablets.Single(t => t.Index == 1);
            var third = tablets.Single(t => t.Index == 2);
            MoveTablet(second, forestSite, new Vector3(-2.5f, 1.2f, -20.5f));
            MoveTablet(third, ritual, new Vector3(3.4f, 1.2f, -24.1f));
            ConfigureRitualView(third);
            Primitive("ForestOldRock", forestSite, new Vector3(-4.2f, .4f, -20.5f), new Vector3(.8f, .8f, 1.2f), Mat("Stone"));
            Primitive("BrokenSurveyPost", forestSite, new Vector3(-.8f, .55f, -20.6f), new Vector3(.13f, 1.1f, .13f), Mat("BoxWood"));
            foreach (float x in new[] { 1.8f, 5f })
                Primitive("OldRitualPillar", ritual, new Vector3(x, 1, -24.1f), new Vector3(.5f, 2, .5f), Mat("Stone"));
            Primitive("FallenLintel", ritual, new Vector3(3.4f, .15f, -25.6f), new Vector3(3.2f, .3f, .4f), Mat("Stone"));
            for (int i = 0; i < 16; i++)
            {
                float angle = i * Mathf.PI * 2 / 16;
                var cut = Primitive("WeatheredEtching_" + i, ritual, new Vector3(3.4f + Mathf.Sin(angle) * 1.25f, .015f, -24.1f + Mathf.Cos(angle) * 1.25f), new Vector3(.25f, .025f, .045f), traceMaterial, false);
                cut.transform.localRotation = Quaternion.Euler(0, i * 22.5f, 0);
            }
            var font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Demon/UI/WindowsChinese.fontsettings");
            Label("RitualLandmark", ritual, new Vector3(3.4f, 2.3f, -24.1f), "旧仪式遗址", font, .018f);
            var trail = new GameObject("ResidualLargeTracks_D3_Editable").transform;
            trail.SetParent(root, false);
            Vector3[] forest = {
                new Vector3(2.2f,.04f,-6.2f), new Vector3(2.4f,.04f,-9.3f), new Vector3(2,.04f,-12.5f),
                new Vector3(1,.04f,-15), new Vector3(-1.8f,.04f,-17), new Vector3(-2.5f,.04f,-18.2f)
            };
            Vector3[] ritualPoints = {
                new Vector3(2.8f,.04f,-7), new Vector3(3,.04f,-9.5f), new Vector3(3,.04f,-12.5f),
                new Vector3(5,.04f,-16), new Vector3(5,.04f,-19.3f), new Vector3(4.6f,.04f,-21), new Vector3(4.8f,.04f,-21.7f)
            };
            var forestNodes = Route("ForestRoute_Editable", root, forest);
            var ritualNodes = Route("RitualRoute_Editable", root, ritualPoints);
            int footprint = 0;
            for (int segment = 0; segment < forest.Length - 1; segment++)
            {
                Vector3 delta = forest[segment + 1] - forest[segment]; float length = delta.magnitude;
                Vector3 direction = delta / length, right = Vector3.Cross(Vector3.up, direction);
                for (float distance = 0; distance < length; distance += 1.4f)
                {
                    var print = Primitive("ResidualLargeFoot_" + footprint, trail, forest[segment] + direction * distance + right * (footprint % 2 == 0 ? -.25f : .25f), new Vector3(.45f, .022f, .9f), traceMaterial, false, PrimitiveType.Sphere);
                    print.transform.localRotation = Quaternion.LookRotation(direction);
                    print.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    footprint++;
                }
            }
            var sign = new GameObject("ForestForkMarker").transform; sign.SetParent(root, false);
            Primitive("Post", sign, new Vector3(2.8f, .7f, -13.5f), new Vector3(.08f, 1.4f, .08f), Mat("BoxWood"));
            Primitive("Face", sign, new Vector3(2.8f, 1.3f, -13.5f), new Vector3(1.6f, .45f, .1f), Mat("BoxWood"));
            Label("ForkName", sign, new Vector3(2.8f, 1.3f, -13.44f), "林间旧迹 →", font, .018f);
            world.Exploration.ConfigureInvestigation(trail.gameObject, forestNodes, ritualNodes, second, third);
            EditorUtility.SetDirty(world.Exploration);
            var plan = world.Campaign.Definition;
            plan.Days[2].Objective = "风向标已经停转。沿南侧林径向西分出的大雪痕寻找T02；正在移动的雪雾仍会接近。";
            plan.Days[3].Objective = "调查山洞西侧外面的旧仪式遗址，寻找T03，核对下一条日期预告。可随时返营或睡觉。";
            EditorUtility.SetDirty(plan); EditorUtility.SetDirty(world.Exploration.Definition);
            var hud = UnityEngine.Object.FindAnyObjectByType<SliceHud>();
            var binding = hud.GetComponent<SliceFontBinding>();
            binding.Configure(font, hud.GetComponentsInChildren<Text>(true), UnityEngine.Object.FindObjectsByType<TextMesh>(FindObjectsInactive.Include));
            EditorUtility.SetDirty(binding);
            var surface = UnityEngine.Object.FindAnyObjectByType<NavMeshSurface>();
            var objects = tablets.Select(t => t.gameObject).Concat(new[] { world.Tablet.gameObject, world.Retrieval.Pickup.gameObject }).Distinct().ToArray();
            bool[] active = objects.Select(go => go.activeSelf).ToArray();
            foreach (var go in objects) go.SetActive(true);
            surface.RemoveData(); surface.navMeshData = null; surface.BuildNavMesh();
            if (!surface.navMeshData) throw new InvalidOperationException("Investigation native navigation bake produced no data.");
            AssetDatabase.CreateAsset(surface.navMeshData, NavigationPath); EditorUtility.SetDirty(surface);
            for (int i = 0; i < objects.Length; i++) objects[i].SetActive(active[i]);
            trail.gameObject.SetActive(false); Save(); Validate();
            var quick = UnityEngine.Object.Instantiate(world.Settings); quick.name = "InvestigationSettings";
            quick.StartDay = 3; quick.StartHour = 8; AssetDatabase.CreateAsset(quick, SettingsPath); AssetDatabase.SaveAssets();
            if (!AssetDatabase.CopyAsset(NineDayAuthoring.ScenePath, ScenePath)) throw new InvalidOperationException("Could not save investigation quick scene.");
            EditorSceneManager.OpenScene(ScenePath); world = UnityEngine.Object.FindAnyObjectByType<SliceSceneController>();
            var serialized = new SerializedObject(world); serialized.FindProperty("settings").objectReferenceValue = AssetDatabase.LoadAssetAtPath<SliceSettings>(SettingsPath); serialized.ApplyModifiedProperties();
            Save(); EditorSceneManager.OpenScene(ScenePath); return Validate();
        }
        static void MoveTablet(NineDayTablet tablet, Transform parent, Vector3 position)
        { tablet.transform.SetParent(parent, true); tablet.transform.position = position; tablet.transform.rotation = Quaternion.Euler(0, 180, 0); EditorUtility.SetDirty(tablet.transform); }
        static Transform[] Route(string name, Transform parent, Vector3[] points)
        {
            var root = new GameObject(name).transform; root.SetParent(parent, false);
            return points.Select((point, i) => { var node = new GameObject("Node_" + i).transform; node.SetParent(root, false); node.position = point; return node; }).ToArray();
        }
        static GameObject Primitive(string name, Transform parent, Vector3 position, Vector3 scale, Material material, bool collision = true, PrimitiveType type = PrimitiveType.Cube)
        {
            var go = GameObject.CreatePrimitive(type); go.name = name; go.transform.SetParent(parent, false); go.transform.localPosition = position; go.transform.localScale = scale; go.GetComponent<Renderer>().sharedMaterial = material;
            if (!collision) UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>()); return go;
        }
        static void Label(string name, Transform parent, Vector3 position, string text, Font font, float size)
        {
            var label = new GameObject(name).AddComponent<TextMesh>(); label.transform.SetParent(parent, false); label.transform.position = position; label.transform.rotation = Quaternion.Euler(0, 180, 0);
            label.font = font; label.fontSize = 64; label.characterSize = size; label.anchor = TextAnchor.MiddleCenter; label.alignment = TextAlignment.Center; label.text = text;
        }
        static Material Mat(string name) => AssetDatabase.LoadAssetAtPath<Material>("Assets/Demon/Art/Materials/" + name + ".mat");
        static void Save() { EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); EditorSceneManager.SaveScene(SceneManager.GetActiveScene()); AssetDatabase.SaveAssets(); }
        static void ConfigureRitualView(NineDayTablet tablet)
        {
            var view = tablet.transform.Find("ReadView");
            var serialized = new SerializedObject(tablet);
            if (!view || serialized.FindProperty("closeView").objectReferenceValue != view) throw new InvalidOperationException("Original T03 reading anchor changed; preserve manual wiring.");
            view.position = new Vector3(4.9f, 1.55f, -21.4f); view.LookAt(tablet.transform.position + Vector3.up * .15f); EditorUtility.SetDirty(view);
        }
        public static string RepairRitualView()
        {
            Editable();
            foreach (string path in new[] { NineDayAuthoring.ScenePath, ScenePath })
            {
                EditorSceneManager.OpenScene(path); var world = UnityEngine.Object.FindAnyObjectByType<SliceSceneController>();
                var flow = world.Exploration; ConfigureRitualView(flow.ThirdTablet);
                var fork = GameObject.Find("Investigation_Authored/ForestForkMarker/ForkName").GetComponent<TextMesh>();
                if (fork.text != "林间旧迹 ←" && fork.text != "林间旧迹 →") throw new InvalidOperationException("Preserving manually customized fork sign.");
                fork.text = "林间旧迹 →"; EditorUtility.SetDirty(fork);
                var stand = flow.RitualRoute.Last(); stand.position = new Vector3(4.8f, .04f, -21.7f); EditorUtility.SetDirty(stand); Save(); Validate();
            }
            return "Saved T03 reading/approach anchors clear of pine canopy; original tablet, navigation and IDs unchanged.";
        }
        public static string PolishRecords()
        {
            Editable();
            var plan = AssetDatabase.LoadAssetAtPath<NineDayDefinition>("Assets/Demon/Data/NineDayDefinition.asset");
            if (!plan || plan.Tablets.Length != 6) throw new InvalidOperationException("Missing original tablet records.");
            plan.Tablets[1].Record = plan.Tablets[1].Record.Replace("当前广播旋律与静电是可替换占位音。", "");
            plan.Tablets[2].Record = plan.Tablets[2].Record.Replace("；当前日轮使用保存的方位参照占位物。", "。");
            EditorUtility.SetDirty(plan); AssetDatabase.SaveAssets();
            return "Removed production notes from player T02/T03 records; date/program text unchanged.";
        }
        public static string AlignChaseHint()
        {
            Editable();
            var data = AssetDatabase.LoadAssetAtPath<ChaseDefinition>("Assets/Demon/Data/ChaseDefinition.asset");
            const string oldHint = "雪痕擦身而过，暂时退开了。用风铃辨认区域，用光束争取时间。";
            const string hint = "雪痕擦身而过，暂时退开了。保持移动，留意雪地动静；可以先回营地整理线索。";
            if (!data || (data.NearPassMessage != oldHint && data.NearPassMessage != hint))
                throw new InvalidOperationException("Preserving a manually customized chase hint; align its early-day wording explicitly.");
            data.NearPassMessage = hint; EditorUtility.SetDirty(data); AssetDatabase.SaveAssets();
            return "Saved early-day-compatible close-pass wording; chase movement/retreat/cooldown unchanged.";
        }
        public static void OpenQuick() { Editable(); EditorSceneManager.OpenScene(ScenePath); }
        public static string Validate()
        {
            var world = UnityEngine.Object.FindAnyObjectByType<SliceSceneController>(); var surface = UnityEngine.Object.FindAnyObjectByType<NavMeshSurface>();
            if (!world || !world.Exploration || !world.Exploration.HasInvestigation || !world.Chase || !surface || AssetDatabase.GetAssetPath(surface.navMeshData) != NavigationPath)
                throw new InvalidOperationException("Missing saved D3–D4 composition/navigation.");
            var flow = world.Exploration;
            if (!flow.SecondTablet || flow.SecondTablet.Index != 1 || !flow.ThirdTablet || flow.ThirdTablet.Index != 2 || flow.SecondTablet.transform.position.x >= 0 || flow.ThirdTablet.transform.position.z > -20 || !world.Cipher.Wheel.ReadCode().SequenceEqual(new[] { 3, 4, 2 }))
                throw new InvalidOperationException("Original T02/T03 or cipher composition failed.");
            Path(flow.ReturnPoint.position, flow.ForestRoute.Last().position); Path(flow.ForestRoute.Last().position, flow.RitualRoute.Last().position); Path(flow.RitualRoute.Last().position, flow.ReturnPoint.position);
            return "Saved original forest T02/outside-cave T03, optional D3–D4 status, residual traces, cipher342 and native roundtrip paths reloaded.";
        }
        static void Path(Vector3 from, Vector3 to)
        {
            var path = new NavMeshPath();
            if (!NavMesh.SamplePosition(from, out var start, 1, NavMesh.AllAreas) || !NavMesh.SamplePosition(to, out var end, 1, NavMesh.AllAreas) || !NavMesh.CalculatePath(start.position, end.position, NavMesh.AllAreas, path) || path.status != NavMeshPathStatus.PathComplete)
                throw new InvalidOperationException("Investigation route is not completely navigable.");
        }
        public static void BeginProbe(string operation)
        {
            if (SessionState.GetBool(Probe, false)) throw new InvalidOperationException("Restore the previous investigation probe first.");
            if (operation == "investigation-review")
                foreach (string marker in new[] { "Temp/DemonReadingReview.ready", "Temp/DemonReadingReview.stop" }) if (File.Exists(marker)) File.Delete(marker);
            var world = UnityEngine.Object.FindAnyObjectByType<SliceSceneController>();
            SessionState.SetBool(Probe, true); SessionState.SetInt(Probe + "day", world.Settings.StartDay); SessionState.SetFloat(Probe + "hour", world.Settings.StartHour); SessionState.SetFloat(Probe + "speed", world.Settings.WalkMetresPerSecond); SessionState.SetString(Probe + "guide", world.Exploration.Definition.Day3Guide);
            if (operation == "investigation-late" || operation == "investigation-review") world.Settings.StartDay = 4;
            if (operation == "investigation-boundary") { world.Settings.StartDay = 2; world.Settings.StartHour = 23.99f; }
            if (operation == "investigation-tuning") { world.Settings.WalkMetresPerSecond = 2.8f; world.Exploration.Definition.Day3Guide = "【保存参数检查】" + world.Exploration.Definition.Day3Guide; EditorUtility.SetDirty(world.Exploration.Definition); }
            EditorUtility.SetDirty(world.Settings); Save();
        }
        public static void RestoreProbe()
        {
            if (!SessionState.GetBool(Probe, false)) return;
            var world = UnityEngine.Object.FindAnyObjectByType<SliceSceneController>();
            world.Settings.StartDay = SessionState.GetInt(Probe + "day", 3); world.Settings.StartHour = SessionState.GetFloat(Probe + "hour", 8); world.Settings.WalkMetresPerSecond = SessionState.GetFloat(Probe + "speed", 3.3f); world.Exploration.Definition.Day3Guide = SessionState.GetString(Probe + "guide", world.Exploration.Definition.Day3Guide);
            EditorUtility.SetDirty(world.Settings); EditorUtility.SetDirty(world.Exploration.Definition); Save(); SessionState.SetBool(Probe, false); Debug.Log("DEMON_INVESTIGATION_PROBE_RESTORED");
        }
        public static string Build()
        {
            Editable(); Directory.CreateDirectory("Builds/WindowsInvestigation");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = new[] { ScenePath }, locationPathName = "Builds/WindowsInvestigation/DemonInvestigation.exe", target = BuildTarget.StandaloneWindows64, options = BuildOptions.Development });
            if (report.summary.result != BuildResult.Succeeded) throw new InvalidOperationException("Investigation Windows build failed.");
            return "Investigation Windows64 succeeded, " + report.summary.totalSize + " bytes.";
        }
    }
}