using UnityEngine;
using FSA = UnityEngine.Serialization.FormerlySerializedAsAttribute;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to control a Camera component's depthTextureMode setting.</summary>
	[ExecuteInEditMode]
	[DisallowMultipleComponent]
	[RequireComponent(typeof(Camera))]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtDepthTextureMode")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Depth Texture Mode")]
	public class SgtDepthTextureMode : MonoBehaviour
	{
		/// <summary>The depth mode that will be applied to the camera.</summary>
		public DepthTextureMode DepthMode { set { depthMode = value; UpdateDepthMode(); } get { return depthMode; } } [FSA("DepthMode")] [SerializeField] private DepthTextureMode depthMode = DepthTextureMode.None;

		[System.NonSerialized]
		private Camera cachedCamera;

		public void UpdateDepthMode()
		{
			if (cachedCamera == null) cachedCamera = GetComponent<Camera>();

			cachedCamera.depthTextureMode = depthMode;
		}

		protected virtual void Update()
		{
			UpdateDepthMode();
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using TARGET = SgtDepthTextureMode;

	[UnityEditor.CustomEditor(typeof(TARGET))]
	public class SgtDepthTextureMode_Editor : SgtEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Draw("depthMode", "The depth mode that will be applied to the camera.");
		}
	}
}
#endif