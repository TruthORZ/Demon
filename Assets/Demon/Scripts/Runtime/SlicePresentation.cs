using UnityEngine;

namespace Demon.Slice
{
    public sealed class SlicePresentation : MonoBehaviour
    {
        [SerializeField] Transform lidPivot;
        [SerializeField] Vector3 openLidEuler = new Vector3(105, 0, 0);
        [SerializeField, Min(.05f)] float lidBlendSeconds = .6f;
        [SerializeField] GameObject lakeSeal;
        [SerializeField] GameObject boxSeal;
        [SerializeField] GameObject descendedFigure;
        [SerializeField] Transform boxReadView;
        [SerializeField] Light sun;
        SliceSceneController world;
        Quaternion authoredLidRotation;
        bool boxOpened;
        public Transform BoxReadView => boxReadView;
        public bool LakeSealVisible => lakeSeal && lakeSeal.activeSelf;
        public Transform LidPivot => lidPivot;

        public void Configure(Transform lid, GameObject lake, GameObject boxLock, GameObject figure,
            Transform readAnchor, Light daylight)
        { lidPivot = lid; lakeSeal = lake; boxSeal = boxLock; descendedFigure = figure; boxReadView = readAnchor; sun = daylight; }

        public void Initialize(SliceSceneController controller)
        {
            if (!lidPivot || !lakeSeal || !boxSeal || !descendedFigure || !boxReadView || !sun)
                throw new System.InvalidOperationException("Presentation has missing authored references.");
            world = controller;
            authoredLidRotation = lidPivot.localRotation;
            Refresh();
        }

        public void OpenBox() => boxOpened = true;
        public void SlamBox()
        {
            boxOpened = false;
            lidPivot.localRotation = authoredLidRotation;
        }

        void Update()
        {
            if (!world || world.Session == null) return;
            Refresh();
            Quaternion target = authoredLidRotation * Quaternion.Euler(boxOpened && world.Session.BoxSeal == 0 ? openLidEuler : Vector3.zero);
            lidPivot.localRotation = Quaternion.Slerp(lidPivot.localRotation, target, 1 - Mathf.Exp(-Time.deltaTime * 5 / lidBlendSeconds));
            float hour = (float)world.Session.Clock.MinuteOfDay / 60;
            float daylight = Mathf.Clamp01(Mathf.Sin((hour - 6) * Mathf.PI / 12));
            var settings = world.Settings;
            sun.intensity = Mathf.Lerp(settings.NightSunIntensity, settings.DaySunIntensity, daylight);
            sun.transform.rotation = Quaternion.Euler(hour / 24 * 360 - 90, -30, 0);
            RenderSettings.ambientLight = Color.Lerp(settings.NightAmbient, settings.DayAmbient, daylight);
            RenderSettings.fogColor = Color.Lerp(settings.NightFog, settings.DayFog, daylight);
            world.Player.ViewCamera.backgroundColor = RenderSettings.fogColor;
        }

        void Refresh()
        {
            lakeSeal.SetActive(world.Session.LakeSeal == 1);
            boxSeal.SetActive(world.Session.BoxSeal == 1);
            descendedFigure.SetActive(world.Session.Descended);
        }
    }
}
