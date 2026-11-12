using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace StandaloneTerrainFootprints.Editor
{
    // Explicit batch verification only. Never runs in a normal Editor session.
    [InitializeOnLoad]
    public static class FootprintVerification
    {
        const string Key = "StandaloneFootprintVerification";
        static int lastFrame = -1;
        static FootprintVerification()
        {
            EditorApplication.update += Tick;
            EditorApplication.playModeStateChanged += Changed;
        }

        public static void RunBatch()
        {
            try
            {
                FootprintSetup.Build();
                Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(FootprintSetup.Root + "/FootprintDecal.shadergraph");
                var errors = ShaderUtil.GetShaderMessages(shader).Where(m => m.severity.ToString() == "Error").ToArray();
                if (errors.Length > 0) throw new Exception(string.Join("\n", errors.Select(m => m.message)));
                SavePipeline("Quality", QualitySettings.renderPipeline);
                SavePipeline("Graphics", GraphicsSettings.defaultRenderPipeline);
                SessionState.SetBool(Key + "Saved", true);
                var testPipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(FootprintSetup.Root + "/Generated/FootprintTestURP.asset");
                QualitySettings.renderPipeline = testPipeline;
                GraphicsSettings.defaultRenderPipeline = testPipeline;
                SessionState.SetInt(Key, 1);
                SessionState.SetFloat(Key + "Start", (float)EditorApplication.timeSinceStartup);
                EditorSceneManager.OpenScene(FootprintSetup.Root + "/Generated/FootprintTest.unity");
                EditorApplication.EnterPlaymode();
            }
            catch (Exception e) { Fail(e); }
        }

        static void SavePipeline(string name, RenderPipelineAsset pipeline)
        {
            SessionState.SetString(Key + name, pipeline == null ? "" : AssetDatabase.GetAssetPath(pipeline));
        }

        static void Tick()
        {
            int phase = SessionState.GetInt(Key, 0);
            if (phase == 0 || phase == 99) return;
            if (EditorApplication.timeSinceStartup - SessionState.GetFloat(Key + "Start", 0) > 180)
            { Fail(new Exception("Timed out during Play Mode verification.")); return; }
            if (!EditorApplication.isPlaying || Time.time < 1) return;
            if (Time.frameCount == lastFrame) return;
            lastFrame = Time.frameCount;
            try
            {
                var emitter = UnityEngine.Object.FindAnyObjectByType<TerrainFootprintEmitter>();
                if (emitter == null) throw new Exception("Test emitter missing.");
                if (phase == 1)
                {
                    emitter.automaticSteps = false;
                    emitter.StampTestPair();
                    Require(emitter.ActiveCount == 2, "Manual pair creates two projectors.");
                    Require(!emitter.StampAt(new Vector3(-10, 0.1f, -10), Vector3.forward, true), "Off-terrain hit rejected.");
                    emitter.snowLayerIndex = 10;
                    Require(!emitter.StampAt(emitter.transform.position, Vector3.forward, true), "Invalid snow layer rejected.");
                    emitter.snowLayerIndex = -1;
                    // Batch Mode has no Game view: initialize the SRP with an actual render first.
                    Capture("Footprints-Warmup.png");
                    foreach (var p in UnityEngine.Object.FindObjectsByType<DecalProjector>())
                        Require(p.IsValid(), "Decal material is valid: " + p.name + " / " + p.material?.shader.name + " / passes=" + p.material?.passCount + " / pipeline=" + GraphicsSettings.currentRenderPipeline?.name);
                    SessionState.SetInt(Key, 2);
                }
                else if (phase == 2)
                {
                    Capture("Footprints-On.png");
                    foreach (var projector in UnityEngine.Object.FindObjectsByType<DecalProjector>()) projector.enabled = false;
                    SessionState.SetInt(Key, 3);
                }
                else if (phase == 3)
                {
                    Capture("Footprints-Off.png");
                    foreach (var projector in UnityEngine.Object.FindObjectsByType<DecalProjector>()) projector.enabled = true;
                    for (int i = 0; i < 60; i++) emitter.StampAt(emitter.transform.position + Vector3.forward * (i % 4) * 0.1f, Vector3.forward, i % 2 == 0);
                    Require(emitter.AllocatedCount == emitter.capacity && emitter.ActiveCount <= emitter.capacity, "Pool remains bounded.");
                    emitter.ClearFootprints();
                    Require(emitter.ActiveCount == 0, "Clear removes active footprints.");
                    emitter.automaticSteps = true;
                    SessionState.SetInt(Key, 4);
                }
                else if (phase == 4)
                {
                    SessionState.SetInt(Key + "Count", emitter.TotalStamped);
                    emitter.transform.position += Vector3.forward * 0.8f;
                    SessionState.SetInt(Key, 5);
                }
                else if (phase == 5)
                {
                    Require(emitter.TotalStamped == SessionState.GetInt(Key + "Count", 0) + 1, "Movement creates one step.");
                    SessionState.SetInt(Key + "Count", emitter.TotalStamped);
                    SessionState.SetFloat(Key + "Stationary", Time.time);
                    SessionState.SetInt(Key, 6);
                }
                else if (phase == 6 && Time.time - SessionState.GetFloat(Key + "Stationary", 0) > 0.3f)
                {
                    Require(emitter.TotalStamped == SessionState.GetInt(Key + "Count", 0), "Stationary source creates no steps.");
                    emitter.transform.position += Vector3.forward * 4;
                    SessionState.SetInt(Key, 7);
                }
                else if (phase == 7)
                {
                    Require(emitter.TotalStamped == SessionState.GetInt(Key + "Count", 0), "Teleport creates no trail.");
                    emitter.lifetime = 0.2f;
                    SessionState.SetInt(Key, 8);
                }
                else if (phase == 8 && emitter.ActiveCount == 0)
                {
                    emitter.enabled = false;
                    emitter.enabled = true;
                    emitter.transform.position = new Vector3(8, 0.15f, 8);
                    emitter.StampTestPair();
                    Require(emitter.ActiveCount == 2, "Disable/re-enable releases and recreates pool.");
                    Shader shader = emitter.leftMaterial.shader;
                    Require(!ShaderUtil.GetShaderMessages(shader).Any(m => m.severity.ToString() == "Error"), "Shader has no compiler errors after rendering.");
                    File.WriteAllText("Logs/Footprint-Verification.txt", "PASS: shader/material import; manual pair; terrain/layer rejection; bounded pool; clear; movement; stationary; teleport; expiry; enable lifecycle; rendered on/off captures.\n");
                    SessionState.SetInt(Key + "Exit", 0);
                    SessionState.SetInt(Key, 99);
                    EditorApplication.ExitPlaymode();
                }
            }
            catch (Exception e) { Fail(e); }
        }

        static void Capture(string name)
        {
            Camera camera = Camera.main;
            var rt = new RenderTexture(1024, 768, 24, RenderTextureFormat.ARGB32);
            rt.Create();
            var oldTarget = camera.targetTexture;
            var oldActive = RenderTexture.active;
            var texture = new Texture2D(1024, 768, TextureFormat.RGB24, false);
            try
            {
                camera.targetTexture = rt;
                camera.Render();
                RenderTexture.active = rt;
                texture.ReadPixels(new Rect(0, 0, 1024, 768), 0, 0);
                texture.Apply();
                Directory.CreateDirectory("Logs/FootprintPreview");
                File.WriteAllBytes("Logs/FootprintPreview/" + name, texture.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = oldTarget;
                RenderTexture.active = oldActive;
                UnityEngine.Object.DestroyImmediate(texture);
                rt.Release(); UnityEngine.Object.DestroyImmediate(rt);
            }
        }

        static void Require(bool value, string message)
        {
            if (!value) throw new Exception(message);
            Debug.Log("Footprint check PASS: " + message);
        }

        static void Fail(Exception e)
        {
            Debug.LogException(e);
            Directory.CreateDirectory("Logs");
            File.WriteAllText("Logs/Footprint-Verification.txt", "FAIL: " + e);
            SessionState.SetInt(Key + "Exit", 1);
            SessionState.SetInt(Key, 99);
            if (EditorApplication.isPlaying) EditorApplication.ExitPlaymode(); else Finish();
        }

        static void Changed(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode && SessionState.GetInt(Key, 0) == 99) Finish();
        }

        static void Finish()
        {
            if (SessionState.GetBool(Key + "Saved", false))
            {
                QualitySettings.renderPipeline = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(SessionState.GetString(Key + "Quality", ""));
                GraphicsSettings.defaultRenderPipeline = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(SessionState.GetString(Key + "Graphics", ""));
            }
            int result = SessionState.GetInt(Key + "Exit", 1);
            SessionState.SetInt(Key, 0);
            EditorApplication.Exit(result);
        }
    }
}
