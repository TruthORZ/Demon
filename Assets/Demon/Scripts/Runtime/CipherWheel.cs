using System;
using Demon.Slice.Domain;
using UnityEngine;
namespace Demon.Slice
{
    public sealed class CipherWheel : Interactable
    {
        [SerializeField] CipherDefinition definition;
        [Tooltip("Order: radio, fixed sun reference, wind-vane root position (not the rotating blade).")]
        [SerializeField] Transform[] landmarks;
        [SerializeField] Transform closeView;
        [SerializeField] TextMesh[] digitLabels;
        GameSceneController world;
        public CipherDefinition Definition => definition;
        public Transform CloseView => closeView;
        public Transform[] Landmarks => (Transform[])landmarks.Clone();
        public override string Prompt => "Examine the bearing dial";
        public void Configure(CipherDefinition data, Transform[] objects, Transform view, TextMesh[] labels)
        { definition = data; landmarks = objects; closeView = view; digitLabels = labels; }
        public void Initialize(GameSceneController controller)
        {
            if (!definition || landmarks == null || landmarks.Length != 3 || !closeView || digitLabels == null || digitLabels.Length != 10)
                throw new InvalidOperationException("Cipher wheel requires saved definition, three fixed landmarks, ten labels and view.");
            BearingCipher.ValidateDigits(definition.DigitsClockwise); world = controller;
            for (int i = 0; i < digitLabels.Length; i++)
            {
                if (!digitLabels[i]) throw new InvalidOperationException("Missing wheel digit label.");
                digitLabels[i].text = definition.DigitsClockwise[i].ToString();
            }
            ReadCode();
        }
        public int[] ReadCode()
        {
            BearingCipher.ValidateDigits(definition.DigitsClockwise);
            int[] code = new int[3];
            for (int i = 0; i < 3; i++)
            {
                if (!landmarks[i]) throw new InvalidOperationException("Missing explicit fixed landmark.");
                Vector3 offset = transform.InverseTransformDirection(landmarks[i].position - transform.position);
                code[i] = definition.DigitsClockwise[BearingCipher.Sector(offset.x, offset.z)];
            }
            return code;
        }
        public override void Use(PlayerController player)
        {
            if (!world || world.Session.Descended) return;
            world.SetMessage(definition.WheelInstructions); player.EnterInspectionView(closeView);
        }
    }
}
