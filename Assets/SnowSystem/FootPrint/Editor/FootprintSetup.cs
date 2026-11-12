using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using StandaloneTerrainFootprints;

namespace StandaloneTerrainFootprints.Editor
{
    public static class FootprintSetup
    {
        public const string Root = "Assets/SnowSystem/FootPrint";
        const string Generated = Root + "/Generated";

        [MenuItem("Tools/Terrain Footprints/Create Assets and Test Scene")]
        public static void Build()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(Generated + "/FootprintEmitter.prefab") != null)
            { Debug.Log("Footprint assets already exist. Existing edited assets were preserved."); return; }
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Directory.CreateDirectory(Generated);
            AssetDatabase.Refresh();
            Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(Root + "/FootprintDecal.shadergraph");
            if (shader == null) throw new InvalidOperationException("FootprintDecal.shadergraph did not import.");
            Material left = MakeMaterial(shader, true);
            Material right = MakeMaterial(shader, false);

            GameObject projectorGO = new GameObject("Footprint Projector");
            var projector = projectorGO.AddComponent<DecalProjector>();
            projector.material = left;
            projector.pivot = Vector3.zero;
            projector.size = new Vector3(0.16f, 0.32f, 0.16f);
            projector.drawDistance = 35;
            GameObject projectorAsset = PrefabUtility.SaveAsPrefabAsset(projectorGO, Generated + "/FootprintProjector.prefab");
            UnityEngine.Object.DestroyImmediate(projectorGO);
            GameObject emitterGO = new GameObject("Terrain Footprint Emitter");
            var emitter = emitterGO.AddComponent<TerrainFootprintEmitter>();
            emitter.projectorPrefab = projectorAsset.GetComponent<DecalProjector>();
            emitter.leftMaterial = left;
            emitter.rightMaterial = right;
            var emitterAsset = PrefabUtility.SaveAsPrefabAsset(emitterGO, Generated + "/FootprintEmitter.prefab");
            UnityEngine.Object.DestroyImmediate(emitterGO);

            var renderer = ScriptableObject.CreateInstance<UniversalRendererData>();
            renderer.name = "Footprint Test Renderer";
            renderer.renderingMode = RenderingMode.Forward;
            AssetDatabase.CreateAsset(renderer, Generated + "/FootprintTestRenderer.asset");
            AddDecals(renderer);
            UniversalRenderPipelineAsset pipeline = UniversalRenderPipelineAsset.Create(renderer);
            pipeline.name = "Footprint Test URP";
            pipeline.msaaSampleCount = 1;
            pipeline.supportsHDR = false;
            AssetDatabase.CreateAsset(pipeline, Generated + "/FootprintTestURP.asset");

            // A saved, editable native Terrain test scene. No existing scene or renderer is altered.
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var data = new TerrainData { heightmapResolution = 129, size = new Vector3(16, 2, 16) };
            AssetDatabase.CreateAsset(data, Generated + "/FootprintTestTerrain.asset");
            var snowTexture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            snowTexture.SetPixels(new[] { Color.white, Color.white, Color.white, Color.white });
            snowTexture.Apply();
            File.WriteAllBytes(Generated + "/TestSnow.png", snowTexture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(snowTexture);
            AssetDatabase.ImportAsset(Generated + "/TestSnow.png");
            var layer = new TerrainLayer { diffuseTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(Generated + "/TestSnow.png"), tileSize = new Vector2(2, 2), smoothness = 0.2f };
            AssetDatabase.CreateAsset(layer, Generated + "/TestSnow.terrainlayer");
            data.terrainLayers = new[] { layer };
            GameObject terrainGO = Terrain.CreateTerrainGameObject(data);
            terrainGO.name = "Footprint Test Terrain";
            var terrain = terrainGO.GetComponent<Terrain>();
            var terrainMaterial = new Material(Shader.Find("Universal Render Pipeline/Terrain/Lit"));
            AssetDatabase.CreateAsset(terrainMaterial, Generated + "/TestTerrain.mat");
            terrain.materialTemplate = terrainMaterial;
            var sun = new GameObject("Sun").AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 1.4f;
            sun.transform.rotation = Quaternion.Euler(30, -40, 0);
            RenderSettings.sun = sun;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.4f, 0.43f, 0.5f);
            var camera = new GameObject("Main Camera").AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.transform.position = new Vector3(8, 2.2f, 5.3f);
            camera.transform.LookAt(new Vector3(8, 0, 8));
            camera.fieldOfView = 45;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.16f, 0.18f, 0.23f);
            camera.nearClipPlane = 0.05f;
            camera.gameObject.AddComponent<UniversalAdditionalCameraData>();
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(emitterAsset);
            instance.transform.position = new Vector3(8, 0.15f, 8);
            instance.GetComponent<TerrainFootprintEmitter>().targetTerrain = terrain;
            PrefabUtility.RecordPrefabInstancePropertyModifications(instance.GetComponent<TerrainFootprintEmitter>());
            PrefabUtility.RecordPrefabInstancePropertyModifications(instance.transform);
            // Static preview stamps also make the scene useful before the user has a player controller.
            for (int i = 0; i < 6; i++)
            {
                var preview = (GameObject)PrefabUtility.InstantiatePrefab(projectorAsset);
                preview.name = "Preview Footprint " + (i + 1);
                preview.transform.position = new Vector3(8 + (i % 2 == 0 ? -0.1f : 0.1f), 0.005f, 7.1f + i * 0.32f);
                preview.transform.rotation = Quaternion.LookRotation(Vector3.down, Vector3.forward);
                preview.GetComponent<DecalProjector>().material = i % 2 == 0 ? left : right;
                PrefabUtility.RecordPrefabInstancePropertyModifications(preview.transform);
                PrefabUtility.RecordPrefabInstancePropertyModifications(preview.GetComponent<DecalProjector>());
            }
            EditorSceneManager.SaveScene(scene, Generated + "/FootprintTest.unity");
            AssetDatabase.SaveAssets();
            Debug.Log("Footprint assets and isolated test scene created. Test pipeline: " + Generated + "/FootprintTestURP.asset");
        }

        static float Profile(float u, float v)
        {
            // Original procedural boot sole, independent of GlobalSnow art. UV up = toes.
            float x = (u - 0.5f) * 2f;
            float y = (v - 0.5f) * 2f;
            float toe = Mathf.Sqrt(Mathf.Pow((x + 0.04f) / 0.79f, 2) + Mathf.Pow((y - 0.30f) / 0.55f, 2));
            float heel = Mathf.Sqrt(Mathf.Pow((x - 0.06f) / 0.56f, 2) + Mathf.Pow((y + 0.55f) / 0.30f, 2));
            float bridge = Mathf.Max(Mathf.Abs((x - 0.06f) / 0.51f), Mathf.Abs((y + 0.13f) / 0.38f));
            float edge = Mathf.Min(toe, Mathf.Min(heel, bridge));
            float shape = 1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(0.80f, 1.04f, edge));
            float tread = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(0.78f, 0.96f, Mathf.Cos(y * 41 + Mathf.Abs(x) * 5)));
            return shape * (1 - 0.12f * tread);
        }

        static Material MakeMaterial(Shader shader, bool left)
        {
            const int w = 256, h = 512;
            var color = new Texture2D(w, h, TextureFormat.RGBA32, true, false);
            var normal = new Texture2D(w, h, TextureFormat.RGBA32, true, true);
            var colors = new Color[w * h];
            var normals = new Color[w * h];
            Func<float, float, float> height = (u, v) => -0.007f * Profile(left ? u : 1 - u, v);
            for (int y = 0; y < h; y++) for (int x = 0; x < w; x++)
            {
                float u = (x + 0.5f) / w, v = (y + 0.5f) / h;
                float depth = Profile(left ? u : 1 - u, v);
                float dx = (height(u + 1f / w, v) - height(u - 1f / w, v)) / (2f * 0.16f / w);
                float dy = (height(u, v + 1f / h) - height(u, v - 1f / h)) / (2f * 0.32f / h);
                Vector3 n = new Vector3(-dx, -dy, 1).normalized;
                colors[y * w + x] = new Color(0.40f, 0.46f, 0.54f, depth * 0.5f);
                normals[y * w + x] = new Color(n.x * 0.5f + 0.5f, n.y * 0.5f + 0.5f, n.z * 0.5f + 0.5f, 1);
            }
            color.SetPixels(colors); color.Apply(); normal.SetPixels(normals); normal.Apply();
            string name = left ? "Left" : "Right";
            string cp = Generated + "/Sole" + name + ".png", np = Generated + "/Sole" + name + "Normal.png";
            File.WriteAllBytes(cp, color.EncodeToPNG()); File.WriteAllBytes(np, normal.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(color); UnityEngine.Object.DestroyImmediate(normal);
            AssetDatabase.ImportAsset(cp); AssetDatabase.ImportAsset(np);
            var ci = (TextureImporter)AssetImporter.GetAtPath(cp);
            ci.wrapMode = TextureWrapMode.Clamp; ci.alphaSource = TextureImporterAlphaSource.FromInput;
            ci.alphaIsTransparency = true; ci.textureCompression = TextureImporterCompression.Uncompressed; ci.SaveAndReimport();
            var ni = (TextureImporter)AssetImporter.GetAtPath(np);
            ni.textureType = TextureImporterType.NormalMap; ni.wrapMode = TextureWrapMode.Clamp;
            ni.textureCompression = TextureImporterCompression.Uncompressed; ni.SaveAndReimport();
            var mat = new Material(shader) { name = "Footprint " + name, enableInstancing = true };
            mat.SetTexture("Base_Map", AssetDatabase.LoadAssetAtPath<Texture2D>(cp));
            mat.SetTexture("Normal_Map", AssetDatabase.LoadAssetAtPath<Texture2D>(np));
            mat.SetFloat("Normal_Blend", 1);
            AssetDatabase.CreateAsset(mat, Generated + "/Footprint" + name + ".mat");
            return mat;
        }

        static void AddDecals(UniversalRendererData renderer)
        {
            foreach (var f in renderer.rendererFeatures) if (f is DecalRendererFeature) return;
            var feature = ScriptableObject.CreateInstance<DecalRendererFeature>();
            feature.name = "Terrain Footprint Decals";
            AssetDatabase.AddObjectToAsset(feature, renderer);
            var so = new SerializedObject(feature);
            so.FindProperty("m_Settings.technique").enumValueIndex = 1; // DBuffer: albedo + normal support.
            so.FindProperty("m_Settings.dBufferSettings.surfaceData").enumValueIndex = 1;
            so.ApplyModifiedPropertiesWithoutUndo();
            renderer.rendererFeatures.Add(feature);
            EditorUtility.SetDirty(renderer);
            renderer.SetDirty();
            AssetDatabase.SaveAssets();
        }

        [MenuItem("Tools/Terrain Footprints/Add Decal Feature to Selected Renderer")]
        public static void EnableSelectedRenderer()
        {
            var renderer = Selection.activeObject as UniversalRendererData;
            if (renderer == null) { EditorUtility.DisplayDialog("Footprints", "Select the Universal Renderer Data used by your camera in the Project window first.", "OK"); return; }
            foreach (var f in renderer.rendererFeatures)
                if (f is DecalRendererFeature) { EditorUtility.DisplayDialog("Footprints", "Decal feature already exists. Enable it and use DBuffer with Albedo Normal. Existing settings were preserved.", "OK"); return; }
            if (!EditorUtility.DisplayDialog("Enable decals", "Add one Decal Renderer Feature to " + AssetDatabase.GetAssetPath(renderer) + "? This affects cameras using this renderer; terrain materials and GlobalSnow stay unchanged.", "Add", "Cancel")) return;
            Undo.RecordObject(renderer, "Add footprint decals");
            AddDecals(renderer);
        }

        [MenuItem("Tools/Terrain Footprints/Add Emitter to Selected Terrain")]
        public static void AddEmitter()
        {
            var terrain = Selection.activeGameObject != null ? Selection.activeGameObject.GetComponent<Terrain>() : null;
            if (terrain == null) { EditorUtility.DisplayDialog("Footprints", "Select the GameObject with the native Terrain component.", "OK"); return; }
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Generated + "/FootprintEmitter.prefab");
            if (prefab == null) { EditorUtility.DisplayDialog("Footprints", "Run Create Assets and Test Scene first.", "OK"); return; }
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, terrain.gameObject.scene);
            Undo.RegisterCreatedObjectUndo(go, "Add terrain footprint emitter");
            Vector3 pos = terrain.transform.position + terrain.terrainData.size * 0.5f;
            pos.y = terrain.SampleHeight(pos) + terrain.transform.position.y + 0.15f;
            go.transform.position = pos;
            go.GetComponent<TerrainFootprintEmitter>().targetTerrain = terrain;
            PrefabUtility.RecordPrefabInstancePropertyModifications(go.GetComponent<TerrainFootprintEmitter>());
            PrefabUtility.RecordPrefabInstancePropertyModifications(go.transform);
            Selection.activeGameObject = go;
            EditorSceneManager.MarkSceneDirty(go.scene);
        }
    }

    [CustomEditor(typeof(TerrainFootprintEmitter))]
    public sealed class FootprintEmitterInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var emitter = (TerrainFootprintEmitter)target;
            EditorGUILayout.HelpBox("Assign Target Terrain. For walking, Follow Target = player feet/root, and assign its CharacterController. In Play Mode, Stamp Test Pair works without a player. Visual indentation only; Terrain height/collision are unchanged.", MessageType.Info);
            using (new EditorGUI.DisabledScope(!Application.isPlaying))
            {
                if (GUILayout.Button("Stamp Test Pair / 盖一对测试脚印")) emitter.StampTestPair();
                if (GUILayout.Button("Clear Footprints / 清除脚印")) emitter.ClearFootprints();
            }
        }
    }
}
