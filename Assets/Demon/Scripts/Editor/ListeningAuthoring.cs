using System;
using System.IO;
using System.Linq;
using Demon.Slice.Domain;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Demon.Slice.Editor
{
    public static class ListeningAuthoring
    {
        public const string ScenePath = "Assets/Demon/Scenes/DemonListening.unity";
        const string DefinitionPath = "Assets/Demon/Data/ListeningDefinition.asset";
        const string QuickSettingsPath = "Assets/Demon/Data/ListeningSettings.asset";
        static Font font;
        static Material stone, snow, wood, metal, line, frozen;
        public static string Create()
        {
            RequireEditable();
            if (File.Exists(ScenePath) || File.Exists(DefinitionPath) || File.Exists(QuickSettingsPath))
                throw new InvalidOperationException("Listening outputs already exist; edit saved assets instead of regenerating.");
            EditorSceneManager.OpenScene(NineDayAuthoring.ScenePath);
            var world = UnityEngine.Object.FindAnyObjectByType<SliceSceneController>();
            if (world.Listening || GameObject.Find("Listening_Authored")) throw new InvalidOperationException("Existing listening content must be preserved.");
            var fixedTablet = UnityEngine.Object.FindObjectsByType<NineDayTablet>(FindObjectsInactive.Include).Single(t => t.Index == 4);
            if (!fixedTablet.transform.Find("FixedStone") || !fixedTablet.transform.Find("TabletLabel")) throw new InvalidOperationException("Unexpected T05 structure.");
            var data = ScriptableObject.CreateInstance<ListeningDefinition>(); AssetDatabase.CreateAsset(data, DefinitionPath);
            Sound("Assets/Demon/Audio/PrototypeChimeLow.wav", 660);
            Sound("Assets/Demon/Audio/PrototypeChimeHigh.wav", 990);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Demon/UI/WindowsChinese.fontsettings");
            stone = Mat("Stone"); snow = Mat("Snow"); wood = Mat("BoxWood"); metal = Mat("RuneSlot"); line = Mat("DormantLines"); frozen = Mat("FrozenLines");
            var root = new GameObject("Listening_Authored").transform;
            var tool = world.gameObject.AddComponent<ListeningTool>(); world.ConfigureListening(tool);
            // Existing T05 identity/discovery slot survives. Its saved placeholder body is hidden, not deleted.
            fixedTablet.transform.Find("FixedStone").gameObject.SetActive(false);
            fixedTablet.transform.Find("TabletLabel").gameObject.SetActive(false);
            var boardRoot = new GameObject("T05_Editable").transform; boardRoot.SetParent(fixedTablet.transform, false);
            boardRoot.localScale = Vector3.one * .68f;
            var tablet = Board(boardRoot, AssetDatabase.LoadAssetAtPath<ListeningDefinition>(DefinitionPath));
            tablet.ConfigureListening(tool); fixedTablet.ConfigureEditable(tablet); EditorUtility.SetDirty(fixedTablet);
            var target = new GameObject("SnowDisturbance_Target").transform; target.SetParent(root, false); target.position = new Vector3(2, .15f, -7);
            for (int i = 0; i < 2; i++) Primitive("SnowPrint_" + i, target, new Vector3(i == 0 ? -.3f : .3f, 0, 0), new Vector3(.4f, .09f, .85f), stone, false, PrimitiveType.Sphere);
            var chimes = new ListeningChime[2];
            for (int i = 0; i < 2; i++)
            {
                var sensor = new GameObject(i == 0 ? "Chime_A_Low" : "Chime_B_High").transform;
                sensor.SetParent(root, false); sensor.position = new Vector3(i == 0 ? 4.5f : 10.5f, .15f, -7);
                Primitive("Post", sensor, new Vector3(.7f, 1.1f, 0), new Vector3(.1f, 2.4f, .1f), wood);
                Primitive("Arm", sensor, new Vector3(.3f, 2.3f, 0), new Vector3(.9f, .08f, .08f), wood, false);
                var hanging = new GameObject("HangingVisual").transform; hanging.SetParent(sensor, false); hanging.localPosition = new Vector3(0, 2.2f, 0);
                for (int tube = 0; tube < 3; tube++) Primitive("Tube_" + tube, hanging, new Vector3((tube - 1) * .14f, -.35f, 0), new Vector3(.065f, .52f + tube * .08f, .065f), metal, false);
                Label("RegionLabel", sensor, new Vector3(.65f, 1.8f, -.1f), i == 0 ? "A · 低音" : "B · 高音", .018f);
                var audio = new GameObject("SpatialAudio").AddComponent<AudioSource>(); audio.transform.SetParent(sensor, false); audio.transform.localPosition = new Vector3(0, 1.9f, 0);
                audio.playOnAwake = false; audio.spatialBlend = 1; audio.rolloffMode = AudioRolloffMode.Linear;
                audio.minDistance = 1; audio.maxDistance = 16; audio.dopplerLevel = 0; audio.volume = .85f;
                chimes[i] = sensor.gameObject.AddComponent<ListeningChime>();
                chimes[i].Configure(target, 1.55f, audio, AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Demon/Audio/PrototypeChime" + (i == 0 ? "Low" : "High") + ".wav"), hanging);
            }
            var points = new Transform[8]; float[] xs = { 2, 4.5f, 7.5f, 10.5f, 12, 10.5f, 7.5f, 4.5f };
            var route = new GameObject("ProxyRoute_Editable").transform; route.SetParent(root, false);
            for (int i = 0; i < points.Length; i++) { points[i] = new GameObject("Point_" + i).transform; points[i].SetParent(route, false); points[i].position = new Vector3(xs[i], .15f, -7); }
            root.gameObject.AddComponent<ListeningTrailProxy>().Configure(target, points, world, tool);
            Primitive("ListeningOccluder", root, new Vector3(7.5f, 1.35f, -8.6f), new Vector3(6, 2.7f, .18f), wood);
            Label("OccluderInstruction", root, new Vector3(7.5f, 1.6f, -8.71f), "木墙后听风铃\n沉默不代表安全", .023f);
            Label("T05_Title", fixedTablet.transform, new Vector3(.15f, 1.1f, -.2f), "T05 · 风铃监听", .02f);
            var observation = new GameObject("OccludedView").transform; observation.SetParent(root, false); observation.position = new Vector3(7.5f, 1.65f, -9.45f); observation.rotation = Quaternion.identity;
            tool.Configure(AssetDatabase.LoadAssetAtPath<ListeningDefinition>(DefinitionPath), tablet, chimes);
            var plans = world.Campaign.Definition;
            plans.Days[5].Title = "风铃监听";
            plans.Days[5].Objective = "右侧T05可补全：中心RING、下中CHIME。Enter固化后去右后方雪径观察风铃，再绕木墙听。当前雪痕为工具教学占位。";
            plans.Tablets[4].Record = "【T05 · 前人监听工具】\n补全即启用风铃，不改变日期。\n恶魔进入区域时报警一次，离开后重新待命。停在区域内不重复报警。\n不同位置与音色提示区域；响度取决于玩家离风铃的距离，沉默不代表安全。";
            EditorUtility.SetDirty(plans); BindFonts(world);
            EditorUtility.SetDirty(world); EditorUtility.SetDirty(tool);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); EditorSceneManager.SaveScene(SceneManager.GetActiveScene()); AssetDatabase.SaveAssets();
            var settings = UnityEngine.Object.Instantiate(world.Settings); settings.name = "ListeningSettings"; settings.StartDay = 6; settings.StartHour = 10; settings.InitialLakeSealed = false;
            AssetDatabase.CreateAsset(settings, QuickSettingsPath); AssetDatabase.SaveAssets();
            if (!AssetDatabase.CopyAsset(NineDayAuthoring.ScenePath, ScenePath)) throw new InvalidOperationException("Could not create the D6 test scene.");
            EditorSceneManager.OpenScene(ScenePath); world = UnityEngine.Object.FindAnyObjectByType<SliceSceneController>();
            var serialized = new SerializedObject(world); serialized.FindProperty("settings").objectReferenceValue = AssetDatabase.LoadAssetAtPath<SliceSettings>(QuickSettingsPath); serialized.ApplyModifiedProperties();
            var body = world.Player.GetComponent<CharacterController>(); body.enabled = false;
            world.Player.transform.position = new Vector3(7.1f, .04f, -6.1f); world.Player.transform.rotation = Quaternion.identity; body.enabled = true;
            PrefabUtility.RecordPrefabInstancePropertyModifications(world.Player.transform);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); EditorSceneManager.SaveScene(SceneManager.GetActiveScene()); AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(ScenePath); return Validate();
        }
        static SliceTablet Board(Transform root, ListeningDefinition data)
        {
            var tablet = root.gameObject.AddComponent<SliceTablet>();
            Primitive("Slab", root, new Vector3(0, 0, .15f), new Vector3(2.85f, 2.85f, .3f), stone);
            var slots = new SliceRuneSlot[9]; var positions = new Vector3[9];
            for (int i = 0; i < 9; i++)
            {
                positions[i] = new Vector3((i % 3 - 1) * .86f, (1 - i / 3) * .86f, -.04f);
                var go = Primitive("Slot_" + (i + 1), root, positions[i], new Vector3(.66f, .035f, .66f), metal, true, PrimitiveType.Cylinder); go.transform.localRotation = Quaternion.Euler(90, 0, 0);
                slots[i] = go.AddComponent<SliceRuneSlot>(); slots[i].Index = i;
                slots[i].Label = Label("Rune_" + i, root, positions[i] + Vector3.back * .075f, SliceTablet.RuneLabel(data.Initial[i]), .014f);
                if (i == 3 || i == 5 || i == 8) { go.SetActive(false); slots[i].Label.gameObject.SetActive(false); }
            }
            int[] order = { 0, 1, 2, 4, 6, 7 }; var lines = new Renderer[5];
            for (int i = 0; i < 5; i++)
            {
                Vector3 a = positions[order[i]], b = positions[order[i + 1]];
                var go = Primitive("FixedLine_" + i, root, (a + b) / 2 + Vector3.forward * .014f, new Vector3(.035f, Vector3.Distance(a, b), .025f), line, false);
                go.transform.localRotation = Quaternion.FromToRotation(Vector3.up, b - a); lines[i] = go.GetComponent<Renderer>();
            }
            var tray = new SliceRuneTrayItem[data.LooseRunes.Length];
            for (int i = 0; i < tray.Length; i++)
            {
                Vector3 pos = new Vector3(2.18f, .95f - i * .72f, -.04f);
                var go = Primitive("Tray_" + data.LooseRunes[i], root, pos, new Vector3(1.12f, .48f, .16f), metal);
                tray[i] = go.AddComponent<SliceRuneTrayItem>(); tray[i].Rune = data.LooseRunes[i];
                tray[i].Label = Label("TrayLabel_" + i, root, pos + Vector3.back * .1f, SliceTablet.RuneLabel(tray[i].Rune) + " 1", .014f);
            }
            var view = new GameObject("CloseView").transform; view.SetParent(root, false); view.localPosition = new Vector3(.45f, .2f, -4.1f); view.LookAt(root.TransformPoint(new Vector3(.45f, .05f, 0)));
            tablet.Configure(view, slots, tray, lines, frozen); return tablet;
        }
        static void BindFonts(SliceSceneController world)
        {
            var binding = UnityEngine.Object.FindAnyObjectByType<SliceHud>().GetComponent<SliceFontBinding>();
            binding.Configure(font, binding.GetComponentsInChildren<Text>(true), UnityEngine.Object.FindObjectsByType<TextMesh>(FindObjectsInactive.Include)); EditorUtility.SetDirty(binding);
        }
        static Material Mat(string name) => AssetDatabase.LoadAssetAtPath<Material>("Assets/Demon/Art/Materials/" + name + ".mat");
        static GameObject Primitive(string name, Transform parent, Vector3 pos, Vector3 scale, Material mat, bool collider = true, PrimitiveType type = PrimitiveType.Cube)
        {
            var go = GameObject.CreatePrimitive(type); go.name = name; go.transform.SetParent(parent, false); go.transform.localPosition = pos; go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = mat; if (!collider) UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>()); return go;
        }
        static TextMesh Label(string name, Transform parent, Vector3 pos, string text, float size)
        {
            var mesh = new GameObject(name).AddComponent<TextMesh>(); mesh.transform.SetParent(parent, false); mesh.transform.localPosition = pos;
            mesh.font = font; mesh.fontSize = 64; mesh.characterSize = size; mesh.anchor = TextAnchor.MiddleCenter; mesh.alignment = TextAlignment.Center; mesh.text = text; return mesh;
        }
        static void Sound(string path, float frequency)
        {
            if (File.Exists(path)) throw new InvalidOperationException("Preserving existing sound: " + path);
            const int rate = 22050; int count = rate * 2;
            using (var stream = new BinaryWriter(File.Create(path)))
            {
                stream.Write(System.Text.Encoding.ASCII.GetBytes("RIFF")); stream.Write(36 + count * 2); stream.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));
                stream.Write(16); stream.Write((short)1); stream.Write((short)1); stream.Write(rate); stream.Write(rate * 2); stream.Write((short)2); stream.Write((short)16);
                stream.Write(System.Text.Encoding.ASCII.GetBytes("data")); stream.Write(count * 2);
                for (int i = 0; i < count; i++) { double t = (double)i / rate; double envelope = Math.Min(1, t * 90) * Math.Exp(-t * 3.7); double signal = Math.Sin(t * frequency * Math.PI * 2) + .35 * Math.Sin(t * frequency * 2.73 * Math.PI * 2); stream.Write((short)(signal * envelope * 11000)); }
            }
        }
        const string TuningKey = "Demon.Listening.Restore";
        public static void BeginTuning()
        {
            var tool = UnityEngine.Object.FindAnyObjectByType<SliceSceneController>().Listening;
            var proxy = UnityEngine.Object.FindAnyObjectByType<ListeningTrailProxy>();
            SessionState.SetFloat(TuningKey + "radius", tool.Chimes[0].RadiusMetres);
            SessionState.SetFloat(TuningKey + "speed", proxy.MetresPerSecond);
            SessionState.SetVector3(TuningKey + "view", tool.Tablet.CloseView.localPosition);
            SessionState.SetBool(TuningKey, true);
            SetFloat(tool.Chimes[0], "radiusMetres", 2.25f); SetFloat(proxy, "metresPerSecond", 1.25f);
            tool.Tablet.CloseView.localPosition += Vector3.right * .08f;
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        }
        public static void RestoreTuning()
        {
            if (!SessionState.GetBool(TuningKey, false)) return;
            var tool = UnityEngine.Object.FindAnyObjectByType<SliceSceneController>().Listening;
            SetFloat(tool.Chimes[0], "radiusMetres", SessionState.GetFloat(TuningKey + "radius", 1.55f));
            SetFloat(UnityEngine.Object.FindAnyObjectByType<ListeningTrailProxy>(), "metresPerSecond", SessionState.GetFloat(TuningKey + "speed", 2));
            tool.Tablet.CloseView.localPosition = SessionState.GetVector3(TuningKey + "view", Vector3.zero);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            SessionState.SetBool(TuningKey, false); Debug.Log("DEMON_LISTENING_TUNING_RESTORED");
        }
        static void SetFloat(UnityEngine.Object obj, string field, float value)
        {
            var serialized = new SerializedObject(obj); serialized.FindProperty(field).floatValue = value; serialized.ApplyModifiedProperties(); EditorUtility.SetDirty(obj);
        }
        public static string RepairReadability()
        {
            RequireEditable();
            const string materialPath = "Assets/Demon/Art/Materials/WorldFontDepth.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (!material)
            {
                var shader = Shader.Find("Demon/WorldFontDepth");
                if (!shader) throw new InvalidOperationException("Import WorldFontDepth shader first.");
                material = new Material(shader); AssetDatabase.CreateAsset(material, materialPath);
            }
            foreach (var path in new[] { NineDayAuthoring.ScenePath, ScenePath })
            {
                EditorSceneManager.OpenScene(path);
                var world = UnityEngine.Object.FindAnyObjectByType<SliceSceneController>();
                if (!world.Listening) throw new InvalidOperationException("Preserving scene without listening content.");
                var hud = UnityEngine.Object.FindAnyObjectByType<SliceHud>();
                hud.GetComponent<SliceFontBinding>().ConfigureWorldMaterial(material); EditorUtility.SetDirty(hud.GetComponent<SliceFontBinding>());
                foreach (var text in hud.GetComponentsInChildren<Text>(true))
                {
                    var outline = text.GetComponent<Outline>(); if (!outline) outline = text.gameObject.AddComponent<Outline>();
                    outline.effectColor = new Color(.02f, .035f, .05f, .95f); outline.effectDistance = new Vector2(1.5f, -1.5f); outline.useGraphicAlpha = true; EditorUtility.SetDirty(outline);
                }
                var root = GameObject.Find("Listening_Authored").transform;
                var label = root.Find("OccluderInstruction").GetComponent<TextMesh>();
                label.characterSize = .009f; label.transform.localPosition = new Vector3(4.9f, 1.6f, -8.71f); EditorUtility.SetDirty(label);
                var view = root.Find("VisibleView"); if (!view) { view = new GameObject("VisibleView").transform; view.SetParent(root, false); }
                view.position = new Vector3(7.5f, 3.6f, -4.8f); view.LookAt(new Vector3(7.5f, .8f, -7));
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            }
            AssetDatabase.SaveAssets(); EditorSceneManager.OpenScene(ScenePath); return "Depth-tested world fonts, outlined overlay, compact wall sign and saved region overview added without replacing authored content.";
        }
        public static string Validate()
        {
            var world = UnityEngine.Object.FindAnyObjectByType<SliceSceneController>();
            if (!world || !world.Listening || !world.Listening.Definition || !world.Listening.Tablet) throw new InvalidOperationException("Missing saved listening composition.");
            if (world.Listening.Chimes.Length != 2 || world.Listening.Chimes.Any(c => !c || !c.MonitoredTarget || !c.Source || c.Source.spatialBlend != 1)) throw new InvalidOperationException("Invalid spatial chime references.");
            if (world.Listening.Tablet.GetComponentsInChildren<SliceRuneSlot>(true).Length != 9) throw new InvalidOperationException("T05 must preserve nine authored slots.");
            if (new SerializedObject(world.Listening.Tablet).FindProperty("listeningTool").objectReferenceValue != world.Listening) throw new InvalidOperationException("T05 board ownership missing.");
            if (world.Tablet == world.Listening.Tablet || world.Settings.Target[4] != RuneKind.Exchange) throw new InvalidOperationException("T07 was replaced by T05.");
            if (SceneManager.GetActiveScene().path == ScenePath && (world.Settings.StartDay != 6 || world.Settings.StartHour != 10)) throw new InvalidOperationException("Quick-scene settings were not persisted.");
            return "Saved T05, independent board, two spatial chimes, route and composition references verified.";
        }
        static void RequireEditable()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || SceneManager.GetActiveScene().isDirty) throw new InvalidOperationException("Leave Play and save current scene first.");
        }
        public static void OpenQuick() { RequireEditable(); EditorSceneManager.OpenScene(ScenePath); }
        public static string Build()
        {
            RequireEditable(); Directory.CreateDirectory("Builds/WindowsListening");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = new[] { ScenePath }, locationPathName = "Builds/WindowsListening/DemonListening.exe", target = BuildTarget.StandaloneWindows64, options = BuildOptions.Development });
            if (report.summary.result != BuildResult.Succeeded) throw new InvalidOperationException("Listening build failed.");
            return "Listening Windows64 build succeeded, " + report.summary.totalSize + " bytes.";
        }
    }
}
