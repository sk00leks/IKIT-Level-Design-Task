using UnityEngine;
using System.Collections.Generic;
using FSA = UnityEngine.Serialization.FormerlySerializedAsAttribute;

namespace SpaceGraphicsToolkit
{
	/// <summary>This is the base class for all starfields, providing a simple interface for generating meshes from a list of stars, as well as the material to render it.</summary>
	public abstract class SgtQuads : MonoBehaviour
	{
		public enum LayoutType
		{
			Grid,
			Custom
		}

		/// <summary>The base color will be multiplied by this.</summary>
		public Color Color { set { if (color != value) { color = value; DirtyMaterial(); } } get { return color; } } [FSA("Color")] [SerializeField] protected Color color = Color.white;

		/// <summary>The Color.rgb values are multiplied by this, allowing you to quickly adjust the overall brightness.</summary>
		public float Brightness { set { if (brightness != value) { brightness = value; DirtyMaterial(); } } get { return brightness; } } [FSA("Brightness")] [SerializeField] protected float brightness = 1.0f;

		/// <summary>The main texture of this material.</summary>
		public Texture MainTex { set { if (mainTex != value) { mainTex = value; DirtyMaterial(); } } get { return mainTex; } } [FSA("MainTex")] [SerializeField] protected Texture mainTex;

		/// <summary>The layout of cells in the texture.</summary>
		public LayoutType Layout { set { if (layout != value) { layout = value; DirtyMaterial(); } } get { return layout; } } [FSA("Layout")] [SerializeField] protected LayoutType layout;

		/// <summary>The amount of columns in the texture.</summary>
		public int LayoutColumns { set { if (layoutColumns != value) { layoutColumns = value; DirtyMaterial(); } } get { return layoutColumns; } } [FSA("LayoutColumns")] [SerializeField] protected int layoutColumns = 1;

		/// <summary>The amount of rows in the texture.</summary>
		public int LayoutRows { set { if (layoutRows != value) { layoutRows = value; DirtyMaterial(); } } get { return layoutRows; } } [FSA("LayoutRows")] [SerializeField] protected int layoutRows = 1;

		/// <summary>The rects of each cell in the texture.</summary>
		public List<Rect> LayoutRects { get { if (layoutRects == null) layoutRects = new List<Rect>(); return layoutRects; } } [FSA("LayoutRects")] [SerializeField] protected List<Rect> layoutRects;

		[System.NonSerialized]
		protected Material material;

		[System.NonSerialized]
		protected Mesh mesh;

		[System.NonSerialized]
		private bool dirtyMaterial = true;

		[System.NonSerialized]
		private bool dirtyMesh = true;

		[System.NonSerialized]
		protected SgtShaderProperties shaderProperties = new SgtShaderProperties();

		protected static List<Vector4> tempCoords = new List<Vector4>();

		public void DirtyMesh()
		{
			dirtyMesh = true;
		}

		public void DirtyMaterial()
		{
			dirtyMaterial = true;
		}

		private void UpdateMesh()
		{
			dirtyMesh = false;

			var count = BeginQuads();

			BuildRects();
			ConvertRectsToCoords();

			if (mesh == null)
			{
				mesh = SgtHelper.CreateTempMesh("Quads Mesh (Generated)");
			}
			else
			{
				mesh.Clear();
			}

			BuildMesh(mesh, count);

			EndQuads();
		}

		protected virtual void OnEnable()
		{
			SgtCamera.OnCameraDraw += HandleCameraDraw;
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
			SgtHelper.DestroyOldGameObjects(transform, "Quads Model");
		}
#endif

		protected virtual void LateUpdate()
		{
			if (dirtyMesh == true)
			{
				UpdateMesh();
			}

			if (dirtyMaterial == true)
			{
				UpdateMaterial();
			}

			material.SetFloat(SgtShader._Scale, transform.lossyScale.x);
			material.SetFloat(SgtShader._ScaleRecip, SgtHelper.Reciprocal(transform.lossyScale.x));
		}

		protected virtual void OnDestroy()
		{
			SgtHelper.Destroy(mesh);
			SgtHelper.Destroy(material);
		}

		protected virtual void OnDidApplyAnimationProperties()
		{
			DirtyMesh();
			DirtyMaterial();
		}

#if UNITY_EDITOR
		protected virtual void OnValidate()
		{
			UpdateMesh();
			UpdateMaterial();
		}
#endif

		protected abstract void HandleCameraDraw(Camera camera);

		protected abstract int BeginQuads();

		protected abstract void EndQuads();

		protected virtual void UpdateMaterial()
		{
			dirtyMaterial = false;

			material.SetTexture(SgtShader._MainTex, mainTex);
			material.SetColor(SgtShader._Color, SgtHelper.Brighten(Color, Color.a * Brightness, false));
		}

		protected void BuildRects()
		{
			if (layout == LayoutType.Grid)
			{
				if (layoutRects == null) layoutRects = new List<Rect>();

				layoutRects.Clear();

				if (layoutColumns > 0 && layoutRows > 0)
				{
					var invX = SgtHelper.Reciprocal(layoutColumns);
					var invY = SgtHelper.Reciprocal(layoutRows   );

					for (var y = 0; y < layoutRows; y++)
					{
						var offY = y * invY;

						for (var x = 0; x < layoutColumns; x++)
						{
							var offX = x * invX;
							var rect = new Rect(offX, offY, invX, invY);

							layoutRects.Add(rect);
						}
					}
				}
			}
		}

		protected abstract void BuildMesh(Mesh mesh, int count);

		protected static void ExpandBounds(ref bool minMaxSet, ref Vector3 min, ref Vector3 max, Vector3 position, float radius)
		{
			var radius3 = new Vector3(radius, radius, radius);

			if (minMaxSet == false)
			{
				minMaxSet = true;

				min = position - radius3;
				max = position + radius3;
			}

			min = Vector3.Min(min, position - radius3);
			max = Vector3.Max(max, position + radius3);
		}

		private void ConvertRectsToCoords()
		{
			tempCoords.Clear();

			if (layoutRects != null)
			{
				for (var i = 0; i < layoutRects.Count; i++)
				{
					var rect = layoutRects[i];

					tempCoords.Add(new Vector4(rect.xMin, rect.yMin, rect.xMax, rect.yMax));
				}
			}

			if (tempCoords.Count == 0) tempCoords.Add(default(Vector4));
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using TARGET = SgtQuads;

	public class SgtQuads_Editor : SgtEditor
	{
		protected virtual void DrawMaterial(ref bool dirtyMaterial)
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Draw("color", ref dirtyMaterial, "The base color will be multiplied by this.");
			BeginError(Any(tgts, t => t.Brightness < 0.0f));
				Draw("brightness", ref dirtyMaterial, "The Color.rgb values are multiplied by this, allowing you to quickly adjust the overall brightness.");
			EndError();
		}

		protected virtual void DrawMainTex(ref bool dirtyMaterial)
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			BeginError(Any(tgts, t => t.MainTex == null));
				Draw("mainTex", ref dirtyMaterial, "The main texture of this material.");
			EndError();
		}

		protected virtual void DrawLayout(ref bool dirtyMesh)
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Draw("layout", ref dirtyMesh, "The layout of cells in the texture.");
			BeginIndent();
				if (Any(tgts, t => t.Layout == SgtQuads.LayoutType.Grid))
				{
					BeginError(Any(tgts, t => t.LayoutColumns <= 0));
						Draw("layoutColumns", ref dirtyMesh, "The amount of columns in the texture.");
					EndError();
					BeginError(Any(tgts, t => t.LayoutRows <= 0));
						Draw("layoutRows", ref dirtyMesh, "The amount of rows in the texture.");
					EndError();
				}

				if (Any(tgts, t => t.Layout == SgtQuads.LayoutType.Custom))
				{
					Draw("rects", ref dirtyMesh, "The rects of each cell in the texture.");
				}
			EndIndent();
		}
	}
}
#endif