using System;
using Demon.Slice.Domain;
using UnityEngine;
using UnityEngine.Events;
namespace Demon.Slice
{
    public sealed class ThreatPickup : SliceInteractable
    {
        [SerializeField] SliceSceneController world;
        [SerializeField] Transform closeView;
        [SerializeField] GameObject itemVisual;
        [SerializeField, Min(.1f)] float holdSeconds = 1.2f;
        [SerializeField] string itemName = "练习碎片";
        [SerializeField] UnityEvent onCollected = new UnityEvent();
        Func<bool> requestBegin;
        Action collected;
        public void BindAttempt(Func<bool> request, Action completed)
        { if (requestBegin != null) throw new InvalidOperationException("Pickup already has an attempt owner."); requestBegin = request; collected = completed; }
        public void UnbindAttempt() { requestBegin = null; collected = null; }
        public PickupAttempt Attempt { get; private set; }
        public Transform CloseView => closeView;
        public float HoldSeconds => holdSeconds;
        public int CollectionCount { get; private set; }
        public override string Prompt => "按住 E 拾取" + itemName;
        public void Configure(SliceSceneController owner, Transform view, GameObject visual)
        { world = owner; closeView = view; itemVisual = visual; }
        void Awake()
        {
            if (!world || !closeView || !itemVisual) throw new InvalidOperationException("Pickup requires saved composition, view and item visual.");
            Attempt = new PickupAttempt(holdSeconds);
        }
        public override void Use(SlicePlayer player)
        {
            if (!isActiveAndEnabled || world.Session.Descended || Attempt.Active || Attempt.Completed ||
                (requestBegin != null && !requestBegin()) || !Attempt.Begin()) return;
            world.SetMessage("按住 E 完成拾取；松开或 Esc 放下。伸手时无法继续照射。");
            player.EnterPickupView(this, closeView);
        }
        public void TickHeld(bool held, float seconds)
        {
            if (!Attempt.Active) return;
            if (Attempt.Advance(seconds, held))
            {
                CollectionCount++; itemVisual.SetActive(false);
                world.SetMessage(itemName + "已取得。"); world.Player.ExitCloseView();
                collected?.Invoke();
                onCollected.Invoke(); // Listeners may disable the item or load another scene.
            }
            else if (!held) { world.SetMessage("已放下，靠近后可以重新拾取。"); world.Player.ExitCloseView(); }
        }
        public bool Cancel(bool interrupted) => Attempt != null && Attempt.Cancel(interrupted);
        void OnDisable()
        {
            if (world && world.Player && world.Player.CurrentPickup == this) world.Player.ExitCloseView();
            else Attempt?.Cancel(false);
        }
    }
}
