using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Demon.Slice
{
    [DefaultExecutionOrder(-200)]
    public sealed class DayJumpMenu : MonoBehaviour
    {
        [SerializeField] GameSceneController world;
        [SerializeField] GameObject panel;
        [SerializeField] Button[] dayButtons;
        [SerializeField] Button finalNightButton;
        [SerializeField] Button closeButton;
        [SerializeField] Text statusText;
        [SerializeField] Key toggleKey = Key.F8;
        [SerializeField] bool availableInPlayer = true;
        bool open, loading, ownsPause;
        GameObject previousSelection;
        bool previousPlayerMenu;
        float previousScale;
        bool previousAudioPause, previousCursorVisible;
        CursorLockMode previousCursorLock;
        public bool IsOpen => open;
        public bool IsLoading => loading;
        public bool IsReady { get; private set; }

        public void Configure(GameSceneController controller, GameObject window, Button[] days, Button night, Button close, Text status)
        { world = controller; panel = window; dayButtons = days; finalNightButton = night; closeButton = close; statusText = status; }

        void Awake()
        {
            if (!world || !panel || dayButtons == null || dayButtons.Length != 9 || !finalNightButton || !closeButton || !statusText)
                throw new InvalidOperationException("Day jump menu requires its saved world, panel, nine buttons, night/close buttons and status text.");
            for (int i = 0; i < dayButtons.Length; i++)
            {
                if (!dayButtons[i]) throw new InvalidOperationException("Missing day jump button.");
                int day = i + 1;
                dayButtons[i].onClick.AddListener(() => JumpToDay(day));
            }
            finalNightButton.onClick.AddListener(JumpToFinalNight);
            closeButton.onClick.AddListener(Close);
            panel.SetActive(false);
            if (!Application.isEditor && !availableInPlayer) gameObject.SetActive(false);
        }

        void Start() { IsReady = true; }

        void Update()
        {
            if (!IsReady || loading || world.Session == null || world.Campaign.Progress == null) return;
            var keyboard = Keyboard.current;
            if (keyboard == null) return;
            if (keyboard[toggleKey].wasPressedThisFrame) { if (open) Close(); else Open(); return; }
            if (!open) return;
            if (keyboard.escapeKey.wasPressedThisFrame) { Close(); return; }
            for (int i = 0; i < 9; i++)
                if (keyboard[(Key)((int)Key.Digit1 + i)].wasPressedThisFrame || keyboard[(Key)((int)Key.Numpad1 + i)].wasPressedThisFrame)
                { JumpToDay(i + 1); return; }
            if (keyboard.digit0Key.wasPressedThisFrame || keyboard.numpad0Key.wasPressedThisFrame) JumpToFinalNight();
        }

        public void Open()
        {
            if (open || loading || !world || world.Session == null) return;
            if (world.Playthrough) { world.Playthrough.PrepareDaySelection(); if (world.Playthrough.Waiting) return; }
            open = true;
            previousScale = Time.timeScale; previousAudioPause = AudioListener.pause;
            previousCursorLock = Cursor.lockState; previousCursorVisible = Cursor.visible;
            ownsPause = true; Time.timeScale = 0; AudioListener.pause = true;
            previousPlayerMenu = world.Player.MenuOpen;
            world.Player.SetMenuOpen(true);
            Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
            panel.SetActive(true);
            if (EventSystem.current)
            {
                previousSelection = EventSystem.current.currentSelectedGameObject;
                dayButtons[Mathf.Clamp(world.Session.Clock.Day - 1, 0, dayButtons.Length - 1)].Select();
            }
            statusText.text = "Day " + world.Session.Clock.Day + " · Tablets recovered: " + world.Campaign.Progress.FoundCount + "/6\nChoose a day to begin at the cabin with earlier watches complete.";
        }

        public void Close()
        {
            if (!open) return;
            open = false; panel.SetActive(false); RestorePause();
            world.Player.SetMenuOpen(previousPlayerMenu);
            if (EventSystem.current) EventSystem.current.SetSelectedGameObject(previousSelection && previousSelection.activeInHierarchy ? previousSelection : null);
            previousSelection = null;
            Cursor.lockState = previousCursorLock; Cursor.visible = previousCursorVisible;
        }

        public void JumpToDay(int day) => Request(day, false);
        public void JumpToFinalNight() => Request(8, true);
        void Request(int day, bool night)
        {
            if (loading || day < 1 || day > 9 || !world || world.Session == null) return;
            loading = true; Close();
            try
            {
                world.Restart();
                CompletedDayReplay.Run(world, day, night);
                if (!world.Session.Descended) world.SetMessage("Day " + day + " " + (night ? "after dark" : "") + " · Previous watches complete.");
            }
            catch (Exception error)
            {
                loading = false; Open();
                statusText.text = "Could not resume this watch. Restart from Day 1.\n" + error.Message;
                Debug.LogException(error, this);
            }
            finally { loading = false; if (!open) world.Player.SetMenuOpen(false); }
        }

        void RestorePause()
        { if (!ownsPause) return; Time.timeScale = previousScale; AudioListener.pause = previousAudioPause; ownsPause = false; }
        void OnDisable() { if (open) Close(); RestorePause(); }
    }
}
