using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Demon.Slice.Domain;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Demon.Slice.Editor
{
    public static class DemonSliceAuthoring
    {
        public const string ScenePath = "Assets/Demon/Scenes/DemonSlice.unity";
        const string Root = "Assets/Demon";
        static Font font;

        [MenuItem("Demon/Slice/Create Graybox Scene")]
        public static void CreateScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Leave Play Mode before authoring.");
            if (File.Exists(ScenePath))
                throw new InvalidOperationException("DemonSlice already exists. Open and edit it; the authoring tool will not overwrite it.");
            if (SceneManager.GetActiveScene().isDirty)
                throw new InvalidOperationException("The current scene has unsaved changes. Save it before creating a new scene.");

            foreach (string folder in new[] { "Scenes", "Prefabs", "Data", "Art/Materials", "UI" })
                Folder(Root + "/" + folder);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(.4f, .5f, .65f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 18;
            RenderSettings.fogEndDistance = 65;
            RenderSettings.skybox = null;

            var settings = ScriptableObject.CreateInstance<SliceSettings>();
            string settingsPath = Root + "/Data/SliceSettings.asset";
            if (AssetDatabase.LoadAssetAtPath<SliceSettings>(settingsPath))
                throw new InvalidOperationException("Settings already exist; refusing to replace authored data.");
            AssetDatabase.CreateAsset(settings, settingsPath);
            string fontPath = Root + "/UI/WindowsChinese.fontsettings";
            font = AssetDatabase.LoadAssetAtPath<Font>(fontPath);
            if (!font)
            {
                font = Font.CreateDynamicFontFromOSFont(new[] { "Microsoft YaHei", "Microsoft YaHei UI", "SimHei", "Arial" }, 40);
                font.name = "WindowsChinese";
                AssetDatabase.CreateAsset(font, fontPath);
            }
            var snow = Material("Snow", new Color(.81f, .87f, .91f));
            var ice = Material("Lake", new Color(.035f, .12f, .18f), .65f);
            var stone = Material("Stone", new Color(.19f, .24f, .29f));
            var slotMat = Material("RuneSlot", new Color(.38f, .43f, .47f));
            var lineMat = Material("DormantLines", new Color(.12f, .17f, .2f));
            var glow = Material("SealGlow", new Color(.22f, .75f, .8f), .4f, true);
            var frozen = Material("FrozenLines", new Color(.9f, .48f, .18f), .2f, true);
            var wood = Material("BoxWood", new Color(.25f, .15f, .095f));
            var dark = Material("Figure", new Color(.018f, .023f, .03f));
            var paper = Material("Paper", new Color(.83f, .8f, .67f));

            var world = new GameObject("SliceComposition").AddComponent<SliceSceneController>();
            var environment = new GameObject("Environment").transform;
            Primitive("SnowGround", PrimitiveType.Cube, environment, new Vector3(0, -.15f, -2), new Vector3(26, .3f, 16), snow);
            Primitive("LakeSurface", PrimitiveType.Cube, environment, new Vector3(0, -.1f, 12), new Vector3(26, .12f, 12), ice);
            Boundary("LakeBoundary", environment, new Vector3(0, 1, 6), new Vector3(26, 3, .3f));
            Boundary("WestBoundary", environment, new Vector3(-13, 1, 0), new Vector3(.3f, 3, 28));
            Boundary("EastBoundary", environment, new Vector3(13, 1, 0), new Vector3(.3f, 3, 28));
            Boundary("SouthBoundary", environment, new Vector3(0, 1, -10), new Vector3(26, 3, .3f));
            for (int i = 0; i < 12; i++)
            {
                float x = i % 2 == 0 ? -10 : 10;
                Primitive("SnowBank_" + i, PrimitiveType.Sphere, environment,
                    new Vector3(x + Mathf.Sin(i) * .5f, -.1f, -8 + i * 1.15f), new Vector3(3, 1.2f, 2.5f), snow);
            }
            var sun = new GameObject("ClockDrivenSun").AddComponent<Light>();
            sun.type = LightType.Directional; sun.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(40, -30, 0); RenderSettings.sun = sun;
            var warm = new GameObject("ShedLamp").AddComponent<Light>();
            warm.type = LightType.Point; warm.range = 9; warm.intensity = 4; warm.color = new Color(1, .7f, .4f);
            warm.transform.position = new Vector3(-4.2f, 2.4f, 1.2f);

            var playerObject = new GameObject("Player");
            playerObject.transform.position = new Vector3(0, .04f, -4);
            var body = playerObject.AddComponent<CharacterController>();
            body.height = 1.8f; body.radius = .28f; body.center = new Vector3(0, .9f, 0);
            var camera = new GameObject("PlayerCamera").AddComponent<Camera>();
            camera.transform.SetParent(playerObject.transform, false);
            camera.transform.localPosition = new Vector3(0, 1.65f, 0);
            camera.nearClipPlane = .05f; camera.farClipPlane = 100; camera.fieldOfView = 65;
            camera.clearFlags = CameraClearFlags.SolidColor; camera.tag = "MainCamera";
            camera.gameObject.AddComponent<AudioListener>();
            var player = playerObject.AddComponent<SlicePlayer>();
            player.Configure(camera, AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/InputSystem_Actions.inputactions"));
            SavePrefab(playerObject, "SlicePlayer");

            var tabletObject = new GameObject("Tablet");
            tabletObject.transform.position = new Vector3(0, 1.5f, .6f);
            var tablet = tabletObject.AddComponent<SliceTablet>();
            Primitive("Slab", PrimitiveType.Cube, tabletObject.transform, new Vector3(0, 0, .15f), new Vector3(2.85f, 2.85f, .3f), stone);
            var slotList = new List<SliceRuneSlot>();
            Vector3[] centres = new Vector3[9];
            for (int i = 0; i < 9; i++)
            {
                Vector3 pos = new Vector3((i % 3 - 1) * .86f, (1 - i / 3) * .86f, -.04f);
                centres[i] = pos;
                var go = Primitive("Slot_" + (i + 1), PrimitiveType.Cylinder, tabletObject.transform,
                    pos, new Vector3(.66f, .035f, .66f), slotMat);
                go.transform.localRotation = Quaternion.Euler(90, 0, 0);
                var slot = go.AddComponent<SliceRuneSlot>(); slot.Index = i;
                slot.Label = Label("Rune", tabletObject.transform, pos + new Vector3(0, 0, -.075f),
                    SliceTablet.RuneLabel(settings.Initial[i]), .014f);
                slotList.Add(slot);
            }
            int[] path = { 2, 1, 0, 4, 6, 7, 8, 2 };
            var lines = new List<Renderer>();
            for (int i = 0; i < path.Length - 1; i++)
            {
                Vector3 a = centres[path[i]]; Vector3 b = centres[path[i + 1]];
                var line = Primitive("FixedLine_" + i, PrimitiveType.Cube, tabletObject.transform,
                    (a + b) * .5f + new Vector3(0, 0, .012f), new Vector3(.035f, Vector3.Distance(a, b), .025f), lineMat, false);
                line.transform.localRotation = Quaternion.FromToRotation(Vector3.up, b - a);
                lines.Add(line.GetComponent<Renderer>());
            }
            var trayList = new List<SliceRuneTrayItem>();
            for (int i = 0; i < settings.LooseRunes.Length; i++)
            {
                Vector3 pos = new Vector3(2.18f, 1.03f - .67f * i, .04f);
                var token = Primitive("Tray_" + settings.LooseRunes[i], PrimitiveType.Cube, tabletObject.transform,
                    pos, new Vector3(1.12f, .48f, .16f), slotMat);
                var item = token.AddComponent<SliceRuneTrayItem>(); item.Rune = settings.LooseRunes[i];
                item.Label = Label("RuneLabel", tabletObject.transform, pos + new Vector3(0, 0, -.1f),
                    SliceTablet.RuneLabel(item.Rune) + "  1", .014f);
                trayList.Add(item);
            }
            Label("TrayTitle", tabletObject.transform, new Vector3(2.18f, 1.46f, -.08f), "RUNES", .025f);
            var tabletView = Anchor("CloseView", tabletObject.transform, new Vector3(.45f, .2f, -4.1f),
                tabletObject.transform.position + new Vector3(.45f, .05f, 0));
            tablet.Configure(tabletView, slotList.ToArray(), trayList.ToArray(), lines.ToArray(), frozen);
            SavePrefab(tabletObject, "SliceTablet");

            var boxObject = new GameObject("Box"); boxObject.transform.position = new Vector3(-4.1f, 0, 1.3f);
            var box = boxObject.AddComponent<SliceBox>();
            Primitive("Body", PrimitiveType.Cube, boxObject.transform, new Vector3(0, .45f, 0), new Vector3(1.65f, .9f, 1.15f), wood);
            Primitive("Record", PrimitiveType.Cube, boxObject.transform, new Vector3(0, .915f, 0), new Vector3(1.1f, .025f, .7f), paper, false);
            var lid = new GameObject("LidPivot").transform; lid.SetParent(boxObject.transform, false); lid.localPosition = new Vector3(0, .98f, .57f);
            Primitive("Lid", PrimitiveType.Cube, lid, new Vector3(0, 0, -.57f), new Vector3(1.8f, .15f, 1.3f), wood);
            var lockObject = Primitive("SealLock", PrimitiveType.Cube, boxObject.transform, new Vector3(0, .7f, -.61f), new Vector3(.22f, .32f, .09f), glow, false);
            var readView = Anchor("ReadView", boxObject.transform, new Vector3(0, 1.8f, -1.5f), boxObject.transform.position + Vector3.up * .8f);
            SavePrefab(boxObject, "SliceBox"); box.Configure(world);
            PrefabUtility.RecordPrefabInstancePropertyModifications(box);
            EditorUtility.SetDirty(box);
            var shed = new GameObject("OpenToolShed").transform;
            shed.position = new Vector3(-4.1f, 0, 1.3f);
            for (int i = 0; i < 4; i++) Primitive("Post_" + i, PrimitiveType.Cube, shed,
                new Vector3(i % 2 == 0 ? -1.6f : 1.6f, 1.45f, i < 2 ? -1.4f : 1.4f), new Vector3(.16f, 2.9f, .16f), wood);
            Primitive("Roof", PrimitiveType.Cube, shed, new Vector3(0, 2.95f, 0), new Vector3(3.8f, .18f, 3.4f), snow);

            var bedObject = new GameObject("SleepPoint"); bedObject.transform.position = new Vector3(-4.2f, 0, -2.4f);
            Primitive("SleepingBag", PrimitiveType.Cube, bedObject.transform, new Vector3(0, .2f, 0), new Vector3(1.3f, .4f, 2), stone);
            bedObject.AddComponent<SliceBed>().Configure(world);
            Label("SleepLabel", bedObject.transform, new Vector3(0, .95f, -.4f), "SLEEP", .04f);

            var sealObject = new GameObject("LakeSeal"); sealObject.transform.position = new Vector3(0, .04f, 9);
            for (int i = 0; i < 16; i++)
            {
                float angle = i * Mathf.PI * 2 / 16;
                var bar = Primitive("SealBar_" + i, PrimitiveType.Cube, sealObject.transform,
                    new Vector3(Mathf.Sin(angle) * 2.6f, .1f, Mathf.Cos(angle) * 2.6f), new Vector3(1.04f, .12f, .12f), glow, false);
                bar.transform.localRotation = Quaternion.Euler(0, i * 360f / 16, 0);
            }
            sealObject.SetActive(false);
            var figure = new GameObject("EntityPlaceholder"); figure.transform.position = new Vector3(0, 0, 9);
            Primitive("Body", PrimitiveType.Capsule, figure.transform, new Vector3(0, 2.5f, 0), new Vector3(1.9f, 2.5f, 1.6f), dark, false);
            var eyeMat = Material("Eyes", new Color(1, .14f, .06f), .1f, true);
            foreach (float x in new[] { -.32f, .32f })
                Primitive("Eye", PrimitiveType.Sphere, figure.transform, new Vector3(x, 4.1f, -.68f), Vector3.one * .16f, eyeMat, false);
            figure.SetActive(false);
            var endingView = Anchor("EndingView", environment, new Vector3(0, 2, 2.8f), new Vector3(0, 3.3f, 9));
            var presentation = new GameObject("WorldPresentation").AddComponent<SlicePresentation>();
            presentation.Configure(lid, sealObject, lockObject, figure, readView, sun);
            var hud = CreateHud();
            world.Configure(settings, player, tablet, presentation, hud, endingView);
            AddFontBinding();
            EditorSceneManager.SaveScene(scene, ScenePath);
            var existing = EditorBuildSettings.scenes.Where(s => s.path != ScenePath).ToList();
            existing.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = existing.ToArray();
            AssetDatabase.SaveAssets();
            Selection.activeObject = world.gameObject;
            Debug.Log("DEMON_SLICE_CREATED " + ScenePath);
        }

        static SliceHud CreateHud()
        {
            var canvasObject = new GameObject("SliceHUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080); scaler.matchWidthOrHeight = .5f;
            var hud = canvasObject.AddComponent<SliceHud>();
            Text clock = Text("Clock", canvasObject.transform, new Vector2(32, -24), new Vector2(800, 100), 27, TextAnchor.UpperLeft);
            Text message = Text("Message", canvasObject.transform, new Vector2(32, -145), new Vector2(1200, 110), 26, TextAnchor.UpperLeft);
            Text help = Text("Instructions", canvasObject.transform, new Vector2(32, 28), new Vector2(1300, 150), 23, TextAnchor.LowerLeft, true);
            Text focus = Text("Focus", canvasObject.transform, new Vector2(0, -190), new Vector2(1100, 80), 28, TextAnchor.MiddleCenter, false, true);
            Text held = Text("HeldRune", canvasObject.transform, new Vector2(0, 90), new Vector2(1550, 100), 25, TextAnchor.MiddleCenter, true, true);
            Text crosshair = Text("Crosshair", canvasObject.transform, Vector2.zero, new Vector2(40, 40), 22, TextAnchor.MiddleCenter, false, true);
            crosshair.text = "+";
            var reading = Panel("ReadingPanel", canvasObject.transform, new Vector2(1100, 780), new Color(.025f, .035f, .05f, .96f));
            Text page = Text("RecordText", reading.transform, new Vector2(45, -35), new Vector2(1010, 710), 28, TextAnchor.UpperLeft);
            var ending = Panel("EndingPanel", canvasObject.transform, new Vector2(1080, 390), new Color(.02f, .025f, .04f, .88f));
            var endingRect = ending.GetComponent<RectTransform>(); endingRect.anchorMin = endingRect.anchorMax = new Vector2(.5f, .25f);
            Text end = Text("EndingText", ending.transform, new Vector2(30, -20), new Vector2(1020, 350), 30, TextAnchor.MiddleCenter);
            reading.SetActive(false); ending.SetActive(false);
            hud.Configure(clock, message, help, focus, held, reading, page, ending, end);
            return hud;
        }

        static GameObject Panel(string name, Transform parent, Vector2 size, Color colour)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false); var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f); rect.sizeDelta = size;
            go.GetComponent<Image>().color = colour; go.GetComponent<Image>().raycastTarget = false; return go;
        }

        static Text Text(string name, Transform parent, Vector2 position, Vector2 size, int pointSize,
            TextAnchor alignment, bool bottom = false, bool centre = false)
        {
            var go = new GameObject(name, typeof(RectTransform)); go.transform.SetParent(parent, false);
            var text = go.AddComponent<Text>(); text.font = font; text.fontSize = pointSize;
            text.color = new Color(.92f, .95f, 1); text.alignment = alignment; text.raycastTarget = false;
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = centre ? new Vector2(.5f, bottom ? 0 : .5f) : new Vector2(0, bottom ? 0 : 1);
            rect.pivot = centre ? new Vector2(.5f, bottom ? 0 : .5f) : new Vector2(0, bottom ? 0 : 1);
            rect.anchoredPosition = position; rect.sizeDelta = size;
            return text;
        }

        static TextMesh Label(string name, Transform parent, Vector3 position, string words, float size)
        {
            var go = new GameObject(name); go.transform.SetParent(parent, false); go.transform.localPosition = position;
            var text = go.AddComponent<TextMesh>(); text.font = font; text.text = words;
            text.fontSize = 64; text.characterSize = size; text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center; text.color = new Color(.92f, .96f, 1);
            go.GetComponent<MeshRenderer>().sharedMaterial = font.material; return text;
        }

        static GameObject Primitive(string name, PrimitiveType type, Transform parent, Vector3 position, Vector3 scale, Material material, bool collision = true)
        {
            var go = GameObject.CreatePrimitive(type); go.name = name; go.transform.SetParent(parent, false);
            go.transform.localPosition = position; go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = material;
            if (!collision) UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());
            return go;
        }

        static Transform Anchor(string name, Transform parent, Vector3 localPosition, Vector3 worldTarget)
        {
            var go = new GameObject(name); go.transform.SetParent(parent, false); go.transform.localPosition = localPosition;
            go.transform.LookAt(worldTarget); return go.transform;
        }

        static void Boundary(string name, Transform parent, Vector3 position, Vector3 size)
        {
            var go = new GameObject(name); go.transform.SetParent(parent, false); go.transform.localPosition = position;
            go.AddComponent<BoxCollider>().size = size;
        }

        static void SavePrefab(GameObject go, string name)
        {
            string path = Root + "/Prefabs/" + name + ".prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path)) throw new InvalidOperationException("Refusing to overwrite " + path);
            PrefabUtility.SaveAsPrefabAssetAndConnect(go, path, InteractionMode.AutomatedAction);
        }

        static Material Material(string name, Color colour, float smoothness = .15f, bool emission = false)
        {
            string path = Root + "/Art/Materials/" + name + ".mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path); if (existing) return existing;
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (!shader) throw new InvalidOperationException("Installed URP Lit shader was not found.");
            var material = new Material(shader) { name = name };
            material.SetColor("_BaseColor", colour); material.SetFloat("_Smoothness", smoothness);
            if (emission) { material.EnableKeyword("_EMISSION"); material.SetColor("_EmissionColor", colour * 2); }
            AssetDatabase.CreateAsset(material, path); return material;
        }

        static void Folder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/'); Folder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        public static string ValidateSavedScene()
        {
            var scene = SceneManager.GetActiveScene();
            if (scene.path != ScenePath) throw new InvalidOperationException("Open the saved DemonSlice scene first.");
            var root = UnityEngine.Object.FindAnyObjectByType<SliceSceneController>();
            if (!root || !root.Settings || !root.Player || !root.Tablet) throw new InvalidOperationException("Scene composition is incomplete.");
            foreach (var box in UnityEngine.Object.FindObjectsByType<SliceBox>())
                if (new SerializedObject(box).FindProperty("world").objectReferenceValue != root)
                    throw new InvalidOperationException("Box requires the scene composition reference: " + box.name);
            foreach (var bed in UnityEngine.Object.FindObjectsByType<SliceBed>())
                if (new SerializedObject(bed).FindProperty("world").objectReferenceValue != root)
                    throw new InvalidOperationException("Bed requires the scene composition reference: " + bed.name);
            if (UnityEngine.Object.FindObjectsByType<SliceRuneSlot>(FindObjectsInactive.Include).Length != 9)
                throw new InvalidOperationException("Expected nine saved rune slots.");
            foreach (var component in UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include))
            {
                var serialized = new SerializedObject(component);
                var iterator = serialized.GetIterator();
                while (iterator.NextVisible(true))
                    if (iterator.propertyType == SerializedPropertyType.ObjectReference && iterator.objectReferenceEntityIdValue != default && !iterator.objectReferenceValue)
                        throw new InvalidOperationException("Missing object reference: " + component.name + "/" + iterator.propertyPath);
            }
            return "Saved scene, nine slots and serialized object references verified.";
        }

        public static string RepairBoxBinding()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || SceneManager.GetActiveScene().path != ScenePath)
                throw new InvalidOperationException("Open the saved slice outside Play Mode before repairing its binding.");
            var root = UnityEngine.Object.FindAnyObjectByType<SliceSceneController>();
            var boxes = UnityEngine.Object.FindObjectsByType<SliceBox>();
            if (!root || boxes.Length != 1) throw new InvalidOperationException("Expected one scene composition and one box.");
            var serialized = new SerializedObject(boxes[0]);
            var field = serialized.FindProperty("world");
            if (field.objectReferenceValue && field.objectReferenceValue != root)
                throw new InvalidOperationException("Preserving a different existing box binding; inspect it first.");
            field.objectReferenceValue = root;
            serialized.ApplyModifiedProperties();
            PrefabUtility.RecordPrefabInstancePropertyModifications(boxes[0]);
            EditorUtility.SetDirty(boxes[0]);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            EditorSceneManager.OpenScene(ScenePath);
            return ValidateSavedScene();
        }

        public static string AddFontBinding()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Leave Play Mode before editing font bindings.");
            if (SceneManager.GetActiveScene().path != ScenePath && !string.IsNullOrEmpty(SceneManager.GetActiveScene().path))
                throw new InvalidOperationException("Open the slice before editing font bindings.");
            var hud = UnityEngine.Object.FindAnyObjectByType<SliceHud>();
            if (!hud) throw new InvalidOperationException("Slice HUD not found.");
            var binding = hud.GetComponent<SliceFontBinding>();
            if (!binding) binding = Undo.AddComponent<SliceFontBinding>(hud.gameObject);
            Undo.RecordObject(binding, "Bind slice text fonts");
            binding.Configure(AssetDatabase.LoadAssetAtPath<Font>(Root + "/UI/WindowsChinese.fontsettings"),
                hud.GetComponentsInChildren<Text>(true), UnityEngine.Object.FindObjectsByType<TextMesh>(FindObjectsInactive.Include));
            EditorUtility.SetDirty(binding);
            // One-time migration of the untouched prototype sizes; custom artist edits are preserved.
            string tabletPath = Root + "/Prefabs/SliceTablet.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(tabletPath))
            {
                var prefab = PrefabUtility.LoadPrefabContents(tabletPath);
                try
                {
                    bool changed = false;
                    foreach (var label in prefab.GetComponentsInChildren<TextMesh>(true)) changed |= FitLegacyLabel(label);
                    if (changed) PrefabUtility.SaveAsPrefabAsset(prefab, tabletPath);
                }
                finally { PrefabUtility.UnloadPrefabContents(prefab); }
            }
            foreach (var label in UnityEngine.Object.FindObjectsByType<TextMesh>(FindObjectsInactive.Include))
                if (FitLegacyLabel(label) && PrefabUtility.IsPartOfPrefabInstance(label))
                    PrefabUtility.RecordPrefabInstancePropertyModifications(label);
            if (SceneManager.GetActiveScene().path == ScenePath)
                EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            return "Bound Windows font face to authored overlay and world labels.";
        }

        static bool FitLegacyLabel(TextMesh label)
        {
            float oldSize = label.name == "Rune" ? .052f : label.name == "RuneLabel" ? .045f :
                label.name == "TrayTitle" ? .055f : label.name == "SleepLabel" ? .1f : -1;
            if (oldSize < 0 || !Mathf.Approximately(label.characterSize, oldSize)) return false;
            Undo.RecordObject(label, "Fit prototype rune label");
            label.characterSize = label.name == "TrayTitle" ? .025f : label.name == "SleepLabel" ? .04f : .014f;
            EditorUtility.SetDirty(label);
            return true;
        }

        public static string BuildWindows()
        {
            if (!File.Exists(ScenePath)) throw new InvalidOperationException("Create the slice before building.");
            Directory.CreateDirectory("Builds/Windows");
            var result = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = new[] { ScenePath }, locationPathName = "Builds/Windows/DemonSlice.exe",
                target = BuildTarget.StandaloneWindows64, options = BuildOptions.Development });
            if (result.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Windows build failed: " + result.summary.result);
            return "Windows64 build succeeded, " + result.summary.totalSize + " bytes.";
        }
    }
}
