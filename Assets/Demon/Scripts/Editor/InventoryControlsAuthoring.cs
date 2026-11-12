using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Demon.Slice.Editor
{
    public static partial class CompletePlaythroughAuthoring
    {
        [MenuItem("Demon/Main Route/Apply Grid Inventory And Click Flashlight")]
        static void ApplyInventoryMenu() => Debug.Log(ApplyInventoryControls());
        public static string ApplyInventoryControls()
        {
            var world = Current(); var flow = world.Playthrough; var scene = world.gameObject.scene;
            // Once migrated, validate without regenerating or overwriting subsequent Inspector edits.
            if (new SerializedObject(flow).FindProperty("journalInventory").objectReferenceValue)
                return ValidateInventoryControls();
            const string backup = "Logs/DemonCheckpoints/task024-before-inventory-controls-20261111";
            Directory.CreateDirectory(backup);
            EditorSceneManager.SaveScene(scene, backup + "/live-before-grid-" + DateTime.UtcNow.ToString("yyyyMMddHHmmss") + ".unity", true);
            var saved = new SerializedObject(flow);
            var bag = (GameObject)saved.FindProperty("journalPanel").objectReferenceValue;
            var body = (Text)saved.FindProperty("journalText").objectReferenceValue;
            if (!bag.GetComponent<JournalInventory>())
            {
                var font = body.font;
                var reader = body.GetComponentInParent<ScrollRect>(true); var readRect = (RectTransform)reader.transform;
                Undo.RecordObject(readRect, "Make room for inventory grid");
                readRect.anchorMin = readRect.anchorMax = new Vector2(.5f, .5f);
                readRect.anchoredPosition = new Vector2(325, -20); readRect.sizeDelta = new Vector2(390, 420);
                body.rectTransform.sizeDelta = new Vector2(-24, 420); body.fontSize = 23;
                body.text = "点击左侧格子查看内容。";
                var heading = Label("SelectedItem_当前物品", bag.transform, new Vector2(.5f, .5f), new Vector2(325, 230), new Vector2(390, 45), font, 27);
                heading.text = "背包为空";
                Label("GridHint_格子说明", bag.transform, new Vector2(.5f, .5f), new Vector2(-210, 230), new Vector2(620, 45), font, 21).text = "收集物品　·　点击格子查看";
                var view = Rect("GridViewport_格子滚动区", bag.transform, new Vector2(-210, -20), new Vector2(620, 420));
                var image = view.gameObject.AddComponent<Image>(); image.color = new Color(.035f, .05f, .06f);
                view.gameObject.AddComponent<RectMask2D>(); var scroll = view.gameObject.AddComponent<ScrollRect>();
                scroll.horizontal = false; scroll.movementType = ScrollRect.MovementType.Clamped; scroll.viewport = view;
                var content = Rect("GridContent_格子布局", view, Vector2.zero, new Vector2(0, 450));
                content.anchorMin = new Vector2(0, 1); content.anchorMax = Vector2.one; content.pivot = new Vector2(.5f, 1);
                var grid = content.gameObject.AddComponent<GridLayoutGroup>(); grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                grid.constraintCount = 6; grid.cellSize = new Vector2(94, 94); grid.spacing = new Vector2(8, 8);
                grid.padding = new RectOffset(8, 8, 8, 8); grid.childAlignment = TextAnchor.UpperLeft;
                content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                scroll.content = content;
                var cells = new JournalSlot[24];
                for (int i = 0; i < cells.Length; i++) cells[i] = InventoryCell("Slot_" + (i + 1).ToString("00"), content, font);
                var template = InventoryCell("SlotTemplate_溢出格模板", bag.transform, font); template.gameObject.SetActive(false);
                var inventory = Undo.AddComponent<JournalInventory>(bag);
                inventory.Configure(content, template, cells, heading, body, scroll, reader);
                saved.FindProperty("journalInventory").objectReferenceValue = inventory; saved.ApplyModifiedProperties();
                var close = (Button)saved.FindProperty("journalClose").objectReferenceValue;
                close.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -285);
                Undo.RegisterCreatedObjectUndo(view.gameObject, "Create saved inventory grid");
            }
            var finale = new SerializedObject(world.Finale); var hand = finale.FindProperty("secondHand").objectReferenceValue as Transform;
            finale.FindProperty("secondHand").objectReferenceValue = null; finale.ApplyModifiedProperties();
            if (hand) Undo.DestroyObjectImmediate(hand.gameObject);
            // These assets are main-route-owned. Legacy prototype data retains its hold input wording.
            const string beamPath = Data + "/MainRouteBeam.asset";
            var beam = AssetDatabase.LoadAssetAtPath<BeamDefinition>(beamPath);
            if (!beam)
            { beam = UnityEngine.Object.Instantiate(world.Tether.Definition); beam.name = "MainRouteBeam"; AssetDatabase.CreateAsset(beam, beamPath); }
            beam.ReadyText = "T06 · 点击鼠标左键打开手电筒，稳定照住雪痕区域。\n再次点击关闭；移开、遮挡或超距后恢复运动。";
            beam.LockedText = "运动已交换：雪痕停止，风向标转动。\n保持照射；关闭手电、转身或进入近景会失锁。";
            EditorUtility.SetDirty(beam);
            var tether = new SerializedObject(world.Tether); tether.FindProperty("definition").objectReferenceValue = beam; tether.ApplyModifiedProperties();
            world.Settings.Instructions = "WASD 移动　Shift 奔跑　E 交互\n左键 点击开关手电 / T06牵制　Tab 背包　F8 调试\n拼板：鼠标点选　Enter 确认　Esc 返回";
            flow.Definition.Entry("flashlight").Body = "点击鼠标左键打开手电筒，再次点击关闭，无电池消耗。取得T06后，打开手电并稳定照射雪痕区域可牵制运动。背包和谜题中的点击不会操作手电。";
            EditorUtility.SetDirty(world.Settings); EditorUtility.SetDirty(flow.Definition);
            UpdateMainInputText(world.Exploration.Definition);
            UpdateMainInputText(world.Campaign.Definition);
            var binding = UnityEngine.Object.FindAnyObjectByType<SceneFontBinding>(); var fonts = new SerializedObject(binding);
            var labels = fonts.FindProperty("overlayLabels");
            var existing = Enumerable.Range(0, labels.arraySize).Select(i => labels.GetArrayElementAtIndex(i).objectReferenceValue).ToHashSet();
            foreach (var label in bag.GetComponentsInChildren<Text>(true))
                if (existing.Add(label)) { int at = labels.arraySize; labels.InsertArrayElementAtIndex(at); labels.GetArrayElementAtIndex(at).objectReferenceValue = label; }
            fonts.ApplyModifiedProperties();
            AssetDatabase.SaveAssets(); EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            return ValidateInventoryControls();
        }
        static void UpdateMainInputText(UnityEngine.Object asset)
        {
            if (!AssetDatabase.GetAssetPath(asset).StartsWith(Data, StringComparison.Ordinal)) throw new InvalidOperationException("Do not rewrite shared legacy instruction assets.");
            var data = new SerializedObject(asset); var property = data.GetIterator();
            while (property.NextVisible(true))
                if (property.propertyType == SerializedPropertyType.String)
                    property.stringValue = property.stringValue.Replace("按住鼠标右键", "点击左键打开手电并").Replace("按住右键", "点击左键打开手电并").Replace("松开右键", "再次点击左键关闭手电").Replace("再松开", "再关闭手电");
            data.ApplyModifiedProperties(); EditorUtility.SetDirty(asset);
        }
        static JournalSlot InventoryCell(string name, Transform parent, Font font)
        {
            var rect = Rect(name, parent, Vector2.zero, new Vector2(94, 98));
            var image = rect.gameObject.AddComponent<Image>(); image.color = new Color(.11f, .15f, .18f);
            var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = image;
            var colors = button.colors; colors.disabledColor = Color.white; button.colors = colors;
            var border = rect.gameObject.AddComponent<Outline>(); border.effectColor = new Color(.32f, .42f, .5f, .8f); border.effectDistance = new Vector2(1, -1);
            var title = Label("ItemName_物品名称", rect, new Vector2(.5f, .5f), new Vector2(0, -10), new Vector2(84, 58), font, 17); title.text = "空格";
            var kind = Label("ItemKind_物品类型", rect, new Vector2(.5f, .5f), new Vector2(0, 33), new Vector2(82, 24), font, 16); kind.text = "";
            var highlight = Rect("Selection_选中标记", rect, new Vector2(0, -43), new Vector2(76, 3)).gameObject.AddComponent<Image>();
            highlight.color = new Color(.9f, .72f, .37f); highlight.raycastTarget = false; highlight.enabled = false;
            var slot = rect.gameObject.AddComponent<JournalSlot>(); slot.Configure(button, title, kind, highlight); return slot;
        }
        public static string ValidateInventoryControls()
        {
            var world = Current(); var flow = world.Playthrough; var saved = new SerializedObject(flow);
            var inventory = saved.FindProperty("journalInventory").objectReferenceValue as JournalInventory;
            if (!inventory || inventory.AuthoredSlots.Count < 24) throw new InvalidOperationException("Saved inventory grid or cells missing.");
            if (new SerializedObject(world.Finale).FindProperty("secondHand").objectReferenceValue) throw new InvalidOperationException("Clock still references a second hand.");
            if (world.gameObject.scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<Transform>(true)).Any(x => x.name == "SecondHand")) throw new InvalidOperationException("Second hand object remains.");
            if (!AssetDatabase.GetAssetPath(world.Tether.Definition).StartsWith(Data)) throw new InvalidOperationException("Main-route beam instructions must be independent.");
            return "Saved 24-slot expandable inventory, reader, main-route click controls and two-hand clock verified. RealSecondsPerDay=" + world.Settings.RealSecondsPerDay;
        }
    }
}
