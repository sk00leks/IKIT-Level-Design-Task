using UnityEngine;
using System.Collections.Generic;
using FSA = UnityEngine.Serialization.FormerlySerializedAsAttribute;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to create simple thrusters that can apply forces to Rigidbodies based on their position. You can also use sprites to change the graphics</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtThrusterRoll")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Thruster Roll")]
	public class SgtThrusterRoll : MonoBehaviour
	{
		public class CameraState : SgtCameraState
		{
			public Quaternion LocalRotation;
		}

		/// <summary>The rotation offset in degrees.</summary>
		public Vector3 Rotation { set { rotation = value; } get { return rotation; } } [FSA("Rotation")] [SerializeField] private Vector3 rotation = new Vector3(0.0f, 90.0f, 90.0f);

		[System.NonSerialized]
		private List<CameraState> cameraStates;

		protected virtual void OnEnable()
		{
			SgtCamera.OnCameraPreCull   += CameraPreCull;
			SgtCamera.OnCameraPreRender += CameraPreRender;
		}

		protected virtual void OnDisable()
		{
			SgtCamera.OnCameraPreCull   -= CameraPreCull;
			SgtCamera.OnCameraPreRender -= CameraPreRender;
		}

		private void CameraPreCull(Camera camera)
		{
			Revert();
			{
				var direction = transform.forward;
				var adjacent  = transform.position - camera.transform.position;
				var cross     = Vector3.Cross(direction, adjacent);

				if (cross != Vector3.zero)
				{
					transform.rotation = Quaternion.LookRotation(cross, direction) * Quaternion.Euler(rotation);
				}
			}
			Save(camera);
		}

		private void CameraPreRender(Camera camera)
		{
			Restore(camera);
		}

		public void Save(Camera camera)
		{
			var cameraState = SgtCameraState.Find(ref cameraStates, camera);
		
			cameraState.LocalRotation = transform.localRotation;
		}

		private void Restore(Camera camera)
		{
			var cameraState = SgtCameraState.Restore(cameraStates, camera);

			if (cameraState != null)
			{
				transform.localRotation = cameraState.LocalRotation;
			}
		}

		public void Revert()
		{
			transform.localRotation = Quaternion.identity;
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using TARGET = SgtThrusterRoll;

	[UnityEditor.CanEditMultipleObjects]
	[UnityEditor.CustomEditor(typeof(TARGET))]
	public class SgtThrusterRoll_Editor : SgtEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Draw("rotation", "The rotation offset in degrees.");
		}
	}
}
#endif