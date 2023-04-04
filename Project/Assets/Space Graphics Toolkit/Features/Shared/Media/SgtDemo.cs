using UnityEngine;
using UnityEngine.Rendering;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component is used by all the demo scenes to perform common tasks. Including modifying the current scene to make it look consistent between different rendering pipelines.</summary>
	[ExecuteInEditMode]
	[AddComponentMenu("")]
	public class SgtDemo : MonoBehaviour
	{
		/// <summary>If you enable this setting and your project is running with HDRP then a <b>Volume</b> component will be added to the scene that adjusts the camera exposure to match the other pipelines.</summary>
		public bool ChangeExposureInHDRP { set { changeExposureInHDRP = value; } get { return changeExposureInHDRP; } } [SerializeField] private bool changeExposureInHDRP = true;

		/// <summary>If you enable this setting and your project is running with HDRP then a <b>Volume</b> component will be added to the scene that adjusts the background to match the other pipelines.</summary>
		public bool ChangeVisualEnvironmentInHDRP { set { changeVisualEnvironmentInHDRP = value; } get { return changeVisualEnvironmentInHDRP; } } [SerializeField] private bool changeVisualEnvironmentInHDRP = true;

		/// <summary>If you enable this setting and your project is running with HDRP then a <b>Volume</b> component will be added to the scene that adjusts the fog to match the other pipelines.</summary>
		public bool ChangeFogInHDRP { set { changeFogInHDRP = value; } get { return changeFogInHDRP; } } [SerializeField] private bool changeFogInHDRP = true;

		/// <summary>If you enable this setting and your project is running with HDRP then a <b>Volume</b> component will be added to the scene that adjusts the clouds to match the other pipelines.</summary>
		public bool ChangeCloudsInHDRP { set { changeCloudsInHDRP = value; } get { return changeCloudsInHDRP; } } [SerializeField] private bool changeCloudsInHDRP = true;

		/// <summary>If you enable this setting and your project is running with HDRP then a <b>Volume</b> component will be added to the scene that adjusts the motion blur to match the other pipelines.</summary>
		public bool ChangeMotionBlurInHDRP { set { changeMotionBlurInHDRP = value; } get { return changeMotionBlurInHDRP; } } [SerializeField] private bool changeMotionBlurInHDRP = true;

		/// <summary>If you enable this setting and your project is running with HDRP then any lights missing the <b>HDAdditionalLightData</b> component will have it added.</summary>
		public bool UpgradeLightsInHDRP { set { upgradeLightsInHDRP = value; } get { return upgradeLightsInHDRP; } } [SerializeField] private bool upgradeLightsInHDRP = true;

		/// <summary>If you enable this setting and your project is running with HDRP then any cameras missing the <b>HDAdditionalCameraData</b> component will have it added.</summary>
		public bool UpgradeCamerasInHDRP { set { upgradeCamerasInHDRP = value; } get { return upgradeCamerasInHDRP; } } [SerializeField] private bool upgradeCamerasInHDRP = true;

		/// <summary>If you enable this setting and your project is running with the new InputSystem then the <b>EventSystem's InputModule</b> component will be upgraded.</summary>
		public bool UpgradeInputModule { set { upgradeInputModule = value; } get { return upgradeInputModule; } } [SerializeField] private bool upgradeInputModule = true;

		/// <summary>If you enable this setting and your project is running with HDRP and your scene contains <b>SgtAtmosphere</b> components with scattering, then their <b>ScatteringHdr</b> settings will be enabled.</summary>
		public bool ForceScatteringHdrInHDRP { set { forceScatteringHdrInHDRP = value; } get { return forceScatteringHdrInHDRP; } } [SerializeField] private bool forceScatteringHdrInHDRP = true;

		/// <summary>If you set this then the specified skybox will be placed on top of the main camera.</summary>
		public Transform SkyboxRoot { set { skyboxRoot = value; } get { return skyboxRoot; } } [SerializeField] private Transform skyboxRoot;

		protected virtual void OnEnable()
		{
			var crp = GraphicsSettings.currentRenderPipeline;

			if (crp != null && crp.GetType().ToString().Contains("HighDefinition") == true)
			{
				if (changeExposureInHDRP == true || changeVisualEnvironmentInHDRP == true || changeFogInHDRP == true)
				{
					TryCreateVolume();
				}

				if (upgradeLightsInHDRP == true)
				{
					TryUpgradeLights();
				}

				if (upgradeCamerasInHDRP == true)
				{
					TryUpgradeCameras();
				}
			}

			if (upgradeInputModule == true)
			{
				TryUpgradeEventSystem();
			}
		}

		protected virtual void LateUpdate()
		{
			if (skyboxRoot != null)
			{
				var camera = Camera.main;

				if (camera != null)
				{
					skyboxRoot.position = camera.transform.position;
				}
			}

			if (forceScatteringHdrInHDRP == true)
			{
				TryForceScattering();
			}
		}

		private void TryCreateVolume()
		{
	#if __HDRP__
			var volume = GetComponent<Volume>();

			if (volume == null)
			{
				volume = gameObject.AddComponent<Volume>();
			}

			var profile = volume.profile;

			if (profile == null)
			{
				profile = ScriptableObject.CreateInstance<VolumeProfile>();

				profile.hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor;
			}

			if (profile.components.Count == 0)
			{
				name = "SgtDemo (Volume Added)";

				if (changeExposureInHDRP == true)
				{
					var exposure = profile.Add<UnityEngine.Rendering.HighDefinition.Exposure>(true);

					exposure.fixedExposure.value = 14.0f;
				}

				if (changeVisualEnvironmentInHDRP == true)
				{
					var visualEnvironment = profile.Add<UnityEngine.Rendering.HighDefinition.VisualEnvironment>(true);

					visualEnvironment.skyType.value = 0;
				}

				if (changeFogInHDRP == true)
				{
					var fog = profile.Add<UnityEngine.Rendering.HighDefinition.Fog>(true);

					fog.enabled.value = false;
				}

				#if UNITY_2021_2_OR_NEWER
					if (changeCloudsInHDRP == true)
					{
						var clouds = profile.Add<UnityEngine.Rendering.HighDefinition.VolumetricClouds>(true);

						clouds.enable.value = false;
					}
				#endif

				if (changeMotionBlurInHDRP == true)
				{
					var motionBlur = profile.Add<UnityEngine.Rendering.HighDefinition.MotionBlur>(true);

					motionBlur.intensity.value = 0.0f;
				}
			}

			volume.profile = profile;
	#endif
		}

		private void TryUpgradeLights()
		{
	#if __HDRP__
			foreach (var light in FindObjectsOfType<Light>())
			{
				if (light.GetComponent<UnityEngine.Rendering.HighDefinition.HDAdditionalLightData>() == null)
				{
					light.gameObject.AddComponent<UnityEngine.Rendering.HighDefinition.HDAdditionalLightData>();
				}
			}
	#endif
		}

		private void TryUpgradeCameras()
		{
	#if __HDRP__
			foreach (var camera in FindObjectsOfType<Camera>())
			{
				if (camera.GetComponent<UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData>() == null)
				{
					var hdCamera = camera.gameObject.AddComponent<UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData>();

					hdCamera.backgroundColorHDR = Color.black;
				}
			}
	#endif
		}

		private void TryUpgradeEventSystem()
		{
	#if UNITY_EDITOR && ENABLE_INPUT_SYSTEM
			var module = FindObjectOfType<UnityEngine.EventSystems.StandaloneInputModule>();

			if (module != null)
			{
				module.gameObject.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

				DestroyImmediate(module);
			}
	#endif
		}

		private void TryForceScattering()
		{
	#if __HDRP__
			foreach (var atmosphere in FindObjectsOfType<SgtAtmosphere>())
			{
				atmosphere.ScatteringHdr = true;
			}
	#endif
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using UnityEditor;
	using TARGET = SgtDemo;

	[CustomEditor(typeof(TARGET))]
	public class SgtDemo_Editor : SgtEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Draw("changeExposureInHDRP", "If you enable this setting and your project is running with HDRP then a Volume component will be added to this GameObject that adjusts the camera exposure to match the other pipelines.");
			Draw("changeVisualEnvironmentInHDRP", "If you enable this setting and your project is running with HDRP then a Volume component will be added to this GameObject that adjusts the background to match the other pipelines.");
			Draw("changeFogInHDRP", "If you enable this setting and your project is running with HDRP then a Volume component will be added to the scene that adjusts the fog to match the other pipelines.");
			Draw("changeCloudsInHDRP", "If you enable this setting and your project is running with HDRP then a Volume component will be added to the scene that adjusts the clouds to match the other pipelines.");
			Draw("changeMotionBlurInHDRP", "If you enable this setting and your project is running with HDRP then a <b>Volume</b> component will be added to the scene that adjusts the motion blur to match the other pipelines.");
			Draw("upgradeLightsInHDRP", "If you enable this setting and your project is running with HDRP then any lights missing the HDAdditionalLightData component will have it added.");
			Draw("upgradeCamerasInHDRP", "If you enable this setting and your project is running with HDRP then any cameras missing the HDAdditionalCameraData component will have it added.");
			Draw("upgradeInputModule", "If you enable this setting and your project is running with the new InputSystem then the EventSystem's InputModule component will be upgraded.");
			Draw("forceScatteringHdrInHDRP", "If you enable this setting and your project is running with HDRP and your scene contains <b>SgtAtmosphere</b> components with scattering, then their <b>ScatteringHdr</b> settings will be enabled.");
			Draw("skyboxRoot", "If you set this then the specified skybox will be placed on top of the main camera.");
		}
	}
}
#endif