using System;
using UnityEngine;

namespace Demon.Slice
{
    public enum PreparationRecordKind { Solar, Handover }

    // Saved, local interaction; the existing exploration owner supplies dated content and availability.
    public sealed class PreparationRecord : Interactable
    {
        [SerializeField] ExplorationFlow owner;
        [SerializeField] PreparationRecordKind kind;
        [SerializeField] Transform closeView;
        [SerializeField] Transform aimTarget;
        [SerializeField] Transform approach;
        [SerializeField] string prompt;
        public ExplorationFlow Owner => owner;
        public PreparationRecordKind Kind => kind;
        public Transform CloseView => closeView;
        public Transform AimTarget => aimTarget;
        public Transform Approach => approach;
        public override string Prompt => prompt;
        public void Configure(ExplorationFlow flow, PreparationRecordKind id, Transform view, Transform aim, Transform stand, string text)
        { owner = flow; kind = id; closeView = view; aimTarget = aim; approach = stand; prompt = text; }
        public override void Use(PlayerController player)
        {
            if (!owner) throw new InvalidOperationException("Preparation record requires a saved exploration owner.");
            owner.ReadPreparation(this);
        }
    }
}
