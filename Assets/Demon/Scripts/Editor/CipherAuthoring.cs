using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace Demon.Slice.Editor
{
    public static class CipherAuthoring
    {
        public const string ScenePath = "Assets/Demon/Scenes/DemonCipher.unity";
        const string DefinitionPath = "Assets/Demon/Data/CipherDefinition.asset";
        const string SettingsPath = "Assets/Demon/Data/CipherSettings.asset";
        static Font font;
        static Material wood, stone, paper, metal;
        static void Editable()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || SceneManager.GetActiveScene().isDirty)
                throw new InvalidOperationException("Leave Play and save current scene first.");
        }
        public static string Create()
        {
            Editable();
            if (File.Exists(ScenePath) || File.Exists(DefinitionPath) || File.Exists(SettingsPath)) throw new InvalidOperationException("Cipher outputs exist; edit saved content rather than regenerate.");
            EditorSceneManager.OpenScene(NineDayAuthoring.ScenePath);
            var world = UnityEngine.Object.FindAnyObjectByType<SliceSceneController>();
            if (world.Cipher || GameObject.Find("Cipher_Authored")) throw new InvalidOperationException("Preserving existing cipher content.");
            var data = ScriptableObject.CreateInstance<CipherDefinition>(); AssetDatabase.CreateAsset(data, DefinitionPath);
            font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Demon/UI/WindowsChinese.fontsettings");
            wood = Mat("BoxWood"); stone = Mat("Stone"); paper = Mat("Paper"); metal = Mat("RuneSlot");
            var root = new GameObject("Cipher_Authored").transform;
            var wheelRoot = new GameObject("FixedBearingWheel").transform; wheelRoot.SetParent(root, false); wheelRoot.position = new Vector3(-7, .85f, -4);
            var wheel = wheelRoot.gameObject.AddComponent<CipherWheel>();
            Primitive("WheelStand", wheelRoot, new Vector3(0, -.45f, 0), new Vector3(.15f, .9f, .15f), wood);
            Primitive("WheelFace", wheelRoot, Vector3.zero, new Vector3(2.5f, .035f, 2.5f), stone, true, PrimitiveType.Cylinder);
            var digits = new TextMesh[10];
            for (int i = 0; i < 10; i++)
            {
                float angle = i * Mathf.PI * 2 / 10;
                Vector3 pos = new Vector3(Mathf.Sin(angle), .05f, Mathf.Cos(angle));
                digits[i] = Label("DirectionDigit_" + i, wheelRoot, pos, data.DigitsClockwise[i].ToString(), .024f);
                digits[i].transform.localRotation = Quaternion.Euler(90, 0, 0);
                var tick = Primitive("SectorMark_" + i, wheelRoot, new Vector3(Mathf.Sin(angle) * 1.18f, .04f, Mathf.Cos(angle) * 1.18f), new Vector3(.03f, .015f, .12f), paper, false); tick.transform.localRotation = Quaternion.Euler(0, i * 36, 0);
            }
            Primitive("NorthArrow", wheelRoot, new Vector3(0, .05f, .4f), new Vector3(.05f, .025f, .65f), paper, false);
            var north = Label("FixedNorth", wheelRoot, new Vector3(0, .07f, 0), "北 ↑", .02f); north.transform.localRotation = Quaternion.Euler(90, 0, 0);
            var wheelView = Anchor("CloseView", wheelRoot, new Vector3(0, 3.2f, -2), wheelRoot.position);
            var landmarks = new Transform[3];
            landmarks[0] = new GameObject("Radio_FixedPosition").transform; landmarks[0].SetParent(root, false); landmarks[0].position = wheelRoot.position + new Vector3(Mathf.Sin(324 * Mathf.Deg2Rad) * 5, -.85f, Mathf.Cos(324 * Mathf.Deg2Rad) * 5);
            Primitive("RadioBody", landmarks[0], new Vector3(0, 1, 0), new Vector3(1.1f, .65f, .45f), wood);
            Primitive("Antenna", landmarks[0], new Vector3(.4f, 1.8f, 0), new Vector3(.025f, 1.2f, .025f), metal, false);
            Primitive("Speaker", landmarks[0], new Vector3(-.15f, 1, -.24f), new Vector3(.5f, .4f, .025f), metal, false);
            Label("RadioSymbol", landmarks[0], new Vector3(0, 1.5f, -.25f), "电台", .026f);
            landmarks[1] = new GameObject("Sun_FixedPositionPlaceholder").transform; landmarks[1].SetParent(root, false); landmarks[1].position = wheelRoot.position + new Vector3(0, -.85f, 8);
            Primitive("SolarReferenceDisk", landmarks[1], new Vector3(0, 4.2f, 0), Vector3.one * .95f, paper, false, PrimitiveType.Sphere);
            for (int i = 0; i < 8; i++)
            {
                float angle = i * Mathf.PI / 4;
                var ray = Primitive("SolarRay_" + i, landmarks[1], new Vector3(Mathf.Sin(angle) * .8f, 4.2f + Mathf.Cos(angle) * .8f, 0), new Vector3(.08f, .3f, .08f), paper, false); ray.transform.localRotation = Quaternion.Euler(0, 0, -i * 45);
            }
            Label("SunSymbol", landmarks[1], new Vector3(0, 3.2f, -.2f), "太阳", .027f);
            landmarks[2] = new GameObject("WindVane_FixedPosition").transform; landmarks[2].SetParent(root, false); landmarks[2].position = wheelRoot.position + new Vector3(Mathf.Sin(72 * Mathf.Deg2Rad) * 8, -.85f, Mathf.Cos(72 * Mathf.Deg2Rad) * 8);
            Primitive("Post", landmarks[2], new Vector3(0, 1.5f, 0), new Vector3(.09f, 3, .09f), wood);
            var blade = Primitive("RotatingBlade_OrientationIsNotCode", landmarks[2], new Vector3(0, 2.8f, 0), new Vector3(1.35f, .12f, .15f), metal, false);
            Primitive("Tail", blade.transform, new Vector3(.4f, .6f, 0), new Vector3(.3f, 1.5f, .9f), paper, false);
            Label("VaneSymbol", landmarks[2], new Vector3(0, 2.2f, -.2f), "风向标", .026f);
            wheel.Configure(AssetDatabase.LoadAssetAtPath<CipherDefinition>(DefinitionPath), landmarks, wheelView, digits);
            var boxRoot = new GameObject("BoxA").transform; boxRoot.SetParent(root, false); boxRoot.position = new Vector3(-8, 0, -.3f);
            var box = boxRoot.gameObject.AddComponent<CipherBox>();
            Primitive("Body", boxRoot, new Vector3(0, .45f, 0), new Vector3(1.85f, .9f, 1.25f), wood);
            var lid = new GameObject("LidPivot").transform; lid.SetParent(boxRoot, false); lid.localPosition = new Vector3(0, .98f, .6f);
            Primitive("Lid", lid, new Vector3(0, 0, -.6f), new Vector3(1.95f, .12f, 1.35f), wood);
            Primitive("LockPlate", boxRoot, new Vector3(0, 1.15f, -.7f), new Vector3(1.9f, .6f, .12f), stone);
            Primitive("SymbolPanel", boxRoot, new Vector3(0, 1.7f, -.65f), new Vector3(2.1f, .45f, .1f), stone);
            Label("SymbolsInOrder", boxRoot, new Vector3(0, 1.7f, -.72f), "电台   太阳   风向标", .014f);
            var buttons = new CipherDigit[3];
            for (int i = 0; i < 3; i++)
            {
                var button = Primitive("Digit_" + i, boxRoot, new Vector3((i - 1) * .58f, 1.15f, -.81f), new Vector3(.46f, .46f, .12f), metal);
                buttons[i] = button.AddComponent<CipherDigit>(); buttons[i].Index = i;
                buttons[i].Label = Label("DigitLabel_" + i, button.transform, new Vector3(0, 0, -.65f), "0", .06f);
            }
            var tablet = Primitive("T06_Inside", boxRoot, new Vector3(0, .94f, 0), new Vector3(.9f, .035f, .6f), stone, false);
            var t06 = Label("T06_Label", tablet.transform, new Vector3(0, .7f, 0), "T06", .025f); t06.transform.localRotation = Quaternion.Euler(90, 0, 0);
            var lockView = Anchor("LockView", boxRoot, new Vector3(0, 1.3f, -2.6f), boxRoot.position + new Vector3(0, 1.2f, -.6f));
            var readView = Anchor("ReadView", boxRoot, new Vector3(0, 1.9f, -1.7f), boxRoot.position + Vector3.up * .8f);
            box.Configure(wheel, lockView, readView, buttons, lid, tablet); world.ConfigureCipher(box);
            var oldT06 = UnityEngine.Object.FindObjectsByType<NineDayTablet>(FindObjectsInactive.Include).Single(t => t.Index == 5); oldT06.gameObject.SetActive(false);
            var plan = world.Campaign.Definition;
            plan.Days[0].Objective = "熟悉湖岸、箱子与睡袋。西侧箱子A有三位机械锁，旁边轮盘可观察方位；箱子B暂时打不开。";
            plan.Days[6].Objective = "对照电台、太阳、风向标的固定方位，按箱面符号顺序输入密码，解开箱子A取得T06；也可提前探索。";
            plan.Tablets[5].Record = "【T06 · 已固化运动交换工具】\n从箱子A取得。\n稳定光束锁定恶魔区域时，交换恶魔与风向标的运动；失锁再交换一次复位。\n脚印停止时风向标转动；移开光束后恢复。\n\n风铃辨认所在区域，稳定光束争取行动时间；两种工具用途不同。";
            EditorUtility.SetDirty(plan); EditorUtility.SetDirty(world); EditorUtility.SetDirty(box);
            var binding = UnityEngine.Object.FindAnyObjectByType<SliceHud>().GetComponent<SliceFontBinding>(); binding.Configure(font, binding.GetComponentsInChildren<Text>(true), UnityEngine.Object.FindObjectsByType<TextMesh>(FindObjectsInactive.Include)); EditorUtility.SetDirty(binding);
            Save();
            var settings = UnityEngine.Object.Instantiate(world.Settings); settings.name = "CipherSettings"; settings.StartDay = 1; settings.StartHour = 10; settings.InitialLakeSealed = true; AssetDatabase.CreateAsset(settings, SettingsPath); AssetDatabase.SaveAssets();
            if (!AssetDatabase.CopyAsset(NineDayAuthoring.ScenePath, ScenePath)) throw new InvalidOperationException("Could not copy cipher quick scene.");
            EditorSceneManager.OpenScene(ScenePath); world = UnityEngine.Object.FindAnyObjectByType<SliceSceneController>();
            var serialized = new SerializedObject(world); serialized.FindProperty("settings").objectReferenceValue = AssetDatabase.LoadAssetAtPath<SliceSettings>(SettingsPath); serialized.ApplyModifiedProperties();
            var body = world.Player.GetComponent<CharacterController>(); body.enabled = false; world.Player.transform.SetPositionAndRotation(new Vector3(-8, .04f, -3.5f), Quaternion.identity); body.enabled = true;
            PrefabUtility.RecordPrefabInstancePropertyModifications(world.Player.transform); Save(); EditorSceneManager.OpenScene(ScenePath); return Validate();
        }
        static void Save() { EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); EditorSceneManager.SaveScene(SceneManager.GetActiveScene()); AssetDatabase.SaveAssets(); }
        static Material Mat(string name) => AssetDatabase.LoadAssetAtPath<Material>("Assets/Demon/Art/Materials/" + name + ".mat");
        static GameObject Primitive(string name, Transform parent, Vector3 pos, Vector3 scale, Material mat, bool collider = true, PrimitiveType kind = PrimitiveType.Cube)
        {
            var go = GameObject.CreatePrimitive(kind); go.name = name; go.transform.SetParent(parent, false); go.transform.localPosition = pos; go.transform.localScale = scale; go.GetComponent<Renderer>().sharedMaterial = mat;
            if (!collider) UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>()); return go;
        }
        static TextMesh Label(string name, Transform parent, Vector3 pos, string text, float size)
        {
            var label = new GameObject(name).AddComponent<TextMesh>(); label.transform.SetParent(parent, false); label.transform.localPosition = pos; label.font = font; label.fontSize = 64; label.characterSize = size; label.anchor = TextAnchor.MiddleCenter; label.alignment = TextAlignment.Center; label.text = text; return label;
        }
        static Transform Anchor(string name, Transform parent, Vector3 pos, Vector3 look)
        { var go = new GameObject(name).transform; go.SetParent(parent, false); go.localPosition = pos; go.LookAt(look); return go; }
        public static void OpenQuick() { Editable(); EditorSceneManager.OpenScene(ScenePath); }
        public static string Validate()
        {
            var world = UnityEngine.Object.FindAnyObjectByType<SliceSceneController>();
            if (!world || !world.Cipher || !world.Cipher.Wheel || !world.Cipher.Wheel.Definition || !world.Cipher.CloseView) throw new InvalidOperationException("Missing saved cipher composition.");
            if (world.Cipher.GetComponentsInChildren<CipherDigit>(true).Length != 3 || world.Cipher.Wheel.ReadCode().Length != 3) throw new InvalidOperationException("Saved cipher digits/landmarks incomplete.");
            if (UnityEngine.Object.FindObjectsByType<NineDayTablet>(FindObjectsInactive.Include).Length != 6) throw new InvalidOperationException("Old six tablet identities were lost.");
            return "Saved box A, fixed wheel/three landmarks, three digits and independent acquisition composition verified.";
        }
        const string Tune = "Demon.Cipher.Restore";
        public static void BeginTuning()
        {
            var box = UnityEngine.Object.FindAnyObjectByType<SliceSceneController>().Cipher;
            SessionState.SetString(Tune + "digits", string.Join(",", box.Wheel.Definition.DigitsClockwise)); SessionState.SetVector3(Tune + "view", box.CloseView.localPosition); SessionState.SetBool(Tune, true);
            var data = box.Wheel.Definition; int old = data.DigitsClockwise[9]; data.DigitsClockwise[9] = data.DigitsClockwise[8]; data.DigitsClockwise[8] = old; EditorUtility.SetDirty(data);
            box.CloseView.localPosition += Vector3.right * .08f; Save();
        }
        public static void RestoreTuning()
        {
            if (!SessionState.GetBool(Tune, false)) return;
            var box = UnityEngine.Object.FindAnyObjectByType<SliceSceneController>().Cipher;
            box.Wheel.Definition.DigitsClockwise = SessionState.GetString(Tune + "digits", "").Split(',').Select(int.Parse).ToArray(); EditorUtility.SetDirty(box.Wheel.Definition);
            box.CloseView.localPosition = SessionState.GetVector3(Tune + "view", Vector3.zero); Save(); SessionState.SetBool(Tune, false); Debug.Log("DEMON_CIPHER_TUNING_RESTORED");
        }
        public static string Build()
        {
            Editable(); Directory.CreateDirectory("Builds/WindowsCipher");
            var result = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = new[] { ScenePath }, locationPathName = "Builds/WindowsCipher/DemonCipher.exe", target = BuildTarget.StandaloneWindows64, options = BuildOptions.Development });
            if (result.summary.result != BuildResult.Succeeded) throw new InvalidOperationException("Cipher Windows build failed."); return "Cipher Windows64 build succeeded, " + result.summary.totalSize + " bytes.";
        }
    }
}
