using UnityEngine;
using FSA = UnityEngine.Serialization.FormerlySerializedAsAttribute;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component rotates the current GameObject.</summary>
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtRotate")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Rotate")]
	public class SgtRotate : MonoBehaviour
	{
		/// <summary>The speed of the rotation in degrees per second.</summary>
		public Vector3 AngularVelocity { set { angularVelocity = value; } get { return angularVelocity; } } [FSA("AngularVelocity")] [SerializeField] private Vector3 angularVelocity = Vector3.up;

		/// <summary>The rotation space.</summary>
		public Space RelativeTo { set { relativeTo = value; } get { return relativeTo; } } [FSA("RelativeTo")] [SerializeField] private Space relativeTo;

		protected virtual void Update()
		{
			transform.Rotate(angularVelocity * Time.deltaTime, relativeTo);
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using TARGET = SgtRotate;

	[UnityEditor.CanEditMultipleObjects]
	[UnityEditor.CustomEditor(typeof(TARGET))]
	public class SgtRotate_Editor : SgtEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			BeginError(Any(tgts, t => t.AngularVelocity.magnitude == 0.0f));
				Draw("angularVelocity", "The speed of the rotation in degrees per second.");
			EndError();
			Draw("relativeTo", "The rotation space.");
		}
	}
}
#endif