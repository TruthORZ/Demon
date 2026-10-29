using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Demon.Slice
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class SlicePlayer : MonoBehaviour
    {
        enum ViewMode { Explore, Tablet, Reading, Returning, Sequence, Ending, Lock, Inspect, Pickup }
        [SerializeField] Camera viewCamera;
        [SerializeField] InputActionAsset inputActions;
        CharacterController body;
        SliceSceneController world;
        InputActionAsset actions;
        InputAction move, look, interact, sprint;
        Transform closeAnchor;
        Vector3 homePosition;
        Quaternion homeRotation;
        ViewMode mode;
        float pitch, verticalSpeed;
        SliceTablet activeTablet;
        CipherBox activeLock;
        ThreatPickup activePickup;
        public ThreatPickup CurrentPickup => activePickup;
        public bool IsPickingUp => mode == ViewMode.Pickup;
        public bool InspectingLock => mode == ViewMode.Lock;
        public bool CanUseTools => mode == ViewMode.Explore && Cursor.lockState == CursorLockMode.Locked;
        readonly RaycastHit[] interactionHits = new RaycastHit[32];
        public Camera ViewCamera => viewCamera;
        public bool InspectingTablet => mode == ViewMode.Tablet;
        public SliceTablet ActiveTablet => activeTablet;
        public bool Reading => mode == ViewMode.Reading;
        public string FocusPrompt { get; private set; } = "";

        public void Configure(Camera camera, InputActionAsset source)
        { viewCamera = camera; inputActions = source; }

        public void Initialize(SliceSceneController controller)
        {
            if (!viewCamera || !inputActions)
                throw new System.InvalidOperationException("Player camera or Input Actions is missing.");
            world = controller;
            body = GetComponent<CharacterController>();
            actions = Instantiate(inputActions);
            move = actions.FindAction("Player/Move", true);
            look = actions.FindAction("Player/Look", true);
            interact = actions.FindAction("Player/Interact", true);
            sprint = actions.FindAction("Player/Sprint", true);
            actions.FindActionMap("Player", true).Enable();
            homePosition = viewCamera.transform.localPosition;
            homeRotation = viewCamera.transform.localRotation;
            pitch = viewCamera.transform.localEulerAngles.x;
            LockCursor(true);
        }

        void Update()
        {
            if (!world || world.Session == null) return;
            var keyboard = Keyboard.current;
            if (world.EndingPresented)
            {
                BlendCloseView();
                if (keyboard != null && keyboard.rKey.wasPressedThisFrame)
                    SceneManager.LoadScene(SceneManager.GetActiveScene().path);
                return;
            }
            if (mode == ViewMode.Sequence) { FocusPrompt = ""; BlendCloseView(); return; }
            if (mode == ViewMode.Explore)
                Explore();
            else
            {
                FocusPrompt = "";
                BlendCloseView();
                if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
                    ExitCloseView();
                if (mode == ViewMode.Pickup) { activePickup.TickHeld(interact.IsPressed(), Time.deltaTime); return; }
                if (mode == ViewMode.Tablet && keyboard != null && keyboard.enterKey.wasPressedThisFrame)
                    activeTablet.TryCompile();
                if (mode == ViewMode.Lock && keyboard != null && keyboard.enterKey.wasPressedThisFrame) activeLock.TryConfirm();
                if ((mode == ViewMode.Tablet || mode == ViewMode.Lock) && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                {
                    Ray ray = viewCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
                    if (TryInteractionHit(ray, 20, out var hit))
                    {
                        if (mode == ViewMode.Tablet) activeTablet.Click(hit.collider);
                        else activeLock.Click(hit.collider);
                    }
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
                var target = hit.collider.GetComponentInParent<SliceInteractable>();
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

        public void EnterTabletView(SliceTablet tablet, Transform anchor)
        {
            activeTablet = tablet;
            BeginView(anchor, ViewMode.Tablet);
        }

        public void EnterPickupView(ThreatPickup pickup, Transform anchor) { activePickup = pickup; BeginView(anchor, ViewMode.Pickup); }
        public bool InterruptPickup()
        {
            if (mode != ViewMode.Pickup || !activePickup) return false;
            bool cancelled = activePickup.Cancel(true); ExitCloseView(); return cancelled;
        }
        public void EnterLockView(CipherBox box, Transform anchor) { activeLock = box; BeginView(anchor, ViewMode.Lock); }
        public void EnterInspectionView(Transform anchor) => BeginView(anchor, ViewMode.Inspect);
        public void EnterReadingView(Transform anchor) => BeginView(anchor, ViewMode.Reading);
        public void EnterSequenceView(Transform anchor) => BeginView(anchor, ViewMode.Sequence);
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
            closeAnchor = anchor;
            mode = next;
            LockCursor(false);
        }

        public void ExitCloseView()
        {
            if (mode == ViewMode.Ending || mode == ViewMode.Sequence || mode == ViewMode.Explore) return;
            if (mode == ViewMode.Reading && world.OnReadingClosed()) return;
            if (mode == ViewMode.Pickup) { activePickup.Cancel(false); activePickup = null; }
            mode = ViewMode.Returning;
            closeAnchor = null;
            activeTablet = null; activeLock = null;
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

        static void LockCursor(bool locked)
        { Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None; Cursor.visible = !locked; }

        void OnEnable() { if (actions) actions.FindActionMap("Player", true).Enable(); }
        void OnDisable() { if (mode == ViewMode.Pickup) ExitCloseView(); if (actions) actions.Disable(); LockCursor(false); }
        void OnDestroy() { if (actions) { actions.Disable(); Destroy(actions); } }
    }
}
