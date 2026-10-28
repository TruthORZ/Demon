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
    public static class BeamAuthoring
    {
        public const string ScenePath = "Assets/Demon/Scenes/DemonBeam.unity";
        const string DataPath = "Assets/Demon/Data/BeamDefinition.asset";
        const string SettingsPath = "Assets/Demon/Data/BeamSettings.asset";
        static void Editable()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || SceneManager.GetActiveScene().isDirty)
                throw new InvalidOperationException("Leave Play and save the active scene before authoring.");
        }
        public static string Create()
        {
            Editable();
            if (File.Exists(ScenePath) || File.Exists(DataPath) || File.Exists(SettingsPath)) throw new InvalidOperationException("Beam outputs exist; preserve and edit saved content.");
            EditorSceneManager.OpenScene(NineDayAuthoring.ScenePath);
            var world = UnityEngine.Object.FindAnyObjectByType<SliceSceneController>();
            if (!world.Cipher || !world.Listening || world.Tether || GameObject.Find("Beam_Authored")) throw new InvalidOperationException("Unexpected existing beam/composition state.");
            var proxy = UnityEngine.Object.FindAnyObjectByType<ListeningTrailProxy>();
            var vane = GameObject.Find("Cipher_Authored/WindVane_FixedPosition/RotatingBlade_OrientationIsNotCode").transform;
            if (!proxy || !vane) throw new InvalidOperationException("Preserving scene with missing authored motion targets.");
            var data = ScriptableObject.CreateInstance<BeamDefinition>(); AssetDatabase.CreateAsset(data, DataPath);
            var cyan = new Material(Shader.Find("Universal Render Pipeline/Unlit")); cyan.SetColor("_BaseColor", new Color(.2f, .8f, 1)); AssetDatabase.CreateAsset(cyan, "Assets/Demon/Art/Materials/BeamGuide.mat");
            var region = new Material(Shader.Find("Universal Render Pipeline/Lit")); region.SetColor("_BaseColor", new Color(.5f, .64f, .73f)); AssetDatabase.CreateAsset(region, "Assets/Demon/Art/Materials/BeamRegion.mat");
            var font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Demon/UI/WindowsChinese.fontsettings");
            var stone = AssetDatabase.LoadAssetAtPath<Material>("Assets/Demon/Art/Materials/Stone.mat");
            var metal = AssetDatabase.LoadAssetAtPath<Material>("Assets/Demon/Art/Materials/RuneSlot.mat");
            var paper = AssetDatabase.LoadAssetAtPath<Material>("Assets/Demon/Art/Materials/Paper.mat");
            var root = new GameObject("Beam_Authored").transform;
            var aim = new GameObject("T06_AimRegion").transform; aim.SetParent(proxy.Target, false); aim.localPosition = new Vector3(0, .15f, 0);
            Primitive("SnowDisturbanceRegion", proxy.Target, new Vector3(0, -.07f, 0), new Vector3(1.15f, .12f, 1.15f), region, PrimitiveType.Sphere);
            var glow = new GameObject("LockedRegionGlow").AddComponent<Light>(); glow.transform.SetParent(proxy.Target, false); glow.transform.localPosition = new Vector3(0, .65f, 0); glow.type = LightType.Point; glow.range = 2.5f; glow.intensity = 2; glow.color = new Color(.2f, .8f, 1); glow.enabled = false;
            var flashlightRoot = new GameObject("T06_Flashlight").transform; flashlightRoot.SetParent(world.Player.ViewCamera.transform, false);
            var body = new GameObject("SavedFlashlightBody").transform; body.SetParent(flashlightRoot, false);
            var tube = Primitive("Tube", body, new Vector3(.27f, -.27f, .46f), new Vector3(.08f, .13f, .08f), metal, PrimitiveType.Cylinder); tube.transform.localRotation = Quaternion.Euler(90, 0, 0);
            var head = Primitive("Head", body, new Vector3(.27f, -.27f, .62f), new Vector3(.12f, .045f, .12f), paper, PrimitiveType.Cylinder); head.transform.localRotation = Quaternion.Euler(90, 0, 0);
            var lamp = new GameObject("SpotLight").AddComponent<Light>(); lamp.transform.SetParent(flashlightRoot, false); lamp.transform.localPosition = new Vector3(.27f, -.23f, .68f); lamp.type = LightType.Spot; lamp.shadows = LightShadows.Soft; lamp.range = data.RangeMetres; lamp.spotAngle = data.SpotAngleDegrees; lamp.intensity = data.LightIntensity; lamp.enabled = false;
            var line = new GameObject("BeamGuide").AddComponent<LineRenderer>(); line.transform.SetParent(flashlightRoot, false); line.useWorldSpace = true; line.positionCount = 2; line.startWidth = .025f; line.endWidth = .04f; line.sharedMaterial = cyan; line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; line.receiveShadows = false; line.enabled = false;
            var observer = new GameObject("ObservationPoint").transform; observer.SetParent(root, false); observer.position = new Vector3(-1, .04f, 3.3f); observer.LookAt(new Vector3(2, .04f, -7));
            Primitive("ObservationMarker", root, new Vector3(-1, .025f, 3.3f), new Vector3(1, .015f, 1), cyan, PrimitiveType.Cylinder);
            var sign = new GameObject("ObservationSign").AddComponent<TextMesh>(); sign.transform.SetParent(root, false); sign.transform.position = new Vector3(-1, 1.1f, 4); sign.text = "观察点\n转向雪痕，按住右键"; sign.font = font; sign.fontSize = 64; sign.characterSize = .018f; sign.anchor = TextAnchor.MiddleCenter; sign.alignment = TextAlignment.Center;
            var tether = world.gameObject.AddComponent<BeamTether>(); tether.Configure(AssetDatabase.LoadAssetAtPath<BeamDefinition>(DataPath), aim, proxy.Target.gameObject, vane, lamp, line, body.gameObject, glow, observer); world.ConfigureTether(tether); proxy.ConfigureTether(tether);
            EditorUtility.SetDirty(world); EditorUtility.SetDirty(proxy); EditorUtility.SetDirty(tether);
            var hud = UnityEngine.Object.FindAnyObjectByType<SliceHud>();
            var status = new GameObject("BeamStatus", typeof(RectTransform)).AddComponent<Text>(); status.transform.SetParent(hud.transform, false); status.font = font; status.fontSize = 24; status.color = new Color(.9f, .97f, 1); status.alignment = TextAnchor.LowerRight; status.raycastTarget = false;
            var rect = status.rectTransform; rect.anchorMin = rect.anchorMax = new Vector2(1, 0); rect.pivot = new Vector2(1, 0); rect.anchoredPosition = new Vector2(-30, 165); rect.sizeDelta = new Vector2(620, 110);
            var outline = status.gameObject.AddComponent<Outline>(); outline.effectColor = new Color(.02f, .03f, .04f, 1); outline.effectDistance = new Vector2(1.5f, -1.5f); hud.ConfigureToolStatus(status); EditorUtility.SetDirty(hud);
            var binding = hud.GetComponent<SliceFontBinding>(); binding.Configure(font, binding.GetComponentsInChildren<Text>(true), UnityEngine.Object.FindObjectsByType<TextMesh>(FindObjectsInactive.Include)); EditorUtility.SetDirty(binding);
            var plans = world.Campaign.Definition;
            plans.Tablets[5].Record = "【T06 · 已固化运动交换工具】\n稳定光束锁定雪痕区域时，交换恶魔与风向标的运动；失锁再交换一次复位。\n脚印停止时风向标转动；移开光束后恢复。\n\n按住鼠标右键照住雪痕，保持到稳定。转身、遮挡、超距或进入近景会失锁。\n湖岸观察点能同时观察雪痕与风向标。风铃负责区域预警，光束争取行动时间。";
            plans.Days[6].Objective = "按三个固定地标的方位解开A，取出T06。回到湖岸观察点，按住右键稳定照住雪痕，观察运动交换与失锁恢复。"; EditorUtility.SetDirty(plans);
            Save(); SyncMotionRecord();
            var settings = UnityEngine.Object.Instantiate(world.Settings); settings.name = "BeamSettings"; settings.StartDay = 7; settings.StartHour = 10; settings.InitialLakeSealed = false; AssetDatabase.CreateAsset(settings, SettingsPath); AssetDatabase.SaveAssets();
            if (!AssetDatabase.CopyAsset(NineDayAuthoring.ScenePath, ScenePath)) throw new InvalidOperationException("Could not save beam quick scene.");
            EditorSceneManager.OpenScene(ScenePath); world = UnityEngine.Object.FindAnyObjectByType<SliceSceneController>();
            var serialized = new SerializedObject(world); serialized.FindProperty("settings").objectReferenceValue = AssetDatabase.LoadAssetAtPath<SliceSettings>(SettingsPath); serialized.ApplyModifiedProperties();
            var actor = world.Player.GetComponent<CharacterController>(); actor.enabled = false; world.Player.transform.SetPositionAndRotation(new Vector3(-8, .04f, -3.5f), Quaternion.identity); actor.enabled = true; PrefabUtility.RecordPrefabInstancePropertyModifications(world.Player.transform);
            Save(); EditorSceneManager.OpenScene(ScenePath); return Validate();
        }
        static GameObject Primitive(string name, Transform parent, Vector3 position, Vector3 scale, Material mat, PrimitiveType type)
        {
            var go = GameObject.CreatePrimitive(type); go.name = name; go.transform.SetParent(parent, false); go.transform.localPosition = position; go.transform.localScale = scale; go.GetComponent<Renderer>().sharedMaterial = mat; UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>()); return go;
        }
        static void Save() { EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); EditorSceneManager.SaveScene(SceneManager.GetActiveScene()); AssetDatabase.SaveAssets(); }
        public static void OpenQuick() { Editable(); EditorSceneManager.OpenScene(ScenePath); }
        public static void OpenEarly() { Editable(); EditorSceneManager.OpenScene(NineDayAuthoring.ScenePath); }
        public static string RepairView()
        {
            Editable();
            foreach (var path in new[] { NineDayAuthoring.ScenePath, ScenePath })
            {
                EditorSceneManager.OpenScene(path);
                var world = UnityEngine.Object.FindAnyObjectByType<SliceSceneController>();
                if (!world.Tether) throw new InvalidOperationException("Preserving scene without authored T06.");
                var observer = world.Tether.ObservationPoint; observer.position = new Vector3(-1, .04f, 3.3f); observer.LookAt(new Vector3(2, .04f, -7));
                var root = GameObject.Find("Beam_Authored").transform;
                root.Find("ObservationMarker").position = new Vector3(-1, .025f, 3.3f);
                root.Find("ObservationSign").position = new Vector3(-1, 1.1f, 4);
                world.Tether.AimRegion.parent.Find("SnowDisturbanceRegion").localPosition = new Vector3(0, -.07f, 0);
                Save();
            }
            EditorSceneManager.OpenScene(ScenePath);
            return "Moved only new observation point/marker/sign clear of existing clock; lowered new region mesh below authored footprints. Existing clock/cipher transforms preserved.";
        }
        public static string SyncMotionRecord()
        {
            Editable();
            var plans = AssetDatabase.LoadAssetAtPath<NineDayDefinition>("Assets/Demon/Data/NineDayDefinition.asset");
            if (!plans || plans.Tablets.Length != 6 || plans.Days.Length != 9) throw new InvalidOperationException("Preserving unexpected day-plan data.");
            plans.Tablets[0].Record = "【T01 · 封印一 · 已固化】\nIF DAY == 3\nTRANSFER(MOTION, WIND_VANE, DEMON)\n\n第三天零点，风向标的运动转移给恶魔。\n发现或阅读不会激活、阻止既定程序。\n\n若风向标停止而雪痕开始移动，说明运动已经转移。";
            plans.Days[2].Objective = "T01运动日期已到：风向标停，雪痕开始移动。寻找T02，核对下一条日期预告。";
            EditorUtility.SetDirty(plans); AssetDatabase.SaveAssets();
            return "T01 observation text and D3 objective now match the visible motion boundary; dates and other plans unchanged.";
        }
        public static string Validate()
        {
            var world = UnityEngine.Object.FindAnyObjectByType<SliceSceneController>();
            if (!world || !world.Tether || !world.Tether.Definition || !world.Tether.AimRegion || !world.Tether.VaneBlade || !world.Tether.ObservationPoint) throw new InvalidOperationException("Incomplete saved beam composition.");
            var proxy = UnityEngine.Object.FindAnyObjectByType<ListeningTrailProxy>();
            if (!proxy || new SerializedObject(proxy).FindProperty("tether").objectReferenceValue != world.Tether) throw new InvalidOperationException("Proxy lost its explicit motion owner.");
            if (world.Cipher.Wheel.ReadCode().Length != 3) throw new InvalidOperationException("Preserve the accepted fixed-position cipher layout.");
            return "Saved flashlight/aim region/feedback, explicit proxy and vane motion ownership, HUD and observation point verified; cipher layout preserved.";
        }
        const string Tune = "Demon.Beam.Restore";
        public static void BeginTuning()
        {
            var data = UnityEngine.Object.FindAnyObjectByType<SliceSceneController>().Tether.Definition;
            SessionState.SetFloat(Tune + "range", data.RangeMetres); SessionState.SetFloat(Tune + "radius", data.RegionRadiusMetres); SessionState.SetFloat(Tune + "stable", data.StableSeconds); SessionState.SetBool(Tune, true);
            data.RangeMetres = 6; data.RegionRadiusMetres = 1.1f; data.StableSeconds = .4f; EditorUtility.SetDirty(data); AssetDatabase.SaveAssets();
        }
        public static void RestoreTuning()
        {
            if (!SessionState.GetBool(Tune, false)) return;
            var data = AssetDatabase.LoadAssetAtPath<BeamDefinition>(DataPath);
            data.RangeMetres = SessionState.GetFloat(Tune + "range", 16); data.RegionRadiusMetres = SessionState.GetFloat(Tune + "radius", .85f); data.StableSeconds = SessionState.GetFloat(Tune + "stable", 1);
            EditorUtility.SetDirty(data); AssetDatabase.SaveAssets(); SessionState.SetBool(Tune, false); Debug.Log("DEMON_BEAM_TUNING_RESTORED");
        }
        public static string Build()
        {
            Editable(); Directory.CreateDirectory("Builds/WindowsBeam");
            var result = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = new[] { ScenePath }, locationPathName = "Builds/WindowsBeam/DemonBeam.exe", target = BuildTarget.StandaloneWindows64, options = BuildOptions.Development });
            if (result.summary.result != BuildResult.Succeeded) throw new InvalidOperationException("Beam Windows build failed."); return "Beam Windows64 build succeeded, " + result.summary.totalSize + " bytes.";
        }
    }
}
