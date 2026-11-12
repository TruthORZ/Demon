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
        }
    }
}
