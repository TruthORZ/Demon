using UnityEngine;

namespace Demon.Slice
{
    // Authored disc/rim visuals follow the same light direction and fixed cipher bearing.
    public sealed class SkySunPresentation : MonoBehaviour
    {
        [SerializeField] GameSceneController world;
        [SerializeField] Transform disc;
        [SerializeField] GameObject darkCore;
        [SerializeField, Min(10)] float distanceMetres = 280;
        [SerializeField, Range(.002f, .05f)] float angularRadius = .014f;
        [SerializeField] Renderer discRenderer;
        [SerializeField, ColorUsage(false, true)] Color eclipseRim = new Color(1.8f, 1.95f, 2.2f);
        MaterialPropertyBlock discProperties;
        [SerializeField, Range(0,1)] float nightSkyExposure = .12f;
        Material originalSky, runtimeSky;
        float dayExposure;
        void OnEnable()
        {
            discProperties = new MaterialPropertyBlock();
            originalSky = RenderSettings.skybox;
            if (!discRenderer || !originalSky || !originalSky.HasProperty("_Exposure")) return;
            runtimeSky = new Material(originalSky);
            dayExposure = runtimeSky.GetFloat("_Exposure");
            RenderSettings.skybox = runtimeSky;
        }
        void OnDisable()
        {
            if (!runtimeSky) return;
            if (RenderSettings.skybox == runtimeSky) RenderSettings.skybox = originalSky;
            Destroy(runtimeSky);
        }
        public void Configure(GameSceneController owner, Transform visual, GameObject core)
        { world = owner; disc = visual; darkCore = core; }
        void LateUpdate()
        {
            if (!world || world.Session == null) return;
            var camera = world.Player.ViewCamera;
            Vector3 direction = -world.Presentation.Sun.transform.forward;
            float distance = Mathf.Min(distanceMetres, camera.farClipPlane * .7f);
            disc.position = camera.transform.position + direction * distance;
            disc.localScale = Vector3.one * distance * angularRadius * 2;
            disc.rotation = Quaternion.LookRotation(direction, Vector3.up);
            bool lost = world.Presentation.PermanentNight;
            disc.gameObject.SetActive(lost || world.Presentation.ExpectedSunAboveHorizon);
            darkCore.SetActive(lost);
            if (runtimeSky) runtimeSky.SetFloat("_Exposure", Mathf.Lerp(nightSkyExposure, dayExposure, lost ? 0 : world.Presentation.ExpectedDaylight));
            if (discRenderer)
            {
                Color tint = lost ? eclipseRim : world.Presentation.Sun.color * Mathf.Lerp(.85f, 2.1f, world.Presentation.ExpectedDaylight);
                discProperties.SetColor("_BaseColor", tint);
                discRenderer.SetPropertyBlock(discProperties);
            }
        }
    }
}
