using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Demon.Slice.Editor
{
    // Owns only synthetic devices and steering. Production player movement/collision and chase remain untouched.
    public sealed class SmokePlayerDriver : IDisposable
    {
        readonly PlayerController player;
        readonly GameSettings settings;
        readonly double walkTimeout;
        readonly InputSettings.BackgroundBehavior oldBackground;
        readonly InputSettings.EditorInputBehaviorInPlayMode oldInput;
        readonly bool oldRun;
        public Keyboard Keyboard { get; private set; }
        public Mouse Mouse { get; private set; }
        Vector3[] corners;
        Vector3 previous;
        int corner;
        double walkStarted, lastSteer;
        public float Metres { get; private set; }
        public SmokePlayerDriver(PlayerController actor, GameSettings tuning, double timeout)
        {
            player = actor; settings = tuning; walkTimeout = timeout; previous = actor.transform.position;
            oldBackground = InputSystem.settings.backgroundBehavior;
            oldInput = InputSystem.settings.editorInputBehaviorInPlayMode; oldRun = Application.runInBackground;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            Application.runInBackground = true;
            Keyboard = InputSystem.AddDevice<Keyboard>(); Mouse = InputSystem.AddDevice<Mouse>();
            InputSystem.onAfterUpdate += MakeCurrent; MakeCurrent(); Cursor.lockState = CursorLockMode.Locked;
        }
        void MakeCurrent() { Keyboard?.MakeCurrent(); Mouse?.MakeCurrent(); }
        public void Keys(params Key[] keys) => InputSystem.QueueStateEvent(Keyboard, new KeyboardState(keys));
        public void Aim(Vector3 point, bool right = false)
        {
            var camera = player.ViewCamera.transform; var desired = Quaternion.LookRotation(point - camera.position).eulerAngles;
            float yaw = Mathf.DeltaAngle(player.transform.eulerAngles.y, desired.y);
            float pitch = Mathf.DeltaAngle(0, camera.localEulerAngles.x), targetPitch = Mathf.DeltaAngle(0, desired.x);
            InputSystem.QueueStateEvent(Mouse, new MouseState { delta = new Vector2(yaw / settings.MouseSensitivity, (pitch - targetPitch) / settings.MouseSensitivity), buttons = (ushort)(right ? 2 : 0) });
        }
        public void WalkTo(Vector3 target)
        {
            var path = new NavMeshPath();
            if (!NavMesh.SamplePosition(player.transform.position, out var from, 1.5f, NavMesh.AllAreas) ||
                !NavMesh.SamplePosition(target, out var to, 1.5f, NavMesh.AllAreas) ||
                !NavMesh.CalculatePath(from.position, to.position, NavMesh.AllAreas, path) || path.status != NavMeshPathStatus.PathComplete)
                throw new InvalidOperationException("No complete fixture walking path to " + target);
            corners = path.corners; corner = corners.Length > 1 ? 1 : 0;
            lastSteer = -100; walkStarted = Time.timeAsDouble; Keys();
        }
        public bool Walk(double now)
        {
            Vector3 position = player.transform.position;
            if (now - walkStarted > walkTimeout) throw new InvalidOperationException("Walking blocked at " + position + ", corner=" + corners[corner]);
            Metres += Vector3.Distance(position, previous); previous = position;
            Vector3 delta = corners[corner] - position; delta.y = 0;
            if (delta.magnitude < .2f)
            {
                if (++corner >= corners.Length) { Keys(); return true; }
                lastSteer = -100; delta = corners[corner] - position; delta.y = 0;
            }
            if (now - lastSteer > .15) { Aim(position + delta + Vector3.up * player.ViewCamera.transform.localPosition.y); lastSteer = now; }
            Keys(Key.W); return false;
        }
        public void Dispose()
        {
            if (Keyboard == null) return;
            InputSystem.onAfterUpdate -= MakeCurrent;
            InputSystem.RemoveDevice(Keyboard); InputSystem.RemoveDevice(Mouse);
            InputSystem.settings.backgroundBehavior = oldBackground;
            InputSystem.settings.editorInputBehaviorInPlayMode = oldInput; Application.runInBackground = oldRun;
            Keyboard = null; Mouse = null;
        }
    }
}
