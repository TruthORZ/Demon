using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.InputSystem;

namespace Demon.Slice
{
    [RequireComponent(typeof(CharacterController))]
    [MovedFrom(false, sourceClassName: "SlicePlayer")]
    public sealed partial class PlayerController : MonoBehaviour
    {
        enum ViewMode { Explore, Interaction, Reading, Returning, Sequence, Ending, Inspect, Pickup }
        [SerializeField] Camera viewCamera;
        [SerializeField] InputActionAsset inputActions;
        CharacterController body;
        IPlayerSession world;
        InputActionAsset actions;
        InputAction move, look, interact, sprint;
        Transform closeAnchor;
        Vector3 homePosition;
        Quaternion homeRotation;
        Vector3 spawnPosition;
        Quaternion spawnRotation;
        ViewMode mode;
        float pitch, verticalSpeed;
        float explorationNearPlane;
        Rect explorationViewport;
        [SerializeField, Range(.4f, 1)] float readingViewportWidth = 1;
        int menuClosedFrame = -1;
        ICloseViewInteraction activeInteraction;
        ThreatPickup activePickup;
        public ThreatPickup CurrentPickup => activePickup;
        internal bool Exploring => mode == ViewMode.Explore;
        public bool IsPickingUp => mode == ViewMode.Pickup;
        public bool InspectingLock => mode == ViewMode.Interaction && activeInteraction is CipherBox;
        public bool MenuOpen { get; private set; }
        public bool CanUseTools => !MenuOpen && menuClosedFrame != Time.frameCount && mode == ViewMode.Explore && Cursor.lockState == CursorLockMode.Locked;
        readonly RaycastHit[] interactionHits = new RaycastHit[32];
        public Camera ViewCamera => viewCamera;
        public bool InspectingTablet => mode == ViewMode.Interaction && activeInteraction is RuneTablet;
        public RuneTablet ActiveTablet => activeInteraction as RuneTablet;
        public bool ReadingAt(Transform anchor) => mode == ViewMode.Reading && closeAnchor == anchor;
        public bool InspectingAt(Transform anchor) => mode == ViewMode.Inspect && closeAnchor == anchor;
        public bool Reading => mode == ViewMode.Reading;
        public string FocusPrompt { get; private set; } = "";

        public void Configure(Camera camera, InputActionAsset source)
        { viewCamera = camera; inputActions = source; }

        public void Initialize(IPlayerSession controller)
        {
            if (!viewCamera || !inputActions)
                throw new System.InvalidOperationException("Player camera or Input Actions is missing.");
            world = controller;
            if (!body || !actions)
            {
                body = GetComponent<CharacterController>();
                explorationNearPlane = viewCamera.nearClipPlane;
                explorationViewport = viewCamera.rect;
                spawnPosition = transform.position; spawnRotation = transform.rotation;
                homePosition = viewCamera.transform.localPosition; homeRotation = viewCamera.transform.localRotation;
                actions = Instantiate(inputActions);
            }
            else
            {
                bool wasEnabled = body.enabled; body.enabled = false;
                transform.SetPositionAndRotation(spawnPosition, spawnRotation); body.enabled = wasEnabled;
                ReturnToExploration();
            }
            move = actions.FindAction("Player/Move", true);
            look = actions.FindAction("Player/Look", true);
            interact = actions.FindAction("Player/Interact", true);
            sprint = actions.FindAction("Player/Sprint", true);
            actions.FindActionMap("Player", true).Enable();
            pitch = Mathf.DeltaAngle(0, homeRotation.eulerAngles.x);
            LockCursor(true);
        }

        void Update()
        {
            if (world == null || world.Session == null || MenuOpen || menuClosedFrame == Time.frameCount) return;
            var keyboard = Keyboard.current;
            if (world.EndingPresented)
            {
                BlendCloseView();
                if (keyboard != null && keyboard.rKey.wasPressedThisFrame)
                    world.Restart();
                return;
            }
            if (mode == ViewMode.Sequence) { FocusPrompt = ""; if (!directSequencePose) BlendCloseView(); return; }
            if (mode == ViewMode.Explore)
                Explore();
            else
            {
                FocusPrompt = "";
                BlendCloseView();
                if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
                    ExitCloseView();
                if (mode == ViewMode.Pickup) { activePickup.TickHeld(interact.IsPressed(), Time.deltaTime); return; }
                if (mode == ViewMode.Interaction && keyboard != null && keyboard.enterKey.wasPressedThisFrame)
                    activeInteraction.Confirm();
                if (mode == ViewMode.Interaction && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                {
                    Ray ray = viewCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
                    if (TryInteractionHit(ray, 20, out var hit)) activeInteraction.Click(hit.collider);
                }
            }
        }

        void Explore()
        {
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) LockCursor(true);
                return;
            }
            Vector2 delta = look.ReadValue<Vector2>() * world.Settings.MouseSensitivity;
            transform.Rotate(0, delta.x, 0);
            pitch = Mathf.Clamp(pitch - delta.y, -80, 80);
            viewCamera.transform.localRotation = Quaternion.Euler(pitch, 0, 0);
            Vector2 input = Vector2.ClampMagnitude(move.ReadValue<Vector2>(), 1);
            float speed = sprint.IsPressed() ? world.Settings.SprintMetresPerSecond : world.Settings.WalkMetresPerSecond;
            if (body.isGrounded && verticalSpeed < 0) verticalSpeed = -2;
            verticalSpeed += Physics.gravity.y * Time.deltaTime;
            body.Move(((transform.right * input.x + transform.forward * input.y) * speed +
                Vector3.up * verticalSpeed) * Time.deltaTime);
            FocusPrompt = "";
            Ray ray = new Ray(viewCamera.transform.position, viewCamera.transform.forward);
            if (TryInteractionHit(ray, world.Settings.InteractionMetres, out var hit))
            {
                var target = hit.collider.GetComponentInParent<Interactable>();
                if (target)
                {
                    FocusPrompt = "E  " + target.Prompt;
                    // WasPressedThisFrame intentionally uses the physical press, not the template Hold completion.
                    if (interact.WasPressedThisFrame()) target.Use(this);
                }
            }
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) LockCursor(false);
        }

        bool TryInteractionHit(Ray ray, float distance, out RaycastHit nearest)
        {
            // The authored close camera can move behind the player's body. It must not select its own collider.
            int count = Physics.RaycastNonAlloc(ray, interactionHits, distance, ~0, QueryTriggerInteraction.Ignore);
            nearest = default;
            float best = float.PositiveInfinity;
            for (int i = 0; i < count; i++)
            {
                var hit = interactionHits[i];
                if (hit.collider.transform.IsChildOf(transform) || hit.distance >= best) continue;
                nearest = hit; best = hit.distance;
            }
            return best < float.PositiveInfinity;
        }

        public void EnterTabletView(RuneTablet tablet, Transform anchor)
        {
            EnterInteractionView(tablet, anchor);
        }

        public void EnterPickupView(ThreatPickup pickup, Transform anchor) { activePickup = pickup; BeginView(anchor, ViewMode.Pickup); }
        public bool InterruptPickup()
        {
            if (mode != ViewMode.Pickup || !activePickup) return false;
            bool cancelled = activePickup.Cancel(true); ExitCloseView(); return cancelled;
        }
        public void EnterLockView(CipherBox box, Transform anchor) => EnterInteractionView(box, anchor);
        public void EnterInteractionView(ICloseViewInteraction interaction, Transform anchor)
        {
            if (interaction == null) throw new System.ArgumentNullException(nameof(interaction));
            BeginView(anchor, ViewMode.Interaction);
            activeInteraction = interaction;
        }
        public void EnterInspectionView(Transform anchor) => BeginView(anchor, ViewMode.Inspect);
        public void EnterReadingView(Transform anchor) => BeginView(anchor, ViewMode.Reading);
        bool directSequencePose;
        public void EnterSequenceView(Transform anchor, bool direct = false) { directSequencePose = direct; BeginView(anchor, ViewMode.Sequence); }
        public void EnterEndingView(Transform anchor) => BeginView(anchor, ViewMode.Ending);

        void BeginView(Transform anchor, ViewMode next)
        {
            if (!anchor) throw new System.InvalidOperationException("Close view anchor is missing.");
            if (mode == ViewMode.Explore)
            {
                homePosition = viewCamera.transform.localPosition;
                homeRotation = viewCamera.transform.localRotation;
            }
            if (mode == ViewMode.Pickup && next != ViewMode.Pickup) { activePickup.Cancel(false); activePickup = null; }
            if (next != ViewMode.Interaction) activeInteraction = null;
            closeAnchor = anchor;
            var framing = anchor.GetComponent<InspectionFraming>();
            viewCamera.nearClipPlane = framing && next != ViewMode.Sequence ? framing.NearPlane : explorationNearPlane;
            var viewport = explorationViewport;
            if (next == ViewMode.Reading) viewport.width *= readingViewportWidth;
            viewCamera.rect = viewport;
            mode = next;
            if (next != ViewMode.Sequence && next != ViewMode.Ending)
                viewCamera.transform.SetPositionAndRotation(anchor.position, anchor.rotation);
            LockCursor(false);
        }

        public void ExitCloseView()
        {
            if (mode == ViewMode.Ending || mode == ViewMode.Sequence || mode == ViewMode.Explore) return;
            if (mode == ViewMode.Reading && world.OnReadingClosed()) return;
            if (mode == ViewMode.Pickup) { activePickup.Cancel(false); activePickup = null; }
            mode = ViewMode.Returning;
            viewCamera.rect = explorationViewport;
            viewCamera.nearClipPlane = explorationNearPlane;
            viewCamera.transform.SetLocalPositionAndRotation(homePosition, homeRotation);
            closeAnchor = null;
            activeInteraction = null;
            LockCursor(true);
        }

        void BlendCloseView()
        {
            Transform cam = viewCamera.transform;
            Vector3 position = closeAnchor ? closeAnchor.position : transform.TransformPoint(homePosition);
            Quaternion rotation = closeAnchor ? closeAnchor.rotation : transform.rotation * homeRotation;
            float blend = 1 - Mathf.Exp(-Time.deltaTime * 6 / world.Settings.CameraBlendSeconds);
            cam.SetPositionAndRotation(Vector3.Lerp(cam.position, position, blend), Quaternion.Slerp(cam.rotation, rotation, blend));
            if (mode == ViewMode.Returning && Vector3.Distance(cam.position, position) < .015f &&
                Quaternion.Angle(cam.rotation, rotation) < .5f)
            {
                cam.localPosition = homePosition;
                cam.localRotation = homeRotation;
                mode = ViewMode.Explore;
            }
        }

        public void SetMenuOpen(bool value)
        { if (MenuOpen && !value) menuClosedFrame = Time.frameCount; MenuOpen = value; }

        public void ReturnToExploration()
        {
            if (mode == ViewMode.Pickup && activePickup) activePickup.Cancel(false);
            activePickup = null; activeInteraction = null; closeAnchor = null;
            mode = ViewMode.Explore; FocusPrompt = ""; verticalSpeed = 0;
            viewCamera.rect = explorationViewport;
            viewCamera.nearClipPlane = explorationNearPlane;
            viewCamera.transform.SetLocalPositionAndRotation(homePosition, homeRotation);
            pitch = Mathf.DeltaAngle(0, homeRotation.eulerAngles.x);
            LockCursor(true);
        }

        static void LockCursor(bool locked)
        { Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None; Cursor.visible = !locked; }

        void OnEnable() { if (actions) actions.FindActionMap("Player", true).Enable(); }
        void OnDisable() { if (mode == ViewMode.Pickup) ExitCloseView(); if (actions) actions.Disable(); LockCursor(false); }
        void OnDestroy() { if (actions) { actions.Disable(); Destroy(actions); } }
    }
}
