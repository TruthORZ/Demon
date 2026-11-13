using System;
using System.IO;
using System.Linq;
using System.Text;
using Demon.Slice.Domain;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Demon.Slice.Editor
{
    public static partial class CompletePlaythroughAuthoring
    {
        const string Data = "Assets/Demon/Data/CompletePlaythrough";
        public static string Inspect()
        {
            var scene = SceneManager.GetActiveScene();
            var world = UnityEngine.Object.FindAnyObjectByType<GameSceneController>();
            var text = new StringBuilder("scene=" + scene.path + "; dirty=" + scene.isDirty + "; playing=" + EditorApplication.isPlaying + "\n");
            foreach (var t in scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<Transform>(true)))
                if (t.GetComponent<Interactable>() || t.name.Contains("LogCabin") || t.name.Contains("Sun_") || t.name.Contains("Particle_Snow") || t.name.Contains("Floor") || t.name.Contains("Door") || t == world.Player.transform)
                    text.Append(t.name).Append(" p=").Append(t.position.ToString("F2")).Append(" r=").Append(t.eulerAngles.ToString("F1")).Append(" size=").Append(t.GetComponent<Collider>() ? t.GetComponent<Collider>().bounds.size.ToString("F1") : "-").Append('\n');
            File.WriteAllText("Temp/DemonSliceReports/task023-scene-inspection.txt", text.ToString());
            return text.ToString();
        }
        static GameSceneController Current()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || SceneManager.GetActiveScene().path != DayJumpMenuAuthoring.ScenePath)
                throw new InvalidOperationException("Open the snow scene outside Play Mode.");
            // Preserve the live scene, including Unity's deferred Inspector serialization,
            // before applying an explicit, scoped authoring operation. Never reload over edits.
            if (SceneManager.GetActiveScene().isDirty) SaveCheckpoint();
            return UnityEngine.Object.FindAnyObjectByType<GameSceneController>();
        }
        public static string SaveCheckpoint()
        {
            var scene = SceneManager.GetActiveScene();
            if (EditorApplication.isPlayingOrWillChangePlaymode || scene.path != DayJumpMenuAuthoring.ScenePath) throw new InvalidOperationException("Saved snow scene required.");
            const string backup = "Logs/DemonCheckpoints/task023-work"; Directory.CreateDirectory(backup);
            if (!EditorSceneManager.SaveScene(scene, backup + "/before-save-" + DateTime.UtcNow.ToString("yyyyMMddHHmmss") + ".unity", true)) throw new IOException("Scene checkpoint failed.");
            EditorSceneManager.SaveScene(scene); return "Checkpointed in-memory scene then saved explicit snow target.";
        }

        public static string Create()
        {
            var world = Current();
            if (world.GetComponent<CompletePlaythrough>()) throw new InvalidOperationException("Main route already exists; refusing to regenerate hand edits.");
            Directory.CreateDirectory(Data); AssetDatabase.Refresh();
            var source = new SerializedObject(world);
            string original = AssetDatabase.GetAssetPath(world.Settings);
            string settingsPath = Data + "/MainRouteSettings.asset";
            if (!AssetDatabase.CopyAsset(original, settingsPath)) throw new InvalidOperationException("Main route settings output already exists.");
            var settings = AssetDatabase.LoadAssetAtPath<GameSettings>(settingsPath); settings.RealSecondsPerDay = 720;
            settings.Instructions = "WASD 移动　Shift 奔跑　E 交互\n右键 手电 / T06牵制　Tab 背包　F8 调试\n拼板：鼠标点选　Enter 确认　Esc 返回";
            settings.EndingText = "第九天\n\n湖中封印再次开启。\n这一次，它拥有了实体。\n\n按 R 重新开始";
            EditorUtility.SetDirty(settings); source.FindProperty("settings").objectReferenceValue = settings; source.ApplyModifiedProperties();
            var definition = ScriptableObject.CreateInstance<PlaythroughDefinition>();
            string[] inspections = { "inspect-0", "inspect-1", "inspect-2", "inspect-3" };
            definition.Days = new DayRequirements[9];
            definition.Days[0] = new DayRequirements { Tasks = inspections.Concat(new[] { "note-a", "box-a", "box-b" }).ToArray() };
            for (int day = 2; day <= 5; day++) definition.Days[day - 1] = new DayRequirements { Tasks = inspections.Concat(new[] { "tablet-" + (day - 1) }).ToArray() };
            definition.Days[5] = new DayRequirements { Tasks = inspections.Concat(new[] { "tablet-5", "listening" }).ToArray() };
            definition.Days[6] = new DayRequirements { Tasks = inspections.Concat(new[] { "tablet-6" }).ToArray() };
            definition.Days[7] = new DayRequirements { Tasks = new[] { "handover", "wait", "retrieved", "placed", "compiled", "box-record" } };
            definition.Days[8] = new DayRequirements { Tasks = Array.Empty<string>() };
            definition.Entries = new[] {
                Entry("note-a", "值守须知", "每天检查电台、风向标、湖畔和森林四个巡查点。全部完成后，在夜间休息。当天的重要发现也需要处理完。\n\n屋后的两个箱子，也许有一天会帮上大忙。木屋可以躲避外面的东西。\n\n若当天结束仍有关键事项未完成，本日值守失败。"),
                Entry("note-b", "符文笔记 占位", "DAY 表示日期。TRANSFER 表示转移。EXCHANGE 表示双方交换。\n\n有些符号还无法辨认。请对照设施变化继续观察。"),
                Entry("flashlight", "手电筒", "按住鼠标右键照明，无电池消耗。取得T06后，稳定照射雪痕区域可牵制运动。"),
                Entry("note-t1", "遗落纸条一", "第三日，祂将获得运动。"), Entry("note-t2", "遗落纸条二", "第五日，祂将获得声音。"),
                Entry("note-t3", "遗落纸条三", "第七日，祂将获得光。"), Entry("note-t4", "遗落纸条四", "第九日，祂将获得实体。"),
                Entry("note-t5", "监听工具说明", "补全T05后，风铃会在祂进入监测区域时报警。声音提示区域，沉默不意味着安全。\n\n风铃不会跟随误导笑声，只报告真实的进入事件。"),
                Entry("T07", "最后一块石板", "已取得的破碎石板。带回工具棚台座放置，在场景中补全程序。") };
            AssetDatabase.CreateAsset(definition, Data + "/MainRouteDefinition.asset");
            AssetDatabase.SaveAssets(); AssetDatabase.Refresh(); definition = AssetDatabase.LoadAssetAtPath<PlaythroughDefinition>(Data + "/MainRouteDefinition.asset");
            var flow = Undo.AddComponent<CompletePlaythrough>(world.gameObject); world.ConfigurePlaythrough(flow);
            var hud = UnityEngine.Object.FindAnyObjectByType<GameHud>();
            var font = hud.GetComponentsInChildren<Text>(true).First(x => x.font).font;
            var ui = Rect("CompletePlaythrough_完整流程界面", hud.transform, Vector2.zero, Vector2.zero);
            ui.anchorMin = Vector2.zero; ui.anchorMax = Vector2.one; ui.offsetMin = ui.offsetMax = Vector2.zero;
            var overlay = ui.gameObject.AddComponent<Canvas>(); overlay.overrideSorting = true; overlay.sortingOrder = 90; ui.gameObject.AddComponent<GraphicRaycaster>();
            var tasks = Label("Today_关键事项", ui, new Vector2(1, 1), new Vector2(-24, -110), new Vector2(380, 430), font, 23, TextAnchor.UpperLeft);
            tasks.rectTransform.pivot = new Vector2(1, 1);
            var notice = Label("Notice_中央提示", ui, new Vector2(.5f, .66f), Vector2.zero, new Vector2(1100, 100), font, 32);
            var failure = Panel("Failure_当天失败", ui, font, "当天值守失败", out var failureText);
            var retry = Button("Retry_重试当天", failure.transform, new Vector2(0, -245), "重试当天", font);
            var bag = Panel("Journal_背包记录", ui, font, "背包", out var journalText);
            var scroll = new GameObject("Scroll_可滚动正文", typeof(RectTransform), typeof(Image), typeof(Mask), typeof(ScrollRect));
            var sr = scroll.GetComponent<RectTransform>(); sr.SetParent(bag.transform, false); sr.anchorMin = new Vector2(.08f, .2f); sr.anchorMax = new Vector2(.92f, .86f); sr.offsetMin = sr.offsetMax = Vector2.zero;
            scroll.GetComponent<Image>().color = new Color(.025f, .035f, .045f, .01f); scroll.GetComponent<Mask>().showMaskGraphic = false;
            journalText.rectTransform.SetParent(sr, false); journalText.rectTransform.anchorMin = new Vector2(0, 1); journalText.rectTransform.anchorMax = Vector2.one; journalText.rectTransform.pivot = new Vector2(.5f, 1); journalText.rectTransform.anchoredPosition = Vector2.zero; journalText.rectTransform.sizeDelta = new Vector2(0, 600);
            journalText.alignment = TextAnchor.UpperLeft; journalText.horizontalOverflow = HorizontalWrapMode.Wrap; journalText.verticalOverflow = VerticalWrapMode.Overflow;
            var fit = journalText.gameObject.AddComponent<ContentSizeFitter>(); fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var scrolling = scroll.GetComponent<ScrollRect>(); scrolling.viewport = sr; scrolling.content = journalText.rectTransform; scrolling.horizontal = false; scrolling.movementType = ScrollRect.MovementType.Clamped;
            var close = Button("Close_关闭背包", bag.transform, new Vector2(0, -245), "关闭背包  Tab / Esc", font);
            var black = Panel("Opening_开场与等待", ui, font, "Demon Principles", out var introText);
            var proceed = Button("Continue_继续", black.transform, new Vector2(0, -245), "继续  Enter", font);
            var wait = Button("Wait_等待交班", ui, new Vector2(0, -330), "等待到夜间  T", font);
            var scene = SceneManager.GetActiveScene(); var transforms = scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<Transform>(true)).ToArray();
            var cabin = transforms.First(x => x.name == "LogCabin");
            var safeRoot = new GameObject("CabinSafeZone_木屋安全范围"); Undo.RegisterCreatedObjectUndo(safeRoot, "Create main route safe zone"); safeRoot.transform.SetParent(cabin, false);
            var floors = cabin.GetComponentsInChildren<MeshFilter>().Where(x => x.name == "Floor" || x.name == "Floor_02").ToArray();
            if (floors.Length == 0) throw new InvalidOperationException("Saved cabin floors not found.");
            var localBounds = new Bounds(); bool first = true;
            foreach (var floor in floors)
            {
                var b = floor.sharedMesh.bounds;
                for (int i = 0; i < 8; i++)
                {
                    Vector3 corner = b.center + Vector3.Scale(b.extents, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
                    Vector3 p = cabin.InverseTransformPoint(floor.transform.TransformPoint(corner));
                    if (first) { localBounds = new Bounds(p, Vector3.zero); first = false; } else localBounds.Encapsulate(p);
                }
            }
            var safe = safeRoot.AddComponent<BoxCollider>(); safe.isTrigger = true;
            safe.center = new Vector3(localBounds.center.x, localBounds.max.y + 1.4f, localBounds.center.z);
            safe.size = new Vector3(localBounds.size.x - .35f, 3.6f, localBounds.size.z - .35f);
            var door = cabin.GetComponentsInChildren<Transform>().First(x => x.name == "Door");
            var exit = new GameObject("CabinExit_敌人停留边界").transform; exit.SetParent(world.transform, true); exit.position = door.position + door.forward * 3; exit.position = new Vector3(exit.position.x, world.Player.transform.position.y, exit.position.z);
            var snow = transforms.FirstOrDefault(x => x.name == "Particle_Snow")?.GetComponent<ParticleSystem>();
            var figure = GameObject.CreatePrimitive(PrimitiveType.Capsule); figure.name = "LakeProjection_恶魔投影占位"; figure.transform.SetParent(world.Presentation.transform, false);
            figure.transform.position = world.Retrieval.Pickup.transform.position + new Vector3(12, 2, 0); figure.transform.localScale = new Vector3(1.4f, 2.5f, 1.4f); UnityEngine.Object.DestroyImmediate(figure.GetComponent<Collider>());
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit")); mat.color = new Color(.015f, .02f, .025f); AssetDatabase.CreateAsset(mat, Data + "/LakeProjectionPlaceholder.mat"); figure.GetComponent<Renderer>().sharedMaterial = mat;
            var audio = world.gameObject.AddComponent<AudioSource>(); audio.playOnAwake = false; audio.spatialBlend = 0;
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Demon/Audio/PrototypeChimeLow.wav");
            flow.Configure(definition, tasks, notice, failureText, journalText, introText, failure, bag, black, retry, close, proceed, wait, safe, exit, snow, figure, audio, clip);
            failure.SetActive(false); bag.SetActive(false); black.SetActive(false); wait.gameObject.SetActive(false);
            // Expand the explicit observation array without renumbering the original three sites.
            var obs = world.Exploration.Observations;
            var forest = UnityEngine.Object.Instantiate(obs[2].gameObject, obs[2].transform.parent); forest.name = "ForestInspection_森林巡查";
            forest.transform.position = world.Exploration.SecondTablet.transform.position + new Vector3(3, 0, 2);
            var old = obs[2]; var newSite = forest.GetComponent<StationObservation>();
            var sf = new SerializedObject(newSite); sf.FindProperty("forestPoint").boolValue = true; sf.ApplyModifiedProperties();
            var ed = new SerializedObject(world.Exploration); var array = ed.FindProperty("observations"); array.arraySize = 4; array.GetArrayElementAtIndex(3).objectReferenceValue = newSite; ed.ApplyModifiedProperties();
            foreach (var label in forest.GetComponentsInChildren<TextMesh>()) label.text = "森林巡查点";
            Vector3 desk = world.Exploration.HandoverRecord.transform.position;
            Pickup(flow, "note-a", desk + new Vector3(.35f, .06f, .1f), world.Exploration.HandoverRecord.CloseView, 1, false);
            Pickup(flow, "note-b", desk + new Vector3(-.35f, .06f, .1f), world.Exploration.HandoverRecord.CloseView, 1, false);
            Pickup(flow, "flashlight", desk + new Vector3(0, .08f, -.4f), world.Exploration.HandoverRecord.CloseView, 1, true);
            var stones = new[] { world.Exploration.FirstTablet, world.Exploration.SecondTablet, world.Exploration.ThirdTablet, world.Exploration.FourthTablet, world.Exploration.FifthTablet };
            for (int i = 0; i < stones.Length; i++) Pickup(flow, "note-t" + (i + 1), stones[i].transform.position + new Vector3(.55f, .12f, 0), stones[i].transform, i + 2, false);
            var binding = UnityEngine.Object.FindAnyObjectByType<SceneFontBinding>(); var fonts = new SerializedObject(binding); var labels = fonts.FindProperty("overlayLabels");
            foreach (var label in ui.GetComponentsInChildren<Text>(true)) { int index = labels.arraySize; labels.InsertArrayElementAtIndex(index); labels.GetArrayElementAtIndex(index).objectReferenceValue = label; } fonts.ApplyModifiedProperties();
            EditorUtility.SetDirty(world); EditorUtility.SetDirty(flow); AssetDatabase.SaveAssets(); EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            return "Saved main route UI, definition, independent settings, four observations, safe zone, papers and flashlight. " + Inspect();
        }
        static JournalEntry Entry(string id, string title, string body) => new JournalEntry { Id = id, Title = title, Body = body };
        public static string FinishContent()
        {
            var world = Current(); var flow = world.Playthrough;
            if (!flow) throw new InvalidOperationException("Create main route first.");
            var forest = world.Exploration.Observations[3]; var point = new SerializedObject(forest);
            point.FindProperty("site").intValue = 2; point.FindProperty("forestPoint").boolValue = true; point.ApplyModifiedProperties();
            var data = new SerializedObject(flow); var safe = (BoxCollider)data.FindProperty("cabinSafeZone").objectReferenceValue;
            var obstacle = safe.GetComponent<UnityEngine.AI.NavMeshObstacle>();
            if (!obstacle) obstacle = Undo.AddComponent<UnityEngine.AI.NavMeshObstacle>(safe.gameObject);
            obstacle.shape = UnityEngine.AI.NavMeshObstacleShape.Box; obstacle.center = safe.center; obstacle.size = safe.size; obstacle.carving = true;
            foreach (var door in SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<CabinDoorInteraction>(true)))
            { var field = new SerializedObject(door); field.FindProperty("excludeEnemyNavigation").boolValue = true; field.ApplyModifiedProperties(); }
            var effects = new SerializedObject(world.Consequences); effects.FindProperty("mainRouteHowl").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Demon/Audio/PlaceholderRadioHowl.wav"); effects.ApplyModifiedProperties();
            var voice = world.transform.Find("MisleadingVoice_误导笑声占位");
            if (!voice) { voice = new GameObject("MisleadingVoice_误导笑声占位").transform; voice.SetParent(world.transform, false); }
            var audio = voice.GetComponent<AudioSource>(); if (!audio) audio = voice.gameObject.AddComponent<AudioSource>();
            audio.playOnAwake = false; audio.spatialBlend = 1; audio.minDistance = 2; audio.maxDistance = 55; audio.rolloffMode = AudioRolloffMode.Linear;
            var chase = new SerializedObject(world.Chase); chase.FindProperty("misleadingVoice").objectReferenceValue = audio; chase.FindProperty("laughClip").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Demon/Audio/PlaceholderMisleadingLaugh.wav"); chase.ApplyModifiedProperties();
            var explorePath = Data + "/MainRouteExploration.asset";
            if (!File.Exists(explorePath)) AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(world.Exploration.Definition), explorePath);
            var explore = AssetDatabase.LoadAssetAtPath<ExplorationDefinition>(explorePath);
            explore.Radio.Changed = "电台发出刺耳啸叫，原先的广播却从湖中传来。";
            if (!explore.HandoverRecord.Contains("按T")) explore.HandoverRecord += "\n\n查看后回到屋内，按T或点击等待按钮进入夜间。";
            var ex = new SerializedObject(world.Exploration); ex.FindProperty("definition").objectReferenceValue = explore; ex.ApplyModifiedProperties(); EditorUtility.SetDirty(explore);
            var stones = new[] { world.Exploration.FirstTablet, world.Exploration.SecondTablet, world.Exploration.ThirdTablet, world.Exploration.FourthTablet, world.Exploration.FifthTablet };
            foreach (var pickup in world.GetComponentsInChildren<StoryPickup>(true))
            {
                if (pickup.EntryId.StartsWith("note-t") && int.TryParse(pickup.EntryId.Substring(6), out int index))
                {
                    var p = new SerializedObject(pickup); p.FindProperty("readView").objectReferenceValue = new SerializedObject(stones[index - 1]).FindProperty("closeView").objectReferenceValue; p.ApplyModifiedProperties();
                }
            }
            RenderSettings.sun = world.Presentation.Sun;
            AssetDatabase.SaveAssets(); EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            return "Saved forest site compatibility, explicit enemy navigation exclusion/carving, howl and misleading-voice clips, independent text data and paper views.";
        }
        public static string AddSky()
        {
            var world = Current();
            if (world.GetComponent<SkySunPresentation>()) throw new InvalidOperationException("Sky visuals already exist; edit saved objects.");
            var sun = GameObject.CreatePrimitive(PrimitiveType.Sphere); sun.name = "SkySun_可编辑日轮"; sun.transform.SetParent(world.Presentation.transform, false); UnityEngine.Object.DestroyImmediate(sun.GetComponent<Collider>());
            var white = new Material(Shader.Find("Universal Render Pipeline/Unlit")); white.color = new Color(1, .94f, .75f); AssetDatabase.CreateAsset(white, Data + "/SkySunPlaceholder.mat"); sun.GetComponent<Renderer>().sharedMaterial = white;
            var core = GameObject.CreatePrimitive(PrimitiveType.Sphere); core.name = "DarkCore_永夜黑心"; core.transform.SetParent(sun.transform, false); core.transform.localPosition = new Vector3(0, 0, -.09f); core.transform.localScale = Vector3.one * .91f; UnityEngine.Object.DestroyImmediate(core.GetComponent<Collider>());
            var black = new Material(Shader.Find("Universal Render Pipeline/Unlit")); black.color = new Color(.01f, .015f, .02f); AssetDatabase.CreateAsset(black, Data + "/SkySunDarkPlaceholder.mat"); core.GetComponent<Renderer>().sharedMaterial = black;
            world.gameObject.AddComponent<SkySunPresentation>().Configure(world, sun.transform, core);
            EditorUtility.SetDirty(world); AssetDatabase.SaveAssets(); EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            return "Saved sky sun/rim visuals driven by world light direction and fixed D7 bearing.";
        }
        public static string LabelPickups()
        {
            var world = Current(); var binding = UnityEngine.Object.FindAnyObjectByType<SceneFontBinding>();
            var serial = new SerializedObject(binding); var labels = serial.FindProperty("worldLabels");
            Font font = ((Text)new SerializedObject(world.Playthrough).FindProperty("taskText").objectReferenceValue).font;
            var mat = AssetDatabase.LoadAssetAtPath<Material>(Data + "/PaperPlaceholder.mat");
            if (!mat) { mat = new Material(Shader.Find("Universal Render Pipeline/Lit")); mat.color = new Color(.8f, .72f, .5f); AssetDatabase.CreateAsset(mat, Data + "/PaperPlaceholder.mat"); }
            foreach (var pickup in world.GetComponentsInChildren<StoryPickup>(true))
            {
                if (pickup.EntryId == "flashlight" || pickup.transform.Find("Label_纸条标题")) continue;
                pickup.GetComponent<Renderer>().sharedMaterial = mat;
                var label = new GameObject("Label_纸条标题", typeof(TextMesh)).GetComponent<TextMesh>(); label.transform.SetParent(pickup.transform, false);
                // Ignore the thin paper's nonuniform scale so the authored text remains legible.
                label.transform.localScale = new Vector3(1 / pickup.transform.lossyScale.x, 1 / pickup.transform.lossyScale.y, 1 / pickup.transform.lossyScale.z);
                label.transform.position = pickup.transform.position + Vector3.up * .03f; label.transform.rotation = Quaternion.Euler(90, 0, 0);
                label.text = pickup.EntryId == "note-a" ? "值守须知" : pickup.EntryId == "note-b" ? "符文笔记" : "遗落纸条";
                label.font = font; label.fontSize = 40; label.characterSize = .035f; label.anchor = TextAnchor.MiddleCenter; label.color = Color.black;
                int index = labels.arraySize; labels.InsertArrayElementAtIndex(index); labels.GetArrayElementAtIndex(index).objectReferenceValue = label;
            }
            serial.ApplyModifiedProperties(); AssetDatabase.SaveAssets(); EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            return "Saved parchment placeholder material and readable paper labels, retaining existing font bindings.";
        }
        public static string CompletePresentation()
        {
            var world = Current(); var flow = new SerializedObject(world.Playthrough);
            var black = (GameObject)flow.FindProperty("openingPanel").objectReferenceValue;
            var rect = black.GetComponent<RectTransform>(); rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero; black.GetComponent<Image>().color = Color.black;
            var doors = SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<CabinDoorInteraction>(true)).ToArray();
            var list = flow.FindProperty("cabinDoors"); list.arraySize = doors.Length;
            for (int i = 0; i < doors.Length; i++)
            {
                var door = doors[i]; list.GetArrayElementAtIndex(i).objectReferenceValue = door;
                var native = new SerializedObject(door); native.FindProperty("excludeEnemyNavigation").boolValue = true; native.FindProperty("mainRouteDoor").boolValue = true;
                var vendor = door.GetComponents<MonoBehaviour>().FirstOrDefault(x => x.GetType().Name == "LC_Door");
                if (vendor)
                {
                    var source = new SerializedObject(vendor);
                    int axis = source.FindProperty("rotationOrientation").enumValueIndex;
                    float angle = source.FindProperty("doorOpenAngle").floatValue;
                    native.FindProperty("mainRouteOpenEuler").vector3Value = (axis == 0 ? Vector3.up : axis == 1 ? Vector3.forward : Vector3.right) * angle;
                    native.FindProperty("mainRouteSpeed").floatValue = source.FindProperty("speed").floatValue;
                    native.FindProperty("openClip").objectReferenceValue = source.FindProperty("doorOpenSound").objectReferenceValue;
                    native.FindProperty("closeClip").objectReferenceValue = source.FindProperty("doorCloseSound").objectReferenceValue;
                    // Only this scene instance changes owner, preserving the vendor component and event bindings.
                    vendor.enabled = false; EditorUtility.SetDirty(vendor);
                }
                var audio = door.GetComponent<AudioSource>(); if (!audio) audio = door.gameObject.AddComponent<AudioSource>();
                audio.playOnAwake = false; audio.spatialBlend = 1; audio.minDistance = 1; audio.maxDistance = 20;
                native.FindProperty("mainRouteAudio").objectReferenceValue = audio; native.ApplyModifiedProperties();
            }
            flow.ApplyModifiedProperties(); EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            return "Saved full black opening and explicit restorable scene-only cabin door ownership; vendor assets/events preserved.";
        }
        public static string Validate()
        {
            var world = Current();
            if (!world.Playthrough || !world.GetComponent<SkySunPresentation>() || world.Exploration.Observations.Length != 4 || !world.Exploration.Observations[3].IsForest)
                throw new InvalidOperationException("Saved route composition is incomplete.");
            if (world.Settings.RealSecondsPerDay < 10 || !AssetDatabase.GetAssetPath(world.Settings).StartsWith(Data)) throw new InvalidOperationException("Main route requires independent settings and at least 10 real seconds per day.");
            var root = SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (var component in root.SelectMany(x => x.GetComponentsInChildren<MonoBehaviour>(true)))
            {
                if (!component) throw new InvalidOperationException("Missing scene script.");
                if (!(component is CompletePlaythrough) && !(component is StoryPickup) && !(component is SkySunPresentation)) continue;
                var serialized = new SerializedObject(component); var field = serialized.GetIterator();
                while (field.NextVisible(true))
                    if (field.propertyType == SerializedPropertyType.ObjectReference && field.objectReferenceValue == null)
                        throw new InvalidOperationException(component.GetType().Name + " has missing authored reference: " + field.propertyPath);
            }
            return "Saved route scripts/references, independent settings, forest observation and sky verified.";
        }
        public static string Build()
        {
            Validate(); Directory.CreateDirectory("Builds/WindowsCompletePlaythrough");
            var scenes = EditorBuildSettings.scenes.Where(x => x.path != DayJumpMenuAuthoring.ScenePath).ToList(); scenes.Insert(0, new EditorBuildSettingsScene(DayJumpMenuAuthoring.ScenePath, true)); EditorBuildSettings.scenes = scenes.ToArray();
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = new[] { DayJumpMenuAuthoring.ScenePath }, target = BuildTarget.StandaloneWindows64,
                locationPathName = "Builds/WindowsCompletePlaythrough/Demon.exe", options = BuildOptions.Development });
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded) throw new InvalidOperationException("Main route Windows build failed: " + report.summary.result);
            return "Windows64 main-route build succeeded: " + report.summary.totalSize + " bytes. Entry=" + DayJumpMenuAuthoring.ScenePath;
        }
        public static string BeginEditability()
        {
            var world = Current(); var flow = world.Playthrough; var def = flow.Definition;
            if (SessionState.GetBool("Demon.Complete.Editability", false)) throw new InvalidOperationException("Restore previous editability probe first.");
            SessionState.SetFloat("Demon.Complete.Speed", def.ChaseSpeeds[0]); SessionState.SetString("Demon.Complete.Note", def.Entry("note-a").Body);
            var inventory = (JournalInventory)new SerializedObject(flow).FindProperty("journalInventory").objectReferenceValue;
            var grid = inventory.GetComponentInChildren<GridLayoutGroup>(true);
            SessionState.SetVector3("Demon.Complete.GridSpacing", grid.spacing);
            grid.spacing += new Vector2(-2, 0); SessionState.SetFloat("Demon.Complete.GridExpectedX", grid.spacing.x);
            var rect = ((Text)new SerializedObject(flow).FindProperty("taskText").objectReferenceValue).rectTransform;
            SessionState.SetVector3("Demon.Complete.Layout", rect.anchoredPosition); SessionState.SetBool("Demon.Complete.Editability", true);
            def.ChaseSpeeds[0] = 2.75f; def.Entry("note-a").Body += "\n编辑性验证临时文字。"; rect.anchoredPosition += new Vector2(-12, 0);
            EditorUtility.SetDirty(def); AssetDatabase.SaveAssets(); EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            EditorSceneManager.OpenScene(DayJumpMenuAuthoring.ScenePath);
            world = UnityEngine.Object.FindAnyObjectByType<GameSceneController>();
            if (world.Playthrough.Definition.ChaseSpeeds[0] != 2.75f || !world.Playthrough.Definition.Entry("note-a").Body.Contains("编辑性验证")) throw new InvalidOperationException("Tuning did not survive reopening.");
            return "Temporary note, task layout and D3 speed saved/reopened; runtime probe pending.";
        }
        public static void RestoreEditability()
        {
            if (!SessionState.GetBool("Demon.Complete.Editability", false)) return;
            var world = UnityEngine.Object.FindAnyObjectByType<GameSceneController>(); if (!world || !world.Playthrough) throw new InvalidOperationException("Return to main route to restore editability probe.");
            var flow = world.Playthrough; flow.Definition.ChaseSpeeds[0] = SessionState.GetFloat("Demon.Complete.Speed", 2.6f); flow.Definition.Entry("note-a").Body = SessionState.GetString("Demon.Complete.Note", "");
            var label = (Text)new SerializedObject(flow).FindProperty("taskText").objectReferenceValue; label.rectTransform.anchoredPosition = SessionState.GetVector3("Demon.Complete.Layout", Vector3.zero);
            var inventory = (JournalInventory)new SerializedObject(flow).FindProperty("journalInventory").objectReferenceValue;
            inventory.GetComponentInChildren<GridLayoutGroup>(true).spacing = SessionState.GetVector3("Demon.Complete.GridSpacing", new Vector3(8, 8));
            EditorUtility.SetDirty(flow.Definition); AssetDatabase.SaveAssets(); EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            SessionState.SetBool("Demon.Complete.Editability", false); File.WriteAllText("Temp/DemonSliceReports/complete-editability-restored.txt", "Restored note, D3 speed, task layout and inventory grid spacing after saved/reopened runtime integration probe.");
        }
        public static string FinalizeRouteContent()
        {
            var world = Current();
            string path = Data + "/MainRouteSchedule.asset";
            if (File.Exists(path)) throw new InvalidOperationException("Main route schedule already authored; edit saved content instead of regenerating it.");
            AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(world.Campaign.Definition), path);
            var schedule = AssetDatabase.LoadAssetAtPath<NineDayDefinition>(path);
            string[] objectives = {
                "进入木屋阅读值守须知，检查屋后两个箱子与四处巡查点。完成关键事项并入夜后休息。",
                "完成四处巡查，沿人类脚印前往山洞，寻找T01。",
                "完成四处巡查。风向标已经停转，沿大型雪痕寻找林间T02，留意逼近的雪雾。",
                "完成四处巡查，前往旧仪式遗迹寻找T03。",
                "完成四处巡查。电台发出啸叫，循湖中原广播寻找T04。",
                "完成四处巡查，到旧监听站寻找并补全T05，启用风铃。",
                "完成四处巡查，解开箱A并取得T06。已提前取得的工具继续保留。",
                "检查交接记录，在屋内按T等待夜间。取得T07，带回补全并查看箱B记录。",
                "第九天，恶魔降临。" };
            for (int i = 0; i < 9; i++) schedule.Days[i].Objective = objectives[i];
            schedule.Tablets[1].Record = "【T02 · 已固化】\nIF DAY == 5\nEXCHANGE(SOUND, RADIO, DEMON)\n\n第五日零点，原广播从电台转移到湖中声源，电台发出啸叫。发现或阅读不会改变既定日期程序。";
            var campaign = new SerializedObject(world.Campaign); campaign.FindProperty("definition").objectReferenceValue = schedule; campaign.ApplyModifiedProperties(); EditorUtility.SetDirty(schedule);
            var scene = SceneManager.GetActiveScene();
            var deskParent = scene.GetRootGameObjects().Single(x => x.name.StartsWith("02_")).transform;
            var desk = deskParent.Find("D01_桌面线索");
            if (!desk) { desk = new GameObject("D01_桌面线索").transform; Undo.RegisterCreatedObjectUndo(desk.gameObject, "Organize D1 clues"); desk.SetParent(deskParent, false); desk.position = world.Exploration.HandoverRecord.transform.position; }
            var stones = new[] { world.Exploration.FirstTablet, world.Exploration.SecondTablet, world.Exploration.ThirdTablet, world.Exploration.FourthTablet, world.Exploration.FifthTablet };
            var pickups = scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<StoryPickup>(true)).ToArray();
            foreach (var pickup in pickups)
            {
                Transform parent = desk;
                if (pickup.EntryId.StartsWith("note-t") && int.TryParse(pickup.EntryId.Substring(6), out int index)) parent = stones[index - 1].transform.parent;
                Undo.RecordObject(pickup.transform, "Keep clues with authored point"); pickup.transform.SetParent(parent, true);
            }
            var flow = new SerializedObject(world.Playthrough); var black = (GameObject)flow.FindProperty("openingPanel").objectReferenceValue;
            var canvas = black.GetComponent<Canvas>(); if (!canvas) canvas = black.AddComponent<Canvas>(); canvas.overrideSorting = true; canvas.sortingOrder = 200;
            if (!black.GetComponent<GraphicRaycaster>()) black.AddComponent<GraphicRaycaster>();
            var binding = new SerializedObject(UnityEngine.Object.FindAnyObjectByType<SceneFontBinding>()); var labels = binding.FindProperty("worldLabels");
            foreach (var label in world.Exploration.Observations[3].GetComponentsInChildren<TextMesh>(true))
            {
                bool present = false; for (int i = 0; i < labels.arraySize; i++) if (labels.GetArrayElementAtIndex(i).objectReferenceValue == label) present = true;
                if (!present) { int i = labels.arraySize; labels.InsertArrayElementAtIndex(i); labels.GetArrayElementAtIndex(i).objectReferenceValue = label; }
            }
            binding.ApplyModifiedProperties(); AssetDatabase.SaveAssets(); EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            return "Saved independent main-route objectives, point-group clue ownership, forest font and black-screen overlay.";
        }
        public static bool TryStandpoint(Interactable item, GameSceneController world, out Vector3 feet, out Vector3 aim)
        {
            Physics.SyncTransforms(); feet = aim = Vector3.zero;
            var body = world.Player.GetComponent<CharacterController>();
            foreach (var target in item.GetComponentsInChildren<Collider>(true))
            {
                if (target.isTrigger || target.GetComponentInParent<Interactable>() != item) continue;
                Vector3 center = target.bounds.center;
                for (int i = 0; i < 48; i++)
                {
                    float angle = (i % 16) * Mathf.PI / 8; float radius = 1.1f + (i / 16) * .6f;
                    Vector3 p0 = center + new Vector3(Mathf.Sin(angle) * radius, .8f, Mathf.Cos(angle) * radius);
                    var ground = Physics.RaycastAll(p0, Vector3.down, 5, ~0, QueryTriggerInteraction.Ignore).Where(x => x.normal.y > .6f && !x.collider.GetComponentInParent<Interactable>() && !x.collider.transform.IsChildOf(world.Player.transform)).OrderBy(x => x.distance).FirstOrDefault();
                    if (!ground.collider) continue;
                    Vector3 foot = ground.point + Vector3.up * .04f;
                    if (Physics.OverlapCapsule(foot + Vector3.up * .3f, foot + Vector3.up * (body.height - .3f), body.radius * .8f, ~0, QueryTriggerInteraction.Ignore).Any(x => !x.transform.IsChildOf(world.Player.transform))) continue;
                    Vector3 eye = foot + Vector3.up * (body.height - .15f); Vector3 delta = center - eye;
                    if (delta.magnitude > world.Settings.InteractionMetres) continue;
                    var hit = Physics.RaycastAll(eye, delta.normalized, delta.magnitude + .05f, ~0, QueryTriggerInteraction.Ignore).Where(x => !x.collider.transform.IsChildOf(world.Player.transform)).OrderBy(x => x.distance).FirstOrDefault();
                    if (hit.collider && hit.collider.GetComponentInParent<Interactable>() == item) { feet = foot; aim = center; return true; }
                }
            }
            return false;
        }
        public static string InspectReachability()
        {
            var world = Current(); var text = new StringBuilder();
            var items = SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<StoryPickup>(true)).Cast<Interactable>().Concat(new[] { world.Exploration.Observations[3] });
            foreach (var item in items) { bool clear = TryStandpoint(item, world, out var feet, out var aim); text.Append(item.name).Append(" reachable=").Append(clear).Append(" feet=").Append(feet.ToString("F2")).Append(" aim=").Append(aim.ToString("F2")).Append('\n'); }
            File.WriteAllText("Temp/DemonSliceReports/complete-reachability.txt", text.ToString()); return text.ToString();
        }
        public static string RepairBlockedRecords()
        {
            var world = Current(); var scene = SceneManager.GetActiveScene();
            var pickups = scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<StoryPickup>(true)).ToArray();
            var result = new StringBuilder();
            foreach (string id in new[] { "flashlight", "note-t1" })
            {
                var item = pickups.Single(x => x.EntryId == id);
                if (TryStandpoint(item, world, out _, out _)) continue;
                Vector3 before = item.transform.position; Undo.RecordObject(item.transform, "Repair blocked new record");
                bool clear = false;
                if (id == "flashlight")
                {
                    item.transform.position = pickups.Single(x => x.EntryId == "note-a").transform.position + new Vector3(-.35f, .04f, .02f);
                    clear = TryStandpoint(item, world, out _, out _);
                }
                else
                {
                    Vector3 stone = world.Exploration.FirstTablet.transform.position;
                    foreach (float height in new[] { .12f, .3f, .5f })
                    {
                        for (int i = 0; i < 16 && !clear; i++)
                        {
                            float angle = i * Mathf.PI / 8;
                            item.transform.position = stone + new Vector3(Mathf.Sin(angle) * .65f, height, Mathf.Cos(angle) * .65f);
                            clear = TryStandpoint(item, world, out _, out _);
                        }
                        if (clear) break;
                    }
                }
                if (!clear) { item.transform.position = before; throw new InvalidOperationException("No clear position found for " + id + "; original position retained."); }
                result.Append(id).Append(" moved ").Append(before.ToString("F2")).Append(" -> ").Append(item.transform.position.ToString("F2")).Append('\n');
            }
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            return result + InspectReachability();
        }
        static void Pickup(CompletePlaythrough owner, string id, Vector3 position, Transform view, int day, bool hide)
        {
            var item = GameObject.CreatePrimitive(PrimitiveType.Cube); item.name = "Record_" + id; item.transform.SetParent(owner.transform, true); item.transform.position = position; item.transform.localScale = id == "flashlight" ? new Vector3(.12f, .12f, .32f) : new Vector3(.35f, .045f, .25f);
            Undo.RegisterCreatedObjectUndo(item, "Create authored record"); item.AddComponent<StoryPickup>().Configure(owner, id, view, day, hide);
        }
        static RectTransform Rect(string name, Transform parent, Vector2 position, Vector2 size)
        { var r = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>(); r.SetParent(parent, false); r.anchorMin = r.anchorMax = new Vector2(.5f, .5f); r.anchoredPosition = position; r.sizeDelta = size; return r; }
        static Text Label(string name, Transform parent, Vector2 anchor, Vector2 position, Vector2 size, Font font, int fontSize, TextAnchor align = TextAnchor.MiddleCenter)
        {
            var r = Rect(name, parent, position, size); r.anchorMin = r.anchorMax = anchor;
            var t = r.gameObject.AddComponent<Text>(); t.font = font; t.fontSize = fontSize; t.alignment = align; t.color = new Color(.94f, .92f, .85f); t.raycastTarget = false;
            r.gameObject.AddComponent<Outline>().effectColor = Color.black; return t;
        }
        static GameObject Panel(string name, Transform parent, Font font, string title, out Text body)
        {
            var r = Rect(name, parent, Vector2.zero, new Vector2(1120, 680)); r.gameObject.AddComponent<Image>().color = new Color(.018f, .025f, .035f, .99f);
            Label("Title", r, new Vector2(.5f, .5f), new Vector2(0, 275), new Vector2(1000, 70), font, 34).text = title;
            body = Label("Body", r, new Vector2(.5f, .5f), new Vector2(0, 10), new Vector2(980, 400), font, 25); return r.gameObject;
        }
        static Button Button(string name, Transform parent, Vector2 position, string title, Font font)
        {
            var r = Rect(name, parent, position, new Vector2(460, 66)); var image = r.gameObject.AddComponent<Image>(); image.color = new Color(.13f, .2f, .25f);
            var b = r.gameObject.AddComponent<Button>(); b.targetGraphic = image; Label("Label", r, new Vector2(.5f, .5f), Vector2.zero, new Vector2(440, 60), font, 25).text = title; return b;
        }
    }
}
