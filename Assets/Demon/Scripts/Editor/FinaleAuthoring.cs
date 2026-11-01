using System;
using System.IO;
using System.Linq;
using Demon.Slice.Domain;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Demon.Slice.Editor
{
    public static class FinaleAuthoring
    {
        const string Marker = "Finale_Authored";
        static readonly string[] Scenes = { DemonSliceAuthoring.ScenePath, NineDayAuthoring.ScenePath };

        [MenuItem("Demon/Nine Days/Migrate To Midnight Finale")]
        public static string Migrate()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || SceneManager.GetActiveScene().isDirty)
                throw new InvalidOperationException("Leave Play and save current edits before migration.");
            foreach (string path in Scenes)
            {
                EditorSceneManager.OpenScene(path);
                var existing = UnityEngine.Object.FindAnyObjectByType<SliceSceneController>();
                if (!existing || !existing.Settings || !existing.Tablet)
                    throw new InvalidOperationException("Missing existing composition: " + path);
                if (existing.Finale || GameObject.Find(Marker))
                    throw new InvalidOperationException("Finale already exists; edit saved content instead of regenerating: " + path);
                if (!existing.Tablet.transform.Find("FixedLine_6") || !existing.Tablet.transform.Find("FixedLine_7"))
                    throw new InvalidOperationException("Unexpected legacy tablet; inspect before changing: " + path);
            }
            EnsureSound("Assets/Demon/Audio/PrototypeClockTick.wav", false);
            EnsureSound("Assets/Demon/Audio/PrototypeBoxSlam.wav", true);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            foreach (string path in Scenes)
            {
                EditorSceneManager.OpenScene(path);
                Upgrade(UnityEngine.Object.FindAnyObjectByType<SliceSceneController>());
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
                AssetDatabase.SaveAssets();
                EditorSceneManager.OpenScene(path);
                Validate();
            }
            EditorSceneManager.OpenScene(NineDayAuthoring.ScenePath);
            return "Migrated both saved scenes/settings; direct return line, authored clock/views, placeholder sounds and D8 night gate reloaded and verified.";
        }

        static void Upgrade(SliceSceneController world)
        {
            var data = world.Settings;
            data.Target[5] = RuneKind.None; data.Initial[5] = RuneKind.None;
            data.LooseRunes = data.LooseRunes.Where(r => (int)r != 5).ToArray();
            var defaults = ScriptableObject.CreateInstance<SliceSettings>();
            data.BoxText = defaults.BoxText;
            UnityEngine.Object.DestroyImmediate(defaults);
            if (!world.Campaign) data.StartHour = 22; // D8 quick scene previews the new night route.
            EditorUtility.SetDirty(data);

            Transform tablet = world.Tablet.transform;
            var slots = world.Tablet.GetComponentsInChildren<SliceRuneSlot>(true);
            var unused = slots.Single(s => s.Index == 5);
            unused.gameObject.SetActive(false); unused.Label.gameObject.SetActive(false);
            Record(unused.gameObject); Record(unused.Label.gameObject);
            Transform direct = tablet.Find("FixedLine_6");
            Vector3 a = slots.Single(s => s.Index == 8).transform.localPosition;
            Vector3 b = slots.Single(s => s.Index == 2).transform.localPosition;
            direct.localPosition = (a + b) / 2 + Vector3.forward * .012f;
            direct.localScale = new Vector3(.035f, Vector3.Distance(a, b), .025f);
            direct.localRotation = Quaternion.FromToRotation(Vector3.up, b - a); Record(direct);
            tablet.Find("FixedLine_7").gameObject.SetActive(false); Record(tablet.Find("FixedLine_7").gameObject);
            foreach (var slot in slots) { slot.Label.text = SliceTablet.RuneLabel(data.Initial[slot.Index]); Record(slot.Label); }

            var root = new GameObject(Marker).transform;
            var faceRoot = new GameObject("WallClock").transform;
            faceRoot.SetParent(root, false); faceRoot.position = new Vector3(-2.5f, 2.15f, 2.55f);
            var dark = Material("Figure");
            var paper = Material("Paper");
            var face = Primitive("ClockFace", PrimitiveType.Cylinder, faceRoot, Vector3.zero, new Vector3(1.15f, .035f, 1.15f), paper);
            face.transform.localRotation = Quaternion.Euler(90, 0, 0);
            for (int i = 0; i < 12; i++)
            {
                float angle = i * Mathf.PI / 6;
                var mark = Primitive("HourMark_" + i, PrimitiveType.Cube, faceRoot,
                    new Vector3(Mathf.Sin(angle) * .48f, Mathf.Cos(angle) * .48f, -.05f), new Vector3(.025f, .065f, .02f), dark);
                mark.transform.localRotation = Quaternion.Euler(0, 0, -i * 30);
            }
            var font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Demon/UI/WindowsChinese.fontsettings");
            Label("12", faceRoot, new Vector3(0, .36f, -.065f), font);
            Label("3", faceRoot, new Vector3(.36f, 0, -.065f), font);
            Label("6", faceRoot, new Vector3(0, -.36f, -.065f), font);
            Label("9", faceRoot, new Vector3(-.36f, 0, -.065f), font);
            Transform hours = Hand("HourHand", faceRoot, .25f, .045f, -.09f, dark);
            Transform minutes = Hand("MinuteHand", faceRoot, .39f, .025f, -.12f, dark);
            Transform seconds = Hand("SecondHand", faceRoot, .43f, .012f, -.15f, Material("Eyes"));
            Transform view = new GameObject("ClockView").transform;
            view.SetParent(root, false); view.position = faceRoot.position + new Vector3(0, 0, -1.75f);
            view.LookAt(faceRoot.position);
            var clockLight = new GameObject("ClockLamp").AddComponent<Light>();
            clockLight.transform.SetParent(root, false); clockLight.transform.position = faceRoot.position + new Vector3(0, .25f, -.65f);
            clockLight.type = LightType.Point; clockLight.range = 3; clockLight.intensity = .5f;

            var glow = new GameObject("LakeOpenGlow"); glow.transform.SetParent(root, false);
            glow.transform.position = new Vector3(0, .05f, 9);
            Primitive("OpenLightDisc", PrimitiveType.Sphere, glow.transform, Vector3.zero, new Vector3(2, .045f, 2), Material("SealGlow"));
            var lakeLight = glow.AddComponent<Light>(); lakeLight.type = LightType.Point;
            lakeLight.range = 10; lakeLight.intensity = 4; lakeLight.color = new Color(.25f, .8f, 1);
            var pulse = new GameObject("TabletMidnightPulse").AddComponent<Light>();
            pulse.transform.SetParent(root, false); pulse.transform.position = tablet.position + Vector3.back * .5f;
            pulse.type = LightType.Point; pulse.color = new Color(1, .45f, .12f); pulse.range = 5; pulse.intensity = 0;
            var audio = root.gameObject.AddComponent<AudioSource>(); audio.playOnAwake = false; audio.spatialBlend = 0; audio.volume = .6f;
            var sequence = Undo.AddComponent<SliceFinaleSequence>(world.gameObject);
            sequence.Configure(view, hours, minutes, seconds, glow, pulse, audio,
                AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Demon/Audio/PrototypeClockTick.wav"),
                AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Demon/Audio/PrototypeBoxSlam.wav"));
            world.ConfigureFinale(sequence); EditorUtility.SetDirty(world); EditorUtility.SetDirty(sequence);
            var hud = UnityEngine.Object.FindAnyObjectByType<SliceHud>();
            hud.GetComponent<SliceFontBinding>().Configure(font, hud.GetComponentsInChildren<Text>(true),
                UnityEngine.Object.FindObjectsByType<TextMesh>(FindObjectsInactive.Include));
            EditorUtility.SetDirty(hud.GetComponent<SliceFontBinding>());
            if (world.Campaign) UpgradeSchedule(world.Campaign.Definition);
        }

        static void UpgradeSchedule(NineDayDefinition data)
        {
            data.FinalTabletHour = 22;
            string[] titles = { "值守开始", "发现预告", "预告成真", "观察与推理", "声音的异变", "监听工具", "永夜与运动交换", "等待落空", "第九天" };
            string[] objectives = {
                "熟悉湖岸、箱子B与左侧睡袋。第一夜开封印仍为状态占位。",
                "右侧出现T01，记录第三天的运动转移预告。",
                "T01运动日期已到，发现T02声音交换记录；实际能力与追逐将在后续制作。",
                "右侧出现T03，调查它记录的第七天变化。",
                "T02声音日期已到，发现T04：第九天赋予实体。实际声音效果待接入。",
                "搜寻T05监听工具记录。新版不再跳日；风铃补全与区域监听待接入。",
                "T03光日期已到，学习T06运动交换与光束牵制。密码箱A、永夜和手电操作待接入。",
                "接班未到。T07将在22:00开放；当前可睡到D9，或等待夜间修复并打开B。快速验证可用DemonSlice场景。",
                "第九天。" };
            string[] records = {
                "【T01 · 封印一 · 已固化】\nIF DAY == 3\nTRANSFER(MOTION, WIND_VANE, DEMON)\n\n第三天零点，风向标的运动转移给恶魔。\n发现或阅读不会激活、阻止既定程序。\n\n当前灰盒仅记录语句，实际设施与追逐待制作。",
                "【T02 · 封印二 · 已固化】\nIF DAY == 5\nEXCHANGE(SOUND, RADIO, DEMON)\n\n第五天零点，电台与恶魔交换声音。\n交换两次会恢复原状态。\n\n实际声音演出待制作。",
                "【T03 · 封印三 · 已固化】\nIF DAY == 7\nTRANSFER(LIGHT, SUN, DEMON)\n\n第七天零点，太阳的光转移给恶魔。\n记录内容为设计说明，正式符文与永夜演出待制作。",
                "【T04 · 封印四 · 已固化】\nIF DAY == 9\n\n第九天零点，投影获得实体。\n函数精确签名与实体来源待定，当前规则已接入这一日期状态。",
                "【T05 · 监听工具】\n第六天通过正常搜寻取得，补全后启用风铃，不推进日期。\n\n恶魔进入监测区域响一次，离开后重新监听。\n沉默不等于安全。\n\n当前只读占位，补全和风铃交互待制作。",
                "【T06 · 运动交换与光束牵制】\n设计取得方式：箱子A，符号依次为电台、太阳、风向标；对照数字轮盘的固定方位。\n\n稳定照射：交换恶魔与风向标的运动。\n失去锁定：再次交换，恢复。\n转身、遮挡或超距会失锁；拾取需要短暂移开光束。\n\n当前仅记录，密码箱和手电交互待制作。" };
            for (int i = 0; i < 9; i++) { data.Days[i].Title = titles[i]; data.Days[i].Objective = objectives[i]; }
            for (int i = 0; i < 6; i++) data.Tablets[i].Record = records[i];
            EditorUtility.SetDirty(data);
        }

        public static string Validate()
        {
            var world = UnityEngine.Object.FindAnyObjectByType<SliceSceneController>();
            if (!world || !world.Finale || world.Finale.ClockView == null || world.Settings.Target[5] != RuneKind.None ||
                world.Settings.Initial[5] != RuneKind.None || world.Settings.BoxText.Contains("+1"))
                throw new InvalidOperationException("Missing finale or legacy time rune/text remains.");
            if (world.Tablet.transform.Find("FixedLine_7").gameObject.activeSelf ||
                world.Tablet.GetComponentsInChildren<SliceRuneSlot>(true).Single(s => s.Index == 5).gameObject.activeSelf)
                throw new InvalidOperationException("Legacy return/unused slot still visible.");
            foreach (var component in UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include))
            {
                var property = new SerializedObject(component).GetIterator();
                while (property.NextVisible(true))
                    if (property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceEntityIdValue != default && !property.objectReferenceValue)
                        throw new InvalidOperationException("Missing serialized reference: " + component.name + "/" + property.propertyPath);
            }
            return "Saved no+1 layout, direct return, finale references and text verified after reload.";
        }

        public static string RepairClockSightline()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || SceneManager.GetActiveScene().isDirty)
                throw new InvalidOperationException("Save current scene outside Play first.");
            foreach (string path in Scenes)
            {
                EditorSceneManager.OpenScene(path);
                var world = UnityEngine.Object.FindAnyObjectByType<SliceSceneController>();
                Validate();
                var root = GameObject.Find(Marker).transform;
                Transform clock = root.Find("WallClock");
                foreach (Transform target in new[] { clock, world.Finale.ClockView, root.Find("ClockLamp") })
                {
                    if (Mathf.Abs(target.position.x + 4.1f) > .001f && Mathf.Abs(target.position.x + 2.5f) > .001f)
                        throw new InvalidOperationException("Clock was moved by an author; preserve and inspect: " + target.name);
                    Vector3 p = target.position; p.x = -2.5f; target.position = p;
                }
                root.Find("ClockLamp").GetComponent<Light>().intensity = .5f;
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            }
            EditorSceneManager.OpenScene(NineDayAuthoring.ScenePath);
            return Validate() + " Clock moved clear of the open lid in both scenes.";
        }

        static Material Material(string name) => AssetDatabase.LoadAssetAtPath<Material>("Assets/Demon/Art/Materials/" + name + ".mat");
        static void Record(UnityEngine.Object target) { PrefabUtility.RecordPrefabInstancePropertyModifications(target); EditorUtility.SetDirty(target); }
        static GameObject Primitive(string name, PrimitiveType type, Transform parent, Vector3 position, Vector3 scale, Material material)
        {
            var go = GameObject.CreatePrimitive(type); go.name = name; go.transform.SetParent(parent, false);
            go.transform.localPosition = position; go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = material;
            UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>()); return go;
        }
        static Transform Hand(string name, Transform parent, float length, float width, float z, Material material)
        {
            var pivot = new GameObject(name).transform; pivot.SetParent(parent, false); pivot.localPosition = new Vector3(0, 0, z);
            Primitive("Hand", PrimitiveType.Cube, pivot, Vector3.up * length / 2, new Vector3(width, length, .015f), material);
            return pivot;
        }
        static void Label(string text, Transform parent, Vector3 position, Font font)
        {
            var mesh = new GameObject("Number_" + text).AddComponent<TextMesh>(); mesh.transform.SetParent(parent, false);
            mesh.transform.localPosition = position; mesh.font = font; mesh.text = text; mesh.fontSize = 48;
            mesh.characterSize = .018f; mesh.anchor = TextAnchor.MiddleCenter; mesh.color = Color.black;
        }

        // Short, synthetic placeholder cues saved as ordinary editable/imported WAV assets.
        static void EnsureSound(string path, bool slam)
        {
            if (File.Exists(path)) return;
            const int rate = 22050;
            int count = (int)(rate * (slam ? .35f : .08f));
            using var output = new BinaryWriter(File.Create(path));
            output.Write(System.Text.Encoding.ASCII.GetBytes("RIFF")); output.Write(36 + count * 2);
            output.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt ")); output.Write(16);
            output.Write((short)1); output.Write((short)1); output.Write(rate); output.Write(rate * 2);
            output.Write((short)2); output.Write((short)16);
            output.Write(System.Text.Encoding.ASCII.GetBytes("data")); output.Write(count * 2);
            var noise = new System.Random(slam ? 21 : 7);
            for (int i = 0; i < count; i++)
            {
                double t = i / (double)rate;
                double value = (Math.Sin(2 * Math.PI * (slam ? 65 : 1100) * t) * .6 + (noise.NextDouble() * 2 - 1) * .4) *
                    Math.Exp(-t * (slam ? 18 : 100));
                output.Write((short)(value * 18000));
            }
        }
    }
}
