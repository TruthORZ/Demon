using System;
using Demon.Slice.Domain;
using UnityEngine;
using UnityEngine.InputSystem;
namespace Demon.Slice
{
    public sealed class BeamTether : MonoBehaviour
    {
        [SerializeField] BeamDefinition definition;
        [SerializeField] Transform aimRegion;
        [SerializeField] GameObject targetVisual;
        [SerializeField] Transform vaneBlade;
        [SerializeField] Light flashlight;
        [SerializeField] AudioSource switchSound;
        [SerializeField] LineRenderer beamLine;
        [SerializeField] Transform scatteringVolume;
        [SerializeField] GameObject flashlightBody;
        [SerializeField] Light targetGlow;
        [SerializeField] Transform observationPoint;
        readonly RaycastHit[] hits = new RaycastHit[64];
        GameSession session;
        PlayerController player;
        CipherBox cipher;
        int motionDay;
        CompletePlaythrough main;
        public bool HasLamp => main ? main.HasFlashlight : Equipped;
        public int MotionDay => motionDay;
        public StableBeamMotion Rule { get; private set; }
        public BeamDefinition Definition => definition;
        public Transform AimRegion => aimRegion;
        public Transform VaneBlade => vaneBlade;
        public Transform ObservationPoint => observationPoint;
        public bool Equipped => cipher && cipher.Acquired;
        public bool MotionReady => session != null && session.Clock.Day >= motionDay;
        public bool DemonCanMove => MotionReady && (Rule == null || Rule.DemonMotion == 1);
        public bool VaneCanMove => !MotionReady || (Rule != null && Rule.VaneMotion == 1);
        public bool ProxyCanTravel => session != null && !session.Descended && MotionReady;
        public bool BeamOn { get; private set; }
        public bool LampSwitchedOn { get; private set; }
        internal void RestoreLampSwitch(bool value) => LampSwitchedOn = value;
        public bool TargetVisible => targetVisual && targetVisual.activeInHierarchy;
        public bool LightVisible => flashlight && flashlight.enabled;
        public bool ClearAim { get; private set; }
        public string Status
        {
            get
            {
                if (!Equipped) return "";
                if (main && !HasLamp) return "The torch is on the cabin table.";
                if (Rule.Locked) return main ? "The tracks have stopped." : definition.LockedText;
                if (ClearAim && BeamOn) return main ? "Hold steady…" : $"Steady beam: {Rule.StableSeconds:0.0} / {definition.StableSeconds:0.0} s";
                return main ? "" : definition.ReadyText;
            }
        }
        public void Configure(BeamDefinition data, Transform target, GameObject visible, Transform blade,
            Light lamp, LineRenderer line, GameObject body, Light glow, Transform observer)
        { definition = data; aimRegion = target; targetVisual = visible; vaneBlade = blade; flashlight = lamp; beamLine = line; flashlightBody = body; targetGlow = glow; observationPoint = observer; }
        public void Initialize(GameSession rules, PlayerController actor, CipherBox tool, ListeningTool listener, int firstMotionDay)
        {
            if (!definition || !aimRegion || !targetVisual || !vaneBlade || !flashlight || (!beamLine && !scatteringVolume) || !flashlightBody || !targetGlow || !observationPoint || !tool || !listener || !actor || rules == null || firstMotionDay < 1 || firstMotionDay > 9)
                throw new InvalidOperationException("T06 requires saved aim region, vane, flashlight, feedback and composition references.");
            if (definition.RangeMetres <= 0 || definition.RegionRadiusMetres <= 0) throw new InvalidOperationException("Beam range and region radius must be positive.");
            Rule?.Release(); BeamOn = false; LampSwitchedOn = false; ClearAim = false;
            flashlightBody.SetActive(false); targetGlow.enabled = false;
            if (switchSound) switchSound.Stop();
            session = rules; player = actor; cipher = tool; main = tool.World.Playthrough; motionDay = firstMotionDay; Rule = new StableBeamMotion(definition.StableSeconds); SetLight(false);
            targetVisual.SetActive(ProxyCanTravel);
        }
        void LateUpdate()
        {
            if (Rule == null || player.MenuOpen) return;
            bool exploring = player.CanUseTools && !session.Descended;
            // The main route uses a latched switch. UI and close-view clicks cannot change it.
            if (main && HasLamp && exploring && Mouse.current?.leftButton.wasPressedThisFrame == true)
            {
                LampSwitchedOn = !LampSwitchedOn;
                if (switchSound) switchSound.Play();
            }
            BeamOn = HasLamp && exploring && (main ? LampSwitchedOn : Mouse.current != null && Mouse.current.rightButton.isPressed);
            ClearAim = ProxyCanTravel && CanSeeRegion();
            Rule.Advance(BeamOn && Equipped && ClearAim && MotionReady, Time.deltaTime);
            targetVisual.SetActive(ProxyCanTravel);
            flashlightBody.SetActive(HasLamp && exploring);
            SetLight(BeamOn);
            targetGlow.enabled = Rule.Locked;
            if (!session.Descended && VaneCanMove)
                vaneBlade.Rotate(Vector3.up, definition.VaneDegreesPerSecond * Time.deltaTime, Space.Self);
        }
        public bool CanSeeRegion()
        {
            Vector3 origin = player.ViewCamera.transform.position;
            Vector3 offset = aimRegion.position - origin;
            float distance = offset.magnitude;
            if (distance < .001f || distance > definition.RangeMetres) return false;
            Vector3 forward = player.ViewCamera.transform.forward;
            float along = Vector3.Dot(offset, forward);
            if (along <= 0 || (offset - forward * along).sqrMagnitude > definition.RegionRadiusMetres * definition.RegionRadiusMetres) return false;
            return NearestObstacle(origin, offset / distance, distance) >= distance - .01f;
        }
        float NearestObstacle(Vector3 origin, Vector3 direction, float max)
        {
            int count = Physics.RaycastNonAlloc(origin, direction, hits, max, ~0, QueryTriggerInteraction.Ignore);
            float nearest = max;
            for (int i = 0; i < count; i++)
            {
                if (hits[i].collider.transform.IsChildOf(player.transform) || hits[i].collider.transform.IsChildOf(targetVisual.transform)) continue;
                nearest = Mathf.Min(nearest, hits[i].distance);
            }
            return nearest;
        }
        void SetLight(bool on)
        {
            flashlight.enabled = on;
            if (beamLine) beamLine.enabled = on && !scatteringVolume;
            flashlight.range = definition.RangeMetres; flashlight.spotAngle = definition.SpotAngleDegrees;
            flashlight.intensity = definition.LightIntensity; flashlight.color = definition.BeamColor;
            if (scatteringVolume)
            {
                scatteringVolume.gameObject.SetActive(on);
                float diameter = 2 * Mathf.Tan(flashlight.spotAngle * .5f * Mathf.Deg2Rad) * flashlight.range;
                scatteringVolume.localScale = new Vector3(diameter, diameter, flashlight.range);
                return;
            }
            if (!on || session == null) return;
            var camera = player.ViewCamera.transform;
            float distance = NearestObstacle(camera.position, camera.forward, definition.RangeMetres);
            beamLine.SetPosition(0, flashlight.transform.position);
            beamLine.SetPosition(1, camera.position + camera.forward * distance);
        }
        void OnDisable()
        {
            Rule?.Release(); BeamOn = false; LampSwitchedOn = false; ClearAim = false;
            if (flashlight) flashlight.enabled = false;
            if (scatteringVolume) scatteringVolume.gameObject.SetActive(false);
            if (beamLine) beamLine.enabled = false;
            if (flashlightBody) flashlightBody.SetActive(false);
            if (targetGlow) targetGlow.enabled = false;
        }
        void OnDestroy() => Rule?.Release();
        void OnDrawGizmosSelected()
        {
            if (!definition || !aimRegion) return;
            Gizmos.color = Color.cyan; Gizmos.DrawWireSphere(aimRegion.position, definition.RegionRadiusMetres);
        }
    }
}
