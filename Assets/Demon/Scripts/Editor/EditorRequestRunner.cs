using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Demon.Slice.Domain;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;

namespace Demon.Slice.Editor
{
    // A bounded authoring request, not a runtime bootstrap. No request means no scene or asset writes.
    // Operations and output paths are fixed. A consumed request is never repeated after a domain reload.
    [InitializeOnLoad]
    public static class EditorRequestRunner
    {
        const string RequestPath = "Temp/DemonSliceAuthoring.request.json";
        const string ReportDirectory = "Temp/DemonSliceReports";
        const string ActiveSmokeKey = "Demon.Slice.ActiveSmoke";
        static double nextPoll;
        static ISmokeRun activeRun;
        static TestRunnerApi testApi;
        [Serializable] sealed class Request { public string id; public string operation; }
        [Serializable] sealed class Report
        {
            public string id;
            public string operation;
            public bool success;
            public string message;
            public int frame;
        }

        static EditorRequestRunner()
        {
            EditorApplication.update += Poll;
            EditorApplication.playModeStateChanged += OnPlayState;
        }

        static void Poll()
        {
            if (activeRun != null) { activeRun.Tick(); return; }
            if (EditorApplication.timeSinceStartup < nextPoll || EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            nextPoll = EditorApplication.timeSinceStartup + .5;
            if (!File.Exists(RequestPath)) return;
            Request request = null;
            try
            {
                request = JsonUtility.FromJson<Request>(File.ReadAllText(RequestPath));
                File.Delete(RequestPath);
                if (request == null || request.id == null || !Regex.IsMatch(request.id, "^[a-zA-Z0-9-]{1,64}$"))
                    throw new InvalidOperationException("Invalid authoring request ID.");
                if (EditorApplication.isPlayingOrWillChangePlaymode)
                    throw new InvalidOperationException("Authoring requests cannot interrupt an existing Play session.");
                switch (request.operation)
                {
                    case "day-jump-inspect": Finish(request, true, DayJumpMenuAuthoring.Inspect()); break;
                    case "day-jump-input": Finish(request, true, DayJumpMenuAuthoring.RepairInput()); break;
                    case "day-jump-editability": Finish(request, true, DayJumpMenuAuthoring.ProbeEditability()); break;
                    case "day-jump-hint": Finish(request, true, DayJumpMenuAuthoring.PlaceHint()); break;
                    case "day-jump-create": Finish(request, true, DayJumpMenuAuthoring.Create()); break;
                    case "day-jump-validate": DayJumpMenuAuthoring.OpenSaved(); Finish(request, true, DayJumpMenuAuthoring.Validate()); break;
                    case "day-jump-smoke":
                        DayJumpMenuAuthoring.OpenSaved();
                        SessionState.SetString(ActiveSmokeKey, JsonUtility.ToJson(request)); EditorApplication.EnterPlaymode(); break;
                    case "arch-runtime":
                        OpenNine(); ArchitectureRefactorChecks.BeginProbe();
                        SessionState.SetString(ActiveSmokeKey, JsonUtility.ToJson(request)); EditorApplication.EnterPlaymode(); break;
                    case "arch-validate": Finish(request, true, ArchitectureRefactorChecks.Validate()); break;
                    case "arch-save-guides": Finish(request, true, ArchitectureRefactorChecks.SaveGuideRules()); break;
                    case "refresh":
                        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                        UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
                        Finish(request, true, "Requested asset refresh and script compilation; wait for the Editor to finish.");
                        break;
                    case "create": PrototypeSceneAuthoring.CreateScene(); Finish(request, true, "Created and saved " + PrototypeSceneAuthoring.ScenePath); break;
                    case "validate": OpenSlice(); Finish(request, true, PrototypeSceneAuthoring.ValidateSavedScene()); break;
                    case "fonts": OpenSlice(); Finish(request, true, PrototypeSceneAuthoring.AddFontBinding()); break;
                    case "repair-box": OpenSlice(); Finish(request, true, PrototypeSceneAuthoring.RepairBoxBinding()); break;
                    case "build": Finish(request, true, PrototypeSceneAuthoring.BuildWindows()); break;
                    case "nine-create": NineDayAuthoring.Create(); Finish(request, true, NineDayAuthoring.Validate()); break;
                    case "nine-validate": OpenNine(); Finish(request, true, NineDayAuthoring.Validate()); break;
                    case "nine-repair": OpenNine(); Finish(request, true, NineDayAuthoring.RepairSettings()); break;
                    case "nine-build": Finish(request, true, NineDayAuthoring.Build()); break;
                    case "retrieval-smoke":
                    case "retrieval-late":
                    case "retrieval-sleep":
                    case "retrieval-tuning":
                        FinalTabletAuthoring.OpenQuick(); if (request.operation == "retrieval-tuning") FinalTabletAuthoring.BeginTuning();
                        SessionState.SetString(ActiveSmokeKey, JsonUtility.ToJson(request)); EditorApplication.EnterPlaymode(); break;
                    case "consequences-smoke":
                    case "consequences-skip":
                    case "consequences-late":
                    case "consequences-tuning":
                        ConsequencesAuthoring.OpenQuick(); if (request.operation == "consequences-late" || request.operation == "consequences-tuning") ConsequencesAuthoring.BeginProbe(request.operation == "consequences-late", request.operation == "consequences-tuning");
                        SessionState.SetString(ActiveSmokeKey, JsonUtility.ToJson(request)); EditorApplication.EnterPlaymode(); break;
                    case "exploration-smoke":
                    case "exploration-skip":
                    case "exploration-late":
                    case "exploration-tuning":
                        ExplorationAuthoring.OpenQuick(); if(request.operation == "exploration-late" || request.operation == "exploration-tuning") ExplorationAuthoring.BeginProbe(request.operation == "exploration-late", request.operation == "exploration-tuning");
                        SessionState.SetString(ActiveSmokeKey, JsonUtility.ToJson(request)); EditorApplication.EnterPlaymode(); break;
                    case "signal-smoke":
                    case "signal-boundary":
                    case "signal-skip":
                    case "signal-late":
                    case "signal-tuning":
                    case "signal-old-search":
                    case "signal-first-search":
                        SignalSearchAuthoring.OpenQuick();
                        if (request.operation != "signal-smoke" && request.operation != "signal-skip") SignalSearchAuthoring.BeginProbe(request.operation);
                        SessionState.SetString(ActiveSmokeKey, JsonUtility.ToJson(request)); EditorApplication.EnterPlaymode(); break;
                    case "prep-create": Finish(request, true, PreparationAuthoring.Create()); break;
                    case "prep-validate": PreparationAuthoring.OpenQuick(); Finish(request, true, PreparationAuthoring.Validate()); break;
                    case "prep-build": Finish(request, true, PreparationAuthoring.Build()); break;
                    case "prep-smoke":
                    case "prep-late":
                    case "prep-skip":
                    case "prep-boundary":
                    case "prep-tuning":
                    case "prep-first-search":
                    case "prep-old-search":
                    case "prep-signal-search":
                    case "prep-night":
                    case "prep-early":
                        PreparationAuthoring.OpenQuick();
                        if (request.operation != "prep-smoke" && request.operation != "prep-skip") PreparationAuthoring.BeginProbe(request.operation);
                        SessionState.SetString(ActiveSmokeKey, JsonUtility.ToJson(request)); EditorApplication.EnterPlaymode(); break;
                    case "signal-create": Finish(request, true, SignalSearchAuthoring.Create()); break;
                    case "signal-finish": Finish(request, true, SignalSearchAuthoring.FinishLayout()); break;
                    case "signal-settings": Finish(request, true, SignalSearchAuthoring.RepairSettings()); break;
                    case "signal-low-chime": Finish(request, true, SignalSearchAuthoring.AlignLowChime()); break;
                    case "signal-validate": SignalSearchAuthoring.OpenQuick(); Finish(request, true, SignalSearchAuthoring.Validate()); break;
                    case "signal-build": Finish(request, true, SignalSearchAuthoring.Build()); break;
                    case "investigation-review":
                    case "investigation-smoke":
                    case "investigation-boundary":
                    case "investigation-skip":
                    case "investigation-late":
                    case "investigation-tuning":
                        InvestigationAuthoring.OpenQuick();
                        if (request.operation == "investigation-boundary" || request.operation == "investigation-late" || request.operation == "investigation-tuning" || request.operation == "investigation-review") InvestigationAuthoring.BeginProbe(request.operation);
                        SessionState.SetString(ActiveSmokeKey, JsonUtility.ToJson(request)); EditorApplication.EnterPlaymode(); break;
                    case "investigation-create": Finish(request, true, InvestigationAuthoring.Create()); break;
                    case "investigation-records": Finish(request, true, InvestigationAuthoring.PolishRecords()); break;
                    case "investigation-ritual-view": Finish(request, true, InvestigationAuthoring.RepairRitualView()); break;
                    case "investigation-chase-hint": Finish(request, true, InvestigationAuthoring.AlignChaseHint()); break;
                    case "investigation-validate": InvestigationAuthoring.OpenQuick(); Finish(request, true, InvestigationAuthoring.Validate()); break;
                    case "investigation-build": Finish(request, true, InvestigationAuthoring.Build()); break;
                    case "exploration-create": Finish(request, true, ExplorationAuthoring.Create()); break;
                    case "exploration-snow-collision": Finish(request, true, ExplorationAuthoring.RepairSnowCollision()); break;
                    case "exploration-radio-approach": Finish(request, true, ExplorationAuthoring.RepairRadioApproach()); break;
                    case "exploration-validate": ExplorationAuthoring.OpenQuick(); Finish(request, true, ExplorationAuthoring.Validate()); break;
                    case "exploration-build": Finish(request, true, ExplorationAuthoring.Build()); break;
                    case "consequences-solar-view": Finish(request, true, ConsequencesAuthoring.AlignSolarView()); break;
                    case "consequences-create": Finish(request, true, ConsequencesAuthoring.Create()); break;
                    case "consequences-validate": ConsequencesAuthoring.OpenQuick(); Finish(request, true, ConsequencesAuthoring.Validate()); break;
                    case "consequences-build": Finish(request, true, ConsequencesAuthoring.Build()); break;
                    case "t07-order-wording": Finish(request, true, FinalTabletAuthoring.UpdateOperandWording()); break;
                    case "retrieval-view": Finish(request, true, FinalTabletAuthoring.RepairView()); break;
                    case "retrieval-lighting": Finish(request, true, FinalTabletAuthoring.AlignLighting()); break;
                    case "retrieval-repair": Finish(request, true, FinalTabletAuthoring.RepairQuickSettings()); break;
                    case "retrieval-create": Finish(request, true, FinalTabletAuthoring.Create()); break;
                    case "retrieval-validate": FinalTabletAuthoring.OpenQuick(); Finish(request, true, FinalTabletAuthoring.Validate()); break;
                    case "retrieval-build": Finish(request, true, FinalTabletAuthoring.Build()); break;
                    case "chase-create": Finish(request, true, ChaseAuthoring.Create()); break;
                    case "chase-validate": ChaseAuthoring.OpenQuick(); Finish(request, true, ChaseAuthoring.Validate()); break;
                    case "chase-build": Finish(request, true, ChaseAuthoring.Build()); break;
                    case "chase-smoke":
                    case "chase-tuning":
                        ChaseAuthoring.OpenQuick(); if (request.operation == "chase-tuning") ChaseAuthoring.BeginTuning();
                        SessionState.SetString(ActiveSmokeKey, JsonUtility.ToJson(request)); EditorApplication.EnterPlaymode(); break;
                    case "beam-records": Finish(request, true, BeamAuthoring.SyncMotionRecord()); break;
                    case "beam-view": Finish(request, true, BeamAuthoring.RepairView()); break;
                    case "beam-create": Finish(request, true, BeamAuthoring.Create()); break;
                    case "beam-validate": BeamAuthoring.OpenQuick(); Finish(request, true, BeamAuthoring.Validate()); break;
                    case "beam-build": Finish(request, true, BeamAuthoring.Build()); break;
                    case "beam-smoke":
                    case "beam-tuning":
                    case "beam-early":
                        if (request.operation == "beam-early") BeamAuthoring.OpenEarly(); else BeamAuthoring.OpenQuick();
                        if (request.operation == "beam-tuning") BeamAuthoring.BeginTuning();
                        SessionState.SetString(ActiveSmokeKey, JsonUtility.ToJson(request)); EditorApplication.EnterPlaymode(); break;
                    case "cipher-create": Finish(request, true, CipherAuthoring.Create()); break;
                    case "cipher-validate": CipherAuthoring.OpenQuick(); Finish(request, true, CipherAuthoring.Validate()); break;
                    case "cipher-build": Finish(request, true, CipherAuthoring.Build()); break;
                    case "cipher-smoke":
                    case "cipher-tuning":
                        CipherAuthoring.OpenQuick(); if (request.operation == "cipher-tuning") CipherAuthoring.BeginTuning();
                        SessionState.SetString(ActiveSmokeKey, JsonUtility.ToJson(request)); EditorApplication.EnterPlaymode(); break;
                    case "listening-readability": Finish(request, true, ListeningAuthoring.RepairReadability()); break;
                    case "listening-create": Finish(request, true, ListeningAuthoring.Create()); break;
                    case "listening-validate": ListeningAuthoring.OpenQuick(); Finish(request, true, ListeningAuthoring.Validate()); break;
                    case "listening-build": Finish(request, true, ListeningAuthoring.Build()); break;
                    case "listening-smoke":
                    case "listening-tuning":
                        ListeningAuthoring.OpenQuick();
                        if (request.operation == "listening-tuning") ListeningAuthoring.BeginTuning();
                        SessionState.SetString(ActiveSmokeKey, JsonUtility.ToJson(request)); EditorApplication.EnterPlaymode(); break;
                    case "finale-migrate": Finish(request, true, FinaleAuthoring.Migrate()); break;
                    case "finale-validate": OpenNine(); Finish(request, true, FinaleAuthoring.Validate()); break;
                    case "finale-sightline": Finish(request, true, FinaleAuthoring.RepairClockSightline()); break;
                    case "nine-normal":
                    case "nine-skip":
                    case "nine-tuning":
                    case "nine-late":
                    case "nine-sleep-reserved":
                    case "nine-finale-tuning":
                        OpenNine();
                        if (request.operation == "nine-finale-tuning")
                        {
                            var finale = UnityEngine.Object.FindAnyObjectByType<GameSceneController>().Finale;
                            SessionState.SetFloat("Demon.Finale.OriginalCountdown", finale.ClockCountdownSeconds);
                            SessionState.SetVector3("Demon.Finale.OriginalView", finale.ClockView.localPosition);
                            SessionState.SetBool("Demon.Finale.Restore", true);
                            var serialized = new SerializedObject(finale);
                            serialized.FindProperty("clockCountdownSeconds").floatValue = 3.5f;
                            serialized.ApplyModifiedProperties();
                            finale.ClockView.localPosition += Vector3.right * .08f;
                            EditorUtility.SetDirty(finale);
                            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
                        }
                        if (request.operation == "nine-tuning")
                        {
                            var definition = UnityEngine.Object.FindAnyObjectByType<GameSceneController>().Campaign.Definition;
                            SessionState.SetFloat("Demon.NineDay.OriginalWake", definition.Days[1].WakeHour);
                            SessionState.SetBool("Demon.NineDay.RestoreWake", true);
                            definition.Days[1].WakeHour = 10;
                            EditorUtility.SetDirty(definition); AssetDatabase.SaveAssets();
                        }
                        SessionState.SetString(ActiveSmokeKey, JsonUtility.ToJson(request));
                        EditorApplication.EnterPlaymode();
                        break;
                    case "tests":
                        testApi = ScriptableObject.CreateInstance<TestRunnerApi>();
                        testApi.RegisterCallbacks(new TestCallbacks(request));
                        testApi.Execute(new ExecutionSettings(new Filter { testMode = TestMode.EditMode, assemblyNames = new[] { "Demon.Slice.Tests" } }));
                        break;
                    case "smoke-normal":
                    case "smoke-sleep":
                    case "smoke-input":
                    case "smoke-editability":
                        OpenSlice();
                        if (request.operation == "smoke-editability") BeginTuningProbe();
                        SessionState.SetString(ActiveSmokeKey, JsonUtility.ToJson(request));
                        EditorApplication.EnterPlaymode();
                        break;
                    default: throw new InvalidOperationException("Unsupported bounded authoring operation.");
                }
            }
            catch (Exception exception)
            {
                Finish(request ?? new Request { id = "invalid", operation = "invalid" }, false, exception.ToString());
                Debug.LogException(exception);
            }
        }

        static void OpenSlice()
        {
            if (SceneManager.GetActiveScene().path == PrototypeSceneAuthoring.ScenePath) return;
            if (SceneManager.GetActiveScene().isDirty)
                throw new InvalidOperationException("Save the current scene before opening the slice.");
            EditorSceneManager.OpenScene(PrototypeSceneAuthoring.ScenePath);
        }

        static void OpenNine()
        {
            if (SceneManager.GetActiveScene().isDirty) throw new InvalidOperationException("Save the current scene first.");
            EditorSceneManager.OpenScene(NineDayAuthoring.ScenePath);
        }

        static void OnPlayState(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                string json = SessionState.GetString(ActiveSmokeKey, "");
                if (!string.IsNullOrEmpty(json))
                {
                    SessionState.EraseString(ActiveSmokeKey);
                    var request = JsonUtility.FromJson<Request>(json);
                    if (request.operation == "day-jump-smoke")
                        activeRun = new DayJumpMenuSmokeRun((success, message) => Finish(request, success, message));
                    else if (request.operation == "arch-runtime")
                        activeRun = new ArchitectureSmokeRun((success, message) => Finish(request, success, message));
                    else if (request.operation == "prep-first-search")
                        activeRun = new ExplorationSmokeRun("exploration-smoke", (success, message) => Finish(request, success, message));
                    else if (request.operation == "prep-old-search")
                        activeRun = new InvestigationSmokeRun("investigation-smoke", (success, message) => Finish(request, success, message));
                    else if (request.operation == "prep-signal-search")
                        activeRun = new SignalSearchSmokeRun("signal-smoke", (success, message) => Finish(request, success, message));
                    else if (request.operation == "prep-night")
                        activeRun = new FinalTabletSmokeRun("retrieval-late", (success, message) => Finish(request, success, message));
                    else if (request.operation.StartsWith("prep-"))
                        activeRun = new PreparationSmokeRun(request.operation, (success, message) => Finish(request, success, message));
                    else if (request.operation == "signal-old-search")
                        activeRun = new InvestigationSmokeRun("investigation-smoke", (success, message) => Finish(request, success, message));
                    else if (request.operation == "signal-first-search")
                        activeRun = new ExplorationSmokeRun("exploration-smoke", (success, message) => Finish(request, success, message));
                    else if (request.operation.StartsWith("signal-"))
                        activeRun = new SignalSearchSmokeRun(request.operation, (success, message) => Finish(request, success, message));
                    else if (request.operation.StartsWith("investigation-"))
                        activeRun = new InvestigationSmokeRun(request.operation, (success, message) => Finish(request, success, message));
                    else if (request.operation.StartsWith("exploration-"))
                        activeRun = new ExplorationSmokeRun(request.operation, (success, message) => Finish(request, success, message));
                    else if (request.operation.StartsWith("consequences-"))
                        activeRun = new ConsequencesSmokeRun(request.operation, (success, message) => Finish(request, success, message));
                    else if (request.operation.StartsWith("retrieval-"))
                        activeRun = new FinalTabletSmokeRun(request.operation, (success, message) => Finish(request, success, message));
                    else if (request.operation.StartsWith("chase-"))
                        activeRun = new ChaseSmokeRun(request.operation, (success, message) => Finish(request, success, message));
                    else if (request.operation.StartsWith("beam-"))
                        activeRun = new BeamSmokeRun(request.operation, (success, message) => Finish(request, success, message));
                    else if (request.operation.StartsWith("cipher-"))
                        activeRun = new CipherSmokeRun(request.operation, (success, message) => Finish(request, success, message));
                    else if (request.operation.StartsWith("listening-"))
                        activeRun = new ListeningSmokeRun(request.operation, (success, message) => Finish(request, success, message));
                    else if (request.operation.StartsWith("nine-"))
                        activeRun = new NineDaySmokeRun(request.operation, (success, message) => Finish(request, success, message));
                    else activeRun = new PrototypeSmokeRun(request.id, request.operation, ReportDirectory, (success, message) => Finish(request, success, message));
                    Application.runInBackground = true;
                    EditorApplication.ExecuteMenuItem("Window/General/Game");
                }
            }
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                activeRun?.Cleanup(); activeRun = null;
                ArchitectureRefactorChecks.RestoreProbe();
                PreparationAuthoring.RestoreProbe(); SignalSearchAuthoring.RestoreProbe();
                InvestigationAuthoring.RestoreProbe(); ExplorationAuthoring.RestoreProbe();
                ConsequencesAuthoring.RestoreProbe(); FinalTabletAuthoring.RestoreTuning();
                ChaseAuthoring.RestoreTuning(); BeamAuthoring.RestoreTuning(); CipherAuthoring.RestoreTuning();
                RestoreTuningProbe(); ListeningAuthoring.RestoreTuning();
                if (SessionState.GetBool("Demon.Finale.Restore", false))
                {
                    var finale = UnityEngine.Object.FindAnyObjectByType<GameSceneController>().Finale;
                    var serialized = new SerializedObject(finale);
                    serialized.FindProperty("clockCountdownSeconds").floatValue = SessionState.GetFloat("Demon.Finale.OriginalCountdown", 6);
                    serialized.ApplyModifiedProperties();
                    finale.ClockView.localPosition = SessionState.GetVector3("Demon.Finale.OriginalView", Vector3.zero);
                    EditorUtility.SetDirty(finale);
                    EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                    EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
                    SessionState.SetBool("Demon.Finale.Restore", false);
                    Debug.Log("DEMON_FINALE_TUNING_RESTORED");
                }
                if (SessionState.GetBool("Demon.NineDay.RestoreWake", false))
                {
                    var definition = AssetDatabase.LoadAssetAtPath<NineDayDefinition>("Assets/Demon/Data/NineDayDefinition.asset");
                    definition.Days[1].WakeHour = SessionState.GetFloat("Demon.NineDay.OriginalWake", 8);
                    EditorUtility.SetDirty(definition); AssetDatabase.SaveAssets();
                    SessionState.SetBool("Demon.NineDay.RestoreWake", false);
                    Debug.Log("DEMON_NINE_DAY_WAKE_RESTORED");
                }
            }
        }

        const string TuningKey = "Demon.Slice.TuningProbe";
        static void BeginTuningProbe()
        {
            if (SessionState.GetBool(TuningKey, false)) throw new InvalidOperationException("A tuning probe requires restoration first.");
            var root = UnityEngine.Object.FindAnyObjectByType<GameSceneController>();
            SessionState.SetFloat(TuningKey + "day", root.Settings.RealSecondsPerDay);
            SessionState.SetFloat(TuningKey + "sun", root.Settings.DaySunIntensity);
            SessionState.SetVector3(TuningKey + "anchor", root.Tablet.CloseView.localPosition);
            SessionState.SetBool(TuningKey, true);
            root.Settings.RealSecondsPerDay = 720;
            root.Settings.DaySunIntensity = .37f;
            root.Tablet.CloseView.localPosition += Vector3.right * .15f;
            SaveTuning(root);
        }

        static void RestoreTuningProbe()
        {
            if (!SessionState.GetBool(TuningKey, false)) return;
            var root = UnityEngine.Object.FindAnyObjectByType<GameSceneController>();
            root.Settings.RealSecondsPerDay = SessionState.GetFloat(TuningKey + "day", 1440);
            root.Settings.DaySunIntensity = SessionState.GetFloat(TuningKey + "sun", 1.15f);
            root.Tablet.CloseView.localPosition = SessionState.GetVector3(TuningKey + "anchor", Vector3.zero);
            SaveTuning(root);
            SessionState.SetBool(TuningKey, false);
            Debug.Log("DEMON_SLICE_TUNING_RESTORED");
        }

        static void SaveTuning(GameSceneController root)
        {
            EditorUtility.SetDirty(root.Settings);
            PrefabUtility.RecordPrefabInstancePropertyModifications(root.Tablet.CloseView);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();
        }

        static void Finish(Request request, bool success, string message)
        {
            Directory.CreateDirectory(ReportDirectory);
            string id = Regex.IsMatch(request.id ?? "", "^[a-zA-Z0-9-]{1,64}$") ? request.id : "invalid";
            File.WriteAllText(Path.Combine(ReportDirectory, id + ".json"), JsonUtility.ToJson(new Report {
                id = id, operation = request.operation, success = success, message = message,
                frame = Application.isPlaying ? Time.frameCount : 0 }, true));
            Debug.Log("DEMON_SLICE_RESULT " + id + " " + success + " " + message);
        }

        sealed class TestCallbacks : ICallbacks
        {
            readonly Request request;
            public TestCallbacks(Request request) { this.request = request; }
            public void RunStarted(ITestAdaptor testsToRun) { }
            public void TestStarted(ITestAdaptor test) { }
            public void TestFinished(ITestResultAdaptor result) { }
            public void RunFinished(ITestResultAdaptor result)
            {
                Directory.CreateDirectory(ReportDirectory);
                TestRunnerApi.SaveResultToFile(result, Path.GetFullPath(Path.Combine(ReportDirectory, request.id + ".xml")));
                Finish(request, result.FailCount == 0 && result.PassCount > 0,
                    $"EditMode: passed={result.PassCount}, failed={result.FailCount}, skipped={result.SkipCount}");
            }
        }


    }
}
