using UnityEngine;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to rotate the current GameObject based on mouse/finger drags. NOTE: To function, this component requires the <b>SgtInputManager</b> component to be in your scene.</summary>
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtCameraPivot")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Camera Pivot")]
	public class SgtCameraPivot : MonoBehaviour
	{
		/// <summary>Is this component currently listening for inputs?</summary>
		public bool Listen { set { listen = value; } get { return listen; } } [SerializeField] private bool listen = true;

		/// <summary>How quickly the rotation transitions from the current to the target value (-1 = instant).</summary>
		public float Damping { set { damping = value; } get { return damping; } } [SerializeField] private float damping = 10.0f;

		/// <summary>The keys/fingers required to pitch down/up.</summary>
		public SgtInputManager.Axis PitchControls { set { pitchControls = value; } get { return pitchControls; } } [SerializeField] private SgtInputManager.Axis pitchControls = new SgtInputManager.Axis(1, true, SgtInputManager.AxisGesture.VerticalDrag, -0.1f, KeyCode.None, KeyCode.None, KeyCode.None, KeyCode.None, 45.0f);

		/// <summary>The keys/fingers required to yaw left/right.</summary>
		public SgtInputManager.Axis YawControls { set { yawControls = value; } get { return yawControls; } } [SerializeField] private SgtInputManager.Axis yawControls = new SgtInputManager.Axis(1, true, SgtInputManager.AxisGesture.HorizontalDrag, 0.1f, KeyCode.None, KeyCode.None, KeyCode.None, KeyCode.None, 45.0f);

		[System.NonSerialized]
		private Vector3 remainingDelta;

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
			remainingDelta.x += pitchControls.GetValue(Time.deltaTime);
			remainingDelta.y += yawControls  .GetValue(Time.deltaTime);
		}

		private void DampenDelta()
		{
			// Dampen remaining delta
			var factor   = SgtHelper.DampenFactor(damping, Time.deltaTime);
			var newDelta = Vector3.Lerp(remainingDelta, Vector3.zero, factor);

			// Rotate by difference
			var euler = transform.localEulerAngles;

			euler.x = -Mathf.DeltaAngle(euler.x, 0.0f);

			euler += remainingDelta - newDelta;

			euler.x = Mathf.Clamp(euler.x, -89.0f, 89.0f);

			transform.localEulerAngles = euler;

			// Update remaining
			remainingDelta = newDelta;
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using TARGET = SgtCameraPivot;

	[UnityEditor.CanEditMultipleObjects]
	[UnityEditor.CustomEditor(typeof(TARGET))]
	public class SgtCameraPivot_Editor : SgtEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Draw("listen", "Is this component currently listening for inputs?");
			Draw("damping", "How quickly the rotation transitions from the current to the target value (-1 = instant).");

			Separator();

			Draw("pitchControls", "The keys/fingers required to pitch down/up.");
			Draw("yawControls", "The keys/fingers required to yaw left/right.");
		}
	}
}
#endif