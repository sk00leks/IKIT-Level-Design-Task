using UnityEngine;
using FSA = UnityEngine.Serialization.FormerlySerializedAsAttribute;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to generate a blurred SgtShadowRing.Texture based on a normal texture.</summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(SgtShadowRing))]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtShadowRingFilter")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Shadow Ring Filter")]
	public class SgtShadowRingFilter : MonoBehaviour
	{
		/// <summary>The source ring texture that will be filtered.</summary>
		public Texture2D Source { set { if (source != value) { source = value; DirtyTexture(); } } get { return source; } } [FSA("Source")] [SerializeField] private Texture2D source;

		/// <summary>The format of the generated texture.</summary>
		public TextureFormat Format { set { if (format != value) { format = value; DirtyTexture(); } } get { return format; } } [FSA("Format")] [SerializeField] private TextureFormat format = TextureFormat.ARGB32;

		/// <summary>The amount of blur iterations.</summary>
		public int Iterations { set { if (iterations != value) { iterations = value; DirtyTexture(); } } get { return iterations; } } [FSA("Iterations")] [SerializeField] private int iterations = 1;

		/// <summary>Overwrite the RGB channels with the alpha?</summary>
		public bool ShareRGB { set { if (shareRGB != value) { shareRGB = value; DirtyTexture(); } } get { return shareRGB; } } [FSA("ShareRGB")] [SerializeField] private bool shareRGB;

		/// <summary>Invert the alpha channel?</summary>
		public bool Invert { set { if (invert != value) { invert = value; DirtyTexture(); } } get { return invert; } } [FSA("Invert")] [SerializeField] private bool invert;

		[System.NonSerialized]
		private Texture2D generatedTexture;

		[System.NonSerialized]
		private SgtShadowRing cachedShadowRing;

		[System.NonSerialized]
		private bool cachedShadowRingSet;

		[System.NonSerialized]
		private static Color[] bufferA;

		[System.NonSerialized]
		private static Color[] bufferB;

		public Texture2D GeneratedTexture
		{
			get
			{
				return generatedTexture;
			}
		}

		public SgtShadowRing CachedShadowRing
		{
			get
			{
				if (cachedShadowRingSet == false)
				{
					cachedShadowRing    = GetComponent<SgtShadowRing>();
					cachedShadowRingSet = true;
				}

				return cachedShadowRing;
			}
		}

#if UNITY_EDITOR
		/// <summary>This method allows you to export the generated texture as an asset.
		/// Once done, you can remove this component, and set the <b>SgtShadowRing</b> component's <b>Texture</b> setting using the exported asset.</summary>
		[ContextMenu("Export Texture")]
		public void ExportTexture()
		{
			var importer = SgtHelper.ExportTextureDialog(generatedTexture, "RingShadow");

			if (importer != null)
			{
				importer.textureCompression  = UnityEditor.TextureImporterCompression.Uncompressed;
				importer.alphaSource         = UnityEditor.TextureImporterAlphaSource.FromInput;
				importer.wrapMode            = TextureWrapMode.Clamp;
				importer.filterMode          = FilterMode.Trilinear;
				importer.anisoLevel          = 16;
				importer.alphaIsTransparency = true;

				importer.SaveAndReimport();
			}
		}
#endif

		public void DirtyTexture()
		{
			UpdateTexture();
		}

		[ContextMenu("Update Texture")]
		public void UpdateTexture()
		{
			if (source == null)
			{
				source = CachedShadowRing.Texture as Texture2D;
			}

			if (source != null)
			{
				var width = source.width;
#if UNITY_EDITOR
				SgtHelper.MakeTextureReadable(source);
#endif
				// Destroy if invalid
				if (generatedTexture != null)
				{
					if (generatedTexture.width != width || generatedTexture.height != 1 || generatedTexture.format != format)
					{
						generatedTexture = SgtHelper.Destroy(generatedTexture);
					}
				}

				// Create?
				if (generatedTexture == null)
				{
					generatedTexture = SgtHelper.CreateTempTexture2D("Ring Shadow (Generated)", width, 1, format);

					generatedTexture.wrapMode = TextureWrapMode.Clamp;

					ApplyTexture();
				}

				if (bufferA == null || bufferA.Length != width)
				{
					bufferA = new Color[width];
					bufferB = new Color[width];
				}

				for (var x = 0; x < width; x++)
				{
					bufferA[x] = bufferB[x] = source.GetPixel(x, 0);
				}

				if (invert == true)
				{
					for (var x = 0; x < width; x++)
					{
						var a = bufferA[x];

						bufferA[x] = bufferB[x] = new Color(1.0f - a.r, 1.0f - a.g, 1.0f - a.b, 1.0f - a.a);
					}
				}

				if (shareRGB == true)
				{
					for (var x = 0; x < width; x++)
					{
						var a = bufferA[x].a;

						bufferA[x] = bufferB[x] = new Color(a, a, a, a);
					}
				}

				for (var i = 0 ; i < iterations; i++)
				{
					SwapBuffers();

					for (var x = width - 2; x >= 1; x--)
					{
						WritePixel(x);
					}
				}

				for (var x = 0; x < width; x++)
				{
					generatedTexture.SetPixel(x, 0, bufferB[x]);
				}

				generatedTexture.SetPixel(        0, 0, Color.white);
				generatedTexture.SetPixel(width - 1, 0, Color.white);

				generatedTexture.Apply();
			}
		}

		[ContextMenu("Apply Texture")]
		public void ApplyTexture()
		{
			if (generatedTexture != null)
			{
				CachedShadowRing.Texture = generatedTexture;
			}
			else
			{
				CachedShadowRing.Texture = source;
			}
		}

		protected virtual void OnEnable()
		{
			UpdateTexture();
			ApplyTexture();
		}

		protected virtual void OnDisable()
		{
			CachedShadowRing.Texture = source;
		}

		protected virtual void OnDestroy()
		{
			SgtHelper.Destroy(generatedTexture);
		}

		private void WritePixel(int x)
		{
			var a = bufferA[x - 1];
			var b = bufferA[x    ];
			var c = bufferA[x + 1];

			bufferB[x] = (a + b + c) / 3.0f;
		}

		private void SwapBuffers()
		{
			var bufferT = bufferA;

			bufferA = bufferB;
			bufferB = bufferT;
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using TARGET = SgtShadowRingFilter;

	[UnityEditor.CanEditMultipleObjects]
	[UnityEditor.CustomEditor(typeof(TARGET))]
	public class SgtShadowRingFilter_Editor : SgtEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			var dirtyTexture = false;

			BeginError(Any(tgts, t => t.Source == null));
				Draw("source", ref dirtyTexture, "The source ring texture that will be filtered.");
			EndError();
			Draw("format", ref dirtyTexture, "The format of the generated texture.");

			Separator();

			BeginError(Any(tgts, t => t.Iterations <= 0));
				Draw("iterations", ref dirtyTexture, "The amount of blur iterations.");
			EndError();
			Draw("shareRGB", ref dirtyTexture, "Overwrite the RGB channels with the alpha?");
			Draw("invert", ref dirtyTexture, "Invert the alpha channel?");

			if (dirtyTexture == true) Each(tgts, t => t.DirtyTexture(), true, true);
		}
	}
}
#endif