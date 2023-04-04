using UnityEngine;
using System.Collections.Generic;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component adds basic Pitch/Yaw controls to the current GameObject (e.g. camera) using mouse or touch controls.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtDragPitchYaw")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Drag Pitch Yaw")]
	public class SgtDragPitchYaw : MonoBehaviour
	{
		/// <summary>The key that must be held for this component to activate on desktop platforms.
		/// None = Any mouse button.</summary>
		public KeyCode Key { set { key = value; } get { return key; } } [SerializeField] private KeyCode key = KeyCode.Mouse0;

		/// <summary>Fingers that began touching the screen on top of these UI layers will be ignored.</summary>
		public LayerMask GuiLayers { set { guiLayers = value; } get { return guiLayers; } } [SerializeField] private LayerMask guiLayers = 1 << 5;

		/// <summary>The target pitch angle in degrees.</summary>
		public float Pitch { set { pitch = value; } get { return pitch; } } [SerializeField] private float pitch;

		/// <summary>The speed the pitch changed relative to the mouse/finger drag distance.</summary>
		public float PitchSensitivity { set { pitchSensitivity = value; } get { return pitchSensitivity; } } [SerializeField] private float pitchSensitivity = 0.1f;

		/// <summary>The minimum value of the pitch value.</summary>
		public float PitchMin { set { pitchMin = value; } get { return pitchMin; } } [SerializeField] private float pitchMin = -90.0f;

		/// <summary>The maximum value of the pitch value.</summary>
		public float PitchMax { set { pitchMax = value; } get { return pitchMax; } } [SerializeField] private float pitchMax = 90.0f;

		/// <summary>The target yaw angle in degrees.</summary>
		public float Yaw { set { yaw = value; } get { return yaw; } } [SerializeField] private float yaw;

		/// <summary>The speed the yaw changed relative to the mouse/finger drag distance.</summary>
		public float YawSensitivity { set { yawSensitivity = value; } get { return yawSensitivity; } } [SerializeField] private float yawSensitivity = 0.1f;

		/// <summary>Clamp the yaw value?</summary>
		public bool YawClamp { set { yawClamp = value; } get { return yawClamp; } } [SerializeField] private bool yawClamp;

		/// <summary>The minimum value of the pitch value.</summary>
		public float YawMin { set { yawMin = value; } get { return yawMin; } } [SerializeField] private float yawMin = -90.0f;

		/// <summary>The maximum value of the pitch value.</summary>
		public float YawMax { set { yawMax = value; } get { return yawMax; } } [SerializeField] private float yawMax = 90.0f;

		/// <summary>How quickly the rotation transitions from the current to the target value (-1 = instant).</summary>
		public float Damping { set { damping = value; } get { return damping; } } [SerializeField] private float damping = 10.0f;

		[SerializeField]
		private float currentPitch;

		[SerializeField]
		private float currentYaw;

		[System.NonSerialized]
		private List<SgtInputManager.Finger> fingers = new List<SgtInputManager.Finger>();

		protected virtual void OnEnable()
		{
			SgtInputManager.EnsureThisComponentExists();

			SgtInputManager.OnFingerDown += HandleFingerDown;
			SgtInputManager.OnFingerUp   += HandleFingerUp;
		}

		protected virtual void OnDisable()
		{
			SgtInputManager.OnFingerDown -= HandleFingerDown;
			SgtInputManager.OnFingerUp   -= HandleFingerUp;
		}

		private void HandleFingerDown(SgtInputManager.Finger finger)
		{
			if (finger.Index == SgtInputManager.HOVER_FINGER_INDEX) return;

			if (SgtInputManager.PointOverGui(finger.ScreenPosition, guiLayers) == true) return;

			if (key != KeyCode.None && SgtInputManager.IsDown(key) == false) return;

			fingers.Add(finger);
		}

		private void HandleFingerUp(SgtInputManager.Finger finger)
		{
			fingers.Remove(finger);
		}

		protected virtual void Update()
		{
			// Calculate delta
			if (CanRotate == true && Application.isPlaying == true)
			{
				var delta = SgtInputManager.GetAverageDeltaScaled(fingers);

				pitch -= delta.y * pitchSensitivity;
				yaw   += delta.x *   yawSensitivity;
			}
			
			pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);

			if (yawClamp == true)
			{
				yaw = Mathf.Clamp(yaw, yawMin, yawMax);
			}

			// Smoothly dampen values
			var factor = SgtHelper.DampenFactor(damping, Time.deltaTime);

			currentPitch = Mathf.Lerp(currentPitch, pitch, factor);
			currentYaw   = Mathf.Lerp(currentYaw  , yaw  , factor);

			// Apply new rotation
			transform.localRotation = Quaternion.Euler(currentPitch, currentYaw, 0.0f);
		}

		private bool CanRotate
		{
			get
			{
				if (SgtInputManager.IsDown(key) == true)
				{
					return true;
				}

				return false;
			}
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using UnityEditor;
	using TARGET = SgtDragPitchYaw;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtDragPitchYaw_Editor : SgtEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Draw("key", "The key that must be held for this component to activate on desktop platforms.\n\nNone = Any mouse button.");
			Draw("guiLayers", "Fingers that began touching the screen on top of these UI layers will be ignored.");

			Separator();

			Draw("pitch", "The target pitch angle in degrees.");
			Draw("pitchSensitivity", "The speed the camera rotates relative to the mouse/finger drag distance.");
			Draw("pitchMin", "The minimum value of the pitch value.");
			Draw("pitchMax", "The maximum value of the pitch value.");

			Separator();

			Draw("yaw", "The target yaw angle in degrees.");
			Draw("yawSensitivity", "The speed the yaw changed relative to the mouse/finger drag distance.");
			Draw("yawClamp", "Clamp the yaw value?");
			if (Any(tgts, t => t.YawClamp == true))
			{
				BeginIndent();
					Draw("yawMin", "The minimum value of the yaw value.", "Min");
					Draw("yawMax", "The maximum value of the yaw value.", "Max");
				EndIndent();
			}

			Separator();

			Draw("damping", "How quickly the rotation transitions from the current to the target value (-1 = instant).");
		}
	}
}
#endif