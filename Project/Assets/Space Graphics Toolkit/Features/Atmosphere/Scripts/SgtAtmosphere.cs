using UnityEngine;
using FSA = UnityEngine.Serialization.FormerlySerializedAsAttribute;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to draw a volumetric atmosphere. The atmosphere is rendered using two materials, one for the surface (inner), and one for the sky (outer).
	/// The outer part of the atmosphere is automatically generated by this component using the OuterMesh you specify.
	/// The inner part of the atmosphere is provided by you (e.g. a normal sphere GameObject), and is specified in the SgtSharedMaterial component that this component automatically adds.</summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(SgtSharedMaterial))]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtAtmosphere")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Atmosphere")]
	public class SgtAtmosphere : MonoBehaviour
	{
		/// <summary>The base color will be multiplied by this.</summary>
		public Color Color { set { if (color != value) { color = value; DirtyMaterials(); } } get { return color; } } [FSA("Color")] [SerializeField] private Color color = Color.white;

		/// <summary>The Color.rgb values are multiplied by this, allowing you to quickly adjust the overall brightness.</summary>
		public float Brightness { set { if (brightness != value) { brightness = value; DirtyMaterials(); } } get { return brightness; } } [FSA("Brightness")] [SerializeField] private float brightness = 1.0f;

		/// <summary>This allows you to adjust the render queue of the atmosphere materials. You can normally adjust the render queue in the material settings, but since these materials are procedurally generated your changes will be lost.</summary>
		public SgtRenderQueue RenderQueue { set { if (renderQueue != value) { renderQueue = value; DirtyMaterials(); } } get { return renderQueue; } } [FSA("RenderQueue")] [SerializeField] private SgtRenderQueue renderQueue = SgtRenderQueue.GroupType.Transparent;

		/// <summary>The look up table associating optical depth with atmospheric color for the planet surface. The left side is used when the atmosphere is thin (e.g. center of the planet when looking from space). The right side is used when the atmosphere is thick (e.g. the horizon).</summary>
		public Texture InnerDepthTex { set { if (innerDepthTex != value) { innerDepthTex = value; DirtyMaterials(); } } get { return innerDepthTex; } } [FSA("InnerDepthTex")] [SerializeField] private Texture innerDepthTex;

		/// <summary>The radius of the meshes set in the SgtSharedMaterial.</summary>
		public float InnerMeshRadius { set { if (innerMeshRadius != value) { innerMeshRadius = value; DirtyMaterials(); } } get { return innerMeshRadius; } } [FSA("InnerMeshRadius")] [SerializeField] private float innerMeshRadius;

		/// <summary>The look up table associating optical depth with atmospheric color for the planet sky. The left side is used when the atmosphere is thin (e.g. edge of the atmosphere when looking from space). The right side is used when the atmosphere is thick (e.g. the horizon).</summary>
		public Texture2D OuterDepthTex { set { if (outerDepthTex != value) { outerDepthTex = value; DirtyMaterials(); } } get { return outerDepthTex; } } [FSA("OuterDepthTex")] [SerializeField] private Texture2D outerDepthTex;

		/// <summary>This allows you to set the mesh used to render the atmosphere. This should be a sphere.</summary>
		public Mesh OuterMesh { set { outerMesh = value; } get { return outerMesh; } } [FSA("OuterMesh")] [SerializeField] private Mesh outerMesh;

		/// <summary>This allows you to set the radius of the OuterMesh. If this is incorrectly set then the atmosphere will render incorrectly.</summary>
		public float OuterMeshRadius { set { outerMeshRadius = value; } get { return outerMeshRadius; } } [FSA("OuterMeshRadius")] [SerializeField] private float outerMeshRadius = 1.0f;

		/// <summary>If you have a big object that is both inside and outside of the atmosphere, it can cause a sharp intersection line. Increasing this setting allows you to change the smoothness of this intersection.</summary>
		public float OuterSoftness { set { if (outerSoftness != value) { outerSoftness = value; DirtyMaterials(); } } get { return outerSoftness; } } [FSA("OuterSoftness")] [Range(0.0f, 1000.0f)] [SerializeField] private float outerSoftness;

		/// <summary>This allows you to set how high the atmosphere extends above the surface of the planet in local space.</summary>
		public float Height { set { if (height != value) { height = value; DirtyMaterials(); } } get { return height; } } [FSA("Height")] [SerializeField] private float height = 0.1f;

		/// <summary>This allows you to adjust the fog level of the atmosphere on the surface.</summary>
		public float InnerFog { set { if (innerFog != value) { innerFog = value; DirtyMaterials(); } } get { return innerFog; } } [FSA("InnerFog")] [SerializeField] private float innerFog;

		/// <summary>This allows you to adjust the fog level of the atmosphere in the sky.</summary>
		public float OuterFog { set { if (outerFog != value) { outerFog = value; DirtyMaterials(); } } get { return outerFog; } } [FSA("OuterFog")] [SerializeField] private float outerFog;

		/// <summary>This allows you to control how quickly the sky reaches maximum opacity as the camera enters the atmosphere.
		/// 1 = You must go down to ground level before the opacity reaches max.
		/// 2 = You must go at least half way down to the surface.</summary>
		public float Sky { set { sky = value; } get { return sky; } } [FSA("Sky")] [SerializeField] private float sky = 1.0f;

		/// <summary>This allows you to set the altitude where atmospheric density reaches its maximum point. The lower you set this, the foggier the horizon will appear when approaching the surface.</summary>
		public float Middle { set { middle = value; } get { return middle; } } [FSA("Middle")] [Range(0.0f, 1.0f)] [SerializeField] private float middle = 0.5f;

		/// <summary>This allows you to offset the camera distance in world space when rendering the atmosphere, giving you fine control over the render order.</summary>
		public float CameraOffset { set { cameraOffset = value; } get { return cameraOffset; } } [FSA("CameraOffset")] [SerializeField] private float cameraOffset;

		/// <summary>If you enable this then nearby SgtLight and SgtShadow casters will be found and applied to the lighting calculations.</summary>
		public bool Lit { set { if (lit != value) { lit = value; DirtyMaterials(); } } get { return lit; } } [FSA("Lit")] [SerializeField] private bool lit;

		/// <summary>The atmosphere will always be lit by this amount.</summary>
		public Color AmbientColor { set { if (ambientColor != value) { ambientColor = value; DirtyMaterials(); } } get { return ambientColor; } } [FSA("AmbientColor")] [SerializeField] private Color ambientColor;

		/// <summary>The <b>AmbientColor</b> will be multiplied by this.</summary>
		public float AmbientBrightness { set { if (ambientBrightness != value) { ambientBrightness = value; DirtyMaterials(); } } get { return ambientBrightness; } } [SerializeField] private float ambientBrightness = 1.0f;

		/// <summary>The maximum amount of <b>SgtLight</b> components that can light this object.</summary>
		public int MaxLights { set { if (maxLights != value) { maxLights = value; DirtyMaterials(); } } get { return maxLights; } } [SerializeField] [Range(0, SgtLight.MAX_LIGHTS)] private int maxLights = 2;

		/// <summary>The maximum amount of <b>SgtShadowSphere</b> components that can shade this object.</summary>
		public int MaxSphereShadows { set { if (maxSphereShadows != value) { maxSphereShadows = value; DirtyMaterials(); } } get { return maxSphereShadows; } } [SerializeField] [Range(0, SgtShadow.MAX_SPHERE_SHADOWS)] private int maxSphereShadows = 2;

		/// <summary>The look up table associating light angle with surface color. The left side is used on the dark side, the middle is used on the horizon, and the right side is used on the light side.</summary>
		public Texture LightingTex { set { if (lightingTex != value) { lightingTex = value; DirtyMaterials(); } } get { return lightingTex; } } [FSA("LightingTex")] [SerializeField] private Texture lightingTex;

		/// <summary>If you enable this then light will scatter through the atmosphere. This means light entering the eye will come from all angles, especially around the light point.</summary>
		public bool Scattering { set { if (scattering != value) { scattering = value; DirtyMaterials(); } } get { return scattering; } } [FSA("Scattering")] [SerializeField] private bool scattering;

		/// <summary>If you enable this then atmospheric scattering will be applied to the surface material.</summary>
		public bool GroundScattering { set { if (groundScattering != value) { groundScattering = value; DirtyMaterials(); } } get { return groundScattering; } } [FSA("GroundScattering")] [SerializeField] private bool groundScattering = true;

		/// <summary>The look up table associating light angle with scattering color. The left side is used on the dark side, the middle is used on the horizon, and the right side is used on the light side.</summary>
		public Texture ScatteringTex { set { if (scatteringTex != value) { scatteringTex = value; DirtyMaterials(); } } get { return scatteringTex; } } [FSA("ScatteringTex")] [SerializeField] private Texture scatteringTex;

		/// <summary>The scattering is multiplied by this value, allowing you to easily adjust the brightness of the effect.</summary>
		public float ScatteringStrength { set { if (scatteringStrength != value) { scatteringStrength = value; DirtyMaterials(); } } get { return scatteringStrength; } } [FSA("ScatteringStrength")] [SerializeField] private float scatteringStrength = 3.0f;

		/// <summary>The Mie scattering term, allowing you to adjust the distribution of front scattered light.</summary>
		public float ScatteringMie { set { if (scatteringMie != value) { scatteringMie = value; DirtyMaterials(); } } get { return scatteringMie; } } [FSA("ScatteringMie")] [SerializeField] private float scatteringMie = 50.0f;

		/// <summary>The Rayleigh scattering term, allowing you to adjust the distribution of front and back scattered light.</summary>
		public float ScatteringRayleigh { set { if (scatteringRayleigh != value) { scatteringRayleigh = value; DirtyMaterials(); } } get { return scatteringRayleigh; } } [FSA("ScatteringRayleigh")] [SerializeField] private float scatteringRayleigh = 0.1f;

		/// <summary>Should the scattering make full use of the high range color buffer? Or only fill into the LDR 0..1 (0..255) range?</summary>
		public bool ScatteringHdr { set { if (scatteringHdr != value) { scatteringHdr = value; DirtyMaterials(); } } get { return scatteringHdr; } } [SerializeField] private bool scatteringHdr;

		/// <summary>Should the night side of the atmosphere have different sky values?</summary>
		public bool Night { set { night = value; } get { return night; } } [FSA("Night")] [SerializeField] private bool night;

		/// <summary>The 'Sky' value of the night side.</summary>
		public float NightSky { set { nightSky = value; } get { return nightSky; } } [FSA("NightSky")] [SerializeField] private float nightSky = 0.25f;

		/// <summary>The transition style between the day and night.</summary>
		public SgtEase.Type NightEase { set { nightEase = value; } get { return nightEase; } } [FSA("NightEase")] [SerializeField] private SgtEase.Type nightEase;

		/// <summary>The start point of the day/sunset transition (0 = dark side, 1 = light side).</summary>
		public float NightStart { set { nightStart = value; } get { return nightStart; } } [FSA("NightStart")] [Range(0.0f, 1.0f)] [SerializeField] private float nightStart = 0.4f;

		/// <summary>The end point of the day/sunset transition (0 = dark side, 1 = light side).</summary>
		public float NightEnd { set { nightEnd = value; } get { return nightEnd; } } [FSA("NightEnd")] [Range(0.0f, 1.0f)] [SerializeField] private float nightEnd = 0.6f;

		/// <summary>The power of the night transition.</summary>
		public float NightPower { set { nightPower = value; } get { return nightPower; } } [FSA("NightPower")] [SerializeField] private float nightPower = 2.0f;

		// The material applied to the surface
		[System.NonSerialized]
		protected Material innerMaterial;

		// The material applied to the sky
		[System.NonSerialized]
		protected Material outerMaterial;

		[System.NonSerialized]
		private Transform cachedTransform;

		[System.NonSerialized]
		private bool cachedTransformSet;

		[System.NonSerialized]
		private SgtSharedMaterial cachedSharedMaterial;

		[System.NonSerialized]
		private bool cachedSharedMaterialSet;

		public Transform CachedTransform
		{
			get
			{
				CacheTransform(); return cachedTransform;
			}
		}

		public SgtSharedMaterial CachedSharedMaterial
		{
			get
			{
				if (cachedSharedMaterialSet == false)
				{
					cachedSharedMaterial    = GetComponent<SgtSharedMaterial>();
					cachedSharedMaterialSet = true;
				}

				return cachedSharedMaterial;
			}
		}

		public float OuterRadius
		{
			get
			{
				return innerMeshRadius + height;
			}
		}

		public Material InnerMaterial
		{
			get
			{
				return innerMaterial;
			}
		}

		public void DirtyMaterials()
		{
			UpdateMaterials();
		}

		protected virtual void UpdateMaterials()
		{
			if (innerMaterial == null)
			{
				innerMaterial = SgtHelper.CreateTempMaterial("Atmosphere Inner (Generated)", SgtHelper.ShaderNamePrefix + "AtmosphereInner");
			}

			if (outerMaterial == null)
			{
				outerMaterial = SgtHelper.CreateTempMaterial("Atmosphere Outer (Generated)", SgtHelper.ShaderNamePrefix + "AtmosphereOuter");
			}

			var finalColor = SgtHelper.Brighten(color, brightness);
			var innerRatio = SgtHelper.Divide(innerMeshRadius, OuterRadius);

			innerMaterial.renderQueue = outerMaterial.renderQueue = renderQueue;

			innerMaterial.SetColor(SgtShader._Color, finalColor);
			outerMaterial.SetColor(SgtShader._Color, finalColor);

			innerMaterial.SetTexture(SgtShader._DepthTex, innerDepthTex);
			outerMaterial.SetTexture(SgtShader._DepthTex, outerDepthTex);

			innerMaterial.SetFloat(SgtShader._InnerRatio, innerRatio);
			innerMaterial.SetFloat(SgtShader._InnerScale, 1.0f / (1.0f - innerRatio));

			if (outerSoftness > 0.0f)
			{
				SgtHelper.EnableKeyword("_SOFTNESS", outerMaterial);

				outerMaterial.SetFloat(SgtShader._SoftParticlesFactor, SgtHelper.Reciprocal(outerSoftness));
			}
			else
			{
				SgtHelper.DisableKeyword("_SOFTNESS", outerMaterial);
			}

			if (lit == true && scattering == true && scatteringHdr == true)
			{
				SgtHelper.EnableKeyword("_HDR", innerMaterial);
				SgtHelper.EnableKeyword("_HDR", outerMaterial);
			}
			else
			{
				SgtHelper.DisableKeyword("_HDR", innerMaterial);
				SgtHelper.DisableKeyword("_HDR", outerMaterial);
			}

			if (lit == true)
			{
				SgtHelper.EnableKeyword("_LIT", innerMaterial);
				SgtHelper.EnableKeyword("_LIT", outerMaterial);

				innerMaterial.SetColor(SgtShader._AmbientColor, SgtHelper.Brighten(ambientColor, ambientBrightness));
				outerMaterial.SetColor(SgtShader._AmbientColor, SgtHelper.Brighten(ambientColor, ambientBrightness));

				innerMaterial.SetTexture(SgtShader._LightingTex, lightingTex);
				outerMaterial.SetTexture(SgtShader._LightingTex, lightingTex);

				if (scattering == true)
				{
					outerMaterial.SetTexture(SgtShader._ScatteringTex, scatteringTex);
					outerMaterial.SetFloat(SgtShader._ScatteringMie, scatteringMie);
					outerMaterial.SetFloat(SgtShader._ScatteringRayleigh, scatteringRayleigh);

					SgtHelper.EnableKeyword("_SCATTERING", outerMaterial);

					if (groundScattering == true)
					{
						innerMaterial.SetTexture(SgtShader._ScatteringTex, scatteringTex);
						innerMaterial.SetFloat(SgtShader._ScatteringMie, scatteringMie);
						innerMaterial.SetFloat(SgtShader._ScatteringRayleigh, scatteringRayleigh);

						SgtHelper.EnableKeyword("_SCATTERING", innerMaterial);
					}
					else
					{
						SgtHelper.DisableKeyword("_SCATTERING", innerMaterial);
					}
				}
				else
				{
					SgtHelper.DisableKeyword("_SCATTERING", innerMaterial);
					SgtHelper.DisableKeyword("_SCATTERING", outerMaterial);
				}
			}
			else
			{
				SgtHelper.DisableKeyword("_LIT", innerMaterial);
				SgtHelper.DisableKeyword("_LIT", outerMaterial);
			}

			CachedSharedMaterial.Material = innerMaterial;
		}

		public static SgtAtmosphere Create(int layer = 0, Transform parent = null)
		{
			return Create(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public static SgtAtmosphere Create(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			return SgtHelper.CreateGameObject("Atmosphere", layer, parent, localPosition, localRotation, localScale).AddComponent<SgtAtmosphere>();
		}

#if UNITY_EDITOR
		[UnityEditor.MenuItem(SgtHelper.GameObjectMenuPrefix + "Atmosphere", false, 10)]
		public static void CreateMenuItem()
		{
			var parent     = SgtHelper.GetSelectedParent();
			var atmosphere = Create(parent != null ? parent.gameObject.layer : 0, parent);

			SgtHelper.SelectAndPing(atmosphere);
		}
#endif

		protected virtual void OnEnable()
		{
			SgtCamera.OnCameraDraw += HandleCameraDraw;

			SgtHelper.OnCalculateOcclusion += HandleCalculateOcclusion;

			CacheTransform();

			DirtyMaterials();
		}

		protected virtual void OnDisable()
		{
			SgtCamera.OnCameraDraw -= HandleCameraDraw;

			SgtHelper.OnCalculateOcclusion -= HandleCalculateOcclusion;

			CachedSharedMaterial.Material = null;
		}

#if UNITY_EDITOR
		protected virtual void Start()
		{
			// Upgrade scene
			// NOTE: This must be done in Start because when done in OnEnable this fails to dirty the scene
			SgtHelper.DestroyOldGameObjects(transform, "Atmosphere Model");
		}
#endif

		protected virtual void LateUpdate()
		{
			var scale        = SgtHelper.Divide(outerMeshRadius, OuterRadius);
			var worldToLocal = Matrix4x4.Scale(new Vector3(scale, scale, scale)) * cachedTransform.worldToLocalMatrix;

			innerMaterial.SetMatrix(SgtShader._WorldToLocal, worldToLocal);
			outerMaterial.SetMatrix(SgtShader._WorldToLocal, worldToLocal);

			// Write lights and shadows
			SgtHelper.SetTempMaterial(innerMaterial, outerMaterial);

			var mask   = 1 << gameObject.layer;
			var lights = SgtLight.Find(lit, mask, cachedTransform.position);

			SgtShadow.Find(lit, mask, lights);
			SgtShadow.FilterOutSphere(cachedTransform.position);
			SgtShadow.WriteSphere(maxSphereShadows);
			SgtShadow.WriteRing(1);

			SgtLight.FilterOut(cachedTransform.position);
			SgtLight.Write(cachedTransform.position, cachedTransform, null, scatteringStrength, maxLights);
		}

		protected virtual void OnDestroy()
		{
			SgtHelper.Destroy(outerMaterial);
			SgtHelper.Destroy(innerMaterial);
		}

		protected virtual void OnDidApplyAnimationProperties()
		{
			DirtyMaterials();
		}

#if UNITY_EDITOR
		protected virtual void OnDrawGizmosSelected()
		{
			if (SgtHelper.Enabled(this) == true)
			{
				var r1 = innerMeshRadius;
				var r2 = OuterRadius;

				SgtHelper.DrawSphere(transform.position, transform.right * transform.lossyScale.x * r1, transform.up * transform.lossyScale.y * r1, transform.forward * transform.lossyScale.z * r1);
				SgtHelper.DrawSphere(transform.position, transform.right * transform.lossyScale.x * r2, transform.up * transform.lossyScale.y * r2, transform.forward * transform.lossyScale.z * r2);
			}
		}
#endif

		private void HandleCameraDraw(Camera camera)
		{
			if (SgtHelper.CanDraw(gameObject, camera) == false) return;

			// Write camera-dependent shader values
			if (innerMaterial != null && outerMaterial != null)
			{
				var localPosition  = cachedTransform.InverseTransformPoint(camera.transform.position);
				var localDistance  = localPosition.magnitude;
				var innerThickness = default(float);
				var outerThickness = default(float);
				var innerRatio     = SgtHelper.Divide(innerMeshRadius, OuterRadius);
				var middleRatio    = Mathf.Lerp(innerRatio, 1.0f, middle);
				var distance       = SgtHelper.Divide(localDistance, OuterRadius);
				var innerDensity   = 1.0f - innerFog;
				var outerDensity   = 1.0f - outerFog;

				SgtHelper.CalculateHorizonThickness(innerRatio, middleRatio, distance, out innerThickness, out outerThickness);

				innerMaterial.SetFloat(SgtShader._HorizonLengthRecip, SgtHelper.Reciprocal(innerThickness * innerDensity));
				outerMaterial.SetFloat(SgtShader._HorizonLengthRecip, SgtHelper.Reciprocal(outerThickness * outerDensity));

				if (outerDepthTex != null)
				{
#if UNITY_EDITOR
					SgtHelper.MakeTextureReadable(outerDepthTex);
#endif
					outerMaterial.SetFloat(SgtShader._Sky, GetSky(camera.transform.position, localDistance));
				}
			}

			var scale  = SgtHelper.Divide(OuterRadius, outerMeshRadius);
			var matrix = cachedTransform.localToWorldMatrix * Matrix4x4.Scale(Vector3.one * scale);

			if (cameraOffset != 0.0f)
			{
				var direction = Vector3.Normalize(camera.transform.position - cachedTransform.position);

				matrix = Matrix4x4.Translate(direction * cameraOffset) * matrix;
			}

			Graphics.DrawMesh(outerMesh, matrix, outerMaterial, gameObject.layer, camera, 0, null, false);
		}

		private float GetSky(Vector3 eye, float localDistance)
		{
			var height01 = Mathf.InverseLerp(OuterRadius, innerMeshRadius, localDistance);
			var mul      = outerDepthTex.GetPixelBilinear(height01 / (1.0f - outerFog), 0.0f).a;

			if (lit == true && night == true)
			{
				var mask   = 1 << gameObject.layer;
				var lights = SgtLight.Find(lit, mask, cachedTransform.position);

				var lighting        = 0.0f;
				var cameraDirection = (eye - cachedTransform.position).normalized;

				for (var i = 0; i < lights.Count && i < 2; i++)
				{
					var light     = lights[i];
					var position  = default(Vector3);
					var direction = default(Vector3);
					var color     = default(Color);
					var intensity = 0.0f;

					SgtLight.Calculate(light, cachedTransform.position, null, null, ref position, ref direction, ref color, ref intensity);

					var dot     = Vector3.Dot(direction, cameraDirection) * 0.5f + 0.5f;
					var night01 = Mathf.InverseLerp(nightEnd, nightStart, dot);
					var night   = SgtEase.Evaluate(nightEase, 1.0f - Mathf.Pow(night01, nightPower));

					if (night > lighting)
					{
						lighting = night;
					}
				}

				return Mathf.Lerp(nightSky, sky, lighting) * mul;
			}
		
			return sky * mul;
		}

		private bool GetPoint(Vector3 ray, Vector3 dir, ref Vector3 point)
		{
			var a = Vector3.Dot(ray, dir);
			var b = Vector3.Dot(ray, ray) - 1.0f;

			if (b <= 0.0f) { point = ray; return true; } // Inside?

			var c = a * a - b;

			if (c < 0.0f) { return false; } // Miss?

			var d = -a - Mathf.Sqrt(c);

			if (d < 0.0f) { return false; } // Behind?

			point = ray + dir * d; return true;
		}

		private bool GetLength(Vector3 ray, Vector3 dir, float len, ref float length)
		{
			var a = default(Vector3);
			var b = default(Vector3);

			if (GetPoint(ray, dir, ref a) == true && GetPoint(ray + dir * len, -dir, ref b) == true)
			{
				length = Vector3.Distance(a, b); return true;
			}

			return false;
		}

		private void HandleCalculateOcclusion(int layers, Vector4 worldEye, Vector4 worldTgt, ref float occlusion)
		{
			if (SgtOcclusion.IsValid(occlusion, layers, gameObject) == true && OuterRadius > 0.0f && outerDepthTex != null)
			{
				SgtOcclusion.TryScaleBackDistantPositions(cachedTransform, ref worldEye, ref worldTgt, OuterRadius);

				var eye    = cachedTransform.InverseTransformPoint(worldEye) / OuterRadius;
				var tgt    = cachedTransform.InverseTransformPoint(worldTgt) / OuterRadius;
				var dir    = Vector3.Normalize(tgt - eye);
				var len    = Vector3.Magnitude(tgt - eye);
				var length = default(float);

				if (GetLength(eye, dir, len, ref length) == true)
				{
					var localPosition  = cachedTransform.InverseTransformPoint(worldEye);
					var localDistance  = localPosition.magnitude;
					var innerThickness = default(float);
					var outerThickness = default(float);
					var innerRatio     = SgtHelper.Divide(innerMeshRadius, OuterRadius);
					var middleRatio    = Mathf.Lerp(innerRatio, 1.0f, middle);
					var distance       = SgtHelper.Divide(localDistance, OuterRadius);
					var outerDensity   = 1.0f - outerFog;

					SgtHelper.CalculateHorizonThickness(innerRatio, middleRatio, distance, out innerThickness, out outerThickness);

					length *= SgtHelper.Reciprocal(outerThickness * outerDensity);

					var depth = outerDepthTex.GetPixelBilinear(length, length).a;

					depth = Mathf.Clamp01(depth + (1.0f - depth) * GetSky(worldEye, localDistance));

					occlusion += (1.0f - occlusion) * depth;
				}
			}
		}

		private void CacheTransform()
		{
			if (cachedTransformSet == false)
			{
				cachedTransform    = GetComponent<Transform>();
				cachedTransformSet = true;
			}
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using TARGET = SgtAtmosphere;

	[UnityEditor.CanEditMultipleObjects]
	[UnityEditor.CustomEditor(typeof(TARGET))]
	public class SgtAtmosphere_Editor : SgtEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			var dirtyMaterials = false;

			Draw("color", ref dirtyMaterials, "The base color will be multiplied by this.");
			BeginError(Any(tgts, t => t.Brightness < 0.0f));
				Draw("brightness", ref dirtyMaterials, "The Color.rgb values are multiplied by this, allowing you to quickly adjust the overall brightness.");
			EndError();
			Draw("renderQueue", ref dirtyMaterials, "This allows you to adjust the render queue of the atmosphere materials. You can normally adjust the render queue in the material settings, but since these materials are procedurally generated your changes will be lost.");

			Separator();

			BeginError(Any(tgts, t => t.InnerDepthTex == null));
				Draw("innerDepthTex", ref dirtyMaterials, "The look up table associating optical depth with atmospheric color for the planet surface. The left side is used when the atmosphere is thin (e.g. center of the planet when looking from space). The right side is used when the atmosphere is thick (e.g. the horizon).");
			EndError();
			BeginError(Any(tgts, t => t.InnerMeshRadius <= 0.0f));
				Draw("innerMeshRadius", ref dirtyMaterials, "The radius of the meshes set in the SgtSharedMaterial.");
			EndError();

			Separator();

			BeginError(Any(tgts, t => t.OuterDepthTex == null));
				Draw("outerDepthTex", ref dirtyMaterials, "The look up table associating optical depth with atmospheric color for the planet sky. The left side is used when the atmosphere is thin (e.g. edge of the atmosphere when looking from space). The right side is used when the atmosphere is thick (e.g. the horizon).");
			EndError();
			BeginError(Any(tgts, t => t.OuterMesh == null));
				Draw("outerMesh", ref dirtyMaterials, "This allows you to set the mesh used to render the atmosphere. This should be a sphere.");
			EndError();
			BeginError(Any(tgts, t => t.OuterMeshRadius <= 0.0f));
				Draw("outerMeshRadius", ref dirtyMaterials, "This allows you to set the radius of the OuterMesh. If this is incorrectly set then the atmosphere will render incorrectly.");
			EndError();
			Draw("outerSoftness", ref dirtyMaterials, "If you have a big object that is both inside and outside of the atmosphere, it can cause a sharp intersection line. Increasing this setting allows you to change the smoothness of this intersection.");

			if (Any(tgts, t => t.OuterSoftness > 0.0f))
			{
				SgtHelper.RequireDepth();
			}

			Separator();

			BeginError(Any(tgts, t => t.Height <= 0.0f));
				Draw("height", ref dirtyMaterials, "This allows you to set how high the atmosphere extends above the surface of the planet in local space.");
			EndError();
			BeginError(Any(tgts, t => t.InnerFog >= 1.0f));
				Draw("innerFog", ref dirtyMaterials, "This allows you to adjust the fog level of the atmosphere on the surface.");
			EndError();
			BeginError(Any(tgts, t => t.OuterFog >= 1.0f));
				Draw("outerFog", ref dirtyMaterials, "This allows you to adjust the fog level of the atmosphere in the sky.");
			EndError();
			BeginError(Any(tgts, t => t.Sky < 0.0f));
				Draw("sky", "This allows you to control how quickly the sky reaches maximum opacity as the camera enters the atmosphere.\n\n1 = You must go down to ground level before the opacity reaches max.\n\n2 = You must go at least half way down to the surface."); // Updated automatically
			EndError();
			Draw("middle", "This allows you to set the altitude where atmospheric density reaches its maximum point. The lower you set this, the foggier the horizon will appear when approaching the surface."); // Updated automatically
			Draw("cameraOffset", "This allows you to offset the camera distance in world space when rendering the atmosphere, giving you fine control over the render order."); // Updated automatically

			Separator();

			Draw("lit", ref dirtyMaterials, "If you enable this then nearby SgtLight and SgtShadow casters will be found and applied to the lighting calculations.");

			if (Any(tgts, t => t.Lit == true))
			{
				if (SgtLight.InstanceCount == 0)
				{
					Warning("You need to add the SgtLight component to your scene lights for them to work with SGT.");
				}

				BeginIndent();
					BeginError(Any(tgts, t => t.LightingTex == null));
						Draw("lightingTex", "The look up table associating light angle with surface color. The left side is used on the dark side, the middle is used on the horizon, and the right side is used on the light side.");
					EndError();
					Draw("ambientColor", ref dirtyMaterials, "The atmosphere will always be lit by this amount.");
					Draw("ambientBrightness", ref dirtyMaterials, "The <b>AmbientColor</b> will be multiplied by this.");
					Draw("maxLights", "The maximum amount of <b>SgtLight</b> components that can light this object."); // Updated automatically
					Draw("maxSphereShadows", "The maximum amount of <b>SgtShadowSphere</b> components that can shade this object."); // Updated automatically
					Draw("scattering", ref dirtyMaterials, "If you enable this then light will scatter through the atmosphere. This means light entering the eye will come from all angles, especially around the light point.");
					if (Any(tgts, t => t.Scattering == true))
					{
						BeginIndent();
							Draw("groundScattering", ref dirtyMaterials, "If you enable this then atmospheric scattering will be applied to the surface material.");
							BeginError(Any(tgts, t => t.ScatteringTex == null));
								Draw("scatteringTex", ref dirtyMaterials, "The look up table associating light angle with scattering color. The left side is used on the dark side, the middle is used on the horizon, and the right side is used on the light side.");
							EndError();
							Draw("scatteringStrength", ref dirtyMaterials, "The scattering is multiplied by this value, allowing you to easily adjust the brightness of the effect.");
							Draw("scatteringMie", ref dirtyMaterials, "The Mie scattering term, allowing you to adjust the distribution of front scattered light.");
							Draw("scatteringRayleigh", ref dirtyMaterials, "The Rayleigh scattering term, allowing you to adjust the distribution of front and back scattered light.");
							Draw("scatteringHdr", ref dirtyMaterials, "Should the scattering make full use of the high range color buffer? Or only fill into the LDR 0..1 (0..255) range?");
						EndIndent();
					}
					Draw("night", "Should the night side of the atmosphere have different sky values?"); // Updated automatically
					if (Any(tgts, t => t.Night == true))
					{
						BeginIndent();
							Draw("nightSky", "The 'Sky' value of the night side."); // Updated automatically
							Draw("nightEase", "The transition style between the day and night."); // Updated automatically
							BeginError(Any(tgts, t => t.NightStart >= t.NightEnd));
								Draw("nightStart", "The start point of the day/sunset transition (0 = dark side, 1 = light side)."); // Updated automatically
								Draw("nightEnd", "The end point of the day/sunset transition (0 = dark side, 1 = light side)."); // Updated automatically
							EndError();
							BeginError(Any(tgts, t => t.NightPower < 1.0f));
								Draw("nightPower", "The power of the night transition."); // Updated automatically
							EndError();
						EndIndent();
					}
				EndIndent();
			}

			if (Any(tgts, t => (t.InnerDepthTex == null || t.OuterDepthTex == null) && t.GetComponent<SgtAtmosphereDepthTex>() == null))
			{
				Separator();

				if (Button("Add InnerDepthTex & OuterDepthTex") == true)
				{
					Each(tgts, t => SgtHelper.GetOrAddComponent<SgtAtmosphereDepthTex>(t.gameObject));
				}
			}

			if (Any(tgts, t => t.Lit == true && t.LightingTex == null && t.GetComponent<SgtAtmosphereLightingTex>() == null))
			{
				Separator();

				if (Button("Add LightingTex") == true)
				{
					Each(tgts, t => SgtHelper.GetOrAddComponent<SgtAtmosphereLightingTex>(t.gameObject));
				}
			}

			if (Any(tgts, t => t.Lit == true && t.Scattering == true && t.ScatteringTex == null && t.GetComponent<SgtAtmosphereScatteringTex>() == null))
			{
				Separator();

				if (Button("Add ScatteringTex") == true)
				{
					Each(tgts, t => SgtHelper.GetOrAddComponent<SgtAtmosphereScatteringTex>(t.gameObject));
				}
			}

			if (Any(tgts, t => SetOuterMeshAndOuterMeshRadius(t, false)))
			{
				Separator();

				if (Button("Set Outer Mesh & Outer Mesh Radius") == true)
				{
					Each(tgts, t => SetOuterMeshAndOuterMeshRadius(t, true));
				}
			}

			if (Any(tgts, t => AddInnerRendererAndSetInnerMeshRadius(t, false)))
			{
				Separator();

				if (Button("Add Inner Renderer & Set Inner Mesh Radius") == true)
				{
					Each(tgts, t => AddInnerRendererAndSetInnerMeshRadius(t, true));
				}
			}

			if (dirtyMaterials == true)
			{
				Each(tgts, t => t.DirtyMaterials(), true, true);
			}
		}

		private bool SetOuterMeshAndOuterMeshRadius(SgtAtmosphere atmosphere, bool apply)
		{
			if (atmosphere.OuterMesh == null)
			{
				var mesh = SgtHelper.LoadFirstAsset<Mesh>("Geosphere40 t:mesh");

				if (mesh != null)
				{
					if (apply == true)
					{
						atmosphere.OuterMesh       = mesh;
						atmosphere.OuterMeshRadius = SgtHelper.GetMeshRadius(mesh);
					}

					return true;
				}
			}

			return false;
		}

		private bool AddInnerRendererAndSetInnerMeshRadius(SgtAtmosphere atmosphere, bool apply)
		{
			if (atmosphere.CachedSharedMaterial.RendererCount == 0)
			{
				var meshRenderer = atmosphere.GetComponentInParent<MeshRenderer>();

				if (meshRenderer != null)
				{
					var meshFilter = meshRenderer.GetComponent<MeshFilter>();

					if (meshFilter != null)
					{
						var mesh = meshFilter.sharedMesh;

						if (mesh != null)
						{
							if (apply == true)
							{
								atmosphere.CachedSharedMaterial.AddRenderer(meshRenderer);
								atmosphere.InnerMeshRadius = SgtHelper.GetMeshRadius(mesh);
							}

							return true;
						}
					}
				}
			}

			return false;
		}
	}
}
#endif