using UnityEngine;
using System.Collections.Generic;

namespace SpaceGraphicsToolkit
{
	/// <summary>This base class handles calculation of a shadow matrix and shadow texture.</summary>
	public abstract class SgtShadow : SgtLinkedBehaviour<SgtShadow>
	{
		public const int MAX_SPHERE_SHADOWS = 16;

		public const int MAX_RING_SHADOWS = 1;

		private static List<ShadowProperties> cachedShadowProperties = new List<ShadowProperties>();

		private static List<string> cachedShadowKeywords = new List<string>();

		private static List<SgtShadow> tempShadows = new List<SgtShadow>();

		public abstract void CalculateShadow(SgtLight light);

		[System.NonSerialized]
		private bool calculatedThisFrame;

		[System.NonSerialized]
		protected bool cachedActive;

		[System.NonSerialized]
		protected Matrix4x4 cachedMatrix;

		[System.NonSerialized]
		protected float cachedRatio;

		[System.NonSerialized]
		protected Vector4 cachedPower;

		[System.NonSerialized]
		protected float cachedRadius;

		private class ShadowProperties
		{
			public int Texture;
			public int Matrix;
			public int Ratio;
		}

		private static ShadowProperties GetShadowProperties(int index)
		{
			for (var i = cachedShadowProperties.Count; i <= index; i++)
			{
				var properties = new ShadowProperties();
				var prefix     = "_Shadow" + (i + 1);

				properties.Texture = Shader.PropertyToID(prefix + "Texture");
				properties.Matrix  = Shader.PropertyToID(prefix + "Matrix");
				properties.Ratio   = Shader.PropertyToID(prefix + "Ratio");

				cachedShadowProperties.Add(properties);
			}

			return cachedShadowProperties[index];
		}

		private static string GetShadowKeyword(int index)
		{
			for (var i = cachedShadowKeywords.Count; i <= index; i++)
			{
				cachedShadowKeywords.Add("SHADOW_" + i);
			}

			return cachedShadowKeywords[index];
		}

		public static List<SgtShadow> Find(bool lit, int mask, List<SgtLight> lights)
		{
			tempShadows.Clear();

			if (lit == true && lights != null && lights.Count > 0)
			{
				var shadow = FirstInstance;

				for (var i = 0; i < InstanceCount; i++)
				{
					var mask2 = 1 << shadow.gameObject.layer;

					if ((mask & mask2) != 0)
					{
						if (shadow.calculatedThisFrame == false)
						{
							shadow.calculatedThisFrame = true;

							shadow.CalculateShadow(lights[0]);
						}

						if (shadow.cachedActive == true)
						{
							tempShadows.Add(shadow);
						}
					}

					shadow = shadow.NextInstance;
				}
			}

			return tempShadows;
		}

		public static void FilterOutSphere(Vector3 center)
		{
			for (var i = tempShadows.Count - 1; i >= 0; i--)
			{
				var tempShadow = tempShadows[i];

				if (tempShadow is SgtShadowSphere && tempShadow.transform.position == center)
				{
					tempShadows.RemoveAt(i);
				}
			}
		}

		public static void FilterOutRing(Vector3 center)
		{
			for (var i = tempShadows.Count - 1; i >= 0; i--)
			{
				var tempShadow = tempShadows[i];

				if (tempShadow is SgtShadowRing && tempShadow.transform.position == center)
				{
					tempShadows.RemoveAt(i);
				}
			}
		}

		public static void FilterOutMiss(Vector3 center, float radius)
		{
			for (var i = tempShadows.Count - 1; i >= 0; i--)
			{
				var tempShadow = tempShadows[i];

				// Skip if overlapping
				if (Vector3.Distance(center, tempShadow.transform.position) > radius + tempShadow.cachedRadius)
				{
					var point = tempShadow.cachedMatrix.MultiplyPoint(center);

					if (point.z > 0.0f)
					{
						var distance = Mathf.Sqrt(point.x * point.x + point.y * point.y);

						if (distance * tempShadow.cachedRadius <= radius + tempShadow.cachedRadius)
						{
							continue;
						}
					}

					tempShadows.RemoveAt(i);
				}
			}
		}

		private static Matrix4x4[] tempSphereMatrix = new Matrix4x4[MAX_SPHERE_SHADOWS];
		private static Vector4[]   tempSpherePower  = new Vector4[MAX_SPHERE_SHADOWS];

		public static void WriteSphere(int maxShadows)
		{
			var shadowCount = 0;

			for (var i = 0; i < tempShadows.Count && shadowCount < maxShadows; i++)
			{
				var shadow = tempShadows[i];

				if (shadow is SgtShadowSphere)
				{
					tempSphereMatrix[shadowCount] = shadow.cachedMatrix;
					tempSpherePower[shadowCount] = shadow.cachedPower;

					shadowCount += 1;
				}
			}

			foreach (var tempMaterial in SgtHelper.tempMaterials)
			{
				if (tempMaterial != null)
				{
					tempMaterial.SetInt("_SphereShadowCount", shadowCount);

					if (shadowCount > 0)
					{
						tempMaterial.SetMatrixArray("_SphereShadowMatrix", tempSphereMatrix);
						tempMaterial.SetVectorArray("_SphereShadowPower", tempSpherePower);
					}
				}
			}
		}
		
		private static Texture     tempRingTexture;
		private static Matrix4x4[] tempRingMatrix = new Matrix4x4[MAX_RING_SHADOWS];
		private static float[]     tempRingRatio  = new float[MAX_RING_SHADOWS];

		public static void WriteRing(int maxShadows)
		{
			var shadowCount = 0;

			for (var i = 0; i < tempShadows.Count && shadowCount < maxShadows; i++)
			{
				var shadowRing = tempShadows[i] as SgtShadowRing;

				if (shadowRing != null)
				{
					tempRingTexture = shadowRing.Texture;
					tempRingMatrix[shadowCount] = shadowRing.cachedMatrix;
					tempRingRatio[shadowCount] = shadowRing.cachedRatio;

					shadowCount += 1;
				}
			}

			foreach (var tempMaterial in SgtHelper.tempMaterials)
			{
				if (tempMaterial != null)
				{
					tempMaterial.SetInt("_RingShadowCount", shadowCount);

					if (shadowCount > 0)
					{
						tempMaterial.SetTexture("_RingShadowTexture", tempRingTexture);
						tempMaterial.SetMatrixArray("_RingShadowMatrix", tempRingMatrix);
						tempMaterial.SetFloatArray("_RingShadowRatio", tempRingRatio);
					}
				}
			}
		}

		protected virtual void Update()
		{
			calculatedThisFrame = false;
		}
	}
}