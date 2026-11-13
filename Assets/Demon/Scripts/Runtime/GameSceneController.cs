using System;
using Demon.Slice.Domain;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Demon.Slice
{
    [MovedFrom(false, sourceClassName: "SliceSceneController")]
    public sealed class GameSceneController : MonoBehaviour, IPlayerSession, IHudState, ISceneFeedback, ITabletProgram
    {
        [SerializeField] GameSettings settings;
        [SerializeField] PlayerController player;
        [SerializeField] RuneTablet tablet;
        [SerializeField] WorldPresentation presentation;
        [SerializeField] GameHud hud;
        [SerializeField] Transform endingView;
        [SerializeField] NineDayFlow campaign;
        [SerializeField] FinaleSequence finale;
        [SerializeField] ListeningTool listeningTool;
        [SerializeField] CipherBox cipherBox;
        [SerializeField] BeamTether tether;
        [SerializeField] ChaseController chase;
        [SerializeField] FinalTabletRetrieval retrieval;
        [SerializeField] WorldAttributeConsequences consequences;
        [SerializeField] ExplorationFlow exploration;
        [SerializeField] CompletePlaythrough playthrough;
        public CompletePlaythrough Playthrough => playthrough;
        public void ConfigurePlaythrough(CompletePlaythrough flow) => playthrough = flow;
        public ExplorationFlow Exploration => exploration;
        public void ConfigureExploration(ExplorationFlow flow) => exploration = flow;
        public WorldAttributeConsequences Consequences => consequences;
        public WorldPresentation Presentation => presentation;
        public void ConfigureConsequences(WorldAttributeConsequences effects) => consequences = effects;
        public FinalTabletRetrieval Retrieval => retrieval;
        public void ConfigureRetrieval(FinalTabletRetrieval owner) => retrieval = owner;
        public ChaseController Chase => chase;
        public void ConfigureChase(ChaseController controller) => chase = controller;
        public BeamTether Tether => tether;
        public void ConfigureTether(BeamTether beam) => tether = beam;
        public CipherBox Cipher => cipherBox;
        public void ConfigureCipher(CipherBox box) => cipherBox = box;
        public ListeningTool Listening => listeningTool;
        public void ConfigureListening(ListeningTool tool) => listeningTool = tool;
        public NineDayFlow Campaign => campaign;
        public FinaleSequence Finale => finale;
        public string ReadingText { get; private set; }
        public void ConfigureCampaign(NineDayFlow flow) => campaign = flow;
        public void ConfigureFinale(FinaleSequence sequence) => finale = sequence;
        bool initialized;
        bool readingBox;
        public GameSession Session { get; private set; }
        RuneBoardModel ITabletProgram.Board => Session.Board;
        bool ITabletProgram.BeginInteraction() => BeginFinalInteraction();
        public GameSettings Settings => settings;
        public PlayerController Player => player;
        public RuneTablet Tablet => tablet;
        public bool EndingPresented { get; private set; }
        public bool HasReadBox { get; private set; }
        public string Message { get; private set; } = "Find the tablet and restore its inscription.";

        public void Configure(GameSettings data, PlayerController actor, RuneTablet stone,
            WorldPresentation visuals, GameHud overlay, Transform endingAnchor)
        {
            settings = data; player = actor; tablet = stone;
            presentation = visuals; hud = overlay; endingView = endingAnchor;
        }

        void Awake()
        {
            if (!settings || !player || !tablet || !presentation || !hud || !endingView)
            {
                Debug.LogError("Demon slice has missing authored scene references.", this);
                enabled = false;
                return;
            }
            InitializeSession();
        }

        public void Restart()
        {
            if (!initialized) throw new InvalidOperationException("Cannot restart an uninitialized scene.");
            InitializeSession();
            if (playthrough) playthrough.CaptureDayStart();
        }

        internal void RestoreSession(GameSession.Snapshot snapshot) => InitializeSession(snapshot);

        void InitializeSession(GameSession.Snapshot restored = null)
        {
            initialized = false;
            if (Session != null) { Session.Changed -= OnStateChanged; Session.Dispose(); }
            readingBox = false; ReadingText = null; HasReadBox = false; EndingPresented = false;
            var board = new RuneBoardModel(settings.Initial, settings.Target, settings.LooseRunes);
            Session = restored != null ? GameSession.FromSnapshot(restored) : new GameSession(settings.StartDay, settings.StartHour * 60, settings.FinalDay, board,
                settings.InitialLakeSealed, campaign ? campaign.Definition.FinalTabletDay : 1);
            player.Initialize(this);
            tablet.Initialize(this);
            presentation.Initialize(this);
            hud.Initialize(this, Session, settings, player, campaign, finale, tether, retrieval, exploration);
            Session.Changed += OnStateChanged;
            initialized = true;
            if (campaign) campaign.Initialize(this);
            if (listeningTool) listeningTool.Initialize(Session, campaign, player, this, chase && chase.Agent ? chase.Agent.transform : null);
            if (cipherBox) cipherBox.Initialize(this);
            if (tether) tether.Initialize(Session, player, cipherBox, listeningTool, campaign ? campaign.Definition.MotionDay : tether.Definition.DemonMotionDay);
            if (consequences) consequences.Initialize(this);
            if (chase) chase.Initialize(Session, player, tether, this);
            if (finale) finale.Initialize(this, presentation);
            if (retrieval) retrieval.Initialize(this);
            if (exploration) exploration.Initialize(Session, campaign, consequences, tether, cipherBox, listeningTool, retrieval, this);
            if (playthrough) playthrough.Initialize(this);
            OnStateChanged();
        }

        void Update()
        {
            if (!initialized || (playthrough && playthrough.BlocksClock)) return;
            float dt = Time.deltaTime;
            if (finale && finale.Tick(dt)) return;
            if (Session.Descended) return;
            double target = Session.Clock.AbsoluteMinutes + dt * GameClock.MinutesPerDay / settings.RealSecondsPerDay;
            if (finale) target = finale.LimitNaturalTime(target);
            Session.Clock.AdvanceTo(target);
        }

        public bool BeginFinalInteraction()
        {
            if (Session == null || Session.Descended) return false;
            if (campaign && !campaign.FinalAvailable)
            { SetMessage("Search the shore on the eighth night."); return false; }
            if (retrieval && !retrieval.ReadyToReconstruct)
            { SetMessage("Bring Tablet VII back to its stand first."); return false; }
            if (finale) finale.ReserveTime();
            return true;
        }

        public bool TryCompile()
        {
            if (Session == null || Session.Descended) return false;
            if (campaign && !campaign.FinalAvailable)
            { SetMessage("The last tablet has yet to surface."); return false; }
            if (!BeginFinalInteraction()) return false;
            bool success = Session.TryCompile();
            if (success)
            {
                SetMessage("A click from the cabin. The latch has sprung." );
                player.ExitCloseView();
            }
            else
            {
                SetMessage(Session.Board.Frozen ? "The lines have fused. Taking the stones out will not undo it." :
                    "The lines fade. Try another order." );
            }
            return success;
        }

        public void ReadBox()
        {
            if (Session.Descended) return;
            if (Session.BoxSeal != 0)
            {
                SetMessage("The lid will not budge." );
                return;
            }
            HasReadBox = true;
            readingBox = true;
            ReadingText = settings.BoxText;
            presentation.OpenBox();
            player.EnterReadingView(presentation.BoxReadView);
            if (finale) finale.RecordOpened();
            SetMessage("There is a letter inside." );
        }

        public bool OnReadingClosed()
        {
            bool wasBox = readingBox;
            if (wasBox && playthrough)
            {
                playthrough.RememberBoxRecord();
                playthrough.Record("box-record");
            }
            readingBox = false;
            return wasBox && finale && finale.FinishRecord();
        }

        public void Sleep()
        {
            if (Session.Descended || (finale && finale.IsDirecting)) return;
            if (playthrough && !playthrough.RequestSleep()) return;
            if (finale) finale.ReleaseForSleep();
            Session.SleepToNextDay((campaign ? campaign.WakeHourFor(Session.Clock.Day + 1) : settings.WakeHour) * 60);
        }

        public void ReadDocument(string text, Transform anchor)
        {
            if (Session.Descended) return;
            readingBox = false;
            ReadingText = text;
            player.EnterReadingView(anchor);
        }

        public void SetMessage(string text) => Message = text;

        void OnStateChanged()
        {
            if (Session.Descended)
            {
                Message = "On the ninth day it came ashore.";
                if (!finale || !finale.IsDirecting) PresentEnding();
            }
        }

        public void PresentEnding()
        {
            EndingPresented = true;
            player.EnterEndingView(endingView);
        }

        void OnDestroy()
        {
            if (Session == null) return;
            Session.Changed -= OnStateChanged;
            Session.Dispose();
        }
    }
}
