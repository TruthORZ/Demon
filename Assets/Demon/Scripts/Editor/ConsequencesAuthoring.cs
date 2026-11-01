using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace Demon.Slice.Editor
{
    public static class ConsequencesAuthoring
    {
        public const string ScenePath = "Assets/Demon/Scenes/DemonConsequences.unity";
        const string SettingsPath = "Assets/Demon/Data/ConsequencesSettings.asset";
        const string BroadcastPath = "Assets/Demon/Audio/PrototypeRadioBroadcast.wav";
        const string StaticPath = "Assets/Demon/Audio/PrototypeRadioStatic.wav";
        const string NormalMat = "Assets/Demon/Art/Materials/NormalSolar.mat";
        const string EmptyMat = "Assets/Demon/Art/Materials/EmptySolar.mat";
        const string RimMat = "Assets/Demon/Art/Materials/SolarRim.mat";
        static void Editable()
        { if (EditorApplication.isPlayingOrWillChangePlaymode || SceneManager.GetActiveScene().isDirty) throw new InvalidOperationException("Leave Play and save current human edits before attribute authoring."); }
        public static string Create()
        {
            Editable();
            foreach (string path in new[] { ScenePath, SettingsPath, BroadcastPath, StaticPath, NormalMat, EmptyMat, RimMat })
                if (File.Exists(path)) throw new InvalidOperationException("Attribute output exists; preserve saved authoring: " + path);
            WriteSound(BroadcastPath, false); WriteSound(StaticPath, true);
            AssetDatabase.ImportAsset(BroadcastPath, ImportAssetOptions.ForceSynchronousImport); AssetDatabase.ImportAsset(StaticPath, ImportAssetOptions.ForceSynchronousImport);
            MaterialAsset(NormalMat, new Color(1, .8f, .18f)); MaterialAsset(EmptyMat, Color.black); MaterialAsset(RimMat, new Color(.85f, .92f, 1));
            foreach (string path in new[] { NineDayAuthoring.ScenePath, FinalTabletAuthoring.ScenePath })
            { EditorSceneManager.OpenScene(path); Install(); Save(); }
            EditorSceneManager.OpenScene(NineDayAuthoring.ScenePath);
            var world = UnityEngine.Object.FindAnyObjectByType<SliceSceneController>();
            var quick = UnityEngine.Object.Instantiate(world.Settings); quick.name = "ConsequencesSettings"; quick.StartDay = 4; quick.StartHour = 23 + 50f / 60; quick.InitialLakeSealed = false;
            quick.Instructions += "\nD5听电台与湖中声源；D7观察失光日轮。可用睡袋跳日。"; AssetDatabase.CreateAsset(quick, SettingsPath);
            var definition = world.Campaign.Definition;
            definition.Tablets[1].Record = "【T02 · 已固化】\nIF DAY == 5\nEXCHANGE(SOUND, RADIO, DEMON)\n\n第5天零点，可辨认的原广播从电台转到湖中恶魔声源；电台只余设备静电。静电底噪是设备环境声，原本已经存在。\n\n日期自动生效，发现与阅读不改变它。当前广播旋律与静电是可替换占位音。";
            definition.Tablets[2].Record = "【T03 · 已固化】\nIF DAY == 7\nTRANSFER(LIGHT, SUN, DEMON)\n\n第7天零点，太阳失去照明，湖中恶魔获得光。到预期日出时，日轮只剩白边，世界没有正常日光。\n\n残边与符文提示光不提供太阳照明。日期自动生效，不依赖是否阅读；当前日轮使用保存的方位参照占位物。";
            definition.Days[4].Objective = "原广播从湖中传来，电台只余静电。对照T02，查看新出现的T04。";
            definition.Days[6].Objective = "预期日出后仍是夜色，太阳参照只剩白边，湖中持续发光。对照太阳符号与轮盘解A，取得T06。";
            EditorUtility.SetDirty(definition); AssetDatabase.SaveAssets();
            if (!AssetDatabase.CopyAsset(NineDayAuthoring.ScenePath, ScenePath)) throw new InvalidOperationException("Cannot save consequences quick scene.");
            EditorSceneManager.OpenScene(ScenePath); world = UnityEngine.Object.FindAnyObjectByType<SliceSceneController>();
            var ws = new SerializedObject(world); ws.FindProperty("settings").objectReferenceValue = AssetDatabase.LoadAssetAtPath<SliceSettings>(SettingsPath); ws.ApplyModifiedProperties();
            var body = world.Player.GetComponent<CharacterController>(); body.enabled = false; world.Player.transform.SetPositionAndRotation(new Vector3(-8, .04f, -2.4f), Quaternion.identity); body.enabled = true; PrefabUtility.RecordPrefabInstancePropertyModifications(world.Player.transform);
            Save(); EditorSceneManager.OpenScene(ScenePath); return Validate();
        }
        static void Install()
        {
            var world = UnityEngine.Object.FindAnyObjectByType<SliceSceneController>();
            if (!world || !world.Cipher || !world.Chase || !world.Finale || !world.Retrieval || world.Consequences || GameObject.Find("Consequences_Authored")) throw new InvalidOperationException("Expected accepted T07 composition without consequences.");
            var root = new GameObject("Consequences_Authored").transform;
            var landmarks = world.Cipher.Wheel.Landmarks;
            var radio = Emitter("Broadcast_TransferredContent", landmarks[0], new Vector3(0, 1, 0), .35f);
            var noise = Emitter("Static_DeviceEnvironment", landmarks[0], new Vector3(0, 1, 0), .035f);
            var lake = Emitter("DemonBroadcast_Lake", root, new Vector3(0, .85f, 9), .35f);
            var disk = landmarks[1].Find("SolarReferenceDisk").GetComponent<Renderer>(); disk.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(NormalMat); var diskScale = disk.transform.localScale; diskScale.z = .03f; disk.transform.localScale = diskScale;
            var rim = new GameObject("WhiteRim_NoIllumination").AddComponent<LineRenderer>(); rim.transform.SetParent(landmarks[1], false); rim.transform.localPosition = new Vector3(0, 4.2f, -.025f);
            rim.useWorldSpace = false; rim.loop = true; rim.positionCount = 64; rim.startWidth = rim.endWidth = .035f; rim.alignment = LineAlignment.TransformZ; rim.numCornerVertices = 3;
            rim.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(RimMat);
            for (int i = 0; i < 64; i++) { float angle = 2 * Mathf.PI * i / 64; rim.SetPosition(i, new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * .5f); }
            rim.gameObject.SetActive(false);
            var effects = world.gameObject.AddComponent<WorldAttributeConsequences>();
            effects.Configure(AssetDatabase.LoadAssetAtPath<AudioClip>(BroadcastPath), AssetDatabase.LoadAssetAtPath<AudioClip>(StaticPath), radio, noise, lake,
                disk, AssetDatabase.LoadAssetAtPath<Material>(NormalMat), AssetDatabase.LoadAssetAtPath<Material>(EmptyMat), rim.gameObject,
                Enumerable.Range(0, 8).Select(i => landmarks[1].Find("SolarRay_" + i).gameObject).ToArray());
            radio.clip = lake.clip = AssetDatabase.LoadAssetAtPath<AudioClip>(BroadcastPath); noise.clip = AssetDatabase.LoadAssetAtPath<AudioClip>(StaticPath);
            world.ConfigureConsequences(effects); EditorUtility.SetDirty(world); EditorUtility.SetDirty(effects);
            Anchor("RadioView", root, landmarks[0].position + new Vector3(0, 1.65f, -2.5f), landmarks[0].position + Vector3.up);
            Anchor("LakeView", root, new Vector3(0, 1.65f, 3.6f), lake.transform.position);
            Anchor("SolarView", root, new Vector3(-7, 1.65f, -2.2f), disk.transform.position);
        }
        static AudioSource Emitter(string name, Transform parent, Vector3 position, float volume)
        {
            var source = new GameObject(name).AddComponent<AudioSource>(); source.transform.SetParent(parent, false); source.transform.localPosition = position;
            source.playOnAwake = false; source.loop = true; source.spatialBlend = 1; source.rolloffMode = AudioRolloffMode.Linear; source.minDistance = 1; source.maxDistance = 24; source.dopplerLevel = 0; source.volume = volume; return source;
        }
        static void Anchor(string name, Transform parent, Vector3 position, Vector3 lookAt)
        { var node = new GameObject(name).transform; node.SetParent(parent, false); node.position = position; node.LookAt(lookAt); }
        static void MaterialAsset(string path, Color color)
        {
            var shader = Shader.Find("Universal Render Pipeline/Unlit"); if (!shader) throw new InvalidOperationException("Installed URP unlit shader missing.");
            var mat = new Material(shader); mat.name = Path.GetFileNameWithoutExtension(path); mat.SetColor("_BaseColor", color); if (mat.HasProperty("_Cull")) mat.SetInt("_Cull", 0); AssetDatabase.CreateAsset(mat, path);
        }
        static void WriteSound(string path, bool noise)
        {
            const int rate = 22050; int length = rate * (noise ? 2 : 4); var random = new System.Random(11); double smooth = 0;
            double[] notes = { 220, 277.1826, 329.6276, 277.1826, 440, 329.6276, 277.1826, 0 };
            using (var file = new BinaryWriter(File.Create(path)))
            {
                file.Write(System.Text.Encoding.ASCII.GetBytes("RIFF")); file.Write(36 + length * 2); file.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));
                file.Write(16); file.Write((short)1); file.Write((short)1); file.Write(rate); file.Write(rate * 2); file.Write((short)2); file.Write((short)16); file.Write(System.Text.Encoding.ASCII.GetBytes("data")); file.Write(length * 2);
                for (int i = 0; i < length; i++)
                {
                    double t = (double)i / rate; smooth = .65 * smooth + .35 * (random.NextDouble() * 2 - 1);
                    double edge = Math.Min(1, Math.Min(i, length - 1 - i) / 220.0); double sample;
                    if (noise) sample = smooth * .22 * edge;
                    else { int beat = Math.Min(7, (int)(t * 2)); double local = t % .5; double envelope = Math.Pow(Math.Sin(Math.PI * Math.Min(1, local / .4)), 2); sample = notes[beat] == 0 || local > .4 ? 0 : .2 * envelope * (Math.Sin(2 * Math.PI * notes[beat] * t) + .18 * Math.Sin(4 * Math.PI * notes[beat] * t)); sample *= edge; }
                    file.Write((short)(sample * short.MaxValue));
                }
            }
        }
        static void Save() { EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); EditorSceneManager.SaveScene(SceneManager.GetActiveScene()); AssetDatabase.SaveAssets(); }
        public static void OpenQuick() { Editable(); EditorSceneManager.OpenScene(ScenePath); }
        public static string AlignSolarView()
        {
            Editable();
            foreach (string path in new[] { NineDayAuthoring.ScenePath, FinalTabletAuthoring.ScenePath, ScenePath })
            {
                EditorSceneManager.OpenScene(path); var world = UnityEngine.Object.FindAnyObjectByType<SliceSceneController>(); var disk = world.Consequences.SolarDisk.transform;
                var scale = disk.localScale; scale.z = .03f; disk.localScale = scale;
                var rim = (new SerializedObject(world.Consequences).FindProperty("whiteRim").objectReferenceValue as GameObject).transform;
                var position = rim.localPosition; position.z = -.025f; rim.localPosition = position; EditorUtility.SetDirty(disk); EditorUtility.SetDirty(rim); Save();
            }
            return Validate();
        }
        public static string Validate()
        {
            var world = UnityEngine.Object.FindAnyObjectByType<SliceSceneController>();
            if (!world || !world.Settings || !world.Player || !world.Presentation || !world.Consequences || !world.Consequences.BroadcastClip || !world.Consequences.RadioBroadcast || !world.Consequences.RadioStatic || !world.Consequences.DemonBroadcast || !world.Finale || !world.Retrieval) throw new InvalidOperationException("Missing saved sound/light composition.");
            if (!world.Cipher.Wheel.ReadCode().SequenceEqual(new[] { 3, 4, 2 })) throw new InvalidOperationException("Solar/root edits changed accepted cipher342.");
            var so = new SerializedObject(world.Consequences);
            foreach (var field in new[] { "staticClip", "solarDisk", "normalSolarMaterial", "emptySolarMaterial", "whiteRim" }) if (!so.FindProperty(field).objectReferenceValue) throw new InvalidOperationException("Missing consequence field: " + field);
            return "Saved D5 broadcast/static routing, D7 solar materials/rim and shared lake-light contract reloaded; cipher342/root positions intact.";
        }
        const string Tune = "Demon.Consequences.Restore";
        public static void BeginProbe(bool late, bool tune)
        {
            var world = UnityEngine.Object.FindAnyObjectByType<SliceSceneController>(); var effects = world.Consequences;
            if (SessionState.GetBool(Tune, false)) throw new InvalidOperationException("Restore the prior attribute probe first.");
            SessionState.SetBool(Tune, true); SessionState.SetString(Tune + "clip", AssetDatabase.GetAssetPath(effects.BroadcastClip));
            SessionState.SetFloat(Tune + "radio", effects.RadioBroadcast.volume); SessionState.SetFloat(Tune + "day", world.Settings.StartDay); SessionState.SetFloat(Tune + "hour", world.Settings.StartHour);
            SessionState.SetFloat(Tune + "sunrise", world.Presentation.SunriseHour);
            if (late) { world.Settings.StartDay = 8; world.Settings.StartHour = 12; EditorUtility.SetDirty(world.Settings); }
            if (tune)
            {
                var so = new SerializedObject(effects); so.FindProperty("broadcastClip").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Demon/Audio/PrototypeChimeHigh.wav"); so.ApplyModifiedProperties();
                effects.RadioBroadcast.volume = .12f; EditorUtility.SetDirty(effects.RadioBroadcast);
                var presentation = new SerializedObject(world.Presentation); presentation.FindProperty("sunriseHour").floatValue = 7.5f; presentation.ApplyModifiedProperties(); EditorUtility.SetDirty(world.Presentation);
            }
            Save();
        }
        public static void RestoreProbe()
        {
            if (!SessionState.GetBool(Tune, false)) return;
            var world = UnityEngine.Object.FindAnyObjectByType<SliceSceneController>(); var effects = world.Consequences;
            world.Settings.StartDay = (int)SessionState.GetFloat(Tune + "day", 4); world.Settings.StartHour = SessionState.GetFloat(Tune + "hour", 23 + 50f / 60); EditorUtility.SetDirty(world.Settings);
            effects.RadioBroadcast.volume = SessionState.GetFloat(Tune + "radio", .35f); EditorUtility.SetDirty(effects.RadioBroadcast);
            var so = new SerializedObject(effects); so.FindProperty("broadcastClip").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(SessionState.GetString(Tune + "clip", BroadcastPath)); so.ApplyModifiedProperties(); EditorUtility.SetDirty(effects);
            var presentation = new SerializedObject(world.Presentation); presentation.FindProperty("sunriseHour").floatValue = SessionState.GetFloat(Tune + "sunrise", 6); presentation.ApplyModifiedProperties(); EditorUtility.SetDirty(world.Presentation);
            Save(); SessionState.SetBool(Tune, false); Debug.Log("DEMON_CONSEQUENCES_PROBE_RESTORED");
        }
        public static string Build()
        {
            Editable(); Directory.CreateDirectory("Builds/WindowsConsequences");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = new[] { ScenePath }, locationPathName = "Builds/WindowsConsequences/DemonConsequences.exe", target = BuildTarget.StandaloneWindows64, options = BuildOptions.Development });
            if (report.summary.result != BuildResult.Succeeded) throw new InvalidOperationException("Consequences Windows build failed."); return "Consequences Windows64 succeeded, " + report.summary.totalSize + " bytes.";
        }
    }
}
