using UnityEngine;
using System.Collections.Generic;
using FSA = UnityEngine.Serialization.FormerlySerializedAsAttribute;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to rotate the current GameObject based on mouse/finger drags. NOTE: This requires the SgtInputManager in your scene to function.</summary>
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtCameraLook")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Camera Look")]
	public class SgtCameraLook : MonoBehaviour
	{
		/// <summary>Is this component currently listening for inputs?</summary>
		public bool Listen { set { listen = value; } get { return listen; } } [SerializeField] private bool listen = true;

		/// <summary>How quickly the rotation transitions from the current to the target value (-1 = instant).</summary>
		public float Damping { set { damping = value; } get { return damping; } } [FSA("Dampening")] [SerializeField] private float damping = 10.0f;

		/// <summary>The keys/fingers required to pitch down/up.</summary>
		public SgtInputManager.Axis PitchControls { set { pitchControls = value; } get { return pitchControls; } } [SerializeField] private SgtInputManager.Axis pitchControls = new SgtInputManager.Axis(1, true, SgtInputManager.AxisGesture.VerticalDrag, -0.1f, KeyCode.None, KeyCode.None, KeyCode.None, KeyCode.None, 45.0f);

		/// <summary>The keys/fingers required to yaw left/right.</summary>
		public SgtInputManager.Axis YawControls { set { yawControls = value; } get { return yawControls; } } [SerializeField] private SgtInputManager.Axis yawControls = new SgtInputManager.Axis(1, true, SgtInputManager.AxisGesture.HorizontalDrag, 0.1f, KeyCode.None, KeyCode.None, KeyCode.None, KeyCode.None, 45.0f);

		/// <summary>The keys/fingers required to roll left/right.</summary>
		public SgtInputManager.Axis RollControls { set { rollControls = value; } get { return rollControls; } } [SerializeField] private SgtInputManager.Axis rollControls = new SgtInputManager.Axis(2, true, SgtInputManager.AxisGesture.Twist, -75.0f, KeyCode.E, KeyCode.Q, KeyCode.None, KeyCode.None, 45.0f);

		[System.NonSerialized]
		private Quaternion remainingDelta = Quaternion.identity;

		protected virtual void OnEnable()
		{
			SgtInputManager.EnsureThisComponentExists();
		}

		protected virtual void Update()
		{
			if (listen == true)
			{
				AddToDelta();
			}

			DampenDelta();
		}

		private void AddToDelta()
		{
			// Get delta from binds
			var delta = default(Vector3);

			delta.x = pitchControls.GetValue(Time.deltaTime);
			delta.y = yawControls  .GetValue(Time.deltaTime);
			delta.z = rollControls .GetValue(Time.deltaTime);

			// Store old rotation
			var oldRotation = transform.localRotation;

			// Rotate
			transform.Rotate(delta.x, delta.y, 0.0f, Space.Self);

			transform.Rotate(0.0f, 0.0f, delta.z, Space.Self);

			// Add to remaining
			remainingDelta *= Quaternion.Inverse(oldRotation) * transform.localRotation;

			// Revert rotation
			transform.localRotation = oldRotation;
		}

		private void DampenDelta()
		{
			// Dampen remaining delta
			var factor   = SgtHelper.DampenFactor(damping, Time.deltaTime);
			var newDelta = Quaternion.Slerp(remainingDelta, Quaternion.identity, factor);

			// Rotate by difference
			transform.localRotation = transform.localRotation * Quaternion.Inverse(newDelta) * remainingDelta;

			// Update remaining
			remainingDelta = newDelta;
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using TARGET = SgtCameraLook;

	[UnityEditor.CanEditMultipleObjects]
	[UnityEditor.CustomEditor(typeof(TARGET))]
	public class SgtCameraLook_Editor : SgtEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Draw("listen", "Is this component currently listening for inputs?");
			Draw("damping", "How quickly the rotation transitions from the current to the target value (-1 = instant).");

			Separator();

			Draw("pitchControls", "The keys/fingers required to pitch down/up.");
			Draw("yawControls", "The keys/fingers required to yaw left/right.");
			Draw("rollControls", "The keys/fingers required to roll left/right.");
		}
	}
}
#endif