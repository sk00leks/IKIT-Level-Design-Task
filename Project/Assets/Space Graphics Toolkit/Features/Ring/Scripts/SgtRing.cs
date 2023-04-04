using UnityEngine;
using FSA = UnityEngine.Serialization.FormerlySerializedAsAttribute;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to render a planetary ring. This ring can be split into multiple segments to improve depth sorting.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtRing")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Ring")]
	public class SgtRing : MonoBehaviour
	{
		/// <summary>The base color will be multiplied by this.</summary>
		public Color Color { set { if (color != value) { color = value; DirtyMaterial(); } } get { return color; } } [FSA("Color")] [SerializeField] private Color color = Color.white;

		/// <summary>The Color.rgb values are multiplied by this, allowing you to quickly adjust the overall brightness.</summary>
		public float Brightness { set { if (brightness != value) { brightness = value; DirtyMaterial(); } } get { return brightness; } } [FSA("Brightness")] [SerializeField] private float brightness = 1.0f;

		/// <summary>This allows you to adjust the render queue of the ring material. You can normally adjust the render queue in the material settings, but since this material is procedurally generated your changes will be lost.</summary>
		public SgtRenderQueue Rayleigh { set { if (renderQueue != value) { renderQueue = value; DirtyMaterial(); } } get { return renderQueue; } } [FSA("Rayleigh")] [SerializeField] private SgtRenderQueue renderQueue = SgtRenderQueue.GroupType.Transparent;

		/// <summary>The texture applied to the ring, where the left side is the inside, and the right side is the outside.</summary>
		public Texture2D MainTex { set { if (mainTex != value) { mainTex = value; DirtyMaterial(); } } get { return mainTex; } } [FSA("MainTex")] [SerializeField] private Texture2D mainTex;

		/// <summary>This allows you to set the mesh used to render the ring.</summary>
		public Mesh Mesh { set { mesh = value; } get { return mesh; } } [FSA("Mesh")] [SerializeField] private Mesh mesh;

		/// <summary>This allows you to specify how much light this ring can block from light flares.</summary>
		public float Occlusion { set { occlusion = value; } get { return occlusion; } } [SerializeField] private float occlusion;

		/// <summary>This allows you to set how many copies of the Mesh are required to complete the ring. For example, if the Mesh is 1/4 of the ring, then Segments should be set to 4.</summary>
		public int Segments { set { segments = value; } get { return segments; } } [FSA("Segments")] [SerializeField] private int segments = 8;

		/// <summary>Should the ring have a detail texture? For example, dust noise when you get close.</summary>
		public bool Detail { set { if (detail != value) { detail = value; DirtyMaterial(); } } get { return detail; } } [FSA("Detail")] [SerializeField] private bool detail;

		/// <summary>This allows you to set the detail texture that gets repeated on the ring surface.</summary>
		public Texture DetailTex { set { if (detailTex != value) { detailTex = value; DirtyMaterial(); } } get { return detailTex; } } [FSA("DetailTex")] [SerializeField] private Texture detailTex;

		/// <summary>The detail texture horizontal tiling.</summary>
		public float DetailScaleX { set { if (detailScaleX != value) { detailScaleX = value; DirtyMaterial(); } } get { return detailScaleX; } } [FSA("DetailScaleX")] [SerializeField] private float detailScaleX = 1.0f;

		/// <summary>The detail texture vertical tiling.</summary>
		public int DetailScaleY { set { if (detailScaleY != value) { detailScaleY = value; DirtyMaterial(); } } get { return detailScaleY; } } [FSA("DetailScaleY")] [SerializeField] private int detailScaleY = 1;

		/// <summary>The UV offset of the detail texture.</summary>
		public Vector2 DetailOffset { set { if (detailOffset != value) { detailOffset = value; DirtyMaterial(); } } get { return detailOffset; } } [FSA("DetailOffset")] [SerializeField] private Vector2 detailOffset;

		/// <summary>The scroll speed of the detail texture UV offset.</summary>
		public Vector2 DetailSpeed { set { if (detailSpeed != value) { detailSpeed = value; DirtyMaterial(); } } get { return detailSpeed; } } [FSA("DetailSpeed")] [SerializeField] private Vector2 detailSpeed;

		/// <summary>The amount the detail texture is twisted around the ring.</summary>
		public float DetailTwist { set { if (detailTwist != value) { detailTwist = value; DirtyMaterial(); } } get { return detailTwist; } } [FSA("DetailTwist")] [SerializeField] private float detailTwist;

		/// <summary>The amount the twisting is pushed to the outer edge.</summary>
		public float DetailTwistBias { set { if (detailTwistBias != value) { detailTwistBias = value; DirtyMaterial(); } } get { return detailTwistBias; } } [FSA("DetailTwistBias")] [SerializeField] private float detailTwistBias = 1.0f;

		/// <summary>Enable this if you want the ring to fade out as the camera approaches.</summary>
		public bool Near { set { if (near != value) { near = value; DirtyMaterial(); } } get { return near; } } [FSA("Near")] [SerializeField] private bool near;

		/// <summary>The lookup table used to calculate the fade opacity based on distance, where the left side is used when the camera is close, and the right side is used when the camera is far.</summary>
		public Texture NearTex { set { if (nearTex != value) { nearTex = value; DirtyMaterial(); } } get { return nearTex; } } [FSA("NearTex")] [SerializeField] private Texture nearTex;

		/// <summary>The distance the fading begins from in world space.</summary>
		public float NearDistance { set { if (nearDistance != value) { nearDistance = value; DirtyMaterial(); } } get { return nearDistance; } } [FSA("NearDistance")] [SerializeField] private float nearDistance = 1.0f;

		/// <summary>If you enable this then light will scatter through the ring atmosphere. This means light entering the eye will come from all angles, especially around the light point.</summary>
		public bool Scattering { set { if (scattering != value) { scattering = value; DirtyMaterial(); } } get { return scattering; } } [FSA("Scattering")] [SerializeField] private bool scattering;

		/// <summary>The mie scattering term, allowing you to adjust the distribution of front scattered light.</summary>
		public float ScatteringMie { set { if (scatteringMie != value) { scatteringMie = value; DirtyMaterial(); } } get { return scatteringMie; } } [FSA("ScatteringMie")] [SerializeField] private float scatteringMie = 8.0f;

		/// <summary>The scattering is multiplied by this value, allowing you to easily adjust the brightness of the effect.</summary>
		public float ScatteringStrength { set { if (scatteringStrength != value) { scatteringStrength = value; DirtyMaterial(); } } get { return scatteringStrength; } } [FSA("ScatteringStrength")] [SerializeField] private float scatteringStrength = 25.0f;

		/// <summary>If you enable this then nearby SgtLight and SgtShadow casters will be found and applied to the lighting calculations.</summary>
		public bool Lit { set { if (lit != value) { lit = value; DirtyMaterial(); } } get { return lit; } } [FSA("Lit")] [SerializeField] private bool lit;

		/// <summary>The ring will always be lit by this amount.</summary>
		public Color AmbientColor { set { if (ambientColor != value) { ambientColor = value; DirtyMaterial(); } } get { return ambientColor; } } [FSA("AmbientColor")] [SerializeField] private Color ambientColor;

		/// <summary>The <b>AmbientColor</b> will be multiplied by this.</summary>
		public float AmbientBrightness { set { if (ambientBrightness != value) { ambientBrightness = value; DirtyMaterial(); } } get { return ambientBrightness; } } [SerializeField] private float ambientBrightness = 1.0f;

		/// <summary>The maximum amount of <b>SgtLight</b> components that can light this object.</summary>
		public int MaxLights { set { if (maxLights != value) { maxLights = value; DirtyMaterial(); } } get { return maxLights; } } [SerializeField] [Range(0, SgtLight.MAX_LIGHTS)] private int maxLights = 2;

		/// <summary>The maximum amount of <b>SgtShadowSphere</b> components that can shade this object.</summary>
		public int MaxSphereShadows { set { if (maxSphereShadows != value) { maxSphereShadows = value; DirtyMaterial(); } } get { return maxSphereShadows; } } [SerializeField] [Range(0, SgtShadow.MAX_SPHERE_SHADOWS)] private int maxSphereShadows = 2;

		/// <summary>The look up table associating light angle with surface color. The left side is used on the dark side, the middle is used on the horizon, and the right side is used on the light side.</summary>
		public Texture LightingTex { set { if (lightingTex != value) { lightingTex = value; DirtyMaterial(); } } get { return lightingTex; } } [FSA("LightingTex")] [SerializeField] private Texture lightingTex;

		// The material applied to all models
		[System.NonSerialized]
		private Material material;

		[System.NonSerialized]
		private SgtRingMesh cachedRingMesh;

		public void DirtyMaterial()
		{
			UpdateMaterial();
		}

		[ContextMenu("Update Material")]
		public virtual void UpdateMaterial()
		{
			if (material == null)
			{
				material = SgtHelper.CreateTempMaterial("Ring (Generated)", SgtHelper.ShaderNamePrefix + "Ring");
			}

			var color = SgtHelper.Brighten(this.color, brightness);

			material.renderQueue = renderQueue;

			material.SetColor(SgtShader._Color, color);
			material.SetTexture(SgtShader._MainTex, mainTex);

			if (detail == true)
			{
				SgtHelper.EnableKeyword("_DETAIL", material);

				material.SetTexture(SgtShader._DetailTex, detailTex);
				material.SetVector(SgtShader._DetailOffset, detailOffset);
				material.SetVector(SgtShader._DetailScale, new Vector2(detailScaleX, detailScaleY));
				material.SetFloat(SgtShader._DetailTwist, detailTwist);
				material.SetFloat(SgtShader._DetailTwistBias, detailTwistBias);
			}
			else
			{
				SgtHelper.DisableKeyword("_DETAIL", material);
			}

			if (near == true)
			{
				SgtHelper.EnableKeyword("_NEAR", material);

				material.SetTexture(SgtShader._NearTex, nearTex);
				material.SetFloat(SgtShader._NearScale, SgtHelper.Reciprocal(nearDistance));
			}
			else
			{
				SgtHelper.DisableKeyword("_NEAR", material);
			}

			if (lit == true)
			{
				SgtHelper.EnableKeyword("_LIT", material);

				material.SetTexture(SgtShader._LightingTex, lightingTex);
				material.SetColor(SgtShader._AmbientColor, SgtHelper.Brighten(ambientColor, ambientBrightness));
			}
			else
			{
				SgtHelper.DisableKeyword("_LIT", material);
			}

			if (scattering == true)
			{
				SgtHelper.EnableKeyword("_SCATTERING", material);

				material.SetFloat(SgtShader._ScatteringMie, scatteringMie * scatteringMie);
			}
			else
			{
				SgtHelper.DisableKeyword("_SCATTERING", material);
			}
		}

		public static SgtRing Create(int layer = 0, Transform parent = null)
		{
			return Create(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public static SgtRing Create(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			return SgtHelper.CreateGameObject("Ring", layer, parent, localPosition, localRotation, localScale).AddComponent<SgtRing>();
		}

#if UNITY_EDITOR
		[UnityEditor.MenuItem(SgtHelper.GameObjectMenuPrefix + "Ring", false, 10)]
		public static void CreateMenuItem()
		{
			var parent = SgtHelper.GetSelectedParent();
			var ring   = Create(parent != null ? parent.gameObject.layer : 0, parent);

			SgtHelper.SelectAndPing(ring);
		}
#endif

		protected virtual void OnEnable()
		{
			SgtCamera.OnCameraDraw += HandleCameraDraw;

			SgtHelper.OnCalculateOcclusion += CalculateOcclusion;

			UpdateMaterial();
		}

		protected virtual void OnDisable()
		{
			SgtCamera.OnCameraDraw -= HandleCameraDraw;

			SgtHelper.OnCalculateOcclusion -= CalculateOcclusion;
		}

		protected virtual void LateUpdate()
		{
			// Write lights and shadows
			SgtHelper.SetTempMaterial(material);

			var mask   = 1 << gameObject.layer;
			var lights = SgtLight.Find(lit, mask, transform.position);

			SgtShadow.Find(lit, mask, lights);
			SgtShadow.FilterOutRing(transform.position);
			SgtShadow.WriteSphere(maxSphereShadows);
			SgtShadow.WriteRing(1);

			SgtLight.FilterOut(transform.position);
			SgtLight.Write(transform.position, null, null, scatteringStrength, maxLights);

			// Update scrolling?
			if (detail == true)
			{
				if (Application.isPlaying == true)
				{
					detailOffset += detailSpeed * Time.deltaTime;
				}

				if (material != null)
				{
					material.SetVector(SgtShader._DetailOffset, detailOffset);
				}
			}
		}

#if UNITY_EDITOR
		protected virtual void Start()
		{
			// Upgrade scene
			// NOTE: This must be done in Start because when done in OnEnable this fails to dirty the scene
			SgtHelper.DestroyOldGameObjects(transform, "Ring Model");
		}
#endif

		protected virtual void OnDestroy()
		{
			SgtHelper.Destroy(material);
		}

		private void HandleCameraDraw(Camera camera)
		{
			if (SgtHelper.CanDraw(gameObject, camera) == false) return;

			if (segments > 0)
			{
				var matrix   = transform.localToWorldMatrix;
				var rotation = Matrix4x4.Rotate(Quaternion.Euler(0.0f, 360.0f / segments, 0.0f));

				for (var i = 0; i < segments; i++)
				{
					Graphics.DrawMesh(mesh, matrix, material, gameObject.layer, camera, 0, null, false);

					matrix *= rotation;
				}
			}
		}

		private void CalculateOcclusion(int layers, Vector4 eye, Vector4 tgt, ref float flareOcclusion)
		{
			if (occlusion > 0.0f && mainTex != null && SgtOcclusion.IsValid(flareOcclusion, layers, gameObject) == true)
			{
				if (cachedRingMesh == null)
				{
					cachedRingMesh = GetComponent<SgtRingMesh>();
				}

				if (cachedRingMesh != null)
				{
					var plane = new Plane(transform.up, transform.position);
					var ray   = new Ray(eye, tgt - eye);
					var dist  = default(float);

					if (plane.Raycast(ray, out dist) == true)
					{
						var point = transform.InverseTransformPoint(ray.GetPoint(dist));
						var u     = Mathf.InverseLerp(cachedRingMesh.RadiusMin, cachedRingMesh.RadiusMax, Vector3.Magnitude(point));

						flareOcclusion += mainTex.GetPixelBilinear(u, 0.5f).a * Mathf.Clamp01(occlusion) * (1.0f - flareOcclusion);
					}
				}
			}
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using TARGET = SgtRing;

	[UnityEditor.CanEditMultipleObjects]
	[UnityEditor.CustomEditor(typeof(TARGET))]
	public class SgtRing_Editor : SgtEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			var dirtyMaterial = false;

			Draw("color", ref dirtyMaterial, "The base color will be multiplied by this.");
			BeginError(Any(tgts, t => t.Brightness < 0.0f));
				Draw("brightness", ref dirtyMaterial, "The Color.rgb values are multiplied by this, allowing you to quickly adjust the overall brightness.");
			EndError();
			Draw("renderQueue", ref dirtyMaterial, "This allows you to adjust the render queue of the ring material. You can normally adjust the render queue in the material settings, but since this material is procedurally generated your changes will be lost.");

			Separator();

			BeginError(Any(tgts, t => t.MainTex == null));
				Draw("mainTex", ref dirtyMaterial, "The texture applied to the ring, where the left side is the inside, and the right side is the outside.");
			EndError();

			BeginError(Any(tgts, t => t.Segments < 1));
				Draw("segments", "This allows you to set how many copies of the Mesh are required to complete the ring. For example, if the Mesh is 1/4 of the ring, then Segments should be set to 4.");
			EndError();
			Draw("occlusion", "This allows you to specify how much light this ring can block from light flares.");
			BeginError(Any(tgts, t => t.Mesh == null));
				Draw("mesh", "This allows you to set the mesh used to render the ring.");
			EndError();

			Separator();

			Draw("detail", ref dirtyMaterial, "Should the ring have a detail texture? For example, dust noise when you get close.");

			if (Any(tgts, t => t.Detail == true))
			{
				BeginIndent();
					BeginError(Any(tgts, t => t.DetailTex == null));
						Draw("detailTex", ref dirtyMaterial, "This allows you to set the detail texture that gets repeated on the ring surface.");
					EndError();
					BeginError(Any(tgts, t => t.DetailScaleX < 0.0f));
						Draw("detailScaleX", ref dirtyMaterial, "The detail texture horizontal tiling.");
					EndError();
					BeginError(Any(tgts, t => t.DetailScaleY < 1));
						Draw("detailScaleY", ref dirtyMaterial, "The detail texture vertical tiling.");
					EndError();
					Draw("detailOffset", ref dirtyMaterial, "The UV offset of the detail texture.");
					Draw("detailSpeed", ref dirtyMaterial, "The scroll speed of the detail texture UV offset.");
					Draw("detailTwist", ref dirtyMaterial, "The amount the detail texture is twisted around the ring.");
					BeginError(Any(tgts, t => t.DetailTwistBias < 1.0f));
						Draw("detailTwistBias", ref dirtyMaterial, "The amount the twisting is pushed to the outer edge.");
					EndError();
				EndIndent();
			}

			Separator();

			Draw("near", ref dirtyMaterial, "Enable this if you want the ring to fade out as the camera approaches.");

			if (Any(tgts, t => t.Near == true))
			{
				BeginIndent();
					BeginError(Any(tgts, t => t.NearTex == null));
						Draw("nearTex", ref dirtyMaterial, "The lookup table used to calculate the fade opacity based on distance, where the left side is used when the camera is close, and the right side is used when the camera is far.");
					EndError();
					BeginError(Any(tgts, t => t.NearDistance <= 0.0f));
						Draw("nearDistance", ref dirtyMaterial, "The distance the fading begins from in world space.");
					EndError();
				EndIndent();
			}

			Separator();

			Draw("lit", ref dirtyMaterial, "If you enable this then nearby SgtLight and SgtShadow casters will be found and applied to the lighting calculations.");

			if (Any(tgts, t => t.Lit == true))
			{
				if (SgtLight.InstanceCount == 0)
				{
					Warning("You need to add the SgtLight component to your scene lights for them to work with SGT.");
				}

				BeginIndent();
					Draw("ambientColor", ref dirtyMaterial, "The ring will always be lit by this amount.");
					Draw("ambientBrightness", ref dirtyMaterial, "The <b>AmbientColor</b> will be multiplied by this.");
					Draw("maxLights", "The maximum amount of <b>SgtLight</b> components that can light this object."); // Updated automatically
					Draw("maxSphereShadows", "The maximum amount of <b>SgtShadowSphere</b> components that can shade this object."); // Updated automatically
					BeginError(Any(tgts, t => t.LightingTex == null));
						Draw("lightingTex", ref dirtyMaterial, "The look up table associating light angle with surface color. The left side is used on the dark side, the middle is used on the horizon, and the right side is used on the light side.");
					EndError();
					Draw("scattering", ref dirtyMaterial, "If you enable this then light will scatter through the ring atmosphere. This means light entering the eye will come from all angles, especially around the light point.");

					if (Any(tgts, t => t.Scattering == true))
					{
						BeginIndent();
							BeginError(Any(tgts, t => t.ScatteringMie <= 0.0f));
								Draw("scatteringMie", ref dirtyMaterial, "The mie scattering term, allowing you to adjust the distribution of front scattered light.");
							EndError();
							Draw("scatteringStrength", "The scattering is multiplied by this value, allowing you to easily adjust the brightness of the effect."); // Updated in LateUpdate
						EndIndent();
					}
				EndIndent();
			}

			if (Any(tgts, t => t.Mesh == null && t.GetComponent<SgtRingMesh>() == null))
			{
				Separator();

				if (Button("Add Mesh") == true)
				{
					Each(tgts, t => SgtHelper.GetOrAddComponent<SgtRingMesh>(t.gameObject));
				}
			}

			if (Any(tgts, t => t.Near == true && t.NearTex == null && t.GetComponent<SgtRingNearTex>() == null))
			{
				Separator();

				if (Button("Add NearTex") == true)
				{
					Each(tgts, t => SgtHelper.GetOrAddComponent<SgtRingNearTex>(t.gameObject));
				}
			}

			if (Any(tgts, t => t.Lit == true && t.LightingTex == null && t.GetComponent<SgtRingLightingTex>() == null))
			{
				Separator();

				if (Button("Add LightingTex") == true)
				{
					Each(tgts, t => SgtHelper.GetOrAddComponent<SgtRingLightingTex>(t.gameObject));
				}
			}

			if (dirtyMaterial == true)
			{
				Each(tgts, t => t.DirtyMaterial(), true, true);
			}
		}
	}
}
#endif