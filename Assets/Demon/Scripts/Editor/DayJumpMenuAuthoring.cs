using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Demon.Slice.Editor
{
    public static class DayJumpMenuAuthoring
    {
        public const string ScenePath = "Assets/Demon/Scenes/DemonNineDay_Art.unity";
        const string Checkpoint = "Logs/DemonCheckpoints/task022-day-jump-20261107";
        const string RootName = "DayJumpMenu_天数快捷菜单";
        static readonly Color Ink = new Color(.92f, .95f, 1);

        [MenuItem("Demon/Debug Day Menu/Create In Snow Scene")]
        public static void CreateFromMenu() => Debug.Log(Create());

        public static string Inspect()
        {
            var scene = SceneManager.GetActiveScene();
            return "scene=" + scene.path + "; dirty=" + scene.isDirty + "; playing=" + EditorApplication.isPlaying +
                "; menus=" + UnityEngine.Object.FindObjectsByType<DayJumpMenu>(FindObjectsInactive.Include).Length;
        }

        public static void OpenSaved()
        {
            var current = SceneManager.GetActiveScene();
            if (current.isDirty)
            {
                if (current.path != ScenePath) throw new InvalidOperationException("Save the current authored scene before opening the saved snow scene.");
                // Preserve authored edits, including the UI module's deferred Inspector serialization.
                EditorSceneManager.SaveScene(current);
            }
            EditorSceneManager.OpenScene(ScenePath);
        }

        public static string Create()
        {
            var scene = SceneManager.GetActiveScene();
            if (EditorApplication.isPlayingOrWillChangePlaymode || scene.path != ScenePath)
                throw new InvalidOperationException("Open the snow integration scene outside Play Mode.");
            if (scene.isDirty) throw new InvalidOperationException("Save current scene edits before adding the debug menu.");
            var world = UnityEngine.Object.FindAnyObjectByType<GameSceneController>();
            var hud = UnityEngine.Object.FindAnyObjectByType<GameHud>();
            if (!world || !hud || !world.Exploration || !world.Exploration.HasFinalPreparation)
                throw new InvalidOperationException("Snow scene composition is incomplete.");
            if (hud.transform.Find(RootName)) throw new InvalidOperationException("Menu already exists; edit it in Hierarchy. Refusing to overwrite authored content.");
            Directory.CreateDirectory(Checkpoint);
            File.WriteAllText(Checkpoint + "/native-before.txt", Inspect());
            var poses = scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<Transform>(true))
                .ToDictionary(x => GlobalObjectId.GetGlobalObjectIdSlow(x).ToString(), x => (x.position, x.rotation, x.lossyScale));
            var root = Rect(RootName, hud.transform, new Vector2(.5f, .5f), Vector2.zero, Vector2.zero);
            root.anchorMin = Vector2.zero; root.anchorMax = Vector2.one; root.offsetMin = root.offsetMax = Vector2.zero;
            var canvas = root.gameObject.AddComponent<Canvas>(); canvas.overrideSorting = true; canvas.sortingOrder = 100;
            root.gameObject.AddComponent<GraphicRaycaster>();
            var menu = root.gameObject.AddComponent<DayJumpMenu>();
            Font font = hud.GetComponentsInChildren<Text>(true).First(x => x.font).font;
            var hint = Label("ShortcutHint_快捷键提示", root, new Vector2(.5f, 1), new Vector2(0, -24), new Vector2(410, 34), font, 21);
            hint.rectTransform.pivot = new Vector2(.5f, 1); hint.text = "F8  天数快捷菜单";
            var shade = Rect("MenuPanel_菜单面板", root, new Vector2(.5f, .5f), Vector2.zero, Vector2.zero);
            shade.anchorMin = Vector2.zero; shade.anchorMax = Vector2.one; shade.offsetMin = shade.offsetMax = Vector2.zero;
            shade.gameObject.AddComponent<Image>().color = new Color(.008f, .014f, .022f, .82f);
            var card = Rect("Card_菜单布局", shade, new Vector2(.5f, .5f), Vector2.zero, new Vector2(940, 820));
            card.gameObject.AddComponent<Image>().color = new Color(.045f, .065f, .09f, .99f);
            Label("Title", card, new Vector2(.5f, 1), new Vector2(0, -53), new Vector2(830, 58), font, 36).text = "天数快捷菜单";
            Label("Explanation", card, new Vector2(.5f, 1), new Vector2(0, -118), new Vector2(830, 72), font, 23).text =
                "调试跳转 · 补齐前置流程\nF8 / Esc 关闭　数字键 1–9 选择天数";
            var status = Label("Status_当前状态", card, new Vector2(.5f, 1), new Vector2(0, -207), new Vector2(830, 100), font, 21);
            status.text = "选择目标后重置流程并回到开局位置，补齐此前各天。";
            var buttons = new Button[9];
            for (int i = 0; i < 9; i++)
            {
                int day = i + 1;
                string text = "[" + day + "]  第 " + day + " 天\n" + (day == 9 ? "00:00 · 结局" : day == 1 ? "值守开始" : "此前各天已完成");
                buttons[i] = MakeButton("D" + day, card, new Vector2((i % 3 - 1) * 280, 80 - i / 3 * 106), new Vector2(260, 88), font, text);
            }
            var night = MakeButton("D08_Night_直接拾取入口", card, new Vector2(0, -268), new Vector2(820, 76), font,
                "[0]  第 8 天夜间 · 湖岸 T07 待拾取");
            var close = MakeButton("Close_继续游戏", card, new Vector2(0, -359), new Vector2(820, 64), font, "继续当前游戏  /  Esc");
            menu.Configure(world, shade.gameObject, buttons, night, close, status);
            shade.gameObject.SetActive(false);

            if (!UnityEngine.Object.FindAnyObjectByType<EventSystem>())
            {
                var events = new GameObject("MenuEventSystem_菜单输入", typeof(EventSystem), typeof(InputSystemUIInputModule));
                Undo.RegisterCreatedObjectUndo(events, "Create day menu event system");
                events.transform.SetParent(root, false);
                AssignSavedInput(events.GetComponent<InputSystemUIInputModule>());
            }
            var binding = UnityEngine.Object.FindAnyObjectByType<SceneFontBinding>();
            if (!binding) throw new InvalidOperationException("Existing Chinese font binding missing.");
            var serialized = new SerializedObject(binding);
            var labels = serialized.FindProperty("overlayLabels");
            var newLabels = root.GetComponentsInChildren<Text>(true);
            int first = labels.arraySize; labels.arraySize += newLabels.Length;
            for (int i = 0; i < newLabels.Length; i++) labels.GetArrayElementAtIndex(first + i).objectReferenceValue = newLabels[i];
            serialized.ApplyModifiedProperties();
            foreach (var pose in poses)
            {
                var transform = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(Parse(pose.Key)) as Transform;
                if (!transform || Vector3.Distance(transform.position, pose.Value.position) > .0001f ||
                    Quaternion.Angle(transform.rotation, pose.Value.rotation) > .01f || Vector3.Distance(transform.lossyScale, pose.Value.lossyScale) > .0001f)
                    throw new InvalidOperationException("Existing layout changed while adding UI: " + pose.Key);
            }
            Undo.RegisterCreatedObjectUndo(root.gameObject, "Create debug day menu");
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            return Validate();
        }

        static GlobalObjectId Parse(string value)
        { if (!GlobalObjectId.TryParse(value, out var id)) throw new InvalidOperationException("Invalid saved object identity."); return id; }

        public static string ProbeEditability()
        {
            OpenSaved();
            RectTransform Card() => UnityEngine.Object.FindAnyObjectByType<DayJumpMenu>().GetComponentsInChildren<RectTransform>(true).Single(x => x.name == "Card_菜单布局");
            Vector2 original = Card().anchoredPosition;
            try
            {
                Card().anchoredPosition = original + new Vector2(17, 11);
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
                EditorSceneManager.OpenScene(ScenePath);
                if (Vector2.Distance(Card().anchoredPosition, original + new Vector2(17, 11)) > .001f)
                    throw new InvalidOperationException("Authored menu layout edit did not persist.");
            }
            finally
            {
                Card().anchoredPosition = original;
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
                EditorSceneManager.OpenScene(ScenePath);
            }
            return "Menu RectTransform edit persisted across save/reload and was restored. " + Validate();
        }

        public static string PlaceHint()
        {
            OpenSaved();
            var hint = UnityEngine.Object.FindAnyObjectByType<DayJumpMenu>().GetComponentsInChildren<Text>(true).Single(x => x.name == "ShortcutHint_快捷键提示");
            var rect = hint.rectTransform; rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, 1);
            rect.anchoredPosition = new Vector2(0, -24); hint.alignment = TextAnchor.MiddleCenter;
            var menu = hint.GetComponentInParent<DayJumpMenu>();
            var status = new SerializedObject(menu).FindProperty("statusText").objectReferenceValue as Text;
            status.text = "选择目标后重置流程并回到开局位置，补齐此前各天。";
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            return "Moved the shortcut hint to the top centre, clear of the existing exploration HUD.";
        }

        public static string Validate()
        {
            if (SceneManager.GetActiveScene().path != ScenePath) throw new InvalidOperationException("Wrong scene.");
            var menu = UnityEngine.Object.FindAnyObjectByType<DayJumpMenu>(FindObjectsInactive.Include);
            if (!menu) throw new InvalidOperationException("Saved menu missing.");
            var data = new SerializedObject(menu);
            foreach (var field in new[] { "world", "panel", "finalNightButton", "closeButton", "statusText" })
                if (!data.FindProperty(field).objectReferenceValue) throw new InvalidOperationException("Missing menu reference: " + field);
            var buttons = data.FindProperty("dayButtons");
            if (buttons.arraySize != 9) throw new InvalidOperationException("Nine day buttons required.");
            for (int i = 0; i < buttons.arraySize; i++) if (!buttons.GetArrayElementAtIndex(i).objectReferenceValue) throw new InvalidOperationException("Missing day button.");
            if (UnityEngine.Object.FindObjectsByType<EventSystem>(FindObjectsInactive.Include).Length != 1) throw new InvalidOperationException("Exactly one UI EventSystem required.");
            var module = UnityEngine.Object.FindAnyObjectByType<InputSystemUIInputModule>();
            if (!module || !module.actionsAsset || !module.point || !module.leftClick || string.IsNullOrEmpty(AssetDatabase.GetAssetPath(module.point)))
                throw new InvalidOperationException("UI input must reference saved imported actions.");
            return "Saved day menu, nine days, D8 night and saved UI input verified. Existing authored world transforms preserved.";
        }

        public static string RepairInput()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || SceneManager.GetActiveScene().path != ScenePath)
                throw new InvalidOperationException("Open the saved snow scene outside Play Mode.");
            AssignSavedInput(UnityEngine.Object.FindAnyObjectByType<InputSystemUIInputModule>());
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            return Validate();
        }

        static void AssignSavedInput(InputSystemUIInputModule module)
        {
            const string path = "Assets/InputSystem_Actions.inputactions";
            var source = AssetDatabase.LoadAssetAtPath<InputActionAsset>(path);
            var references = AssetDatabase.LoadAllAssetsAtPath(path).OfType<InputActionReference>().ToArray();
            InputActionReference Action(string name)
            {
                var id = source.FindAction("UI/" + name, true).id;
                // The importer exposes both qualified and legacy aliases for the same action.
                return references.OrderBy(x => x.name).First(x => x.action.id == id && !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(x)));
            }
            var saved = new SerializedObject(module);
            saved.FindProperty("m_ActionsAsset").objectReferenceValue = source;
            var fields = new[] { "m_PointAction", "m_LeftClickAction", "m_RightClickAction", "m_MiddleClickAction", "m_ScrollWheelAction",
                "m_MoveAction", "m_SubmitAction", "m_CancelAction", "m_TrackedDevicePositionAction", "m_TrackedDeviceOrientationAction" };
            var names = new[] { "Point", "Click", "RightClick", "MiddleClick", "ScrollWheel", "Navigate", "Submit", "Cancel", "TrackedDevicePosition", "TrackedDeviceOrientation" };
            // Module setters keep an equivalent generated reference; author the imported references exactly as its Inspector does.
            for (int i = 0; i < fields.Length; i++) saved.FindProperty(fields[i]).objectReferenceValue = Action(names[i]);
            saved.ApplyModifiedProperties();
            EditorUtility.SetDirty(module);
        }

        static RectTransform Rect(string name, Transform parent, Vector2 anchor, Vector2 position, Vector2 size)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false); rect.anchorMin = rect.anchorMax = anchor;
            rect.anchoredPosition = position; rect.sizeDelta = size; return rect;
        }
        static Text Label(string name, Transform parent, Vector2 anchor, Vector2 position, Vector2 size, Font font, int fontSize, TextAnchor align = TextAnchor.MiddleCenter)
        {
            var rect = Rect(name, parent, anchor, position, size);
            var text = rect.gameObject.AddComponent<Text>(); text.font = font; text.fontSize = fontSize;
            text.color = Ink; text.alignment = align; text.raycastTarget = false;
            var outline = rect.gameObject.AddComponent<Outline>(); outline.effectColor = new Color(0, 0, 0, .6f);
            return text;
        }
        static Button MakeButton(string name, Transform parent, Vector2 position, Vector2 size, Font font, string text)
        {
            var rect = Rect(name, parent, new Vector2(.5f, .5f), position, size);
            var image = rect.gameObject.AddComponent<Image>(); image.color = new Color(.11f, .17f, .23f);
            var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = image;
            var colors = button.colors; colors.highlightedColor = new Color(.65f, .85f, 1); colors.pressedColor = new Color(.4f, .7f, .9f); button.colors = colors;
            var navigation = button.navigation; navigation.mode = Navigation.Mode.None; button.navigation = navigation;
            Label("Label", rect, new Vector2(.5f, .5f), Vector2.zero, size - new Vector2(16, 8), font, 24).text = text;
            return button;
        }
    }
}
