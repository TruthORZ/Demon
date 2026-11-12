using System;
using System.Collections.Generic;
using System.Text;
using Demon.Slice.Domain;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Demon.Slice
{
    // Coordinates the authored main route. Puzzle rules remain in their existing owners.
    public sealed class CompletePlaythrough : MonoBehaviour
    {
        [SerializeField] PlaythroughDefinition definition;
        [SerializeField] Text taskText, noticeText, failureText, journalText, openingText;
        [SerializeField] GameObject failurePanel, journalPanel, openingPanel;
        [SerializeField] Button retryButton, journalClose, openingContinue, waitButton;
        [SerializeField] JournalInventory journalInventory;
        [SerializeField] BoxCollider cabinSafeZone;
        [SerializeField] Transform cabinExit;
        [SerializeField] CabinDoorInteraction[] cabinDoors;
        [SerializeField] ParticleSystem snowfall;
        [SerializeField] GameObject lakeFigure;
        [SerializeField] AudioSource cueAudio;
        [SerializeField] AudioClip cueClip;
        [SerializeField, Min(1)] float stormMultiplier = 2.5f;
        readonly HashSet<string> cues = new HashSet<string>();
        GameSceneController world;
        GameClock boundClock;
        DayCheckpoint checkpoint;
        bool ownsPause, previousAudio, journalOpen, opening, restoring, replay, waited, pendingCheckpoint;
        float previousScale, noticeRemaining, elapsed, snowRate;
        bool noticeSoundPending;
        string currentNotice, lastTasks;
        public GameSceneController World => world;
        public PlaythroughDefinition Definition => definition;
        public DailyProgress Tasks { get; private set; }
        public RecordCollection Records { get; private set; }
        public bool Failed { get; private set; }
        public bool Waiting { get; private set; }
        public bool BlocksClock => Failed || Waiting || opening || journalOpen;
        public bool BlocksMenu => BlocksClock || restoring;
        public bool HasFlashlight => Records != null && Records.Contains("flashlight");
        public bool InsideCabin(Vector3 point)
        {
            Vector3 local = cabinSafeZone.transform.InverseTransformPoint(point) - cabinSafeZone.center;
            Vector3 half = cabinSafeZone.size * .5f;
            return Mathf.Abs(local.x) <= half.x && Mathf.Abs(local.y) <= half.y && Mathf.Abs(local.z) <= half.z;
        }
        public Vector3 CabinExit => cabinExit.position;
        public void Configure(PlaythroughDefinition data, Text tasks, Text notice, Text failure, Text journal, Text intro,
            GameObject failed, GameObject bag, GameObject black, Button retry, Button close, Button proceed, Button wait,
            BoxCollider safe, Transform exit, ParticleSystem snow, GameObject figure, AudioSource audio, AudioClip clip)
        {
            definition = data; taskText = tasks; noticeText = notice; failureText = failure; journalText = journal; openingText = intro;
            failurePanel = failed; journalPanel = bag; openingPanel = black; retryButton = retry; journalClose = close;
            openingContinue = proceed; waitButton = wait; cabinSafeZone = safe; cabinExit = exit; snowfall = snow; lakeFigure = figure; cueAudio = audio; cueClip = clip;
        }
        public void Initialize(GameSceneController owner)
        {
            if (!definition || definition.Days == null || definition.Days.Length != 9 || !taskText || !failurePanel || !journalPanel || !openingPanel || !cabinSafeZone || !cabinExit || !cueAudio)
                throw new InvalidOperationException("Complete playthrough requires its saved definition, UI, safe zone and audio.");
            if (definition.ChaseSpeeds == null || definition.ChaseSpeeds.Length != 6 || definition.LoseMetres <= definition.DetectionMetres)
                throw new InvalidOperationException("Configure six daily chase speeds and a loss distance greater than detection distance.");
            for (int i = 0; i < 6; i++)
                if (definition.ChaseSpeeds[i] <= 0 || (i > 0 && definition.ChaseSpeeds[i] <= definition.ChaseSpeeds[i - 1]) || (i < 5 && definition.ChaseSpeeds[i] >= owner.Settings.SprintMetresPerSecond) || (i == 5 && definition.ChaseSpeeds[i] <= owner.Settings.SprintMetresPerSecond))
                    throw new InvalidOperationException("D3-D7 speeds must increase below sprint, and D8 must exceed sprint.");
            Unbind(); ReleasePause(); world = owner;
            cueAudio.Stop(); noticeRemaining = 0; noticeSoundPending = false; currentNotice = null; noticeText.gameObject.SetActive(false);
            if (cabinDoors != null) foreach (var door in cabinDoors) door.ResetForSession();
            Failed = Waiting = opening = journalOpen = false; waited = false; checkpoint = null;
            Records = new RecordCollection(); Tasks = new DailyProgress(world.Session.Clock.Day); cues.Clear(); lastTasks = null;
            failurePanel.SetActive(false); journalPanel.SetActive(false); openingPanel.SetActive(false);
            pendingCheckpoint = false;
            boundClock = world.Session.Clock; boundClock.CanEnterDay = CanEnterDay; boundClock.DayEntered += EnteredDay; boundClock.Advanced += AfterAdvance;
            if (snowfall && snowRate == 0) snowRate = snowfall.emission.rateOverTimeMultiplier;
            SyncProgress();
        }
        void Start()
        {
            retryButton.onClick.AddListener(RetryDay); journalClose.onClick.AddListener(CloseJournal);
            openingContinue.onClick.AddListener(FinishOpening); waitButton.onClick.AddListener(WaitUntilNight);
            CaptureDayStart();
            if (!replay && world.Session.Clock.Day == 1)
            { opening = true; elapsed = 0; openingText.text = definition.Opening; openingPanel.SetActive(true); Pause(); }
        }
        void Update()
        {
            if (!world || world.Session == null) return;
            if (opening)
            { elapsed += Time.unscaledDeltaTime; if (elapsed >= definition.OpeningSeconds || Keyboard.current?.enterKey.wasPressedThisFrame == true) FinishOpening(); return; }
            if (Waiting)
            {
                elapsed += Time.unscaledDeltaTime;
                if (elapsed >= definition.WaitSeconds)
                {
                    FinishWaiting();
                }
                return;
            }
            if (Failed) return;
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.tabKey.wasPressedThisFrame && !world.Session.Descended && !world.Finale.IsDirecting)
            {
                if (journalOpen) CloseJournal();
                else if (world.Player.CanUseTools) OpenJournal();
                return;
            }
            if (journalOpen) { if (keyboard?.escapeKey.wasPressedThisFrame == true) CloseJournal(); return; }
            if (!world.Player.MenuOpen && !world.Finale.IsDirecting && !world.Session.Descended && world.Session.Clock.Day == 8 && world.Exploration.HandoverRead && InsideCabin(world.Player.transform.position) && !waited && keyboard?.tKey.wasPressedThisFrame == true)
                WaitUntilNight();
            SyncProgress(); RefreshTasks();
            bool showNotice = noticeRemaining > 0 && !world.Player.Reading && !world.Player.MenuOpen && !world.Finale.IsDirecting;
            noticeText.gameObject.SetActive(showNotice);
            if (showNotice)
            {
                noticeText.text = currentNotice; noticeRemaining = Mathf.Max(0, noticeRemaining - Time.unscaledDeltaTime);
                if (noticeSoundPending) { if (cueClip) cueAudio.PlayOneShot(cueClip); noticeSoundPending = false; }
            }
            waitButton.gameObject.SetActive(world.Session.Clock.Day == 8 && world.Exploration.HandoverRead && !waited && InsideCabin(world.Player.transform.position) && !world.Session.Descended);
            if (!replay) RefreshStory();
        }
        void RefreshStory()
        {
            int day = world.Session.Clock.Day;
            bool storm = day == 1 && world.Session.Clock.MinuteOfDay >= definition.NightHour * 60;
            if (snowfall) { var emission = snowfall.emission; emission.rateOverTimeMultiplier = snowRate * (storm ? stormMultiplier : 1); }
            if (storm) Notice("storm", "我得赶紧回家。");
            if (lakeFigure) lakeFigure.SetActive(day >= 3 && day < 9 && world.Session.LakeSeal == 0 && (day >= 7 || !world.Presentation.ExpectedSunAboveHorizon || storm));
            if (day >= 3 && day < 9 && world.Chase.Phase == ChaseController.ChasePhase.Pursuing && Vector3.Distance(world.Chase.Agent.transform.position, world.Player.transform.position) < definition.PursuitNoticeMetres)
                Notice("first-pursuit", "快跑！");
        }
        public void Notice(string id, string text)
        {
            if (!cues.Add(id) || replay) return;
            currentNotice = text; noticeRemaining = definition.NoticeSeconds; noticeSoundPending = true;
        }
        void FinishOpening()
        {
            if (!opening) return;
            opening = false; openingPanel.SetActive(false); ReleasePause(); Notice("arrival", "太冷了，我需要赶快进屋。");
        }
        public void Record(string id) { if (Tasks.Record(id)) lastTasks = null; }
        public void RecordObservation(int site)
        {
            Record("inspect-" + (int)site);
            if (site == 1 && world.Session.Clock.Day == 3) Notice("still-vane", "现在明明有风啊…");
            if (world.Session.Clock.Day == 7) Notice("eternal-night", "太阳…等等，我似乎明白了什么…屋后的那个箱子…");
        }
        void SyncProgress()
        {
            if (Records == null) return;
            for (int i = 0; i < 6; i++) if (world.Campaign.Progress.Found(i))
            { string id = "T0" + (i + 1); Records.Add(id); Record("tablet-" + (i + 1)); }
            if (world.Listening.Active) Record("listening");
            if (world.Cipher.Acquired) { Records.Add("T06"); Record("tablet-6"); }
            if (world.Exploration.HandoverRead) Record("handover");
            if (world.Retrieval.Progress.Acquired) { Records.Add("T07"); Record("retrieved"); }
            if (world.Retrieval.Progress.Placed) Record("placed");
            if (world.Session.Board.Frozen) Record("compiled");
        }
        public void Collect(string id, Transform view)
        {
            var entry = definition.Entry(id); if (entry == null) throw new InvalidOperationException("Unknown journal ID: " + id);
            Records.Add(id); if (id == "note-a") Record("note-a");
            if (id == "flashlight") { world.SetMessage("手电筒已取得。点击鼠标左键开关照明；T06将解锁牵制能力。"); return; }
            world.ReadDocument(entry.Title + "\n\n" + entry.Body + "\n\n已收入背包，Tab可重读。", view);
        }
        public void RememberBoxRecord() => Records.Add("box-record");
        public bool RequestSleep()
        {
            SyncProgress();
            if (Failed || BlocksClock) return false;
            if (world.Session.Clock.Day >= 8) { world.SetMessage("今天不能睡过交班与最后的封印。检查交接记录，等待夜间。"); return false; }
            if (!Tasks.Complete(definition.Required(Tasks.Day))) { world.SetMessage("还有关键事项未完成：" + Missing()); return false; }
            if (world.Session.Clock.MinuteOfDay < definition.NightHour * 60) { world.SetMessage("关键事项已完成，入夜后才能睡觉。"); return false; }
            return true;
        }
        bool CanEnterDay(int day)
        {
            SyncProgress();
            if (!Failed && Tasks.Day == world.Session.Clock.Day && Tasks.Complete(definition.Required(Tasks.Day))) return true;
            Fail("一天结束，关键事项尚未完成。\n" + Missing()); return false;
        }
        void AfterAdvance()
        {
            if (restoring || world.Session.Descended) return;
            if (pendingCheckpoint) { pendingCheckpoint = false; CaptureDayStart(); }
        }
        void EnteredDay(int day)
        {
            Tasks.Begin(day); waited = false; SyncProgress(); CaptureDayStart(); pendingCheckpoint = true;
        }
        string TaskLabel(string id)
        {
            switch (id)
            {
                case "inspect-0": return "检查电台"; case "inspect-1": return "检查风向标"; case "inspect-2": return "湖畔巡查"; case "inspect-3": return "森林巡查";
                case "note-a": return "阅读值守纸条"; case "box-a": return "检查屋后箱A"; case "box-b": return "检查屋后箱B";
                case "listening": return "补全T05监听"; case "handover": return "检查交接记录"; case "wait": return "在屋内等待夜间";
                case "retrieved": return "取得T07"; case "placed": return "带回并放置T07"; case "compiled": return "补全T07"; case "box-record": return "打开箱B并读完记录";
                default: return id.StartsWith("tablet-") ? "发现T0" + id.Substring(7) : id;
            }
        }
        string Missing()
        { var list = new List<string>(); foreach (string id in definition.Required(Tasks.Day)) if (!Tasks.Contains(id)) list.Add(TaskLabel(id)); return string.Join("、", list); }
        void RefreshTasks()
        {
            bool show = !world.Player.Reading && !world.Finale.IsDirecting && !world.Session.Descended;
            taskText.gameObject.SetActive(show);
            if (lastTasks != null) return;
            var text = new StringBuilder("今日关键事项\n");
            foreach (string id in definition.Required(Tasks.Day)) text.Append(Tasks.Contains(id) ? "✓ " : "□ ").Append(TaskLabel(id)).Append('\n');
            text.Append("Tab 背包"); if (Tasks.Day == 8 && !waited) text.Append("  T 屋内等待");
            string value = text.ToString(); if (value != lastTasks) { taskText.text = value; lastTasks = value; }
        }
        public void WaitUntilNight()
        {
            if (BlocksClock || world.Player.MenuOpen || world.Session.Clock.Day != 8 || !world.Exploration.HandoverRead || !InsideCabin(world.Player.transform.position) || waited) return;
            world.Player.ReturnToExploration(); Waiting = true; elapsed = 0; openingContinue.gameObject.SetActive(false); openingText.text = "等待交班……\n\n没有人来。"; openingPanel.SetActive(true); Pause();
        }
        void FinishWaiting()
        {
            Waiting = false; openingPanel.SetActive(false); openingContinue.gameObject.SetActive(true); ReleasePause(); waited = true; Record("wait");
            world.Session.Clock.AdvanceTo(Math.Max(world.Session.Clock.AbsoluteMinutes, 7 * GameClock.MinutesPerDay + world.Campaign.Definition.FinalTabletHour * 60));
            world.Retrieval.RefreshPresentation(); Notice("night-arrival", "天已经黑了。去湖岸寻找最后一块石板。");
        }
        public void WaitForDebugReplay()
        {
            if (!replay) throw new InvalidOperationException("Only debug replay may skip the wait animation.");
            var pose = world.Player.CapturePose(); var cabinPose = pose; cabinPose.position = cabinSafeZone.transform.TransformPoint(cabinSafeZone.center - Vector3.up);
            world.Player.RestorePose(cabinPose); WaitUntilNight();
            if (!Waiting) throw new InvalidOperationException("Debug wait did not satisfy the actual handover/safe-zone conditions.");
            FinishWaiting(); world.Player.RestorePose(pose);
        }
        void OpenJournal()
        {
            var items = new List<JournalInventory.Item>();
            foreach (string id in Records.Capture())
            {
                var entry = definition.Entry(id);
                if (entry != null) items.Add(new JournalInventory.Item(id, entry.Title, entry.Body, id.StartsWith("note-") ? "纸条" : "道具"));
                else if (id.StartsWith("T0") && int.TryParse(id.Substring(1), out int index) && index <= 6)
                    items.Add(new JournalInventory.Item(id, id + " 石板", world.Campaign.Definition.Tablets[index - 1].Record, "石板"));
                else if (id == "box-record") items.Add(new JournalInventory.Item(id, "箱B记录", world.Settings.BoxText, "记录"));
            }
            if (!journalInventory) throw new InvalidOperationException("Assign the saved journal grid in the main-route scene.");
            journalInventory.Show(items);
            journalOpen = true; journalPanel.SetActive(true); Pause();
        }
        public void CloseJournal() { if (!journalOpen) return; journalOpen = false; journalPanel.SetActive(false); ReleasePause(); }
        void Pause()
        {
            if (ownsPause) return; ownsPause = true; previousScale = Time.timeScale; previousAudio = AudioListener.pause;
            Time.timeScale = 0; AudioListener.pause = true; world.Player.SetMenuOpen(true); Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
        }
        void ReleasePause()
        {
            if (!ownsPause) return; ownsPause = false; Time.timeScale = previousScale; AudioListener.pause = previousAudio;
            if (world) world.Player.SetMenuOpen(false); Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false;
        }
        void Fail(string message)
        { if (Failed) return; Failed = true; failureText.text = message + "\n\n重试会恢复当天开始时的状态。"; failurePanel.SetActive(true); Pause(); }
        public void CaptureDayStart()
        {
            if (!world || restoring || world.Session.Descended) return;
            SyncProgress(); var chimes = world.Listening.Chimes; var chimeStates = new ListeningChime.State[chimes.Length];
            for (int i = 0; i < chimes.Length; i++) chimeStates[i] = chimes[i].Capture();
            checkpoint = new DayCheckpoint { session = world.Session.Capture(), day = Tasks.Day, tasks = Tasks.Capture(), records = Records.Capture(),
                discovered = world.Campaign.Progress.Capture(), player = world.Player.CapturePose(), cipher = world.Cipher.Model.Capture(),
                listening = world.Listening.Board.Capture(), beam = world.Tether.Rule.Capture(), exploration = world.Exploration.CaptureMemory(),
                chimes = chimeStates, chase = world.Chase.Capture(), acquired = world.Retrieval.Progress.Acquired, placed = world.Retrieval.Progress.Placed,
                vane = world.Tether.VaneBlade.localRotation, broadcast = world.Consequences.CapturePlayback(), flashlight = world.Tether.LampSwitchedOn,
                interruptions = world.Retrieval.Pickup.Attempt.InterruptCount, boxOpened = world.Presentation.CaptureBox(), waited = waited };
            checkpoint.cues = new string[cues.Count]; cues.CopyTo(checkpoint.cues);
            checkpoint.doors = new CabinDoorInteraction.State[cabinDoors?.Length ?? 0];
            for (int i = 0; i < checkpoint.doors.Length; i++) checkpoint.doors[i] = cabinDoors[i].Capture();
        }
        public void RetryDay()
        {
            if (!Failed || restoring || checkpoint == null) return;
            var saved = checkpoint; ReleasePause(); restoring = true;
            try
            {
                world.RestoreSession(saved.session); Tasks.Restore(saved.day, saved.tasks); Records.Restore(saved.records); waited = saved.waited;
                world.Campaign.Progress.Restore(saved.discovered); world.Campaign.RefreshRestoredState();
                world.Cipher.Restore(saved.cipher); world.Listening.Board.Restore(saved.listening); world.Tether.Rule.Restore(saved.beam);
                world.Exploration.RestoreMemory(saved.exploration); world.Retrieval.Restore(saved.acquired, saved.placed, saved.interruptions);
                world.Presentation.RestoreBox(saved.boxOpened); world.Player.RestorePose(saved.player); world.Chase.Restore(saved.chase);
                world.Tether.VaneBlade.localRotation = saved.vane; world.Consequences.RestorePlayback(saved.broadcast);
                world.Tether.RestoreLampSwitch(saved.flashlight);
                var chimes = world.Listening.Chimes; for (int i = 0; i < chimes.Length; i++) chimes[i].Restore(saved.chimes[i]);
                cues.Clear(); foreach (string id in saved.cues) cues.Add(id); checkpoint = saved;
                for (int i = 0; i < saved.doors.Length; i++) cabinDoors[i].Restore(saved.doors[i]);
                world.SetMessage("已恢复第 " + saved.day + " 天开始时的状态。");
            }
            finally { restoring = false; }
        }
        public void BeginDebugReplay() { replay = true; FinishOpening(); CloseJournal(); }
        public void EndDebugReplay() { replay = false; SyncProgress(); CaptureDayStart(); }
        public void PrepareDayForReplay()
        {
            foreach (var point in world.Exploration.Observations) { point.Use(world.Player); world.Player.ExitCloseView(); }
            if (Tasks.Day == 1)
            { Collect("note-a", world.Player.transform); Collect("flashlight", world.Player.transform); world.Player.ReturnToExploration(); world.Cipher.Use(world.Player); world.Player.ReturnToExploration();
              world.GetComponentInChildren<RecordBox>(true)?.Use(world.Player);
              foreach (var box in world.gameObject.scene.GetRootGameObjects()) { var record = box.GetComponentInChildren<RecordBox>(true); if (record) { record.Use(world.Player); break; } } }
            world.Session.Clock.AdvanceTo(Math.Max(world.Session.Clock.AbsoluteMinutes, (Tasks.Day - 1) * GameClock.MinutesPerDay + definition.NightHour * 60));
        }
        void Unbind()
        { if (boundClock == null) return; boundClock.CanEnterDay = null; boundClock.DayEntered -= EnteredDay; boundClock.Advanced -= AfterAdvance; boundClock = null; }
        void OnDestroy() { Unbind(); ReleasePause(); }
    }
}
