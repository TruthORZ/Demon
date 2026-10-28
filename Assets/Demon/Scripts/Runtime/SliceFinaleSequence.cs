using System;
using Demon.Slice.Domain;
using UnityEngine;

namespace Demon.Slice
{
    // Explicit, saved scene composition owns the sequence. Rune programs never advance time.
    public sealed class SliceFinaleSequence : MonoBehaviour
    {
        enum Phase { Idle, Reserved, Reading, AfterReading, Clock, MidnightHold, Box, Complete }
        [Header("Authored views and clock hands")]
        [SerializeField] Transform clockView;
        [SerializeField] Transform hourHand;
        [SerializeField] Transform minuteHand;
        [SerializeField] Transform secondHand;
        [Header("Pacing - real seconds, except the reserved game minute")]
        [SerializeField, Range(.1f, 10)] float reservedGameMinutes = 1;
        [SerializeField, Min(0)] float afterReadingSeconds = 1.5f;
        [SerializeField, Min(.5f)] float clockCountdownSeconds = 6;
        [SerializeField, Min(0)] float midnightHoldSeconds = 1.2f;
        [SerializeField, Min(0)] float boxRevealSeconds = 1.2f;
        [Header("Saved placeholder effects; replace clips/models in Inspector")]
        [SerializeField] GameObject lakeOpenGlow;
        [SerializeField] Light tabletPulse;
        [SerializeField, Min(0)] float tabletPulseIntensity = 4;
        [SerializeField] AudioSource sequenceAudio;
        [SerializeField] AudioClip tickClip;
        [SerializeField] AudioClip slamClip;
        SliceSceneController world;
        SlicePresentation presentation;
        FinaleTimeWindow window;
        Phase phase;
        float elapsed;
        double countdownStart;
        int lastSecond = -1;
        public bool IsDirecting => phase >= Phase.AfterReading && phase < Phase.Complete;
        public bool TimeReserved => window != null && window.Reserved;
        public Transform ClockView => clockView;
        public float ClockCountdownSeconds => clockCountdownSeconds;
        public float AfterReadingSeconds => afterReadingSeconds;
        public bool LakeGlowVisible => lakeOpenGlow && lakeOpenGlow.activeSelf;

        public void Configure(Transform view, Transform hours, Transform minutes, Transform seconds,
            GameObject glow, Light pulse, AudioSource audio, AudioClip tick, AudioClip slam)
        {
            clockView = view; hourHand = hours; minuteHand = minutes; secondHand = seconds;
            lakeOpenGlow = glow; tabletPulse = pulse; sequenceAudio = audio; tickClip = tick; slamClip = slam;
        }

        public void Initialize(SliceSceneController controller, SlicePresentation visuals)
        {
            if (!clockView || !hourHand || !minuteHand || !secondHand || !lakeOpenGlow || !tabletPulse || !sequenceAudio)
                throw new InvalidOperationException("Finale requires saved clock, views, light and audio references.");
            world = controller; presentation = visuals;
            window = new FinaleTimeWindow((world.Settings.FinalDay - 1) * (double)GameClock.MinutesPerDay, reservedGameMinutes);
            UpdateVisuals();
        }

        public void ReserveTime()
        {
            if (phase == Phase.Idle && window.Reserve(world.Session.Clock.AbsoluteMinutes)) phase = Phase.Reserved;
        }

        public double LimitNaturalTime(double requested) => window.Limit(world.Session.Clock.AbsoluteMinutes, requested);
        public void RecordOpened() { ReserveTime(); if (phase == Phase.Reserved) phase = Phase.Reading; }

        public bool FinishRecord()
        {
            if (phase != Phase.Reading || world.Session.Descended) return false;
            world.Session.Clock.AdvanceTo(window.AlignForCountdown(world.Session.Clock.AbsoluteMinutes));
            phase = Phase.AfterReading; elapsed = 0;
            world.Player.EnterSequenceView(presentation.BoxReadView);
            world.SetMessage("");
            return true;
        }

        public void ReleaseForSleep() { window.Release(); phase = Phase.Idle; }

        // Returns true only when the sequence owns advancement, including the post-midnight reveal.
        public bool Tick(float dt)
        {
            elapsed += dt;
            if (phase == Phase.AfterReading && elapsed >= afterReadingSeconds)
            {
                phase = Phase.Clock; elapsed = 0;
                countdownStart = world.Session.Clock.AbsoluteMinutes;
                world.Player.EnterSequenceView(clockView);
            }
            else if (phase == Phase.Clock)
            {
                float t = Mathf.Clamp01(elapsed / clockCountdownSeconds);
                world.Session.Clock.AdvanceTo(Math.Max(world.Session.Clock.AbsoluteMinutes,
                    countdownStart + (window.Midnight - countdownStart) * t));
                int second = Mathf.FloorToInt(elapsed);
                if (second != lastSecond && tickClip) { sequenceAudio.PlayOneShot(tickClip); lastSecond = second; }
                if (world.Session.Descended)
                {
                    phase = Phase.MidnightHold; elapsed = 0;
                    presentation.SlamBox();
                    if (slamClip) sequenceAudio.PlayOneShot(slamClip);
                }
            }
            else if (phase == Phase.MidnightHold && elapsed >= midnightHoldSeconds)
            {
                phase = Phase.Box; elapsed = 0;
                world.Player.EnterSequenceView(presentation.BoxReadView);
            }
            else if (phase == Phase.Box && elapsed >= boxRevealSeconds)
            {
                phase = Phase.Complete;
                world.PresentEnding();
            }
            UpdateVisuals();
            return IsDirecting;
        }

        void UpdateVisuals()
        {
            double minute = world.Session.Clock.MinuteOfDay;
            hourHand.localRotation = Quaternion.Euler(0, 0, -(float)(minute % 720) / 2);
            minuteHand.localRotation = Quaternion.Euler(0, 0, -(float)(minute % 60) * 6);
            secondHand.localRotation = Quaternion.Euler(0, 0, -(float)(minute % 1) * 360);
            lakeOpenGlow.SetActive(world.Session.LakeSeal == 0 && world.Session.Clock.Day >= 7);
            tabletPulse.intensity = phase == Phase.MidnightHold || phase == Phase.Box ?
                tabletPulseIntensity * (.65f + .35f * Mathf.Sin(elapsed * 16)) : 0;
        }
    }
}
