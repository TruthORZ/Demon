using System;
using System.IO;
using System.Linq;
using Demon.Slice;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.AI;

namespace StandaloneTerrainFootprints.Editor
{
    // Only runs when explicitly invoked; uses the saved production scene and real chase controller.
    [InitializeOnLoad]
    public static class DemonFootprintVerification
    {
        const string Key = "DemonFootprintVerification";
        const string Logs = "Logs/DemonCheckpoints/task025-footprint-merge";
        static int lastFrame = -1;
        static double phaseStart;
        static int count;
        static Vector3 lastPrintPosition;
        static DecalProjector trackedPrint;
        static Camera captureCamera;
        static DemonFootprintVerification()
        {
            EditorApplication.update += Tick;
            EditorApplication.playModeStateChanged += Changed;
        }

        public static void RunBatch() => Run(true);
        public static void VerifyExistingBatch() => Run(false);

        static void Run(bool authoring)
        {
            try
            {
                Directory.CreateDirectory(Logs);
                File.WriteAllText(Logs + "/verification.txt", "RUNNING\n");
                if (authoring) DemonFootprintIntegration.Connect();
                else
                {
                    EditorSceneManager.OpenScene(DemonFootprintIntegration.ScenePath);
                    DemonFootprintIntegration.Validate();
                }
                // Verify an actual saved, human-editable setting, then restore it before Play.
                if (authoring)
                {
                var emitter = UnityEngine.Object.FindAnyObjectByType<DemonFootprints>().Emitter;
                var originalSize = emitter.footprintSize;
                emitter.footprintSize = new Vector2(.43f, .9f);
                EditorSceneManager.MarkSceneDirty(emitter.gameObject.scene);
                EditorSceneManager.SaveScene(emitter.gameObject.scene);
                EditorSceneManager.OpenScene(DemonFootprintIntegration.ScenePath);
                emitter = UnityEngine.Object.FindAnyObjectByType<DemonFootprints>().Emitter;
                Require(emitter.footprintSize == new Vector2(.43f, .9f), "Footprint dimensions survive save/reload.");
                emitter.footprintSize = originalSize;
                EditorSceneManager.MarkSceneDirty(emitter.gameObject.scene);
                EditorSceneManager.SaveScene(emitter.gameObject.scene);
                }
                SessionState.SetInt(Key, 1);
                SessionState.SetFloat(Key + "Start", (float)EditorApplication.timeSinceStartup);
                phaseStart = EditorApplication.timeSinceStartup;
                EditorApplication.EnterPlaymode();
            }
            catch (Exception e) { Fail(e); }
        }

        static void Tick()
        {
            int phase = SessionState.GetInt(Key, 0);
            if (phase == 0 || phase == 99) return;
            if (EditorApplication.timeSinceStartup - SessionState.GetFloat(Key + "Start", 0) > 240)
            { Fail(new Exception("Runtime verification timed out at phase " + phase)); return; }
            if (!EditorApplication.isPlaying || Time.time < 1 || Time.frameCount == lastFrame) return;
            lastFrame = Time.frameCount;
            double elapsed = EditorApplication.timeSinceStartup - phaseStart;
            try
            {
                var world = UnityEngine.Object.FindAnyObjectByType<GameSceneController>();
                var adapter = UnityEngine.Object.FindAnyObjectByType<DemonFootprints>();
                var emitter = adapter.Emitter;
                var menu = UnityEngine.Object.FindAnyObjectByType<DayJumpMenu>();
                if (phase == 1)
                {
                    Require(world.Session.Clock.Day == 1 && emitter.TotalStamped == 0, "D1 has no demon footprints.");
                    Require(!world.Player.GetComponentInChildren<TerrainFootprintEmitter>(true), "Player has no footprint emitter.");
                    // Move the real player while the demon is dormant; only the demon may emit.
                    var body = world.Player.GetComponent<CharacterController>();
                    body.Move(Vector3.forward * .8f);
                    Next(2);
                }
                else if (phase == 2 && elapsed > .5)
                {
                    Require(emitter.TotalStamped == 0, "Player movement creates no prints.");
                    menu.JumpToDay(3);
                    Require(world.Session.Clock.Day == 3, "Existing day menu enters D3.");
                    Next(11);
                }
                else if (phase == 11 && world.Chase.HasNavigation)
                {
                    PlacePlayerInSight(world);
                    Next(3);
                }
                else if (phase == 3)
                {
                    if (emitter.ActiveCount < 4)
                    {
                        if (elapsed > 30) throw new Exception("Real chase did not stamp: phase=" + world.Chase.Phase + ", position=" + world.Chase.Agent.transform.position + ", velocity=" + world.Chase.Agent.velocity + ", prints=" + emitter.TotalStamped);
                        return;
                    }
                    Require(world.Chase.HasNavigation && emitter.TotalStamped >= 4, "Real NavMesh chase produces Terrain decals.");
                    var prints = UnityEngine.Object.FindObjectsByType<DecalProjector>();
                    Require(prints.Any(p => p.material == emitter.leftMaterial) && prints.Any(p => p.material == emitter.rightMaterial), "Chase alternates left/right material.");
                    trackedPrint = prints[0]; lastPrintPosition = trackedPrint.transform.position;
                    count = emitter.TotalStamped;
                    Next(4);
                }
                else if (phase == 4 && emitter.TotalStamped > count)
                {
                    Require(Vector3.Distance(lastPrintPosition, trackedPrint.transform.position) < .001f, "Old footprints remain fixed as demon moves.");
                    // Exercise production frozen state, without synthetic mouse input in Batch Mode.
                    world.Tether.enabled = false;
                    world.Tether.Rule.Advance(true, world.Tether.Definition.StableSeconds + .1f);
                    Next(5);
                }
                else if (phase == 5 && elapsed > .25)
                {
                    Require(world.Chase.Phase == ChaseController.ChasePhase.Frozen, "Motion exchange freezes actual chase.");
                    count = emitter.TotalStamped;
                    Capture(emitter);
                    Next(6);
                }
                else if (phase == 6 && elapsed > 1)
                {
                    Require(emitter.TotalStamped == count, "Frozen demon adds no footprints.");
                    world.Tether.Rule.Release(); world.Tether.enabled = true;
                    Next(7);
                }
                else if (phase == 7 && emitter.TotalStamped > count)
                {
                    Require(true, "Release resumes real chase footprints.");
                    world.Chase.enabled = false; count = emitter.TotalStamped;
                    Next(8);
                }
                else if (phase == 8 && elapsed > 1)
                {
                    Require(emitter.TotalStamped == count, "Disabled chase adds no footprints.");
                    world.Chase.enabled = true;
                    menu.Open(); count = emitter.TotalStamped;
                    Next(9);
                }
                else if (phase == 9 && elapsed > .5)
                {
                    Require(emitter.TotalStamped == count, "Paused menu adds no footprints.");
                    menu.Close();
                    Require(emitter.ActiveCount > 0, "D3 has a trail before retry.");
                    world.Session.Clock.AdvanceTo(world.Session.Clock.NextMidnight);
                    Require(world.Playthrough.Failed, "Incomplete D3 reaches the retry screen.");
                    world.Playthrough.RetryDay();
                    Require(world.Session.Clock.Day == 3 && !world.Playthrough.Failed && emitter.ActiveCount == 0, "Retry restores D3 and clears the old trail.");
                    menu.JumpToDay(1);
                    Require(emitter.ActiveCount == 0, "Actual session reset clears old demon trail.");
                    count = emitter.TotalStamped; Next(10);
                }
                else if (phase == 10 && elapsed > .5)
                {
                    Require(emitter.TotalStamped == count, "Restarted D1 remains footprint-free.");
                    Require(!ShaderUtil.GetShaderMessages(emitter.leftMaterial.shader).Any(m => m.severity.ToString() == "Error"), "Decal shader renders without compiler errors.");
                    File.AppendAllText(Logs + "/verification.txt", "PASS\n");
                    SessionState.SetInt(Key + "Exit", 0); SessionState.SetInt(Key, 99);
                    EditorApplication.ExitPlaymode();
                }
            }
            catch (Exception e) { Fail(e); }
        }

        static void PlacePlayerInSight(GameSceneController world)
        {
            var agent = world.Chase.Agent;
            var body = world.Player.GetComponent<CharacterController>();
            var mesh = NavMesh.CalculateTriangulation();
            var terrain = UnityEngine.Object.FindAnyObjectByType<DemonFootprints>().Emitter.targetTerrain;
            for (int i = 0; i < mesh.indices.Length; i += 3)
            {
                var start = (mesh.vertices[mesh.indices[i]] + mesh.vertices[mesh.indices[i + 1]] + mesh.vertices[mesh.indices[i + 2]]) / 3;
                if (world.Playthrough.InsideCabin(start) || !OnTerrain(start, terrain)) continue;
                for (int angle = 0; angle < 360; angle += 45)
                {
                    var point = start + Quaternion.Euler(0, angle, 0) * Vector3.forward * 15;
                    if (!NavMesh.SamplePosition(point, out var hit, 4, agent.areaMask) || world.Playthrough.InsideCabin(hit.position)
                        || !OnTerrain(hit.position, terrain) || NavMesh.Raycast(start, hit.position, out _, agent.areaMask)) continue;
                    if (!agent.Warp(start)) continue;
                    body.enabled = false;
                    world.Player.transform.position = hit.position + Vector3.up * .15f;
                    body.enabled = true;
                    Physics.SyncTransforms();
                    if (world.Chase.HasSightToPlayer()) return;
                }
            }
            throw new InvalidOperationException("Could not place player on an outdoor chase route in sight of demon.");
        }

        static bool OnTerrain(Vector3 point, Terrain terrain)
        {
            return Physics.Raycast(point + Vector3.up, Vector3.down, out var hit, 2f, ~0, QueryTriggerInteraction.Ignore)
                && hit.collider == terrain.GetComponent<TerrainCollider>();
        }

        static void Next(int phase) { SessionState.SetInt(Key, phase); phaseStart = EditorApplication.timeSinceStartup; }

        static void Capture(TerrainFootprintEmitter emitter)
        {
            var prints = UnityEngine.Object.FindObjectsByType<DecalProjector>();
            Vector3 center = prints.Aggregate(Vector3.zero, (s, p) => s + p.transform.position) / prints.Length;
            captureCamera = new GameObject("Footprint verification camera").AddComponent<Camera>();
            captureCamera.gameObject.AddComponent<UniversalAdditionalCameraData>();
            captureCamera.transform.position = center + new Vector3(0, 5, -5);
            captureCamera.transform.LookAt(center);
            captureCamera.fieldOfView = 60;
            captureCamera.farClipPlane = 120;
            captureCamera.clearFlags = CameraClearFlags.SolidColor;
            captureCamera.backgroundColor = Color.gray;
            Render("DemonFootprints-On.png");
            foreach (var p in prints) Require(p.IsValid(), "Valid decal pass after real PC renderer initialization.");
            foreach (var p in prints) p.enabled = false;
            Render("DemonFootprints-Off.png");
            foreach (var p in prints) p.enabled = true;
            UnityEngine.Object.Destroy(captureCamera.gameObject);
        }

        static void Render(string file)
        {
            var rt = new RenderTexture(1280, 900, 24, RenderTextureFormat.ARGB32);
            rt.Create();
            var old = RenderTexture.active;
            var texture = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
            try
            {
                captureCamera.targetTexture = rt; captureCamera.Render();
                RenderTexture.active = rt;
                texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0); texture.Apply();
                File.WriteAllBytes(Logs + "/" + file, texture.EncodeToPNG());
            }
            finally
            {
                captureCamera.targetTexture = null; RenderTexture.active = old;
                UnityEngine.Object.DestroyImmediate(texture); rt.Release(); UnityEngine.Object.DestroyImmediate(rt);
            }
        }

        static void Require(bool condition, string message)
        {
            if (!condition) throw new Exception(message);
            File.AppendAllText(Logs + "/verification.txt", "OK: " + message + "\n");
        }
        static void Fail(Exception e)
        {
            Directory.CreateDirectory(Logs);
            File.AppendAllText(Logs + "/verification.txt", "FAIL: " + e + "\n"); Debug.LogException(e);
            SessionState.SetInt(Key + "Exit", 1); SessionState.SetInt(Key, 99);
            if (EditorApplication.isPlaying) EditorApplication.ExitPlaymode(); else Finish();
        }
        static void Changed(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode && SessionState.GetInt(Key, 0) == 99) Finish();
        }
        static void Finish()
        {
            Time.timeScale = 1; AudioListener.pause = false;
            // Reload saved scene to discard all temporary runtime verification changes.
            EditorSceneManager.OpenScene(DemonFootprintIntegration.ScenePath);
            int result = SessionState.GetInt(Key + "Exit", 1); SessionState.SetInt(Key, 0);
            EditorApplication.Exit(result);
        }
    }
}
