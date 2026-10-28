using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Demon.Slice.Editor
{
    public static class NineDayAuthoring
    {
        public const string ScenePath = "Assets/Demon/Scenes/DemonNineDay.unity";
        const string SettingsPath = "Assets/Demon/Data/NineDaySettings.asset";
        const string DefinitionPath = "Assets/Demon/Data/NineDayDefinition.asset";

        [MenuItem("Demon/Nine Days/Create Flow Scene")]
        public static void Create()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || SceneManager.GetActiveScene().isDirty)
                throw new InvalidOperationException("Leave Play Mode and save the current scene first.");
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) || AssetDatabase.LoadAssetAtPath<SliceSettings>(SettingsPath) ||
                AssetDatabase.LoadAssetAtPath<NineDayDefinition>(DefinitionPath))
                throw new InvalidOperationException("Nine-day outputs exist; open and edit them instead of regenerating.");
            var settings = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<SliceSettings>("Assets/Demon/Data/SliceSettings.asset"));
            settings.name = "NineDaySettings"; settings.StartDay = 1; settings.StartHour = 8;
            settings.InitialLakeSealed = true;
            AssetDatabase.CreateAsset(settings, SettingsPath);
            var definition = ScriptableObject.CreateInstance<NineDayDefinition>();
            string[] titles = { "值守开始", "第一块石板", "日期节点：D3", "第三块石板", "实体的预告", "监测工具记录", "交换工具记录", "最后的碎片", "第九天" };
            string[] objectives = {
                "熟悉湖岸、箱子和左侧睡袋。可自由睡到次日；夜间封印开启目前仅做状态占位。",
                "雪地右侧出现T01。靠近石板按E记录；按Esc返回。可继续探索或睡觉。",
                "右侧出现T02，T01的日期节点已到。能力效果暂缓制作，可继续查看或睡觉。",
                "右侧出现T03。前面的石板仍可重复查看，发现不是日期程序的开关。",
                "右侧出现T04，T02日期节点已到。阅读内容为流程占位，具体能力尚未接入。",
                "右侧出现T05监测工具记录。此阶段只登记发现，正式工具条件与效果待设计。",
                "右侧出现T06交换工具记录，T03日期节点已到。可以继续睡觉，不强制读完石板。",
                "正前方出现T07。补全铭文，或直接睡到第九天；中心EXCHANGE、下中BOX、下右LAKE。",
                "第九天。" };
            for (int i = 0; i < 9; i++) definition.Days[i] = new DayPlan { Title = titles[i], Objective = objectives[i], WakeHour = 8 };
            string[] records = {
                "【T01 · 封印一 · 已固化】\n日期节点：第3天零点。\n\n这块石板不能重新编写。发现与查看不会激活或推迟它的日期程序。\n\n当前只验证流程；光属性的实际转移和语句内容暂缓。",
                "【T02 · 封印二 · 已固化】\n日期节点：第5天零点。\n\n这块石板已经固化，表面的符文并非程序本身。\n\n声音效果、提供对象与正式语句暂缓制作。",
                "【T03 · 封印三 · 已固化】\n日期节点：第7天零点。\n\n不阅读石板也不会阻止既定日期到来。\n\n运动属性效果与正式语句暂缓制作。",
                "【T04 · 封印四 · 已固化】\n日期节点：第9天零点。\n\n第九天，投影将获得实体。\n\n当前沿用样例日期状态；实体赋予的函数与来源仍待设计。",
                "【T05 · 前人监测工具】\n发现日：第6天。\n\n这里预留监测教学内容。监听对象、条件、区域信息和复位方式仍待设计。\n\n当前可以记录和重复阅读，尚不能进行正式监测。",
                "【T06 · 前人诱导工具】\n发现日：第7天。\n\n这里预留交换与诱导教学内容。条件、声音对象和复位方式仍待设计。\n\n当前只提供查看入口；前六块均不可重写。" };
            int[] nodes = { 3, 5, 7, 9, 0, 0 };
            for (int i = 0; i < 6; i++) definition.Tablets[i] = new FixedTabletPlan {
                Title = "T" + (i + 1).ToString("00") + " 石板记录", DiscoveryDay = i + 2, DateNodeDay = nodes[i], Record = records[i] };
            AssetDatabase.CreateAsset(definition, DefinitionPath);
            if (!AssetDatabase.CopyAsset(DemonSliceAuthoring.ScenePath, ScenePath)) throw new InvalidOperationException("Could not copy the saved slice scene.");
            EditorSceneManager.OpenScene(ScenePath);
            var world = UnityEngine.Object.FindAnyObjectByType<SliceSceneController>();
            var flow = Undo.AddComponent<NineDayFlow>(world.gameObject);
            world.ConfigureCampaign(flow);
            var serialized = new SerializedObject(world);
            serialized.FindProperty("settings").objectReferenceValue = AssetDatabase.LoadAssetAtPath<SliceSettings>(SettingsPath);
            serialized.ApplyModifiedProperties();
            var fixedRoot = new GameObject("FixedTablets_Authored").transform;
            var tablets = new NineDayTablet[6];
            var material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Demon/Art/Materials/Stone.mat");
            var font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Demon/UI/WindowsChinese.fontsettings");
            for (int i = 0; i < tablets.Length; i++)
            {
                var go = new GameObject("T" + (i + 1).ToString("00") + "_FixedTablet");
                go.transform.SetParent(fixedRoot, false);
                go.transform.position = new Vector3(4 + (i % 3) * 3.1f, 1.2f, i < 3 ? 2.5f : -3.2f);
                var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
                body.name = "FixedStone"; body.transform.SetParent(go.transform, false);
                body.transform.localScale = new Vector3(1.4f, 2, .22f); body.GetComponent<Renderer>().sharedMaterial = material;
                var label = new GameObject("TabletLabel").AddComponent<TextMesh>();
                label.transform.SetParent(go.transform, false); label.transform.localPosition = new Vector3(0, .15f, -.13f);
                label.font = font; label.fontSize = 64; label.characterSize = .035f;
                label.anchor = TextAnchor.MiddleCenter; label.alignment = TextAlignment.Center;
                label.text = "T" + (i + 1).ToString("00") + "\nDAY " + (i + 2) + "\nFIXED";
                var anchor = new GameObject("ReadView").transform;
                anchor.SetParent(go.transform, false); anchor.localPosition = new Vector3(0, .2f, -2.3f);
                anchor.LookAt(go.transform.position + Vector3.up * .15f);
                tablets[i] = go.AddComponent<NineDayTablet>(); tablets[i].Configure(flow, i, anchor);
            }
            flow.Configure(AssetDatabase.LoadAssetAtPath<NineDayDefinition>(DefinitionPath), tablets, world.Tablet.gameObject);
            var hud = UnityEngine.Object.FindAnyObjectByType<SliceHud>();
            hud.GetComponent<SliceFontBinding>().Configure(font, hud.GetComponentsInChildren<Text>(true),
                UnityEngine.Object.FindObjectsByType<TextMesh>(FindObjectsInactive.Include));
            EditorUtility.SetDirty(world); EditorUtility.SetDirty(flow); EditorUtility.SetDirty(hud.GetComponent<SliceFontBinding>());
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene()); AssetDatabase.SaveAssets();
            var scenes = EditorBuildSettings.scenes.Where(s => s.path != ScenePath).ToList();
            scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true)); EditorBuildSettings.scenes = scenes.ToArray();
            Validate();
        }

        public static string Validate()
        {
            if (SceneManager.GetActiveScene().path != ScenePath) throw new InvalidOperationException("Open DemonNineDay first.");
            var world = UnityEngine.Object.FindAnyObjectByType<SliceSceneController>();
            if (!world || !world.Campaign || !world.Campaign.Definition || world.Settings.StartDay != 1 || world.Campaign.Definition.FinalTabletDay != 8)
                throw new InvalidOperationException("Nine-day composition/settings incomplete.");
            if (UnityEngine.Object.FindObjectsByType<NineDayTablet>(FindObjectsInactive.Include).Length != 6)
                throw new InvalidOperationException("Expected six authored fixed tablets.");
            var box = UnityEngine.Object.FindAnyObjectByType<SliceBox>();
            if (!box || new SerializedObject(box).FindProperty("world").objectReferenceValue != world)
                throw new InvalidOperationException("Box lost its scene binding.");
            return "Nine-day scene, six fixed tablets, independent settings and box binding verified.";
        }

        public static string RepairSettings()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || SceneManager.GetActiveScene().path != ScenePath)
                throw new InvalidOperationException("Open the nine-day scene outside Play Mode.");
            var world = UnityEngine.Object.FindAnyObjectByType<SliceSceneController>();
            var settings = AssetDatabase.LoadAssetAtPath<SliceSettings>(SettingsPath);
            if (!world || !settings) throw new InvalidOperationException("Nine-day settings are missing.");
            var serialized = new SerializedObject(world);
            var field = serialized.FindProperty("settings");
            if (field.objectReferenceValue && field.objectReferenceValue != settings)
                throw new InvalidOperationException("Preserving a different existing settings assignment.");
            field.objectReferenceValue = settings;
            serialized.ApplyModifiedProperties(); EditorUtility.SetDirty(world);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            EditorSceneManager.OpenScene(ScenePath);
            return Validate();
        }

        public static string Build()
        {
            System.IO.Directory.CreateDirectory("Builds/WindowsNineDay");
            var result = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = new[] { ScenePath }, locationPathName = "Builds/WindowsNineDay/DemonNineDay.exe",
                target = BuildTarget.StandaloneWindows64, options = BuildOptions.Development });
            if (result.summary.result != BuildResult.Succeeded) throw new InvalidOperationException("Nine-day build failed.");
            return "Nine-day Windows64 build succeeded, " + result.summary.totalSize + " bytes.";
        }
    }
}
