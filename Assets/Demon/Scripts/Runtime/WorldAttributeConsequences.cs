using System;
using Demon.Slice.Domain;
using UnityEngine;
namespace Demon.Slice
{
    public sealed class WorldAttributeConsequences : MonoBehaviour
    {
        [Header("Transferable broadcast; change this clip once for both emitters")]
        [SerializeField] AudioClip broadcastClip;
        [SerializeField] AudioClip staticClip;
        [Header("Saved emitters own volume, distance and spatialization")]
        [SerializeField] AudioSource radioBroadcast;
        [SerializeField] AudioSource radioStatic;
        [SerializeField] AudioSource demonBroadcast;
        [Header("Fixed solar reference; root position remains the cipher landmark")]
        [SerializeField] Renderer solarDisk;
        [SerializeField] Material normalSolarMaterial;
        [SerializeField] Material emptySolarMaterial;
        [SerializeField] GameObject whiteRim;
        [SerializeField] GameObject[] solarRays;
        SliceSceneController world;
        int parkedSample;
        public ScheduledWorldAttributes Rule { get; private set; }
        public AudioSource RadioBroadcast => radioBroadcast;
        public AudioSource RadioStatic => radioStatic;
        public AudioSource DemonBroadcast => demonBroadcast;
        public AudioClip BroadcastClip => broadcastClip;
        public bool WhiteRimVisible => whiteRim && whiteRim.activeInHierarchy;
        public Renderer SolarDisk => solarDisk;
        public void Configure(AudioClip broadcast, AudioClip noise, AudioSource radio, AudioSource hiss, AudioSource demon,
            Renderer disk, Material normal, Material empty, GameObject rim, GameObject[] rays)
        { broadcastClip = broadcast; staticClip = noise; radioBroadcast = radio; radioStatic = hiss; demonBroadcast = demon; solarDisk = disk; normalSolarMaterial = normal; emptySolarMaterial = empty; whiteRim = rim; solarRays = rays; }
        public void Initialize(SliceSceneController controller)
        {
            if (!controller.Campaign || !broadcastClip || !staticClip || !radioBroadcast || !radioStatic || !demonBroadcast || !solarDisk || !normalSolarMaterial || !emptySolarMaterial || !whiteRim || solarRays == null || solarRays.Length != 8)
                throw new InvalidOperationException("Scheduled attributes require saved broadcast/static emitters and solar reference assets.");
            foreach (var ray in solarRays) if (!ray) throw new InvalidOperationException("Missing authored solar ray.");
            if (!radioBroadcast.loop || !radioStatic.loop || !demonBroadcast.loop || !radioBroadcast.enabled || !radioStatic.enabled || !demonBroadcast.enabled)
                throw new InvalidOperationException("Attribute emitters must be enabled native looping AudioSources.");
            world = controller;
            var plan = world.Campaign.Definition.Tablets;
            Rule = new ScheduledWorldAttributes(plan[1].DateNodeDay, plan[2].DateNodeDay);
            // A single authored transferable clip owns both playback endpoints; source gains stay editable.
            radioBroadcast.clip = broadcastClip; demonBroadcast.clip = broadcastClip; radioStatic.clip = staticClip;
            world.Session.Clock.Midnight += OnMidnight;
            Rule.AdvanceToDay(world.Session.Clock.Day); ApplyPresentation();
        }
        void OnMidnight(int day) { Rule.AdvanceToDay(day); if (isActiveAndEnabled) ApplyPresentation(); }
        void OnEnable() { if (world && Rule != null) ApplyPresentation(); }
        void LateUpdate() { if (world && Rule != null) ApplyPresentation(); }
        void ApplyPresentation()
        {
            bool above = world.Presentation.ExpectedSunAboveHorizon;
            bool lost = Rule.SunLight == 0;
            solarDisk.sharedMaterial = lost ? emptySolarMaterial : normalSolarMaterial;
            solarDisk.gameObject.SetActive(above); whiteRim.SetActive(above && lost);
            foreach (var ray in solarRays) if (ray.activeSelf != (above && !lost)) ray.SetActive(above && !lost);
            if (world.EndingPresented) { StopAudio(); return; }
            bool atRadio = Rule.RadioSound == 1;
            bool atDemon = Rule.DemonSound == 1 && world.Session.LakeSeal == 0;
            if (!atRadio && radioBroadcast.isPlaying) parkedSample = radioBroadcast.timeSamples;
            if (!atDemon && demonBroadcast.isPlaying) parkedSample = demonBroadcast.timeSamples;
            SetLoop(radioBroadcast, atRadio, parkedSample); SetLoop(demonBroadcast, atDemon, parkedSample);
            SetLoop(radioStatic, true, 0); // Device/environment noise is not the transferable broadcast.
        }
        static void SetLoop(AudioSource source, bool play, int sample)
        {
            if (!play) { if (source.isPlaying) source.Stop(); return; }
            if (!source.isPlaying) { source.Play(); source.timeSamples = Mathf.Clamp(sample, 0, source.clip.samples - 1); }
        }
        void StopAudio() { if (radioBroadcast && radioBroadcast.isPlaying) parkedSample = radioBroadcast.timeSamples; if (demonBroadcast && demonBroadcast.isPlaying) parkedSample = demonBroadcast.timeSamples; if (radioBroadcast) radioBroadcast.Stop(); if (radioStatic) radioStatic.Stop(); if (demonBroadcast) demonBroadcast.Stop(); }
        void OnDisable() => StopAudio();
        void OnDestroy() { if (world && world.Session != null) world.Session.Clock.Midnight -= OnMidnight; }
    }
}
