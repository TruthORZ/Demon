using System;
using Demon.Slice.Domain;
using UnityEngine;
namespace Demon.Slice
{
    public sealed partial class CipherBox : Interactable, ICloseViewInteraction
    {
        [SerializeField] CipherWheel wheel;
        [SerializeField] Transform closeView;
        [SerializeField] Transform recordView;
        [SerializeField] CipherDigit[] digits;
        [SerializeField] Transform lidPivot;
        [SerializeField] Vector3 openLidEuler = new Vector3(105, 0, 0);
        [SerializeField, Min(.05f)] float lidBlendSeconds = .65f;
        [SerializeField] GameObject tabletVisual;
        GameSceneController world;
        ThreeDigitLock model;
        Quaternion restingLid;
        bool initialized;
        public GameSceneController World => world;
        public CipherWheel Wheel => wheel;
        public ThreeDigitLock Model => model;
        public Transform CloseView => closeView;
        public bool Acquired => model != null && model.Acquired;
        public override string Prompt => model == null || !model.Unlocked ? "查看箱子 A 密码锁" : Acquired ? "查看已取得的 T06" : "取出箱内 T06";
        public void Configure(CipherWheel compass, Transform view, Transform reading, CipherDigit[] buttons, Transform lid, GameObject tablet)
        { wheel = compass; closeView = view; recordView = reading; digits = buttons; lidPivot = lid; tabletVisual = tablet; }
        public void Initialize(GameSceneController controller)
        {
            if (!wheel || !closeView || !recordView || digits == null || digits.Length != 3 || !lidPivot || !tabletVisual)
                throw new InvalidOperationException("Box A requires its saved wheel, three digits, views, lid and tablet.");
            if (!initialized) restingLid = lidPivot.localRotation;
            initialized = true; lidPivot.localRotation = restingLid; tabletVisual.SetActive(true);
            world = controller; wheel.Initialize(world); model = new ThreeDigitLock(wheel.ReadCode());
            foreach (var digit in digits) if (!digit || !digit.Label) throw new InvalidOperationException("Missing saved lock digit.");
            RefreshDigits();
        }
        public override void Use(PlayerController player)
        {
            if (!world || world.Session.Descended) return;
            if (world.Playthrough) world.Playthrough.Record("box-a");
            if (model.Unlocked)
            {
                model.TryAcquire(); tabletVisual.SetActive(false);
                world.Campaign.AcquireExchangeTablet(recordView);
                world.SetMessage(world.Tether ? (world.Playthrough ? "T06已取得。点击左键打开手电筒，稳定照住雪痕。" : "T06已取得。回到湖岸观察点，按住右键照住雪痕。") : "T06已取得。");
                if (world.Playthrough && !world.Playthrough.HasFlashlight) world.SetMessage("T06已取得。回木屋桌边拾取手电筒后即可使用光束牵制。");
            }
            else { world.SetMessage(wheel.Definition.LockInstructions); player.EnterLockView(this, closeView); }
        }
        public void Click(Collider target)
        {
            var digit = target.GetComponentInParent<CipherDigit>();
            if (digit && Array.IndexOf(digits, digit) >= 0) { model.Cycle(digit.Index); RefreshDigits(); }
        }
        void RefreshDigits() { foreach (var digit in digits) digit.Label.text = model.At(digit.Index).ToString(); }
        bool ICloseViewInteraction.Confirm() => TryConfirm();
        public bool TryConfirm()
        {
            if (!world || world.Session.Descended) return false;
            if (!model.TryUnlock()) { world.SetMessage(model.Unlocked ? wheel.Definition.UnlockedText : "密码不符。观察轮盘和三个物体的位置，可以继续尝试。"); return false; }
            world.SetMessage(wheel.Definition.UnlockedText); world.Player.ExitCloseView(); return true;
        }
        void Update()
        {
            if (model == null) return;
            Quaternion target = restingLid * Quaternion.Euler(model.Unlocked ? openLidEuler : Vector3.zero);
            lidPivot.localRotation = Quaternion.Slerp(lidPivot.localRotation, target, 1 - Mathf.Exp(-Time.deltaTime * 5 / lidBlendSeconds));
        }
    }
}
