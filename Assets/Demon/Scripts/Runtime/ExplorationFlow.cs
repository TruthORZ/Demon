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
        [Header("Optional D7–D8 authored extension")]
        [SerializeField] PreparationRecord solarRecord;
        [SerializeField] PreparationRecord handoverRecord;
        [SerializeField] Transform practicePoint;
        GameSession session;
        NineDayFlow campaign;
        WorldAttributeConsequences consequences;
        BeamTether tether;
        CipherBox cipher;
        ListeningTool listening;
        FinalTabletRetrieval retrieval;
        ISceneFeedback feedback;
        bool solarRead, handoverRead, sawBeamLock, sawBeamRelease;
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
        public bool HasFinalPreparation => handoverRecord;
        public PreparationRecord SolarRecord => solarRecord;
        public PreparationRecord HandoverRecord => handoverRecord;
        public Transform PracticePoint => practicePoint;
        public bool SolarRead => solarRead;
        public bool HandoverRead => handoverRead;
        public bool PracticedBeam => sawBeamRelease;
        public bool ShowStatus => session != null && !session.Descended && session.Clock.Day <= (HasFinalPreparation ? 8 : HasSignalSearch ? 6 : HasInvestigation ? 4 : 2)
            && !(HasFinalPreparation && session.Clock.Day == 8 && retrieval.Available);
        public string Status => definition.GuideText(ExplorationGuidance.Select(definition.GuideRules, new ExplorationGuideState
        {
            Day = session.Clock.Day, HasPreparation = HasFinalPreparation,
            FirstFound = campaign.Progress.Found(0), SecondFound = campaign.Progress.Found(1),
            ThirdFound = campaign.Progress.Found(2), FourthFound = campaign.Progress.Found(3), FifthFound = campaign.Progress.Found(4),
            ListeningActive = listening && listening.Active, SolarRead = solarRead, HandoverRead = handoverRead,
            ToolAcquired = cipher && cipher.Acquired, BeamLocked = tether.Rule.Locked, BeamReleased = sawBeamRelease
        }), Log.Count, campaign.Definition.FinalTabletHour);
        public void ConfigureInvestigation(GameObject oldTracks, Transform[] forest, Transform[] ritual, NineDayTablet second, NineDayTablet third)
        { largeTrail = oldTracks; forestRoute = forest; ritualRoute = ritual; secondTablet = second; thirdTablet = third; }
        public void ConfigureSignalSearch(NineDayTablet fourth, NineDayTablet fifth, Transform[] lake, Transform[] listening, Transform visible, Transform hidden)
        { fourthTablet = fourth; fifthTablet = fifth; lakeRoute = lake; listeningRoute = listening; visibleListeningPoint = visible; hiddenListeningPoint = hidden; }
        public void ConfigureFinalPreparation(PreparationRecord solar, PreparationRecord handover, Transform practice)
        { solarRecord = solar; handoverRecord = handover; practicePoint = practice; }
        public void Configure(ExplorationDefinition data, StationObservation[] points, GameObject trail, Transform[] route, NineDayTablet tablet, Transform home)
        { definition = data; observations = points; humanTrail = trail; trailWaypoints = route; firstTablet = tablet; returnPoint = home; }
        public void Initialize(GameSession rules, NineDayFlow schedule, WorldAttributeConsequences effects, BeamTether motion, CipherBox tool, ListeningTool listener, FinalTabletRetrieval item, ISceneFeedback messages)
        {
            if (!definition || !humanTrail || observations == null || observations.Length != 3 || trailWaypoints == null || trailWaypoints.Length < 2 || !firstTablet || firstTablet.Index != 0 || !returnPoint || !schedule || !effects || !motion || rules == null || messages == null)
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
                if (!HasInvestigation || !listener || !fourthTablet || fourthTablet.Index != 3 || !fifthTablet || fifthTablet.Index != 4 || !visibleListeningPoint || !hiddenListeningPoint || lakeRoute == null || lakeRoute.Length < 2 || listeningRoute == null || listeningRoute.Length < 2)
                    throw new InvalidOperationException("D5–D6 search requires original T04/T05, complete saved routes and listening points.");
                foreach (var node in lakeRoute) if (!node) throw new InvalidOperationException("Missing lakeside route node.");
                foreach (var node in listeningRoute) if (!node) throw new InvalidOperationException("Missing listening route node.");
            }
            if (solarRecord || handoverRecord || practicePoint)
            {
                if (!HasSignalSearch || !tool || !item || !solarRecord || !handoverRecord || !practicePoint ||
                    solarRecord.Kind != PreparationRecordKind.Solar || handoverRecord.Kind != PreparationRecordKind.Handover)
                    throw new InvalidOperationException("D7–D8 requires complete saved solar/handover records, practice point and existing tools/retrieval.");
                foreach (var record in new[] { solarRecord, handoverRecord })
                    if (record.Owner != this || !record.CloseView || !record.AimTarget || !record.Approach)
                        throw new InvalidOperationException("Preparation record requires explicit owner, close view, aim and approach.");
            }
            if (session != null) { session.Clock.Midnight -= RefreshDate; session.Changed -= RefreshTrails; }
            solarRead = handoverRead = sawBeamLock = sawBeamRelease = false;
            session = rules; campaign = schedule; consequences = effects; tether = motion; cipher = tool; listening = listener; retrieval = item; feedback = messages;
            ExplorationGuidance.Validate(definition.GuideRules);
            Log = new FirstDayInspectionLog();
            session.Clock.Midnight += RefreshDate;
            session.Changed += RefreshTrails;
            RefreshTrails();
        }
        void RefreshDate(int day) => RefreshTrails();
        void RefreshTrails()
        {
            humanTrail.SetActive(session.Clock.Day >= campaign.Definition.Tablets[0].DiscoveryDay && !session.Descended);
            // Residual marks are dated evidence, not the moving threat or a second movement controller.
            if (largeTrail) largeTrail.SetActive(tether.MotionReady && !session.Descended);
            if (HasFinalPreparation)
            {
                solarRecord.gameObject.SetActive(!session.Descended && session.Clock.Day >= campaign.Definition.Tablets[2].DateNodeDay);
                handoverRecord.gameObject.SetActive(!session.Descended && session.Clock.Day >= 8);
            }
        }
        void Update() => RefreshPracticeProgress();
        public void RefreshPracticeProgress()
        {
            if (session == null || !HasFinalPreparation || session.Descended || session.Clock.Day < 7) return;
            // This is a presentation memory, never a prerequisite for tools, sleep or retrieval.
            if (tether.Rule.Locked) sawBeamLock = true;
            else if (sawBeamLock) sawBeamRelease = true;
        }
        public void ReadPreparation(PreparationRecord record)
        {
            if (session == null || !HasFinalPreparation || session.Descended) return;
            bool solar = record == solarRecord;
            if ((!solar && record != handoverRecord) || session.Clock.Day < (solar ? campaign.Definition.Tablets[2].DateNodeDay : 8)) return;
            if (solar) solarRead = true; else handoverRead = true;
            int minute = (int)session.Clock.MinuteOfDay;
            feedback.ReadDocument((solar ? definition.SolarRecord : definition.HandoverRecord) + "\n\n记录时刻：D" + session.Clock.Day + " " +
                (minute / 60).ToString("00") + ":" + (minute % 60).ToString("00") + "\nEsc 放下记录。", record.CloseView);
        }
        public void Observe(StationObservation point)
        {
            if (session == null || session.Descended) return;
            var site = point.Site; ObservationText text; string record;
            switch (site)
            {
                case ObservationSite.Radio:
                    text = definition.Radio; record = consequences.Rule.RadioSound == 1 ? text.Normal : text.Changed; break;
                case ObservationSite.WindVane:
                    text = definition.WindVane; record = tether.Rule.Locked ? text.Special : tether.MotionReady ? text.Changed : text.Normal; break;
                default:
                    text = definition.Lake; record = session.LakeSeal == 1 ? text.Normal : consequences.Rule.DemonLight == 1 ? text.Special : text.Changed; break;
            }
            Log.Record(session.Clock.Day, site);
            int minute = (int)session.Clock.MinuteOfDay;
            feedback.ReadDocument(text.Title + "\n观察时刻：D" + session.Clock.Day + " " + (minute / 60).ToString("00") + ":" + (minute % 60).ToString("00") + "\n\n" + record + "\n\nEsc 放下记录。", point.CloseView);
            feedback.SetMessage("观察已记录。日常检查为可选内容，可以自由睡到次日。");
        }
        void OnDestroy()
        {
            if (session == null || session == null) return;
            session.Clock.Midnight -= RefreshDate;
            session.Changed -= RefreshTrails;
        }
    }
}
