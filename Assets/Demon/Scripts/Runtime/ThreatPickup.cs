using System;
using Demon.Slice.Domain;
using UnityEngine;
using UnityEngine.Events;
namespace Demon.Slice
{
    public sealed class ThreatPickup : Interactable
    {
        [SerializeField] GameSceneController world;
        [SerializeField] Transform closeView;
        [SerializeField] GameObject itemVisual;
        [SerializeField, Min(.1f)] float holdSeconds = 1.2f;
        [SerializeField] string itemName = "Practice shard";
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
        public override string Prompt => "Hold E to lift" + itemName;
        public void Configure(GameSceneController owner, Transform view, GameObject visual)
        { world = owner; closeView = view; itemVisual = visual; }
        void Awake()
        {
            if (!world || !closeView || !itemVisual) throw new InvalidOperationException("Pickup requires saved composition, view and item visual.");
            ResetAttempt();
        }
        public void ResetAttempt()
        { Attempt?.Cancel(false); Attempt = new PickupAttempt(holdSeconds); CollectionCount = 0; itemVisual.SetActive(true); }
        public override void Use(PlayerController player)
        {
            if (!isActiveAndEnabled || world.Session.Descended || Attempt.Active || Attempt.Completed ||
                (requestBegin != null && !requestBegin()) || !Attempt.Begin()) return;
            world.SetMessage("Hold E to lift. Release E or press Esc to set it down. You must lower the torch to reach it.");
            player.EnterPickupView(this, closeView);
        }
        public void TickHeld(bool held, float seconds)
        {
            if (!Attempt.Active) return;
            if (Attempt.Advance(seconds, held))
            {
                CollectionCount++; itemVisual.SetActive(false);
                world.SetMessage(itemName + " collected."); world.Player.ExitCloseView();
                collected?.Invoke();
                onCollected.Invoke(); // Listeners may disable the item or load another scene.
            }
            else if (!held) { world.SetMessage(" set down. You can try again."); world.Player.ExitCloseView(); }
        }
        public bool Cancel(bool interrupted) => Attempt != null && Attempt.Cancel(interrupted);
        void OnDisable()
        {
            if (world && world.Player && world.Player.CurrentPickup == this) world.Player.ExitCloseView();
            else Attempt?.Cancel(false);
        }
    }
}
