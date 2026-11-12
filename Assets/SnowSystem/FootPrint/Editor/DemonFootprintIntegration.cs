using System;
using System.Linq;
using Demon.Slice;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace StandaloneTerrainFootprints.Editor
{
    public static class DemonFootprintIntegration
    {
        public const string ScenePath = "Assets/Demon/Scenes/DemonNineDay_Art.unity";
        public const string RendererPath = "Assets/Settings/PC_Renderer.asset";
        const string Generated = FootprintSetup.Root + "/Generated";

        [MenuItem("Tools/Terrain Footprints/Connect Demon in Art Scene")]
        public static void Connect()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Leave Play Mode first.");
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.OpenScene(ScenePath);
            var world = UnityEngine.Object.FindAnyObjectByType<GameSceneController>();
            if (!world || !world.Chase || !world.Chase.Agent) throw new InvalidOperationException("Art scene has no authored demon chase.");
            var terrains = UnityEngine.Object.FindObjectsByType<Terrain>();
            if (terrains.Length != 1) throw new InvalidOperationException("Expected the Art scene's single native Terrain.");
            var existing = UnityEngine.Object.FindObjectsByType<DemonFootprints>(FindObjectsInactive.Include);
            if (existing.Length > 0)
            {
                if (RemoveOldPrints(world.Chase.Agent.transform))
                {
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                    EditorSceneManager.OpenScene(ScenePath);
                }
                Validate();
                return;
            }
            var projector = AssetDatabase.LoadAssetAtPath<GameObject>(Generated + "/FootprintProjector.prefab");
            if (!projector) throw new InvalidOperationException("Create standalone footprint assets first.");
            var go = new GameObject("Demon Terrain Footprints");
            Undo.RegisterCreatedObjectUndo(go, "Connect demon footprints");
            go.transform.SetParent(world.Chase.transform, false);
            var emitter = go.AddComponent<TerrainFootprintEmitter>();
            emitter.targetTerrain = terrains[0];
            emitter.projectorPrefab = projector.GetComponent<DecalProjector>();
            emitter.leftMaterial = AssetDatabase.LoadAssetAtPath<Material>(Generated + "/FootprintLeft.mat");
            emitter.rightMaterial = AssetDatabase.LoadAssetAtPath<Material>(Generated + "/FootprintRight.mat");
            emitter.followTarget = world.Chase.Agent.transform;
            emitter.automaticSteps = false;
            emitter.footprintSize = new Vector2(.4f, .85f);
            emitter.lateralOffset = .3f;
            emitter.rayStartLift = .4f;
            emitter.maxGroundDistance = 1.25f;
            emitter.projectionDepth = .35f;
            emitter.drawDistance = 60;
            var adapter = go.AddComponent<DemonFootprints>();
            adapter.Configure(world.Chase, emitter);
            RemoveOldPrints(world.Chase.Agent.transform);
            AddRendererFeature();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(ScenePath);
            Validate();
        }

        static bool RemoveOldPrints(Transform agent)
        {
            bool removed = false;
            foreach (string name in new[] { "SnowPrint_0", "SnowPrint_1" })
            {
                var oldPrint = agent.Find(name);
                if (!oldPrint) continue;
                Undo.DestroyObjectImmediate(oldPrint.gameObject);
                removed = true;
            }
            return removed;
        }

        static void AddRendererFeature()
        {
            var renderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RendererPath);
            if (!renderer) throw new InvalidOperationException("Missing current PC renderer.");
            if (renderer.rendererFeatures.Any(f => f is DecalRendererFeature)) return;
            Undo.RecordObject(renderer, "Enable demon footprint decals");
            var feature = ScriptableObject.CreateInstance<DecalRendererFeature>();
            feature.name = "Demon Footprint Decals";
            AssetDatabase.AddObjectToAsset(feature, renderer);
            Undo.RegisterCreatedObjectUndo(feature, "Enable demon footprint decals");
            var serialized = new SerializedObject(feature);
            serialized.FindProperty("m_Settings.technique").enumValueIndex = 1;
            serialized.FindProperty("m_Settings.dBufferSettings.surfaceData").enumValueIndex = 1;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            renderer.rendererFeatures.Add(feature);
            var rendererObject = new SerializedObject(renderer);
            var map = rendererObject.FindProperty("m_RendererFeatureMap");
            map.arraySize = renderer.rendererFeatures.Count;
            for (int i = 0; i < renderer.rendererFeatures.Count; i++)
            {
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(renderer.rendererFeatures[i], out string _, out long localId);
                map.GetArrayElementAtIndex(i).longValue = localId;
            }
            rendererObject.ApplyModifiedPropertiesWithoutUndo();
            renderer.SetDirty(); EditorUtility.SetDirty(renderer);
        }

        public static void Validate()
        {
            var world = UnityEngine.Object.FindAnyObjectByType<GameSceneController>();
            var adapters = UnityEngine.Object.FindObjectsByType<DemonFootprints>(FindObjectsInactive.Include);
            if (adapters.Length != 1) throw new InvalidOperationException("Expected exactly one demon footprint adapter.");
            var adapter = adapters[0];
            var emitter = adapter.Emitter;
            if (!world || adapter.Chase != world.Chase || !emitter || emitter.automaticSteps || emitter.followTarget != world.Chase.Agent.transform || !emitter.targetTerrain || !emitter.projectorPrefab || !emitter.leftMaterial || !emitter.rightMaterial)
                throw new InvalidOperationException("Demon footprint saved references are incomplete.");
            if (world.Player.GetComponentInChildren<TerrainFootprintEmitter>(true)) throw new InvalidOperationException("Player must not have footprints.");
            var renderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RendererPath);
            if (!renderer.rendererFeatures.Any(f => f is DecalRendererFeature && f.isActive)) throw new InvalidOperationException("Missing active decal feature.");
            foreach (string name in new[] { "SnowPrint_0", "SnowPrint_1" })
                if (world.Chase.Agent.transform.Find(name)) throw new InvalidOperationException("Obsolete moving print still present: " + name);
            Debug.Log("Demon footprints: Art scene references, demon-only attachment and renderer validated.");
        }
    }
}
