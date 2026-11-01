using System;
using Demon.Slice.Domain;
using UnityEngine;
namespace Demon.Slice
{
    public sealed class ExplorationFlow : MonoBehaviour
    {
        [SerializeField] ExplorationDefinition definition;
        [SerializeField] StationObservation[] observations;
        [SerializeField] GameObject humanTrail;
        [SerializeField] Transform[] trailWaypoints;
        [SerializeField] NineDayTablet firstTablet;
        [SerializeField] Transform returnPoint;
        [Header("Optional D3–D4 authored extension")]
        [SerializeField] GameObject largeTrail;
        [SerializeField] Transform[] forestRoute;
        [SerializeField] Transform[] ritualRoute;
        [SerializeField] NineDayTablet secondTablet;
        [SerializeField] NineDayTablet thirdTablet;
        [Header("Optional D5–D6 authored extension")]
        [SerializeField] NineDayTablet fourthTablet;
        [SerializeField] NineDayTablet fifthTablet;
        [SerializeField] Transform[] lakeRoute;
        [SerializeField] Transform[] listeningRoute;
        [SerializeField] Transform visibleListeningPoint;
        [SerializeField] Transform hiddenListeningPoint;
        SliceSceneController world;
        public FirstDayInspectionLog Log { get; private set; }
        public ExplorationDefinition Definition => definition;
        public StationObservation[] Observations => (StationObservation[])observations.Clone();
        public Transform[] TrailWaypoints => (Transform[])trailWaypoints.Clone();
        public NineDayTablet FirstTablet => firstTablet;
        public Transform ReturnPoint => returnPoint;
        public bool TrailVisible => humanTrail && humanTrail.activeInHierarchy;
        public bool HasInvestigation => largeTrail;
        public bool LargeTrailVisible => largeTrail && largeTrail.activeInHierarchy;
        public NineDayTablet SecondTablet => secondTablet;
        public NineDayTablet ThirdTablet => thirdTablet;
        public Transform[] ForestRoute => forestRoute == null ? Array.Empty<Transform>() : (Transform[])forestRoute.Clone();
        public Transform[] RitualRoute => ritualRoute == null ? Array.Empty<Transform>() : (Transform[])ritualRoute.Clone();
        public bool HasSignalSearch => fourthTablet;
        public NineDayTablet FourthTablet => fourthTablet;
        public NineDayTablet FifthTablet => fifthTablet;
        public Transform[] LakeRoute => lakeRoute == null ? Array.Empty<Transform>() : (Transform[])lakeRoute.Clone();
        public Transform[] ListeningRoute => listeningRoute == null ? Array.Empty<Transform>() : (Transform[])listeningRoute.Clone();
        public Transform VisibleListeningPoint => visibleListeningPoint;
        public Transform HiddenListeningPoint => hiddenListeningPoint;
        public bool ShowStatus => world && !world.Session.Descended && world.Session.Clock.Day <= (HasSignalSearch ? 6 : HasInvestigation ? 4 : 2);
        public string Status
        {
            get
            {
                int day = world.Session.Clock.Day;
                if (day == 1) return definition.Day1Guide + "\n已记录 " + Log.Count + "/3";
                if (day == 2) return world.Campaign.Progress.Found(0) ? definition.FoundGuide : definition.Day2Guide;
                if (day == 3) return world.Campaign.Progress.Found(1) ? definition.Day3FoundGuide : definition.Day3Guide;
                if (day == 4) return world.Campaign.Progress.Found(2) ? definition.Day4FoundGuide : definition.Day4Guide;
                if (day == 5) return world.Campaign.Progress.Found(3) ? definition.Day5FoundGuide : definition.Day5Guide;
                return world.Listening.Active ? definition.Day6ActiveGuide : world.Campaign.Progress.Found(4) ? definition.Day6FoundGuide : definition.Day6Guide;
            }
        }
        public void ConfigureInvestigation(GameObject oldTracks, Transform[] forest, Transform[] ritual, NineDayTablet second, NineDayTablet third)
        { largeTrail = oldTracks; forestRoute = forest; ritualRoute = ritual; secondTablet = second; thirdTablet = third; }
        public void ConfigureSignalSearch(NineDayTablet fourth, NineDayTablet fifth, Transform[] lake, Transform[] listening, Transform visible, Transform hidden)
        { fourthTablet = fourth; fifthTablet = fifth; lakeRoute = lake; listeningRoute = listening; visibleListeningPoint = visible; hiddenListeningPoint = hidden; }
        public void Configure(ExplorationDefinition data, StationObservation[] points, GameObject trail, Transform[] route, NineDayTablet tablet, Transform home)
        { definition = data; observations = points; humanTrail = trail; trailWaypoints = route; firstTablet = tablet; returnPoint = home; }
        public void Initialize(SliceSceneController controller)
        {
            if (!definition || !humanTrail || observations == null || observations.Length != 3 || trailWaypoints == null || trailWaypoints.Length < 2 || !firstTablet || firstTablet.Index != 0 || !returnPoint || !controller.Campaign || !controller.Consequences || !controller.Tether)
                throw new InvalidOperationException("Exploration requires saved observations, trail/route, original T01 and return point.");
            var sites = new bool[3]; foreach (var point in observations)
            { if (!point || point.Owner != this || !point.CloseView || !point.AimTarget || !point.Approach || (int)point.Site < 0 || (int)point.Site >= sites.Length || sites[(int)point.Site]) throw new InvalidOperationException("Invalid observation composition."); sites[(int)point.Site] = true; }
            foreach (var node in trailWaypoints) if (!node) throw new InvalidOperationException("Missing saved trail waypoint.");
            bool anyExtension = largeTrail || secondTablet || thirdTablet || (forestRoute != null && forestRoute.Length > 0) || (ritualRoute != null && ritualRoute.Length > 0);
            if (anyExtension)
            {
                if (!largeTrail || !secondTablet || secondTablet.Index != 1 || !thirdTablet || thirdTablet.Index != 2 || forestRoute == null || forestRoute.Length < 2 || ritualRoute == null || ritualRoute.Length < 2)
                    throw new InvalidOperationException("D3–D4 exploration requires the complete saved trail/routes and original T02/T03.");
                foreach (var node in forestRoute) if (!node) throw new InvalidOperationException("Missing forest route node.");
                foreach (var node in ritualRoute) if (!node) throw new InvalidOperationException("Missing ritual route node.");
            }
            bool anySignal = fourthTablet || fifthTablet || visibleListeningPoint || hiddenListeningPoint || (lakeRoute != null && lakeRoute.Length > 0) || (listeningRoute != null && listeningRoute.Length > 0);
            if (anySignal)
            {
                if (!HasInvestigation || !controller.Listening || !fourthTablet || fourthTablet.Index != 3 || !fifthTablet || fifthTablet.Index != 4 || !visibleListeningPoint || !hiddenListeningPoint || lakeRoute == null || lakeRoute.Length < 2 || listeningRoute == null || listeningRoute.Length < 2)
                    throw new InvalidOperationException("D5–D6 search requires original T04/T05, complete saved routes and listening points.");
                foreach (var node in lakeRoute) if (!node) throw new InvalidOperationException("Missing lakeside route node.");
                foreach (var node in listeningRoute) if (!node) throw new InvalidOperationException("Missing listening route node.");
            }
            world = controller;
            Log = new FirstDayInspectionLog();
            world.Session.Clock.Midnight += RefreshDate;
            world.Session.Changed += RefreshTrails;
            RefreshTrails();
        }
        void RefreshDate(int day) => RefreshTrails();
        void RefreshTrails()
        {
            humanTrail.SetActive(world.Session.Clock.Day >= world.Campaign.Definition.Tablets[0].DiscoveryDay && !world.Session.Descended);
            // Residual marks are dated evidence, not the moving threat or a second movement controller.
            if (largeTrail) largeTrail.SetActive(world.Tether.MotionReady && !world.Session.Descended);
        }
        public void Observe(StationObservation point)
        {
            if (!world || world.Session.Descended) return;
            var site = point.Site; ObservationText text; string record;
            switch (site)
            {
                case ObservationSite.Radio:
                    text = definition.Radio; record = world.Consequences.Rule.RadioSound == 1 ? text.Normal : text.Changed; break;
                case ObservationSite.WindVane:
                    text = definition.WindVane; record = world.Tether.Rule.Locked ? text.Special : world.Tether.MotionReady ? text.Changed : text.Normal; break;
                default:
                    text = definition.Lake; record = world.Session.LakeSeal == 1 ? text.Normal : world.Consequences.Rule.DemonLight == 1 ? text.Special : text.Changed; break;
            }
            Log.Record(world.Session.Clock.Day, site);
            int minute = (int)world.Session.Clock.MinuteOfDay;
            world.ReadDocument(text.Title + "\n观察时刻：D" + world.Session.Clock.Day + " " + (minute / 60).ToString("00") + ":" + (minute % 60).ToString("00") + "\n\n" + record + "\n\nEsc 放下记录。", point.CloseView);
            world.SetMessage("观察已记录。日常检查为可选内容，可以自由睡到次日。");
        }
        void OnDestroy()
        {
            if (!world || world.Session == null) return;
            world.Session.Clock.Midnight -= RefreshDate;
            world.Session.Changed -= RefreshTrails;
        }
    }
}
