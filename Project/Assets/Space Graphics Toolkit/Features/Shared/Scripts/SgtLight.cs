using UnityEngine;
using System.Collections.Generic;
using FSA = UnityEngine.Serialization.FormerlySerializedAsAttribute;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component marks the attached Light as one that will be used by SGT. Most SGT features only work with a limited amount of lights, so having to explicitly define which ones will be used helps stop you going over this limit.</summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(Light))]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtLight")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Light")]
	public class SgtLight : SgtLinkedBehaviour<SgtLight>
	{
		public const int MAX_LIGHTS = 16;

		/// <summary>The SgtLightPointer component allows you to treat a Directional light like a Point light, and enabling this allows you to notify SGT about this.</summary>
		public bool TreatAsPoint { set { treatAsPoint = value; } get { return treatAsPoint; } } [FSA("TreatAsPoint")] [SerializeField] private bool treatAsPoint = false;

		/// <summary>All light values will be multiplied by this before use.</summary>
		public float Multiplier { set { multiplier = value; } get { return multiplier; } } [SerializeField] private float multiplier = 1.0f;

		/// <summary>This allows you to control the intensity of this light when used by SGT components that implement their own custom lighting (e.g. SgtAtmosphere). This should typically be in the 0..1 range.</summary>
		public float IntensityInSGT { set { intensityInSGT = value; } get { return intensityInSGT; } } [SerializeField] private float intensityInSGT = 1.0f;

		/// <summary>This allows you to control the intensity of the attached light when using the <b>Standard</b> rendering pipeline.
		/// -1 = The attached light intensity will not be modified.</summary>
		public float IntensityInStandard { set  { intensityInStandard = value; } get { return intensityInStandard; } } [SerializeField] private float intensityInStandard = 1.0f;

		/// <summary>This allows you to control the intensity of the attached light when using the <b>URP</b> rendering pipeline.
		/// -1 = The attached light intensity will not be modified.</summary>
		public float IntensityInURP { set  { intensityInURP = value; } get { return intensityInURP; } } [SerializeField] private float intensityInURP = 1.0f;

		/// <summary>This allows you to control the intensity of the attached light when using the <b>HDRP</b> rendering pipeline.
		/// -1 = The attached light intensity will not be modified.</summary>
		public float IntensityInHDRP { set  { intensityInHDRP = value; } get { return intensityInHDRP; } } [SerializeField] private float intensityInHDRP = 120000.0f;

		[System.NonSerialized]
		private Transform cachedTransform;

		[System.NonSerialized]
		private bool cachedTransformSet;

		[System.NonSerialized]
		private Light cachedLight;

		[System.NonSerialized]
		private bool cachedLightSet;

		private static SgtShaderBundle.Pipeline pipe;

	#if __HDRP__
		[System.NonSerialized]
		private UnityEngine.Rendering.HighDefinition.HDAdditionalLightData cachedLightData;
	#endif

		private static List<LightProperties> cachedLightProperties = new List<LightProperties>();

		private static List<string> cachedLightKeywords = new List<string>();

		private static List<SgtLight> tempLights = new List<SgtLight>();

		public Light CachedLight
		{
			get
			{
				if (cachedLightSet == false)
				{
					cachedLight    = GetComponent<Light>();
					cachedLightSet = true;
				}

				return cachedLight;
			}
		}

		public Transform CachedTransform
		{
			get
			{
				if (cachedTransformSet == false)
				{
					cachedTransform    = GetComponent<Transform>();
					cachedTransformSet = true;
				}

				return cachedTransform;
			}
		}

		protected virtual void Update()
		{
			pipe = SgtShaderBundle.DetectProjectPipeline();

			if (SgtShaderBundle.IsStandard(pipe) == true)
			{
				ApplyIntensity(intensityInStandard);
			}
			else if (SgtShaderBundle.IsURP(pipe) == true)
			{
				ApplyIntensity(intensityInURP);
			}
			else if (SgtShaderBundle.IsHDRP(pipe) == true)
			{
				ApplyIntensity(intensityInHDRP);
			}
		}

		private void ApplyIntensity(float intensity)
		{
			if (intensity >= 0.0f)
			{
				if (cachedLightSet == false)
				{
					cachedLight    = GetComponent<Light>();
					cachedLightSet = true;
				}

				#if __HDRP__
					if (cachedLightData == null)
					{
						cachedLightData = GetComponent<UnityEngine.Rendering.HighDefinition.HDAdditionalLightData>();
					}

					if (cachedLightData != null)
					{
						cachedLightData.SetIntensity(intensity * multiplier, UnityEngine.Rendering.HighDefinition.LightUnit.Lux);
					}
				#else
					cachedLight.intensity = intensity * multiplier;
				#endif
			}
		}

		private static Vector3 compareDistanceCenter;

		private static int CompareDistance(SgtLight a, SgtLight b)
		{
			var distA = Vector3.SqrMagnitude(a.CachedTransform.position - compareDistanceCenter);
			var distB = Vector3.SqrMagnitude(b.CachedTransform.position - compareDistanceCenter);

			return distA.CompareTo(distB);
		}

		private static System.Comparison<SgtLight> CompareDistanceDel = CompareDistance;

		public static List<SgtLight> Find(bool lit, int mask, Vector3 center)
		{
			tempLights.Clear();

			if (lit == true)
			{
				var light = FirstInstance;

				for (var i = 0; i < InstanceCount; i++)
				{
					var cachedLight = light.CachedLight;

					if (SgtHelper.Enabled(cachedLight) == true && light.intensityInSGT > 0.0f && light.multiplier > 0.0f && (cachedLight.cullingMask & mask) != 0)
					{
						tempLights.Add(light);
					}

					light = light.NextInstance;
				}

				compareDistanceCenter = center;

				tempLights.Sort(CompareDistanceDel);
			}

			return tempLights;
		}

		public static void FilterOut(Vector3 center)
		{
			for (var i = tempLights.Count - 1; i >= 0; i--)
			{
				var tempLight = tempLights[i];

				if (tempLight.transform.position == center)
				{
					if (tempLight.treatAsPoint == true || tempLight.CachedLight.type != LightType.Directional)
					{
						tempLights.RemoveAt(i);
					}
				}
			}
		}

		public static void Calculate(SgtLight light, Vector3 center, Transform directionTransform, Transform positionTransform, ref Vector3 position, ref Vector3 direction, ref Color color, ref float intensity)
		{
			if (light != null)
			{
				var cachedLight = light.CachedLight;

				direction = -light.transform.forward;
				position  = light.transform.position;
				color     = cachedLight.color;
				intensity = cachedLight.intensity * light.intensityInSGT;

				switch (cachedLight.type)
				{
					case LightType.Point:
					{
						direction = Vector3.Normalize(position - center);

						if (SgtShaderBundle.IsStandard(pipe) == true)
						{
							var dist  = SgtHelper.Divide(Vector3.Distance(center, position), light.CachedLight.range);
							var atten = Mathf.Clamp01(1.0f / (1.0f + 25.0f * dist * dist) * ((1.0f - dist) * 5.0f));

							intensity *= atten;
						}
						// Attenuation is more or less the same in URP and HDRP?
						else
						{
							var dist  = Vector3.Distance(center, position);
							var atten = Mathf.Clamp01(1.0f / (dist * dist));

							intensity *= atten;

							var range = 1.0f - SgtHelper.Divide(dist, light.CachedLight.range);

							intensity *= Mathf.Clamp01(range * range);
						}
					}
					break;

					case LightType.Directional:
					{
						if (light.treatAsPoint == true)
						{
							direction = Vector3.Normalize(position - center);
						}
						else
						{
							position = center + direction * 10000000.0f;
						}
					}
					break;
				}

				// Transform into local space?
				if (directionTransform != null)
				{
					direction = directionTransform.InverseTransformDirection(direction);
				}

				if (positionTransform != null)
				{
					position = positionTransform.InverseTransformPoint(position);
				}
			}
		}

		private static Vector4[] tempColor     = new Vector4[MAX_LIGHTS];
		private static Vector4[] tempScatter   = new Vector4[MAX_LIGHTS];
		private static Vector4[] tempPosition  = new Vector4[MAX_LIGHTS];
		private static Vector4[] tempDirection = new Vector4[MAX_LIGHTS];

		public static void Write(Vector3 center, Transform directionTransform, Transform positionTransform, float scatterStrength, int maxLights)
		{
			var lightCount = 0;

			for (var i = 0; i < tempLights.Count && lightCount < maxLights; i++)
			{
				var light     = tempLights[i];
				var position  = default(Vector3);
				var direction = default(Vector3);
				var color     = default(Color);
				var intensity = default(float);

				Calculate(light, center, directionTransform, positionTransform, ref position, ref direction, ref color, ref intensity);

				tempColor[lightCount] = SgtHelper.Brighten(color, intensity);
				tempScatter[lightCount] = SgtHelper.Brighten(color, intensity * scatterStrength);
				tempPosition[lightCount] = SgtHelper.NewVector4(position, 1.0f);
				tempDirection[lightCount] = direction;

				lightCount += 1;
			}

			foreach (var tempMaterial in SgtHelper.tempMaterials)
			{
				if (tempMaterial != null)
				{
					tempMaterial.SetInt("SgtLightCount", lightCount);

					if (lightCount > 0)
					{
						tempMaterial.SetVectorArray("SgtLightColor", tempColor);
						tempMaterial.SetVectorArray("SgtLightScatter", tempScatter);
						tempMaterial.SetVectorArray("SgtLightPosition", tempPosition);
						tempMaterial.SetVectorArray("SgtLightDirection", tempDirection);
					}
				}
			}
		}

		private class LightProperties
		{
			public int Direction;
			public int Position;
			public int Color;
			public int Scatter;
		}

		private static LightProperties GetLightProperties(int index)
		{
			for (var i = cachedLightProperties.Count; i <= index; i++)
			{
				var properties = new LightProperties();
				var prefix     = "_Light" + (i + 1);

				properties.Direction = Shader.PropertyToID(prefix + "Direction");
				properties.Position  = Shader.PropertyToID(prefix + "Position");
				properties.Color     = Shader.PropertyToID(prefix + "Color");
				properties.Scatter   = Shader.PropertyToID(prefix + "Scatter");

				cachedLightProperties.Add(properties);
			}

			return cachedLightProperties[index];
		}

		private static string GetLightKeyword(int index)
		{
			for (var i = cachedLightKeywords.Count; i <= index; i++)
			{
				cachedLightKeywords.Add("LIGHT_" + i);
			}

			return cachedLightKeywords[index];
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using TARGET = SgtLight;

	[UnityEditor.CanEditMultipleObjects]
	[UnityEditor.CustomEditor(typeof(TARGET))]
	public class SgtLight_Editor : SgtEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Info("This component marks the attached Light as one that will be used by SGT. Most SGT features only work with a limited amount of lights, so having to explicitly define which ones will be used helps stop you going over this limit.");
			
			Separator();

			Draw("treatAsPoint", "The SgtLightPointer component allows you to treat a Directional light like a Point light, and enabling this allows you to notify SGT about this.");
			Draw("multiplier", "All light values will be multiplied by this before use.");
			Draw("intensityInSGT", "This allows you to control the intensity of this light when used by SGT components that implement their own custom lighting (e.g. SgtAtmosphere). This should typically be in the 0..1 range.");
			Draw("intensityInStandard", "This allows you to control the intensity of the attached light when using the Standard rendering pipeline.\n\n-1 = The attached light intensity will not be modified.");
			Draw("intensityInURP", "This allows you to control the intensity of the attached light when using the URP rendering pipeline.\n\n-1 = The attached light intensity will not be modified.");
			Draw("intensityInHDRP", "This allows you to control the intensity of the attached light when using the HDRP rendering pipeline.\n\n-1 = The attached light intensity will not be modified.");
		}
	}
}
#endif