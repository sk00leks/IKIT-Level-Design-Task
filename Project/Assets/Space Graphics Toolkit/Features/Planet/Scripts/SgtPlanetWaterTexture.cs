using UnityEngine;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component can be added alongside the <b>SgtPlanet</b> component to give it an animated water surface texture.</summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(SgtPlanet))]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtPlanetWaterTexture")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Planet Water Texture")]
	public class SgtPlanetWaterTexture : MonoBehaviour
	{
		/// <summary>The generated water texture will be based on this texture.
		/// NOTE: This should be a normal map.</summary>
		public Texture BaseTexture { set { baseTexture = value; } get { return baseTexture; } } [SerializeField] private Texture baseTexture;

		/// <summary>The strength of the normal map.</summary>
		public float Strength { set { strength = value; } get { return strength; } } [SerializeField] private float strength = 1.0f;

		/// <summary>The speed of the water animation.</summary>
		public float Speed { set { speed = value; } get { return speed; } } [SerializeField] private float speed = 5.0f;

		[System.NonSerialized]
		private SgtPlanet cachedPlanet;

		[System.NonSerialized]
		private RenderTexture generatedTexture;

		[SerializeField]
		private float age;

		private static Material cachedMaterial;

		protected virtual void OnEnable()
		{
			cachedPlanet = GetComponent<SgtPlanet>();
		}

		protected virtual void OnDestroy()
		{
			if (generatedTexture != null)
			{
				generatedTexture = SgtHelper.Destroy(generatedTexture);
			}
		}

		protected virtual void Update()
		{
			if (Application.isPlaying == true)
			{
				age += Time.deltaTime * speed;
			}

			if (baseTexture != null)
			{
				if (generatedTexture == null)
				{
					generatedTexture = new RenderTexture(baseTexture.width, baseTexture.height, 0, RenderTextureFormat.ARGB32, 8);

					generatedTexture.wrapMode         = TextureWrapMode.Repeat;
					generatedTexture.useMipMap        = true;
					generatedTexture.autoGenerateMips = false;
					generatedTexture.filterMode       = FilterMode.Trilinear;
					generatedTexture.anisoLevel       = 8;
				}

				if (cachedMaterial == null)
				{
					cachedMaterial = SgtHelper.CreateTempMaterial("PlanetWater (Generated)", SgtHelper.ShaderNamePrefix + "PlanetWater");
				}

				cachedMaterial.SetTexture(SgtShader._MainTex, baseTexture);
				cachedMaterial.SetFloat(SgtShader._Age, age);
				cachedMaterial.SetFloat(SgtShader._NormalStrength, strength);

				Graphics.Blit(null, generatedTexture, cachedMaterial);

				generatedTexture.GenerateMips();

				cachedPlanet.Properties.SetTexture(Shader.PropertyToID("_WaterTexture"), generatedTexture);
			}
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using TARGET = SgtPlanetWaterTexture;

	[UnityEditor.CanEditMultipleObjects]
	[UnityEditor.CustomEditor(typeof(TARGET))]
	public class SgtPlanetWaterTexture_Editor : SgtEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			BeginError(Any(tgts, t => t.BaseTexture == null));
				Draw("baseTexture", "The generated water texture will be based on this texture.\n\nNOTE: This should be a normal map.");
			EndError();
			Draw("strength", "The strength of the normal map.");
			Draw("speed", "The speed of the water animation.");
		}
	}
}
#endif