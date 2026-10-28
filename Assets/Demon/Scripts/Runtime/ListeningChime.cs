using System;
using Demon.Slice.Domain;
using UnityEngine;

namespace Demon.Slice
{
    public sealed class ListeningChime : MonoBehaviour
    {
        [SerializeField] Transform monitoredTarget;
        [Tooltip("World-space spherical region, in metres; center follows this saved transform.")]
        [SerializeField, Min(.1f)] float radiusMetres = 2;
        [SerializeField] AudioSource source;
        [SerializeField] AudioClip enterClip;
        [SerializeField] Transform hangingVisual;
        [SerializeField, Min(.1f)] float swingSeconds = 1.5f;
        [SerializeField, Range(0, 45)] float swingDegrees = 14;
        readonly RegionEntryLatch entry = new RegionEntryLatch();
        ListeningTool tool;
        SliceSceneController world;
        Quaternion restingRotation;
        float swingRemaining;
        public int RingCount { get; private set; }
        public float RadiusMetres => radiusMetres;
        public Transform MonitoredTarget => monitoredTarget;
        public AudioSource Source => source;
        public bool TargetInside => monitoredTarget && (monitoredTarget.position - transform.position).sqrMagnitude <= radiusMetres * radiusMetres;
        public void Configure(Transform target, float radius, AudioSource audio, AudioClip clip, Transform visual)
        { monitoredTarget = target; radiusMetres = radius; source = audio; enterClip = clip; hangingVisual = visual; }
        public void Initialize(ListeningTool owner, SliceSceneController controller)
        {
            if (!monitoredTarget || !source || !enterClip || !hangingVisual)
                throw new InvalidOperationException("Chime requires an explicit target, audio clip/source and hanging visual.");
            tool = owner; world = controller; restingRotation = hangingVisual.localRotation;
            SetListening(tool.Active);
        }
        public void SetListening(bool enabled) => entry.SetEnabled(enabled && isActiveAndEnabled, TargetInside);
        void OnEnable() { if (tool) SetListening(tool.Active && !world.Session.Descended); }
        void OnDisable()
        {
            entry.SetEnabled(false, false); swingRemaining = 0;
            if (source) source.Stop();
            if (tool && hangingVisual) hangingVisual.localRotation = restingRotation;
        }
        void Update()
        {
            if (!tool) return;
            SetListening(tool.Active && !world.Session.Descended);
            if (entry.Sample(TargetInside))
            {
                RingCount++; source.PlayOneShot(enterClip); swingRemaining = swingSeconds;
            }
            if (swingRemaining > 0)
            {
                swingRemaining = Mathf.Max(0, swingRemaining - Time.deltaTime);
                hangingVisual.localRotation = restingRotation * Quaternion.Euler(0, 0,
                    Mathf.Sin((swingSeconds - swingRemaining) * 17) * swingDegrees * swingRemaining / swingSeconds);
            }
            else hangingVisual.localRotation = restingRotation;
        }
        void OnDrawGizmosSelected() { Gizmos.color = new Color(.3f, .8f, .8f, .8f); Gizmos.DrawWireSphere(transform.position, radiusMetres); }
    }
}
