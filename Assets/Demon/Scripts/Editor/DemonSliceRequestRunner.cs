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
    public static class DemonSliceRequestRunner
    {
        const string RequestPath = "Temp/DemonSliceAuthoring.request.json";
        const string ReportDirectory = "Temp/DemonSliceReports";
        const string ActiveSmokeKey = "Demon.Slice.ActiveSmoke";
        static double nextPoll;
        static SmokeRun smoke;
        static NineDaySmokeRun nineSmoke;
        static ListeningSmokeRun listeningSmoke;
        static CipherSmokeRun cipherSmoke;
        static BeamSmokeRun beamSmoke;
        static ChaseSmokeRun chaseSmoke;
        static FinalTabletSmokeRun retrievalSmoke;
        static ConsequencesSmokeRun consequencesSmoke;
        static ExplorationSmokeRun explorationSmoke;
        static InvestigationSmokeRun investigationSmoke;
        static SignalSearchSmokeRun signalSmoke;
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

        static DemonSliceRequestRunner()
        {
            EditorApplication.update += Poll;
            EditorApplication.playModeStateChanged += OnPlayState;
        }

        static void Poll()
        {
            if (smoke != null) { smoke.Tick(); return; }
            if (nineSmoke != null) { nineSmoke.Tick(); return; }
            if (listeningSmoke != null) { listeningSmoke.Tick(); return; }
            if (cipherSmoke != null) { cipherSmoke.Tick(); return; }
            if (beamSmoke != null) { beamSmoke.Tick(); return; }
            if (chaseSmoke != null) { chaseSmoke.Tick(); return; }
            if (retrievalSmoke != null) { retrievalSmoke.Tick(); return; }
            if (consequencesSmoke != null) { consequencesSmoke.Tick(); return; }
            if (explorationSmoke != null) { explorationSmoke.Tick(); return; }
            if (investigationSmoke != null) { investigationSmoke.Tick(); return; }
            if (signalSmoke != null) { signalSmoke.Tick(); return; }
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
                    case "refresh":
                        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                        UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
                        Finish(request, true, "Requested asset refresh and script compilation; wait for the Editor to finish.");
                        break;
                    case "create": DemonSliceAuthoring.CreateScene(); Finish(request, true, "Created and saved " + DemonSliceAuthoring.ScenePath); break;
                    case "validate": OpenSlice(); Finish(request, true, DemonSliceAuthoring.ValidateSavedScene()); break;
                    case "fonts": OpenSlice(); Finish(request, true, DemonSliceAuthoring.AddFontBinding()); break;
                    case "repair-box": OpenSlice(); Finish(request, true, DemonSliceAuthoring.RepairBoxBinding()); break;
                    case "build": Finish(request, true, DemonSliceAuthoring.BuildWindows()); break;
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
                            var finale = UnityEngine.Object.FindAnyObjectByType<SliceSceneController>().Finale;
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
                            var definition = UnityEngine.Object.FindAnyObjectByType<SliceSceneController>().Campaign.Definition;
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
            if (SceneManager.GetActiveScene().path == DemonSliceAuthoring.ScenePath) return;
            if (SceneManager.GetActiveScene().isDirty)
                throw new InvalidOperationException("Save the current scene before opening the slice.");
            EditorSceneManager.OpenScene(DemonSliceAuthoring.ScenePath);
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
                    if (request.operation == "signal-old-search")
                        investigationSmoke = new InvestigationSmokeRun("investigation-smoke", (success, message) => Finish(request, success, message));
                    else if (request.operation == "signal-first-search")
                        explorationSmoke = new ExplorationSmokeRun("exploration-smoke", (success, message) => Finish(request, success, message));
                    else if (request.operation.StartsWith("signal-"))
                        signalSmoke = new SignalSearchSmokeRun(request.operation, (success, message) => Finish(request, success, message));
                    else if (request.operation.StartsWith("investigation-"))
                        investigationSmoke = new InvestigationSmokeRun(request.operation, (success, message) => Finish(request, success, message));
                    else if (request.operation.StartsWith("exploration-"))
                        explorationSmoke = new ExplorationSmokeRun(request.operation, (success, message) => Finish(request, success, message));
                    else if (request.operation.StartsWith("consequences-"))
                        consequencesSmoke = new ConsequencesSmokeRun(request.operation, (success, message) => Finish(request, success, message));
                    else if (request.operation.StartsWith("retrieval-"))
                        retrievalSmoke = new FinalTabletSmokeRun(request.operation, (success, message) => Finish(request, success, message));
                    else if (request.operation.StartsWith("chase-"))
                        chaseSmoke = new ChaseSmokeRun(request.operation, (success, message) => Finish(request, success, message));
                    else if (request.operation.StartsWith("beam-"))
                        beamSmoke = new BeamSmokeRun(request.operation, (success, message) => Finish(request, success, message));
                    else if (request.operation.StartsWith("cipher-"))
                        cipherSmoke = new CipherSmokeRun(request.operation, (success, message) => Finish(request, success, message));
                    else if (request.operation.StartsWith("listening-"))
                        listeningSmoke = new ListeningSmokeRun(request.operation, (success, message) => Finish(request, success, message));
                    else if (request.operation.StartsWith("nine-"))
                        nineSmoke = new NineDaySmokeRun(request.operation, (success, message) => Finish(request, success, message));
                    else smoke = new SmokeRun(request);
                    Application.runInBackground = true;
                    EditorApplication.ExecuteMenuItem("Window/General/Game");
                }
            }
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                signalSmoke?.Cleanup(); signalSmoke = null; SignalSearchAuthoring.RestoreProbe();
                investigationSmoke?.Cleanup(); investigationSmoke = null; InvestigationAuthoring.RestoreProbe();
                explorationSmoke?.Cleanup(); explorationSmoke = null; ExplorationAuthoring.RestoreProbe();
                consequencesSmoke?.Cleanup(); consequencesSmoke = null; ConsequencesAuthoring.RestoreProbe();
                retrievalSmoke?.Cleanup(); retrievalSmoke = null; FinalTabletAuthoring.RestoreTuning();
                chaseSmoke?.Cleanup(); chaseSmoke = null; ChaseAuthoring.RestoreTuning();
                beamSmoke?.Cleanup(); beamSmoke = null; BeamAuthoring.RestoreTuning();
                cipherSmoke?.Cleanup(); cipherSmoke = null; CipherAuthoring.RestoreTuning();
                listeningSmoke?.Cleanup(); listeningSmoke = null;
                smoke = null; nineSmoke = null; RestoreTuningProbe(); ListeningAuthoring.RestoreTuning();
                if (SessionState.GetBool("Demon.Finale.Restore", false))
                {
                    var finale = UnityEngine.Object.FindAnyObjectByType<SliceSceneController>().Finale;
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
            var root = UnityEngine.Object.FindAnyObjectByType<SliceSceneController>();
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
            var root = UnityEngine.Object.FindAnyObjectByType<SliceSceneController>();
            root.Settings.RealSecondsPerDay = SessionState.GetFloat(TuningKey + "day", 1440);
            root.Settings.DaySunIntensity = SessionState.GetFloat(TuningKey + "sun", 1.15f);
            root.Tablet.CloseView.localPosition = SessionState.GetVector3(TuningKey + "anchor", Vector3.zero);
            SaveTuning(root);
            SessionState.SetBool(TuningKey, false);
            Debug.Log("DEMON_SLICE_TUNING_RESTORED");
        }

        static void SaveTuning(SliceSceneController root)
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

        sealed class SmokeRun
        {
            readonly Request request;
            readonly double start = EditorApplication.timeSinceStartup;
            SliceSceneController root;
            int stage;
            int firstFrame;
            double stageTime;
            bool captured;
            bool finished;
            Vector3 startingPosition;
            Transform[] clickTargets;
            int inputStep;
            double initialMinutes;
            float initialGameTime;
            InputSettings.BackgroundBehavior oldBackground;
            InputSettings.EditorInputBehaviorInPlayMode oldEditorInput;
            Keyboard testKeyboard;
            Mouse testMouse;

            public SmokeRun(Request request) { this.request = request; }

            void Require(bool condition, string message)
            { if (!condition) throw new InvalidOperationException(message); }

            public void Tick()
            {
                if (finished) return;
                try
                {
                    double now = EditorApplication.timeSinceStartup;
                    if (now - start > 120) throw new TimeoutException("Play Mode did not finish within 120 seconds.");
                    if (!Application.isPlaying) return;
                    if (!root) root = UnityEngine.Object.FindAnyObjectByType<SliceSceneController>();
                    if (!root || root.Session == null) return;
                    if (stage == 0)
                    {
                        firstFrame = Time.frameCount;
                        initialMinutes = root.Session.Clock.AbsoluteMinutes;
                        initialGameTime = Time.time;
                        Require(root.Session.Clock.Day == 8 && root.Session.BoxSeal == 1 && root.Session.LakeSeal == 0, "Wrong authored initial state.");
                        Capture("start");
                        if (request.operation == "smoke-input")
                        {
                            oldBackground = InputSystem.settings.backgroundBehavior;
                            oldEditorInput = InputSystem.settings.editorInputBehaviorInPlayMode;
                            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
                            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
                            testKeyboard = InputSystem.AddDevice<Keyboard>("DemonSmokeKeyboard");
                            testMouse = InputSystem.AddDevice<Mouse>("DemonSmokeMouse");
                            InputSystem.onAfterUpdate += MakeTestDevicesCurrent;
                            Require(Keyboard.current != null && Mouse.current != null, "Keyboard/mouse device missing.");
                            startingPosition = root.Player.transform.position;
                            Cursor.lockState = CursorLockMode.Locked;
                            InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(Key.W));
                            stageTime = now; stage = 10; return;
                        }
                        root.Tablet.Use(root.Player);
                        Require(root.Player.InspectingTablet, "Close view did not acquire input ownership.");
                        stageTime = now; stage = 1;
                    }
                    else if (stage == 1 && now - stageTime > 2)
                    {
                        Require(Time.frameCount > firstFrame + 5, "Game frames are not advancing.");
                        if (request.operation == "smoke-editability")
                        {
                            double rate = (root.Session.Clock.AbsoluteMinutes - initialMinutes) / (Time.time - initialGameTime);
                            Require(Math.Abs(rate - 2) < .1, "Saved day-length tuning was ignored.");
                            Require(Vector3.Distance(root.Player.ViewCamera.transform.position, root.Tablet.CloseView.position) < .03f,
                                "Saved camera tuning was ignored.");
                            float hour = (float)root.Session.Clock.MinuteOfDay / 60;
                            float expected = Mathf.Lerp(root.Settings.NightSunIntensity, .37f, Mathf.Clamp01(Mathf.Sin((hour - 6) * Mathf.PI / 12)));
                            Require(Mathf.Abs(RenderSettings.sun.intensity - expected) < .01f, "Saved sunlight tuning was ignored.");
                            Finish(request, true, "Saved day length, camera anchor and sunlight edits respected in Play Mode; restore on exit.");
                            finished = true;
                            EditorApplication.delayCall += EditorApplication.ExitPlaymode;
                            return;
                        }
                        if (!captured) { Capture("tablet"); captured = true; stageTime = now; return; }
                        captured = false;
                        if (request.operation == "smoke-sleep")
                        {
                            root.Player.ExitCloseView(); root.Sleep();
                        }
                        else
                        {
                            Require(!root.TryCompile(), "An incomplete tablet unexpectedly compiled.");
                            var board = root.Session.Board;
                            board.PickFromTray(RuneKind.Exchange); board.ClickSlot(4);
                            board.PickFromTray(RuneKind.Box); board.ClickSlot(7);
                            board.PickFromTray(RuneKind.Lake); board.ClickSlot(8);
                            Require(root.TryCompile(), "Target tablet did not compile.");
                            Require(root.Session.ExchangeCount == 1 && root.Session.BoxSeal == 0 && root.Session.LakeSeal == 1, "First exchange failed.");
                            board.ClickSlot(4);
                            Require(board.CompiledAt(4) == RuneKind.Exchange, "Surface removal erased the program.");
                            UnityEngine.Object.FindAnyObjectByType<SliceBox>().Use(root.Player);
                            Require(root.Player.Reading && root.HasReadBox, "Box text did not open.");
                        }
                        stage = 2; stageTime = now;
                    }
                    else if (stage == 2)
                    {
                        if (!captured && now - stageTime > 2)
                        {
                            Capture("record"); captured = true;
                        }
                        if (captured && now - stageTime > 3 && root.Player.Reading) root.Player.ExitCloseView();
                        if (root.Session.Descended)
                        {
                            Require(root.Session.Clock.Day == 9 && root.Session.Clock.MinuteOfDay == 0, "Terminal time leaked beyond D9 midnight.");
                            Require(root.Session.HasBody && root.Session.LakeSeal == 0, "Descent state is inconsistent.");
                            Require(root.Session.ExchangeCount == (request.operation == "smoke-sleep" ? 0 : 2), "Wrong exchange count.");
                            stageTime = now; stage = 3;
                        }
                    }
                    else if (stage == 3 && now - stageTime > 2)
                    {
                        Capture("ending");
                        Finish(request, true, $"Play Mode {request.operation}: frames {firstFrame}->{Time.frameCount}, D9 midnight, exchanges={root.Session.ExchangeCount}; close view, target, box and terminal states verified.");
                        finished = true;
                        EditorApplication.delayCall += EditorApplication.ExitPlaymode;
                    }
                    else if (stage == 10 && now - stageTime > 1)
                    {
                        InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(Key.E));
                        Require(root.Player.transform.position.z > startingPosition.z + .5f, "W did not move the CharacterController through Input System: position=" + root.Player.transform.position + ", cursor=" + Cursor.lockState);
                        stage = 11; stageTime = now;
                    }
                    else if (stage == 11 && now - stageTime > 1)
                    {
                        InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState());
                        Require(root.Player.InspectingTablet, "Physical E press did not enter close view.");
                        Require(Vector3.Distance(root.Player.ViewCamera.transform.position, root.Tablet.CloseView.position) < .03f,
                            "Authored close camera anchor was not respected.");
                        var trays = root.Tablet.GetComponentsInChildren<SliceRuneTrayItem>();
                        var slots = root.Tablet.GetComponentsInChildren<SliceRuneSlot>();
                        clickTargets = new[] {
                            trays.First(t => t.Rune == RuneKind.Exchange).transform, slots.First(s => s.Index == 4).transform,
                            trays.First(t => t.Rune == RuneKind.Box).transform, slots.First(s => s.Index == 7).transform,
                            trays.First(t => t.Rune == RuneKind.Lake).transform, slots.First(s => s.Index == 8).transform };
                        stage = 12; stageTime = now;
                    }
                    else if (stage == 12 && now - stageTime > .2)
                    {
                        if (inputStep >= clickTargets.Length * 2)
                        {
                            Require(root.Session.Board.MatchesTarget() && !root.Session.Board.Frozen,
                                "Mouse raycasts did not place the target layout, or it froze before confirmation.");
                            Capture("input-layout");
                            InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(Key.Enter));
                            stage = 13; stageTime = now;
                        }
                        else
                        {
                            Vector3 screen = root.Player.ViewCamera.WorldToScreenPoint(clickTargets[inputStep / 2].position);
                            Require(screen.z > 0 && screen.x > 0 && screen.x < Screen.width && screen.y > 0 && screen.y < Screen.height,
                                "A selectable authored rune is outside the close camera view.");
                            Ray clickRay = root.Player.ViewCamera.ScreenPointToRay(screen);
                            var clickHit = Physics.RaycastAll(clickRay, 20).Where(h => !h.collider.transform.IsChildOf(root.Player.transform))
                                .OrderBy(h => h.distance).FirstOrDefault();
                            Require(clickHit.collider && clickHit.collider.transform.IsChildOf(clickTargets[inputStep / 2]),
                                "Authored target is blocked: " + clickTargets[inputStep / 2].name + " hit " + (clickHit.collider ? clickHit.collider.name : "nothing"));
                            InputSystem.QueueStateEvent(testMouse, new MouseState {
                                position = new Vector2(screen.x, screen.y), buttons = (ushort)(inputStep % 2 == 0 ? 1 : 0) });
                            inputStep++; stageTime = now;
                        }
                    }
                    else if (stage == 13 && now - stageTime > .6)
                    {
                        InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState());
                        Require(root.Session.Board.Frozen && root.Session.ExchangeCount == 1,
                            "Enter did not confirm exactly one exchange.");
                        stage = 14; stageTime = now;
                    }
                    else if (stage == 14 && now - stageTime > 1)
                    {
                        var box = UnityEngine.Object.FindAnyObjectByType<SliceBox>();
                        var body = root.Player.GetComponent<CharacterController>();
                        body.enabled = false;
                        root.Player.transform.SetPositionAndRotation(box.transform.position + Vector3.back * 2, Quaternion.identity);
                        body.enabled = true;
                        // Approach is level; looking down is required to aim at the waist-high box.
                        InputSystem.QueueStateEvent(testMouse, new MouseState { delta = new Vector2(0, -250) });
                        stage = 15; stageTime = now;
                    }
                    else if (stage == 15 && now - stageTime > .5)
                    {
                        Cursor.lockState = CursorLockMode.Locked;
                        InputSystem.QueueStateEvent(testKeyboard, new KeyboardState(Key.E));
                        stage = 16; stageTime = now;
                    }
                    else if (stage == 16 && now - stageTime > 1)
                    {
                        Require(root.Player.Reading && root.HasReadBox, "E on the saved box instance did not open its record; focus=" + root.Player.FocusPrompt);
                        Capture("box-reading");
                        InputSystem.QueueStateEvent(testKeyboard, new KeyboardState());
                        stage = 17; stageTime = now;
                    }
                    else if (stage == 17 && now - stageTime > .5)
                    {
                        InputSystem.QueueStateEvent(testKeyboard, new KeyboardState(Key.Escape));
                        stage = 18; stageTime = now;
                    }
                    else if (stage == 18 && now - stageTime > 1)
                    {
                        Require(!root.Player.Reading, "Escape did not leave the box record.");
                        Finish(request, true, "W/E, six rune raycasts, Enter, physical E on saved box, reading and Escape passed.");
                        finished = true;
                        RestoreInput();
                        EditorApplication.delayCall += EditorApplication.ExitPlaymode;
                    }
                }
                catch (Exception exception)
                {
                    Finish(request, false, exception.ToString());
                    finished = true;
                    RestoreInput();
                    EditorApplication.delayCall += EditorApplication.ExitPlaymode;
                }
            }

            void RestoreInput()
            {
                if (testKeyboard == null) return;
                InputSystem.onAfterUpdate -= MakeTestDevicesCurrent;
                InputSystem.RemoveDevice(testKeyboard);
                InputSystem.RemoveDevice(testMouse);
                InputSystem.settings.backgroundBehavior = oldBackground;
                InputSystem.settings.editorInputBehaviorInPlayMode = oldEditorInput;
                testKeyboard = null; testMouse = null;
            }

            void MakeTestDevicesCurrent()
            {
                testKeyboard.MakeCurrent();
                testMouse.MakeCurrent();
            }

            void Capture(string label)
            {
                Directory.CreateDirectory(ReportDirectory);
                ScreenCapture.CaptureScreenshot(Path.GetFullPath(Path.Combine(ReportDirectory, request.id + "-" + label + ".png")));
            }
        }
    }
}
