using UnityEngine;
using FSA = UnityEngine.Serialization.FormerlySerializedAsAttribute;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to render volumetric jovian (gas giant) planets.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtJovian")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Jovian")]
	public class SgtJovian : MonoBehaviour
	{
		/// <summary>The base color will be multiplied by this.</summary>
		public Color Color { set { if (color != value) { color = value; DirtyMaterial(); } } get { return color; } } [FSA("Color")] [SerializeField] private Color color = Color.white;

		/// <summary>The Color.rgb values are multiplied by this, allowing you to quickly adjust the overall brightness.</summary>
		public float Brightness { set { if (brightness != value) { brightness = value; DirtyMaterial(); } } get { return brightness; } } [FSA("Brightness")] [SerializeField] private float brightness = 1.0f;

		/// <summary>This allows you to adjust the render queue of the jovian material. You can normally adjust the render queue in the material settings, but since this material is procedurally generated your changes will be lost.</summary>
		public SgtRenderQueue RenderQueue { set { if (renderQueue != value) { renderQueue = value; DirtyMaterial(); } } get { return renderQueue; } } [FSA("RenderQueue")] [SerializeField] private SgtRenderQueue renderQueue = SgtRenderQueue.GroupType.Transparent;

		/// <summary>This allows you to offset the camera distance in world space when rendering the jovian, giving you fine control over the render order.</summary>
		public float CameraOffset { set { cameraOffset = value; } get { return cameraOffset; } } [SerializeField] private float cameraOffset;

		/// <summary>The cube map used as the base texture for the jovian.</summary>
		public Cubemap MainTex { set { if (mainTex != value) { mainTex = value; DirtyMaterial(); } } get { return mainTex; } } [FSA("MainTex")] [SerializeField] private Cubemap mainTex;

		/// <summary>The look up table associating optical depth with atmosphere color. The left side is used when the atmosphere is thin (e.g. edge of the jovian when looking from space). The right side is used when the atmosphere is thick (e.g. the center of the jovian when looking from space).</summary>
		public Texture2D DepthTex { set { if (depthTex != value) { depthTex = value; DirtyMaterial(); } } get { return depthTex; } } [FSA("DepthTex")] [SerializeField] private Texture2D depthTex;

		/// <summary>The cube map used to define how the <b>MainTex</b> should flow across the surface of the jovian.</summary>
		public Cubemap FlowTex { set { if (flowTex != value) { flowTex = value; DirtyMaterial(); } } get { return flowTex; } } [SerializeField] private Cubemap flowTex;

		/// <summary>The speed of the jovian texture flow.</summary>
		public float FlowSpeed { set { if (flowSpeed != value) { flowSpeed = value; DirtyMaterial(); } } get { return flowSpeed; } } [SerializeField] private float flowSpeed = 1.0f;

		/// <summary>The maximum distance the jovian texture can flow in UV space.</summary>
		public float FlowStrength { set { if (flowStrength != value) { flowStrength = value; DirtyMaterial(); } } get { return flowStrength; } } [SerializeField] private float flowStrength = 0.01f;

		/// <summary>The flow texture timing will be offset using noise, and this setting allows you to specify the resolution of that noise.</summary>
		public float FlowNoiseTiling { set { if (flowNoiseTiling != value) { flowNoiseTiling = value; DirtyMaterial(); } } get { return flowNoiseTiling; } } [SerializeField] private float flowNoiseTiling = 1.0f;

		/// <summary>This allows you to control how thick the atmosphere is when the camera is inside its radius.</summary>
		public float Sky { set { if (sky != value) { sky = value; DirtyMaterial(); } } get { return sky; } } [FSA("Sky")] [SerializeField] private float sky = 1.0f;

		/// <summary>If you enable this then nearby SgtLight and SgtShadow casters will be found and applied to the lighting calculations.</summary>
		public bool Lit { set { if (lit != value) { lit = value; DirtyMaterial(); } } get { return lit; } } [FSA("Lit")] [SerializeField] private bool lit;

		/// <summary>The jovian will always be lit by this amount.</summary>
		public Color AmbientColor { set { if (ambientColor != value) { ambientColor = value; DirtyMaterial(); } } get { return ambientColor; } } [FSA("AmbientColor")] [SerializeField] private Color ambientColor;

		/// <summary>The <b>AmbientColor</b> will be multiplied by this.</summary>
		public float AmbientBrightness { set { if (ambientBrightness != value) { ambientBrightness = value; DirtyMaterial(); } } get { return ambientBrightness; } } [SerializeField] private float ambientBrightness = 1.0f;

		/// <summary>The maximum amount of <b>SgtLight</b> components that can light this object.</summary>
		public int MaxLights { set { if (maxLights != value) { maxLights = value; DirtyMaterial(); } } get { return maxLights; } } [SerializeField] [Range(0, SgtLight.MAX_LIGHTS)] private int maxLights = 2;

		/// <summary>The maximum amount of <b>SgtShadowSphere</b> components that can shade this object.</summary>
		public int MaxSphereShadows { set { if (maxSphereShadows != value) { maxSphereShadows = value; DirtyMaterial(); } } get { return maxSphereShadows; } } [SerializeField] [Range(0, SgtShadow.MAX_SPHERE_SHADOWS)] private int maxSphereShadows = 2;

		/// <summary>The look up table associating light angle with surface color. The left side is used on the dark side, the middle is used on the horizon, and the right side is used on the light side.</summary>
		public Texture LightingTex { set { if (lightingTex != value) { lightingTex = value; DirtyMaterial(); } } get { return lightingTex; } } [FSA("LightingTex")] [SerializeField] private Texture lightingTex;

		/// <summary>If you enable this then light will scatter through the jovian atmosphere. This means light entering the eye will come from all angles, especially around the light point.</summary>
		public bool Scattering { set { if (scattering != value) { scattering = value; DirtyMaterial(); } } get { return scattering; } } [FSA("Scattering")] [SerializeField] private bool scattering;

		/// <summary>The look up table associating light angle with scattering color. The left side is used on the dark side, the middle is used on the horizon, and the right side is used on the light side.</summary>
		public Texture ScatteringTex { set { if (scatteringTex != value) { scatteringTex = value; DirtyMaterial(); } } get { return scatteringTex; } } [FSA("ScatteringTex")] [SerializeField] private Texture scatteringTex;

		/// <summary>The scattering is multiplied by this value, allowing you to easily adjust the brightness of the effect.</summary>
		public float ScatteringStrength { set { if (scatteringStrength != value) { scatteringStrength = value; DirtyMaterial(); } } get { return scatteringStrength; } } [FSA("ScatteringStrength")] [SerializeField] private float scatteringStrength = 3.0f;

		/// <summary>This allows you to set the mesh used to render the jovian. This should be a sphere.</summary>
		public Mesh Mesh { set { if (mesh != value) { mesh = value; DirtyMaterial(); } } get { return mesh; } } [FSA("Mesh")] [SerializeField] private Mesh mesh;

		/// <summary>This allows you to set the radius of the Mesh. If this is incorrectly set then the jovian will render incorrectly.</summary>
		public float MeshRadius { set { if (meshRadius != value) { meshRadius = value; DirtyMaterial(); } } get { return meshRadius; } } [FSA("MeshRadius")] [SerializeField] private float meshRadius = 1.0f;

		/// <summary>This allows you to set the radius of the jovian in local space.</summary>
		public float Radius { set { if (radius != value) { radius = value; DirtyMaterial(); } } get { return radius; } } [FSA("Radius")] [SerializeField] private float radius = 1.0f;

		/// <summary>This allows you to set the radius of the jovian in local space when using distance calculations. Distance calculations normally tell you how far from the surface of a planet you are, but since a jovian isn't solid, you may want to customize this based on your project.</summary>
		public float DistanceRadius { set { if (distanceRadius != value) { distanceRadius = value; DirtyMaterial(); } } get { return distanceRadius; } } [SerializeField] private float distanceRadius = 0.9f;

		/// <summary>The temporary material rendering the jovian.</summary>
		[System.NonSerialized]
		private Material material;

		[System.NonSerialized]
		private bool dirtyMaterial = true;

		[System.NonSerialized]
		private Transform cachedTransform;

		[System.NonSerialized]
		private bool cachedTransformSet;

		public Transform CachedTransform
		{
			get
			{
				CacheTransform(); return cachedTransform;
			}
		}

		public void DirtyMaterial()
		{
			UpdateMaterial();
		}

		private void UpdateMaterial()
		{
			if (material == null)
			{
				material = SgtHelper.CreateTempMaterial("Jovian Material (Generated)", SgtHelper.ShaderNamePrefix + "Jovian");
			}

			material.renderQueue = renderQueue;

			material.SetTexture(SgtShader._CubeTex, mainTex);
			material.SetTexture(SgtShader._FlowTex, flowTex);
			material.SetFloat(SgtShader._FlowSpeed, flowSpeed);
			material.SetFloat(SgtShader._FlowStrength, flowStrength);
			material.SetFloat(SgtShader._FlowNoiseTiling, flowNoiseTiling);
			material.SetTexture(SgtShader._DepthTex, depthTex);
			material.SetColor(SgtShader._Color, SgtHelper.Brighten(color, brightness));

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

			SgtHelper.SetTempMaterial(material);

			if (scattering == true)
			{
				material.SetTexture(SgtShader._ScatteringTex, scatteringTex);

				SgtHelper.EnableKeyword("_SCATTERING");
			}
			else
			{
				SgtHelper.DisableKeyword("_SCATTERING");
			}
		}

		public static SgtJovian Create(int layer = 0, Transform parent = null)
		{
			return Create(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public static SgtJovian Create(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			return SgtHelper.CreateGameObject("Jovian", layer, parent, localPosition, localRotation, localScale).AddComponent<SgtJovian>();
		}

#if UNITY_EDITOR
		[UnityEditor.MenuItem(SgtHelper.GameObjectMenuPrefix + "Jovian", false, 10)]
		public static void CreateMenuItem()
		{
			var parent = SgtHelper.GetSelectedParent();
			var jovian = Create(parent != null ? parent.gameObject.layer : 0, parent);

			SgtHelper.SelectAndPing(jovian);
		}
#endif

		protected virtual void OnEnable()
		{
			SgtCamera.OnCameraDraw += HandleCameraDraw;

			SgtHelper.OnCalculateOcclusion += HandleCalculateOcclusion;

			SgtHelper.OnCalculateDistance += HandleCalculateDistance;

			CacheTransform();
		}

		protected virtual void OnDisable()
		{
			SgtCamera.OnCameraDraw -= HandleCameraDraw;

			SgtHelper.OnCalculateOcclusion -= HandleCalculateOcclusion;

			SgtHelper.OnCalculateDistance -= HandleCalculateDistance;
		}

#if UNITY_EDITOR
		protected virtual void Start()
		{
			// Upgrade scene
			// NOTE: This must be done in Start because when done in OnEnable this fails to dirty the scene
			SgtHelper.DestroyOldGameObjects(transform, "Jovian Model");
		}
#endif

		protected virtual void LateUpdate()
		{
			if (dirtyMaterial == true)
			{
				DirtyMaterial(); dirtyMaterial = false;
			}

			// Write lights and shadows
			SgtHelper.SetTempMaterial(material);

			var mask   = 1 << gameObject.layer;
			var lights = SgtLight.Find(lit, mask, transform.position);

			SgtLight.FilterOut(transform.position);

			SgtShadow.Find(lit, mask, lights);
			SgtShadow.FilterOutSphere(transform.position);
			SgtShadow.WriteSphere(maxSphereShadows);
			SgtShadow.WriteRing(1);

			SgtLight.Write(transform.position, transform, null, scatteringStrength, maxLights);

			// Write matrices
			var scale        = radius;
			var localToWorld = transform.localToWorldMatrix * Matrix4x4.Scale(new Vector3(scale, scale, scale)); // Double mesh radius so the max thickness caps at 1.0

			material.SetMatrix(SgtShader._WorldToLocal, localToWorld.inverse);
			material.SetMatrix(SgtShader._LocalToWorld, localToWorld);
		}

		protected virtual void OnDestroy()
		{
			SgtHelper.Destroy(material);
		}

		protected virtual void OnDidApplyAnimationProperties()
		{
			DirtyMaterial();
		}

#if UNITY_EDITOR
		protected virtual void OnDrawGizmosSelected()
		{
			if (SgtHelper.Enabled(this) == true)
			{
				var r0 = transform.lossyScale;

				SgtHelper.DrawSphere(transform.position, transform.right * r0.x, transform.up * r0.y, transform.forward * r0.z);
			}
		}
#endif

		private void HandleCameraDraw(Camera camera)
		{
			if (SgtHelper.CanDraw(gameObject, camera) == false) return;

			if (depthTex != null)
			{
#if UNITY_EDITOR
				SgtHelper.MakeTextureReadable(depthTex);
#endif
				material.SetFloat(SgtShader._Sky, GetSky(camera.transform.position));
			}

			var scale  = SgtHelper.Divide(radius, meshRadius);
			var matrix = transform.localToWorldMatrix * Matrix4x4.Scale(Vector3.one * scale);

			if (cameraOffset != 0.0f)
			{
				var direction = Vector3.Normalize(camera.transform.position - transform.position);

				matrix = Matrix4x4.Translate(direction * cameraOffset) * matrix;
			}

			Graphics.DrawMesh(mesh, matrix, material, gameObject.layer, camera);
		}

		private float GetSky(Vector3 eye)
		{
			var localCameraPosition = transform.InverseTransformPoint(eye);
			var localDistance       = localCameraPosition.magnitude;
			var scaleDistance       = SgtHelper.Divide(localDistance, radius);

			return sky * depthTex.GetPixelBilinear(1.0f - scaleDistance, 0.0f).a;
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
			if (SgtOcclusion.IsValid(occlusion, layers, gameObject) == true && radius > 0.0f && depthTex != null)
			{
				SgtOcclusion.TryScaleBackDistantPositions(cachedTransform, ref worldEye, ref worldTgt, radius);

				var eye    = transform.InverseTransformPoint(worldEye) / radius;
				var tgt    = transform.InverseTransformPoint(worldTgt) / radius;
				var dir    = Vector3.Normalize(tgt - eye);
				var len    = Vector3.Magnitude(tgt - eye);
				var length = default(float);

				if (GetLength(eye, dir, len, ref length) == true)
				{
					var depth = depthTex.GetPixelBilinear(length, length).a;

					depth = Mathf.Clamp01(depth + (1.0f - depth) * GetSky(eye));

					occlusion += (1.0f - occlusion) * depth;
				}
			}
		}

		private void HandleCalculateDistance(Vector3 worldPosition, ref float distance)
		{
			var localPosition = transform.InverseTransformPoint(worldPosition);

			localPosition = localPosition.normalized * distanceRadius;

			var surfacePosition = transform.TransformPoint(localPosition);
			var thisDistance    = Vector3.Distance(worldPosition, surfacePosition);

			if (thisDistance < distance)
			{
				distance = thisDistance;
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
	using TARGET = SgtJovian;

	[UnityEditor.CanEditMultipleObjects]
	[UnityEditor.CustomEditor(typeof(TARGET))]
	public class SgtJovian_Editor : SgtEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			var dirtyMaterial = false;

			Draw("color", ref dirtyMaterial, "The base color will be multiplied by this.");
			BeginError(Any(tgts, t => t.Brightness < 0.0f));
				Draw("brightness", ref dirtyMaterial, "The Color.rgb values are multiplied by this, allowing you to quickly adjust the overall brightness.");
			EndError();
			Draw("renderQueue", ref dirtyMaterial, "This allows you to adjust the render queue of the jovian material. You can normally adjust the render queue in the material settings, but since this material is procedurally generated your changes will be lost.");
			Draw("cameraOffset", "This allows you to offset the camera distance in world space when rendering the jovian, giving you fine control over the render order."); // Updated automatically

			Separator();

			BeginError(Any(tgts, t => t.MainTex == null));
				Draw("mainTex", ref dirtyMaterial, "The cube map used as the base texture for the jovian.");
			EndError();
			BeginError(Any(tgts, t => t.DepthTex == null));
				Draw("depthTex", ref dirtyMaterial, "The look up table associating optical depth with atmosphere color. The left side is used when the atmosphere is thin (e.g. edge of the jovian when looking from space). The right side is used when the atmosphere is thick (e.g. the center of the jovian when looking from space).");
			EndError();
			Draw("flowTex", ref dirtyMaterial, "The cube map used to define how the <b>MainTex</b> should flow across the surface of the jovian.");
			Draw("flowSpeed", ref dirtyMaterial, "The speed of the jovian texture flow.");
			Draw("flowStrength", ref dirtyMaterial, "The maximum distance the jovian texture can flow in UV space.");
			Draw("flowNoiseTiling", ref dirtyMaterial, "The flow texture timing will be offset using noise, and this setting allows you to specify the resolution of that noise.");

			Separator();

			BeginError(Any(tgts, t => t.Sky < 0.0f));
				Draw("sky", ref dirtyMaterial, "This allows you to control how thick the atmosphere is when the camera is inside its radius"); // Updated when rendering
			EndError();

			Draw("lit", ref dirtyMaterial, "If you enable this then nearby SgtLight and SgtShadow casters will be found and applied to the lighting calculations.");

			if (Any(tgts, t => t.Lit == true))
			{
				if (SgtLight.InstanceCount == 0)
				{
					Warning("You need to add the SgtLight component to your scene lights for them to work with SGT.");
				}

				BeginIndent();
					Draw("ambientColor", ref dirtyMaterial, "The jovian will always be lit by this amount.");
					Draw("ambientBrightness", ref dirtyMaterial, "The <b>AmbientColor</b> will be multiplied by this.");
					Draw("maxLights", "The maximum amount of <b>SgtLight</b> components that can light this object."); // Updated automatically
					Draw("maxSphereShadows", "The maximum amount of <b>SgtShadowSphere</b> components that can shade this object."); // Updated automatically
					BeginError(Any(tgts, t => t.LightingTex == null));
						Draw("lightingTex", ref dirtyMaterial, "The look up table associating light angle with surface color. The left side is used on the dark side, the middle is used on the horizon, and the right side is used on the light side.");
					EndError();
					Draw("scattering", ref dirtyMaterial, "If you enable this then light will scatter through the jovian atmosphere. This means light entering the eye will come from all angles, especially around the light point.");

					if (Any(tgts, t => t.Scattering == true))
					{
						BeginIndent();
							BeginError(Any(tgts, t => t.ScatteringTex == null));
								Draw("scatteringTex", ref dirtyMaterial, "The look up table associating light angle with scattering color. The left side is used on the dark side, the middle is used on the horizon, and the right side is used on the light side.");
							EndError();
							Draw("scatteringStrength", ref dirtyMaterial, "The scattering is multiplied by this value, allowing you to easily adjust the brightness of the effect.");
						EndIndent();
					}
				EndIndent();
			}

			Separator();

			BeginError(Any(tgts, t => t.Mesh == null));
				Draw("mesh", ref dirtyMaterial, "This allows you to set the mesh used to render the jovian. This should be a sphere.");
			EndError();
			BeginError(Any(tgts, t => t.MeshRadius <= 0.0f));
				Draw("meshRadius", ref dirtyMaterial, "This allows you to set the radius of the Mesh. If this is incorrectly set then the jovian will render incorrectly.");
			EndError();
			BeginError(Any(tgts, t => t.Radius <= 0.0f));
				Draw("radius", ref dirtyMaterial, "This allows you to set the radius of the jovian in local space.");
			EndError();
			Draw("distanceRadius", ref dirtyMaterial, "This allows you to set the radius of the jovian in local space when using distance calculations. Distance calculations normally tell you how far from the surface of a planet you are, but since a jovian isn't solid, you may want to customize this based on your project.");

			if (Any(tgts, t => t.DepthTex == null && t.GetComponent<SgtJovianDepthTex>() == null))
			{
				Separator();

				if (Button("Add DepthTex") == true)
				{
					Each(tgts, t => SgtHelper.GetOrAddComponent<SgtJovianDepthTex>(t.gameObject));
				}
			}

			if (Any(tgts, t => t.Lit == true && t.LightingTex == null && t.GetComponent<SgtJovianLightingTex>() == null))
			{
				Separator();

				if (Button("Add LightingTex") == true)
				{
					Each(tgts, t => SgtHelper.GetOrAddComponent<SgtJovianLightingTex>(t.gameObject));
				}
			}

			if (Any(tgts, t => t.Lit == true && t.Scattering == true && t.ScatteringTex == null && t.GetComponent<SgtJovianScatteringTex>() == null))
			{
				Separator();

				if (Button("Add ScatteringTex") == true)
				{
					Each(tgts, t => SgtHelper.GetOrAddComponent<SgtJovianScatteringTex>(t.gameObject));
				}
			}

			if (Any(tgts, t => SetMeshAndMeshRadius(t, false)))
			{
				Separator();

				if (Button("Set Mesh & MeshRadius") == true)
				{
					Each(tgts, t => SetMeshAndMeshRadius(t, true));
				}
			}

			if (dirtyMaterial == true)
			{
				Each(tgts, t => t.DirtyMaterial(), true, true);
			}
		}

		private bool SetMeshAndMeshRadius(SgtJovian jovian, bool apply)
		{
			if (jovian.Mesh == null)
			{
				var mesh = SgtHelper.LoadFirstAsset<Mesh>("Geosphere40 t:mesh");

				if (mesh != null)
				{
					if (apply == true)
					{
						jovian.Mesh       = mesh;
						jovian.MeshRadius = SgtHelper.GetMeshRadius(mesh);
					}

					return true;
				}
			}

			return false;
		}
	}
}
#endif