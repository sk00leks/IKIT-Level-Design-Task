using UnityEngine;

namespace SpaceGraphicsToolkit
{
	/// <summary>All SgtFastBillboards will be updated from here.</summary>
	[ExecuteInEditMode]
	[DefaultExecutionOrder(-100)]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtFastBillboardManager")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Fast Billboard Manager")]
	public class SgtFastBillboardManager : SgtLinkedBehaviour<SgtFastBillboardManager>
	{
		protected override void OnEnable()
		{
			if (InstanceCount > 0)
			{
				Debug.LogWarning("Your scene already contains an instance of SgtFastBillboardManager!", FirstInstance);
			}

			base.OnEnable();

			SgtCamera.OnCameraPreCull += PreCull;
		}

		protected override void OnDisable()
		{
			base.OnDisable();

			SgtCamera.OnCameraPreCull -= PreCull;
		}

		private void PreCull(Camera camera)
		{
			if (this == FirstInstance)
			{
				var cameraRotation = camera.transform.rotation;
				var rollRotation   = cameraRotation;
				var observer       = default(SgtCamera);

				if (SgtCamera.TryFind(camera, ref observer) == true)
				{
					rollRotation *= observer.RollQuaternion;
				}

				var billboard = SgtFastBillboard.FirstInstance;
				var mask      = camera.cullingMask;
				var position  = camera.transform.position;

				for (var i = 0; i < SgtFastBillboard.InstanceCount; i++)
				{
					if ((billboard.Mask & mask) != 0)
					{
						var rotation = default(Quaternion);

						if (billboard.RollWithCamera == true)
						{
							rotation = rollRotation * billboard.Rotation;
						}
						else
						{
							rotation = cameraRotation * billboard.Rotation;
						}

						if (billboard.AvoidClipping == true)
						{
							var directionA = Vector3.Normalize(billboard.transform.position - position);
							var directionB = rotation * Vector3.forward;
							var theta      = Vector3.Angle(directionA, directionB);
							var axis       = Vector3.Cross(directionA, directionB);

							rotation = Quaternion.AngleAxis(theta, -axis) * rotation;
						}

						billboard.cachedTransform.rotation = rotation;
					}

					billboard = billboard.NextInstance;
				}
			}
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using TARGET = SgtFastBillboardManager;

	[UnityEditor.CanEditMultipleObjects]
	[UnityEditor.CustomEditor(typeof(TARGET))]
	public class SgtFastBillboardManager_Editor : SgtEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Info("This component marks where all spawned SgtFloatingObjects will be attached to.");
		}
	}
}
#endif