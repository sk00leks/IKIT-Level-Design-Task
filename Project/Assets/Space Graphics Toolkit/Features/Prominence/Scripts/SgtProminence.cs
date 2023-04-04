using UnityEngine;
using FSA = UnityEngine.Serialization.FormerlySerializedAsAttribute;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to render a halo disc around a star.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtProminence")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Prominence")]
	public class SgtProminence : MonoBehaviour
	{
		/// <summary>The main texture of the prominence.</summary>
		public Texture MainTex { set { if (mainTex != value) { mainTex = value; DirtyMaterial(); } } get { return mainTex; } } [FSA("MainTex")] [SerializeField] private Texture mainTex;

		/// <summary>The base color will be multiplied by this.</summary>
		public Color Color { set { if (color != value) { color = value; DirtyMaterial(); } } get { return color; } } [FSA("Color")] [SerializeField] private Color color = Color.white;

		/// <summary>The Color.rgb values are multiplied by this, allowing you to quickly adjust the overall brightness.</summary>
		public float Brightness { set { if (brightness != value) { brightness = value; DirtyMaterial(); } } get { return brightness; } } [FSA("Brightness")] [SerializeField] private float brightness = 1.0f;

		/// <summary>This allows you to adjust the render queue of the prominence material. You can normally adjust the render queue in the material settings, but since this material is procedurally generated your changes will be lost.</summary>
		public SgtRenderQueue RenderQueue { set { if (renderQueue != value) { renderQueue = value; DirtyMaterial(); } } get { return renderQueue; } } [FSA("RenderQueue")] [SerializeField] private SgtRenderQueue renderQueue = SgtRenderQueue.GroupType.Transparent;

		/// <summary>This allows you to set the random seed used during procedural generation.</summary>
		public int Seed { set { if (seed != value) { seed = value; DirtyMesh(); } } get { return seed; } } [FSA("Seed")] [SerializeField] [SgtSeed] private int seed;

		/// <summary>The amount of planes used to build the prominence.</summary>
		public int PlaneCount { set { if (planeCount != value) { planeCount = value; DirtyMesh(); } } get { return planeCount; } } [FSA("PlaneCount")] [SerializeField] private int planeCount = 8;

		/// <summary>The amount of quads used to build each plane.</summary>
		public int PlaneDetail { set { if (planeDetail != value) { planeDetail = value; DirtyMesh(); } } get { return planeDetail; } } [FSA("PlaneDetail")] [SerializeField] private int planeDetail = 10;

		/// <summary>The inner radius of the prominence planes in local coordinates.</summary>
		public float RadiusMin { set { if (radiusMin != value) { radiusMin = value; DirtyMesh(); } } get { return radiusMin; } } [FSA("RadiusMin")] [SerializeField] private float radiusMin = 1.0f;

		/// <summary>The outer radius of the prominence planes in local coordinates.</summary>
		public float RadiusMax { set { if (radiusMax != value) { radiusMax = value; DirtyMesh(); } } get { return radiusMax; } } [FSA("RadiusMax")] [SerializeField] private float radiusMax = 2.0f;

		/// <summary>Should the plane fade out when it's viewed edge-on?</summary>
		public bool FadeEdge { set { if (fadeEdge != value) { fadeEdge = value; DirtyMaterial(); } } get { return fadeEdge; } } [FSA("FadeEdge")] [SerializeField] private bool fadeEdge;

		/// <summary>How sharp the transition between visible and invisible is.</summary>
		public float FadePower { set { if (fadePower != value) { fadePower = value; DirtyMaterial(); } } get { return fadePower; } } [FSA("FadePower")] [SerializeField] private float fadePower = 2.0f;

		/// <summary>Should the plane fade out when it's in front of the star?</summary>
		public bool ClipNear { set { if (clipNear != value) { clipNear = value; DirtyMaterial(); } } get { return clipNear; } } [FSA("ClipNear")] [SerializeField] private bool clipNear;

		/// <summary>How sharp the transition between visible and invisible is.</summary>
		public float ClipPower { set { if (clipPower != value) { clipPower = value; DirtyMaterial(); } } get { return clipPower; } } [FSA("ClipPower")] [SerializeField] private float clipPower = 2.0f;

		/// <summary>This allows you to offset the camera distance in world space when rendering the prominence, giving you fine control over the render order.</summary>
		public float CameraOffset { set { if (cameraOffset != value) { cameraOffset = value; DirtyMaterial(); } } get { return cameraOffset; } } [FSA("CameraOffset")] [SerializeField] private float cameraOffset;

		/// <summary>Should the prominence be animated to distort? This makes the edges flicker like a flame.</summary>
		public bool Distort { set { if (distort != value) { distort = value; DirtyMaterial(); } } get { return distort; } } [FSA("Distort")] [SerializeField] private bool distort;

		/// <summary>This allows you to set the distortion texture that gets repeated on the prominence surface.</summary>
		public Texture DistortTex { set { if (distortTex != value) { distortTex = value; DirtyMaterial(); } } get { return distortTex; } } [FSA("DistortTex")] [SerializeField] private Texture distortTex;

		/// <summary>The distortion texture horizontal tiling.</summary>
		public float DistortScaleX { set { if (distortScaleX != value) { distortScaleX = value; DirtyMaterial(); } } get { return distortScaleX; } } [FSA("DistortScaleX")] [SerializeField] private float distortScaleX = 0.1f;

		/// <summary>The distortion texture vertical tiling.</summary>
		public int DistortScaleY { set { if (distortScaleY != value) { distortScaleY = value; DirtyMaterial(); } } get { return distortScaleY; } } [FSA("DistortScaleY")] [SerializeField] private int distortScaleY = 1;

		/// <summary>The distortion texture strength.</summary>
		public float DistortStrength { set { if (distortStrength != value) { distortStrength = value; DirtyMaterial(); } } get { return distortStrength; } } [FSA("DistortStrength")] [SerializeField] private float distortStrength = 0.1f;

		/// <summary>The UV offset of the distortion texture.</summary>
		public Vector2 DistortOffset { set { if (distortOffset != value) { distortOffset = value; DirtyMaterial(); } } get { return distortOffset; } } [FSA("DistortOffset")] [SerializeField] private Vector2 distortOffset;

		/// <summary>The scroll speed of the distortion texture UV offset.</summary>
		public Vector2 DistortSpeed { set { if (distortSpeed != value) { distortSpeed = value; DirtyMaterial(); } } get { return distortSpeed; } } [FSA("DistortSpeed")] [SerializeField] private Vector2 distortSpeed = new Vector2(0.1f, 0.0f);

		/// <summary>Should the disc have a detail texture? For example, dust noise when you get close.</summary>
		public bool Detail { set { if (detail != value) { detail = value; DirtyMaterial(); } } get { return detail; } } [FSA("Detail")] [SerializeField] private bool detail;

		/// <summary>This allows you to set the detail texture that gets repeated on the prominence surface.</summary>
		public Texture DetailTex { set { if (detailTex != value) { detailTex = value; DirtyMaterial(); } } get { return detailTex; } } [FSA("DetailTex")] [SerializeField] private Texture detailTex;

		/// <summary>The detail texture horizontal tiling.</summary>
		public float DetailScaleX { set { if (detailScaleX != value) { detailScaleX = value; DirtyMaterial(); } } get { return detailScaleX; } } [FSA("DetailScaleX")] [SerializeField] private float detailScaleX = 0.1f;

		/// <summary>The detail texture vertical tiling.</summary>
		public int DetailScaleY { set { if (detailScaleY != value) { detailScaleY = value; DirtyMaterial(); } } get { return detailScaleY; } } [FSA("DetailScaleY")] [SerializeField] private int detailScaleY = 1;

		/// <summary>The detail texture strength.</summary>
		public float DetailStrength { set { if (detailStrength != value) { detailStrength = value; DirtyMaterial(); } } get { return detailStrength; } } [FSA("DetailStrength")] [SerializeField] private float detailStrength = 1.0f;

		/// <summary>The UV offset of the detail texture.</summary>
		public Vector2 DetailOffset { set { if (detailOffset != value) { detailOffset = value; DirtyMaterial(); } } get { return detailOffset; } } [FSA("DetailOffset")] [SerializeField] private Vector2 detailOffset = new Vector2(0.5f, 0.5f);

		/// <summary>The scroll speed of the detail texture UV offset.</summary>
		public Vector2 DetailSpeed { set { if (detailSpeed != value) { detailSpeed = value; DirtyMaterial(); } } get { return detailSpeed; } } [FSA("DetailSpeed")] [SerializeField] private Vector2 detailSpeed = new Vector2(0.1f, 0.0f);

		// The material applied to all segments
		[System.NonSerialized]
		private Material material;

		// The mesh applied to all segments
		[System.NonSerialized]
		private Mesh mesh;

		public float Width
		{
			get
			{
				return radiusMax - radiusMin;
			}
		}

		public void DirtyMesh()
		{
			UpdateMesh();
		}

		public void DirtyMaterial()
		{
			UpdateMaterial();
		}

		[ContextMenu("Update Material")]
		public void UpdateMaterial()
		{
			if (material == null)
			{
				material = SgtHelper.CreateTempMaterial("Prominence (Generated)", SgtHelper.ShaderNamePrefix + "Prominence");
			}

			material.renderQueue = renderQueue;

			material.SetTexture(SgtShader._MainTex, mainTex);
			material.SetColor(SgtShader._Color, SgtHelper.Premultiply(SgtHelper.Brighten(color, brightness)));
			material.SetVector(SgtShader._WorldPosition, transform.position);

			SgtHelper.SetTempMaterial(material);

			if (fadeEdge == true)
			{
				SgtHelper.EnableKeyword("_FADE_EDGE");

				material.SetFloat(SgtShader._FadePower, fadePower);
			}
			else
			{
				SgtHelper.DisableKeyword("_FADE_EDGE");
			}

			if (clipNear == true)
			{
				SgtHelper.EnableKeyword("_CLIP_NEAR");

				material.SetFloat(SgtShader._ClipPower, clipPower);
			}
			else
			{
				SgtHelper.DisableKeyword("_CLIP_NEAR");
			}

			if (distort == true)
			{
				SgtHelper.EnableKeyword("_DISTORT", material);

				material.SetTexture(SgtShader._DistortTex, distortTex);
				material.SetVector(SgtShader._DistortScale, new Vector2(distortScaleX, distortScaleY));
				material.SetFloat(SgtShader._DistortStrength, distortStrength);
			}
			else
			{
				SgtHelper.DisableKeyword("_DISTORT", material);
			}

			if (detail == true)
			{
				SgtHelper.EnableKeyword("_DETAIL", material);

				material.SetTexture(SgtShader._DetailTex, detailTex);
				material.SetVector(SgtShader._DetailScale, new Vector2(detailScaleX, detailScaleY));
				material.SetFloat(SgtShader._DetailStrength, detailStrength);
			}
			else
			{
				SgtHelper.DisableKeyword("_DETAIL", material);
			}
		}

		[ContextMenu("Update Mesh")]
		public void UpdateMesh()
		{
			if (mesh == null)
			{
				mesh = SgtHelper.CreateTempMesh("Plane");
			}
			else
			{
				mesh.Clear(false);
			}

			if (planeDetail >= 2)
			{
				var detail    = Mathf.Min(planeDetail, SgtHelper.QuadsPerMesh / 2); // Limit the amount of vertices that get made
				var positions = new Vector3[detail * 2 + 2];
				var normals   = new Vector3[detail * 2 + 2];
				var coords1   = new Vector2[detail * 2 + 2];
				var coords2   = new Vector2[detail * 2 + 2];
				var indices   = new int[detail * 6];
				var angleStep = SgtHelper.Divide(Mathf.PI * 2.0f, detail);
				var coordStep = SgtHelper.Reciprocal(detail);

				for (var i = 0; i <= detail; i++)
				{
					var coord = coordStep * i;
					var angle = angleStep * i;
					var sin   = Mathf.Sin(angle);
					var cos   = Mathf.Cos(angle);
					var offV  = i * 2;

					positions[offV + 0] = new Vector3(sin * radiusMin, 0.0f, cos * radiusMin);
					positions[offV + 1] = new Vector3(sin * radiusMax, 0.0f, cos * radiusMax);

					normals[offV + 0] = Vector3.up;
					normals[offV + 1] = Vector3.up;

					coords1[offV + 0] = new Vector2(0.0f, coord * radiusMin);
					coords1[offV + 1] = new Vector2(1.0f, coord * radiusMax);

					coords2[offV + 0] = new Vector2(radiusMin, 0.0f);
					coords2[offV + 1] = new Vector2(radiusMax, 0.0f);
				}

				for (var i = 0; i < detail; i++)
				{
					var offV = i * 2;
					var offI = i * 6;

					indices[offI + 0] = offV + 0;
					indices[offI + 1] = offV + 1;
					indices[offI + 2] = offV + 2;
					indices[offI + 3] = offV + 3;
					indices[offI + 4] = offV + 2;
					indices[offI + 5] = offV + 1;
				}

				mesh.vertices  = positions;
				mesh.normals   = normals;
				mesh.uv        = coords1;
				mesh.uv2       = coords2;
				mesh.triangles = indices;
			}
		}

		public static SgtProminence Create(int layer = 0, Transform parent = null)
		{
			return Create(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public static SgtProminence Create(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			return SgtHelper.CreateGameObject("Prominence", layer, parent, localPosition, localRotation, localScale).AddComponent<SgtProminence>();
		}

#if UNITY_EDITOR
		[UnityEditor.MenuItem(SgtHelper.GameObjectMenuPrefix + "Prominence", false, 10)]
		public static void CreateMenuItem()
		{
			var parent     = SgtHelper.GetSelectedParent();
			var prominence = Create(parent != null ? parent.gameObject.layer : 0, parent);

			SgtHelper.SelectAndPing(prominence);
		}
#endif

		protected virtual void OnEnable()
		{
			SgtCamera.OnCameraDraw += HandleCameraDraw;

			UpdateMaterial();
			UpdateMesh();
		}

		protected virtual void OnDisable()
		{
			SgtCamera.OnCameraDraw -= HandleCameraDraw;
		}

#if UNITY_EDITOR
		protected virtual void Start()
		{
			// Upgrade scene
			// NOTE: This must be done in Start because when done in OnEnable this fails to dirty the scene
			SgtHelper.DestroyOldGameObjects(transform, "Prominence Model");
		}
#endif

		protected virtual void LateUpdate()
		{
			if (material != null)
			{
				material.SetVector(SgtShader._WorldPosition, transform.position);

				if (distort == true)
				{
					if (Application.isPlaying == true)
					{
						distortOffset += distortSpeed * Time.deltaTime;
					}

					material.SetVector(SgtShader._DistortOffset, distortOffset);
				}

				if (detail == true)
				{
					if (Application.isPlaying == true)
					{
						detailOffset += detailSpeed * Time.deltaTime;
					}

					material.SetVector(SgtShader._DetailOffset, detailOffset);
				}
			}
		}

		protected virtual void OnDestroy()
		{
			if (mesh != null)
			{
				mesh.Clear(false);

				SgtObjectPool<Mesh>.Add(mesh);
			}

			SgtHelper.Destroy(material);
		}

		protected virtual void OnDidApplyAnimationProperties()
		{
			DirtyMesh();
			DirtyMaterial();
		}

#if UNITY_EDITOR
		protected virtual void OnDrawGizmosSelected()
		{
			Gizmos.matrix = transform.localToWorldMatrix;

			Gizmos.DrawWireSphere(Vector3.zero, radiusMin);

			Gizmos.DrawWireSphere(Vector3.zero, radiusMax);
		}
#endif

		private void HandleCameraDraw(Camera camera)
		{
			if (SgtHelper.CanDraw(gameObject, camera) == false) return;

			var matrix = transform.localToWorldMatrix;

			if (cameraOffset != 0.0f)
			{
				var direction = Vector3.Normalize(camera.transform.position - transform.position);

				matrix = Matrix4x4.Translate(direction * cameraOffset) * matrix;
			}

			SgtHelper.BeginRandomSeed(seed);
				for (var i = 0; i < planeCount; i++)
				{
					var rotation = Matrix4x4.Rotate(Random.rotation);

					Graphics.DrawMesh(mesh, matrix * rotation, material, gameObject.layer, camera, 0, null, false);
				}
			SgtHelper.EndRandomSeed();
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using TARGET = SgtProminence;

	[UnityEditor.CanEditMultipleObjects]
	[UnityEditor.CustomEditor(typeof(TARGET))]
	public class SgtProminence_Editor : SgtEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			var dirtyMaterial = false;
			var dirtyMesh     = false;

			Draw("color", ref dirtyMaterial, "The base color will be multiplied by this.");
			Draw("brightness", ref dirtyMaterial, "The Color.rgb values are multiplied by this, allowing you to quickly adjust the overall brightness.");
			Draw("renderQueue", ref dirtyMaterial, "This allows you to adjust the render queue of the prominence material. You can normally adjust the render queue in the material settings, but since this material is procedurally generated your changes will be lost.");

			Separator();
			
			BeginError(Any(tgts, t => t.MainTex == null));
				Draw("mainTex", ref dirtyMaterial, "The main texture of the prominence.");
			EndError();
			Draw("cameraOffset", "This allows you to offset the camera distance in world space when rendering the prominence, giving you fine control over the render order."); // Updated automatically
			Draw("seed", ref dirtyMesh, "This allows you to set the random seed used during procedural generation.");
			BeginError(Any(tgts, t => t.PlaneCount < 1));
				Draw("planeCount", ref dirtyMesh, "The amount of planes used to build the prominence.");
			EndError();
			BeginError(Any(tgts, t => t.PlaneDetail < 3));
				Draw("planeDetail", ref dirtyMesh, "The amount of quads used to build each plane.");
			EndError();
			BeginError(Any(tgts, t => t.RadiusMin <= 0.0f || t.RadiusMin >= t.RadiusMax));
				Draw("radiusMin", ref dirtyMesh, "The inner radius of the prominence planes in local coordinates.");
			EndError();
			BeginError(Any(tgts, t => t.RadiusMax < 0.0f || t.RadiusMin >= t.RadiusMax));
				Draw("radiusMax", ref dirtyMesh, "The outer radius of the prominence planes in local coordinates.");
			EndError();

			Separator();

			Draw("fadeEdge", ref dirtyMaterial, "Should the plane fade out when it's viewed edge-on?");

			if (Any(tgts, t => t.FadeEdge == true))
			{
				BeginIndent();
					BeginError(Any(tgts, t => t.FadePower < 0.0f));
						Draw("fadePower", ref dirtyMaterial, "How sharp the transition between visible and invisible is.");
					EndError();
				EndIndent();
			}

			Draw("clipNear", ref dirtyMaterial, "Should the plane fade out when it's in front of the star?");

			if (Any(tgts, t => t.ClipNear == true))
			{
				BeginIndent();
					BeginError(Any(tgts, t => t.ClipPower < 0.0f));
						Draw("clipPower", ref dirtyMaterial, "How sharp the transition between visible and invisible is.");
					EndError();
				EndIndent();
			}

			Separator();

			Draw("distort", ref dirtyMaterial, "Should the prominence be animated to distort? This makes the edges flicker like a flame.");

			if (Any(tgts, t => t.Distort == true))
			{
				BeginIndent();
					BeginError(Any(tgts, t => t.DistortTex == null));
						Draw("distortTex", ref dirtyMaterial, "This allows you to set the distortion texture that gets repeated on the prominence surface.");
					EndError();
					BeginError(Any(tgts, t => t.DistortScaleX < 0.0f));
						Draw("distortScaleX", ref dirtyMaterial, "The distortion texture horizontal tiling.");
					EndError();
					BeginError(Any(tgts, t => t.DistortScaleY < 1));
						Draw("distortScaleY", ref dirtyMaterial, "The distortion texture vertical tiling.");
					EndError();
					BeginError(Any(tgts, t => t.DistortStrength == 0.0f));
						Draw("distortStrength", ref dirtyMaterial, "The distortion texture strength.");
					EndError();
					Draw("distortOffset", ref dirtyMaterial, "The UV offset of the distortion texture.");
					Draw("distortSpeed", ref dirtyMaterial, "The scroll speed of the distortion texture UV offset.");
				EndIndent();
			}

			Separator();

			Draw("detail", ref dirtyMaterial, "Should the disc have a detail texture? For example, dust noise when you get close.");

			if (Any(tgts, t => t.Detail == true))
			{
				BeginIndent();
					BeginError(Any(tgts, t => t.DetailTex == null));
						Draw("detailTex", ref dirtyMaterial, "This allows you to set the detail texture that gets repeated on the prominence surface.");
					EndError();
					BeginError(Any(tgts, t => t.DetailScaleX < 0.0f));
						Draw("detailScaleX", ref dirtyMaterial, "The detail texture horizontal tiling.");
					EndError();
					BeginError(Any(tgts, t => t.DetailScaleY < 1));
						Draw("detailScaleY", ref dirtyMaterial, "The detail texture vertical tiling.");
					EndError();
					BeginError(Any(tgts, t => t.DetailStrength == 0.0f));
						Draw("detailStrength", ref dirtyMaterial, "The detail texture strength.");
					EndError();
					Draw("detailOffset", ref dirtyMaterial, "The UV offset of the detail texture.");
					Draw("detailSpeed", ref dirtyMaterial, "The scroll speed of the detail texture UV offset.");
				EndIndent();
			}

			if (dirtyMaterial == true) Each(tgts, t => t.DirtyMaterial(), true, true);
			if (dirtyMesh     == true) Each(tgts, t => t.DirtyMesh    (), true, true);
		}
	}
}
#endif