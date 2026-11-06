using System;
using System.Linq;
using Demon.Slice.Domain;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Demon.Slice.Editor
{
    public static class ArchitectureRefactorChecks
    {
        const string Probe = "Demon.Architecture.Restore";
        public static void BeginProbe()
        {
            Editable();
            if (SessionState.GetBool(Probe, false)) throw new InvalidOperationException("Restore the previous architecture probe first.");
            var world = UnityEngine.Object.FindAnyObjectByType<GameSceneController>();
            var plan = world.Campaign.Definition; var beam = world.Tether.Definition; var guide = world.Exploration.Definition;
            SessionState.SetInt(Probe + "motion", plan.Tablets[0].DateNodeDay);
            SessionState.SetInt(Probe + "fallback", beam.DemonMotionDay);
            SessionState.SetInt(Probe + "key", (int)guide.GuideRules[0].Key); SessionState.SetBool(Probe, true);
            plan.Tablets[0].DateNodeDay = 4; beam.DemonMotionDay = 2; guide.GuideRules[0].Key = ExplorationGuideKey.Day7Cipher;
            EditorUtility.SetDirty(plan); EditorUtility.SetDirty(beam); EditorUtility.SetDirty(guide); AssetDatabase.SaveAssets();
        }
        public static void RestoreProbe()
        {
            if (!SessionState.GetBool(Probe, false)) return;
            var plan = AssetDatabase.LoadAssetAtPath<NineDayDefinition>("Assets/Demon/Data/NineDayDefinition.asset");
            var beam = AssetDatabase.LoadAssetAtPath<BeamDefinition>("Assets/Demon/Data/BeamDefinition.asset");
            var guide = AssetDatabase.LoadAssetAtPath<ExplorationDefinition>("Assets/Demon/Data/ExplorationDefinition.asset");
            if (SessionState.GetInt(Probe + "motion", -1) < 1 || SessionState.GetInt(Probe + "fallback", -1) < 1 || SessionState.GetInt(Probe + "key", -1) < 0) throw new InvalidOperationException("Missing architecture probe restore values.");
            plan.Tablets[0].DateNodeDay = SessionState.GetInt(Probe + "motion", -1);
            beam.DemonMotionDay = SessionState.GetInt(Probe + "fallback", -1);
            guide.GuideRules[0].Key = (ExplorationGuideKey)SessionState.GetInt(Probe + "key", -1);
            EditorUtility.SetDirty(plan); EditorUtility.SetDirty(beam); EditorUtility.SetDirty(guide); AssetDatabase.SaveAssets();
            SessionState.SetBool(Probe, false); Debug.Log("DEMON_ARCHITECTURE_PROBE_RESTORED");
        }
        public static string SaveGuideRules()
        {
            Editable();
            var data = AssetDatabase.LoadAssetAtPath<ExplorationDefinition>("Assets/Demon/Data/ExplorationDefinition.asset");
            if (!data) throw new InvalidOperationException("Missing authored exploration definition.");
            ExplorationGuidance.Validate(data.GuideRules);
            EditorUtility.SetDirty(data); AssetDatabase.SaveAssets();
            return "Saved existing exploration text and ordered guide rules using Unity serialization.";
        }
        static void Editable()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || SceneManager.GetActiveScene().isDirty)
                throw new InvalidOperationException("Leave Play and save human scene changes before architecture validation.");
        }
        public static string Validate()
        {
            Editable();
            if (typeof(PlayerController).GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Any(f => f.FieldType == typeof(GameSceneController)))
                throw new InvalidOperationException("Player must consume its narrow session contract.");
            if (!typeof(ICloseViewInteraction).IsAssignableFrom(typeof(RuneTablet)) || !typeof(ICloseViewInteraction).IsAssignableFrom(typeof(CipherBox)))
                throw new InvalidOperationException("Both authored puzzles must preserve the same close-input contract.");
            var plan = ScriptableObject.CreateInstance<NineDayDefinition>();
            try
            {
                plan.Tablets[0] = new FixedTabletPlan { DateNodeDay = 4 };
                if (plan.MotionDay != 4) throw new InvalidOperationException("Motion date ignored authored T01.");
                plan.Tablets[0].DateNodeDay = 0;
                try { var invalid = plan.MotionDay; throw new InvalidOperationException("Invalid motion date accepted."); }
                catch (InvalidOperationException e) when (e.Message.StartsWith("T01 requires")) { }
            }
            finally { UnityEngine.Object.DestroyImmediate(plan); }
            string original = SceneManager.GetActiveScene().path;
            int count = 0;
            try
            {
                foreach (string guid in AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Demon/Scenes" }))
                {
                    var scene = EditorSceneManager.OpenScene(AssetDatabase.GUIDToAssetPath(guid));
                    foreach (var root in scene.GetRootGameObjects())
                        foreach (var transform in root.GetComponentsInChildren<Transform>(true))
                            if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject) != 0)
                                throw new InvalidOperationException("Missing script: " + scene.path + " / " + transform.name);
                    var world = UnityEngine.Object.FindAnyObjectByType<GameSceneController>();
                    if (!world || !world.Player || !world.Tablet || !world.Settings) throw new InvalidOperationException("Missing saved composition: " + scene.path);
                    if (world.Exploration) ExplorationGuidance.Validate(world.Exploration.Definition.GuideRules);
                    count++;
                }
            }
            finally { if (!string.IsNullOrEmpty(original)) EditorSceneManager.OpenScene(original); }
            return "Architecture contracts, T01 date source and " + count + " saved scenes loaded without missing scripts; original scene restored.";
        }
    }
}
