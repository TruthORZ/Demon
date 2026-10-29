using System;
using Demon.Slice.Domain;
using UnityEngine;

namespace Demon.Slice
{
    public sealed class SliceSceneController : MonoBehaviour
    {
        [SerializeField] SliceSettings settings;
        [SerializeField] SlicePlayer player;
        [SerializeField] SliceTablet tablet;
        [SerializeField] SlicePresentation presentation;
        [SerializeField] SliceHud hud;
        [SerializeField] Transform endingView;
        [SerializeField] NineDayFlow campaign;
        [SerializeField] SliceFinaleSequence finale;
        [SerializeField] ListeningTool listeningTool;
        [SerializeField] CipherBox cipherBox;
        [SerializeField] BeamTether tether;
        [SerializeField] ChaseController chase;
        [SerializeField] FinalTabletRetrieval retrieval;
        [SerializeField] WorldAttributeConsequences consequences;
        [SerializeField] ExplorationFlow exploration;
        public ExplorationFlow Exploration => exploration;
        public void ConfigureExploration(ExplorationFlow flow) => exploration = flow;
        public WorldAttributeConsequences Consequences => consequences;
        public SlicePresentation Presentation => presentation;
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
        public SliceFinaleSequence Finale => finale;
        public string ReadingText { get; private set; }
        public void ConfigureCampaign(NineDayFlow flow) => campaign = flow;
        public void ConfigureFinale(SliceFinaleSequence sequence) => finale = sequence;
        bool initialized;
        bool readingBox;
        public SliceSession Session { get; private set; }
        public SliceSettings Settings => settings;
        public SlicePlayer Player => player;
        public SliceTablet Tablet => tablet;
        public bool EndingPresented { get; private set; }
        public bool HasReadBox { get; private set; }
        public string Message { get; private set; } = "找到石板，补全铭文。箱子目前打不开。";

        public void Configure(SliceSettings data, SlicePlayer actor, SliceTablet stone,
            SlicePresentation visuals, SliceHud overlay, Transform endingAnchor)
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
            var board = new RuneBoardModel(settings.Initial, settings.Target, settings.LooseRunes);
            Session = new SliceSession(settings.StartDay, settings.StartHour * 60, settings.FinalDay, board,
                settings.InitialLakeSealed, campaign ? campaign.Definition.FinalTabletDay : 1);
            player.Initialize(this);
            tablet.Initialize(this);
            presentation.Initialize(this);
            hud.Initialize(this);
            Session.Changed += OnStateChanged;
            initialized = true;
            if (campaign) campaign.Initialize(this);
            if (listeningTool) listeningTool.Initialize(this);
            if (cipherBox) cipherBox.Initialize(this);
            if (tether) tether.Initialize(this);
            if (consequences) consequences.Initialize(this);
            if (chase) chase.Initialize(this);
            if (finale) finale.Initialize(this, presentation);
            if (retrieval) retrieval.Initialize(this);
            if (exploration) exploration.Initialize(this);
            OnStateChanged();
        }

        void Update()
        {
            if (!initialized) return;
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
            { SetMessage("最后一块石板会在第八天夜间出现。"); return false; }
            if (retrieval && !retrieval.ReadyToReconstruct)
            { SetMessage("先取得T07，并带回工具棚放置。"); return false; }
            if (finale) finale.ReserveTime();
            return true;
        }

        public bool TryCompile()
        {
            if (Session == null || Session.Descended) return false;
            if (campaign && !campaign.FinalAvailable)
            { SetMessage("最后一块石板尚未出现。"); return false; }
            if (!BeginFinalInteraction()) return false;
            bool success = Session.TryCompile();
            if (success)
            {
                SetMessage("铭文已固化。箱子解封了，湖也关闭了。去看看箱子。" );
                player.ExitCloseView();
            }
            else
            {
                SetMessage(Session.Board.Frozen ? "程序已经固化，取下表面符文也不会中断。" :
                    "铭文尚未对应这块石板的目标。可以继续调整，尚未固化。" );
            }
            return success;
        }

        public void ReadBox()
        {
            if (Session.Descended) return;
            if (Session.BoxSeal != 0)
            {
                SetMessage("箱子被封闭，暂时打不开。" );
                return;
            }
            HasReadBox = true;
            readingBox = true;
            ReadingText = settings.BoxText;
            presentation.OpenBox();
            player.EnterReadingView(presentation.BoxReadView);
            if (finale) finale.RecordOpened();
            SetMessage("箱内有一份记录。" );
        }

        public bool OnReadingClosed()
        {
            bool wasBox = readingBox;
            readingBox = false;
            return wasBox && finale && finale.FinishRecord();
        }

        public void Sleep()
        {
            if (Session.Descended || (finale && finale.IsDirecting)) return;
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
                Message = "第九天，恶魔降临。";
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
