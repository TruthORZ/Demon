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
        [SerializeField] LineRenderer beamLine;
        [SerializeField] GameObject flashlightBody;
        [SerializeField] Light targetGlow;
        [SerializeField] Transform observationPoint;
        readonly RaycastHit[] hits = new RaycastHit[64];
        SliceSceneController world;
        public StableBeamMotion Rule { get; private set; }
        public BeamDefinition Definition => definition;
        public Transform AimRegion => aimRegion;
        public Transform VaneBlade => vaneBlade;
        public Transform ObservationPoint => observationPoint;
        public bool Equipped => world && world.Cipher && world.Cipher.Acquired;
        public bool MotionReady => world && world.Session != null && world.Session.Clock.Day >= definition.DemonMotionDay;
        public bool DemonCanMove => MotionReady && (Rule == null || Rule.DemonMotion == 1);
        public bool VaneCanMove => !MotionReady || (Rule != null && Rule.VaneMotion == 1);
        public bool ProxyCanTravel => world && world.Session != null && !world.Session.Descended && MotionReady;
        public bool BeamOn { get; private set; }
        public bool TargetVisible => targetVisual && targetVisual.activeInHierarchy;
        public bool LightVisible => flashlight && flashlight.enabled;
        public bool ClearAim { get; private set; }
        public string Status
        {
            get
            {
                if (!Equipped) return "";
                if (Rule.Locked) return definition.LockedText;
                if (ClearAim && BeamOn) return $"稳定照射 {Rule.StableSeconds:0.0} / {definition.StableSeconds:0.0} 秒\n保持光束跟随雪痕区域。";
                return definition.ReadyText;
            }
        }
        public void Configure(BeamDefinition data, Transform target, GameObject visible, Transform blade,
            Light lamp, LineRenderer line, GameObject body, Light glow, Transform observer)
        { definition = data; aimRegion = target; targetVisual = visible; vaneBlade = blade; flashlight = lamp; beamLine = line; flashlightBody = body; targetGlow = glow; observationPoint = observer; }
        public void Initialize(SliceSceneController controller)
        {
            if (!definition || !aimRegion || !targetVisual || !vaneBlade || !flashlight || !beamLine || !flashlightBody || !targetGlow || !observationPoint || !controller.Cipher || !controller.Listening)
                throw new InvalidOperationException("T06 requires saved aim region, vane, flashlight, feedback and composition references.");
            if (definition.RangeMetres <= 0 || definition.RegionRadiusMetres <= 0) throw new InvalidOperationException("Beam range and region radius must be positive.");
            world = controller; Rule = new StableBeamMotion(definition.StableSeconds); SetLight(false);
            targetVisual.SetActive(ProxyCanTravel);
        }
        void LateUpdate()
        {
            if (Rule == null) return;
            bool exploring = world.Player.CanUseTools && !world.Session.Descended;
            BeamOn = Equipped && exploring && Mouse.current != null && Mouse.current.rightButton.isPressed;
            ClearAim = ProxyCanTravel && CanSeeRegion();
            Rule.Advance(BeamOn && ClearAim && MotionReady, Time.deltaTime);
            targetVisual.SetActive(ProxyCanTravel);
            flashlightBody.SetActive(Equipped && exploring);
            SetLight(BeamOn);
            targetGlow.enabled = Rule.Locked;
            if (!world.Session.Descended && VaneCanMove)
                vaneBlade.Rotate(Vector3.up, definition.VaneDegreesPerSecond * Time.deltaTime, Space.Self);
        }
        public bool CanSeeRegion()
        {
            Vector3 origin = world.Player.ViewCamera.transform.position;
            Vector3 offset = aimRegion.position - origin;
            float distance = offset.magnitude;
            if (distance < .001f || distance > definition.RangeMetres) return false;
            Vector3 forward = world.Player.ViewCamera.transform.forward;
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
                if (hits[i].collider.transform.IsChildOf(world.Player.transform) || hits[i].collider.transform.IsChildOf(targetVisual.transform)) continue;
                nearest = Mathf.Min(nearest, hits[i].distance);
            }
            return nearest;
        }
        void SetLight(bool on)
        {
            flashlight.enabled = on; beamLine.enabled = on;
            flashlight.range = definition.RangeMetres; flashlight.spotAngle = definition.SpotAngleDegrees;
            flashlight.intensity = definition.LightIntensity; flashlight.color = definition.BeamColor;
            if (!on || !world) return;
            var camera = world.Player.ViewCamera.transform;
            float distance = NearestObstacle(camera.position, camera.forward, definition.RangeMetres);
            beamLine.SetPosition(0, flashlight.transform.position);
            beamLine.SetPosition(1, camera.position + camera.forward * distance);
        }
        void OnDisable()
        {
            Rule?.Release(); BeamOn = false; ClearAim = false;
            if (flashlight) flashlight.enabled = false;
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
