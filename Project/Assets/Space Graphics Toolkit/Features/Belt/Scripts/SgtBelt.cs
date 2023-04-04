using UnityEngine;
using System.Collections.Generic;
using FSA = UnityEngine.Serialization.FormerlySerializedAsAttribute;

namespace SpaceGraphicsToolkit
{
	/// <summary>This base class contains the functionality to render an asteroid belt.</summary>
	public abstract class SgtBelt : SgtQuads
	{
		public enum BlendModeType
		{
			Opaque,
			Additive
		}

		/// <summary>The blend mode used to render the material.</summary>
		public BlendModeType BlendMode { set { blendMode = value; } get { return blendMode; } } [SerializeField] private BlendModeType blendMode;

		/// <summary>The amount of seconds this belt has been animating for.</summary>
		public float OrbitOffset { set { orbitOffset = value; } get { return orbitOffset; } } [FSA("OrbitOffset")] [SerializeField] private float orbitOffset;

		/// <summary>The animation speed of this belt.</summary>
		public float OrbitSpeed { set { orbitSpeed = value; } get { return orbitSpeed; } } [FSA("OrbitSpeed")] [SerializeField] private float orbitSpeed = 1.0f;

		/// <summary>If you enable this then nearby SgtLight and SgtShadow casters will be found and applied to the lighting calculations.</summary>
		public bool Lit { set { if (lit != value) { lit = value; DirtyMaterial(); } } get { return lit; } } [FSA("Lit")] [SerializeField] private bool lit;

		/// <summary>The belt will always be lit by this amount.</summary>
		public Color AmbientColor { set { if (ambientColor != value) { ambientColor = value; DirtyMaterial(); } } get { return ambientColor; } } [FSA("AmbientColor")] [SerializeField] private Color ambientColor;

		/// <summary>The <b>AmbientColor</b> will be multiplied by this.</summary>
		public float AmbientBrightness { set { if (ambientBrightness != value) { ambientBrightness = value; DirtyMaterial(); } } get { return ambientBrightness; } } [SerializeField] private float ambientBrightness = 1.0f;

		/// <summary>The maximum amount of <b>SgtLight</b> components that can light this object.</summary>
		public int MaxLights { set { if (maxLights != value) { maxLights = value; DirtyMaterial(); } } get { return maxLights; } } [SerializeField] [Range(0, SgtLight.MAX_LIGHTS)] private int maxLights = 2;

		/// <summary>The maximum amount of <b>SgtShadowSphere</b> components that can shade this object.</summary>
		public int MaxSphereShadows { set { if (maxSphereShadows != value) { maxSphereShadows = value; DirtyMaterial(); } } get { return maxSphereShadows; } } [SerializeField] [Range(0, SgtShadow.MAX_SPHERE_SHADOWS)] private int maxSphereShadows = 2;

		/// <summary>The look up table associating light angle with surface color. The left side is used on the dark side, the middle is used on the horizon, and the right side is used on the light side.</summary>
		public Texture LightingTex { set { if (lightingTex != value) { lightingTex = value; DirtyMaterial(); } } get { return lightingTex; } } [FSA("LightingTex")] [SerializeField] private Texture lightingTex;

		/// <summary>Instead of just tinting the asteroids with the colors, should the RGB values be raised to the power of the color?</summary>
		public bool PowerRgb { set { if (powerRgb != value) { powerRgb = value; DirtyMaterial(); } } get { return powerRgb; } } [FSA("PowerRgb")] [SerializeField] private bool powerRgb;

		public static Vector3 CalculateLocalPosition(ref SgtBeltAsteroid asteroid, float age)
		{
			var a = asteroid.OrbitAngle + asteroid.OrbitSpeed * age;
			var x = (float)System.Math.Sin(a) * asteroid.OrbitDistance;
			var y = asteroid.Height;
			var z = (float)System.Math.Cos(a) * asteroid.OrbitDistance;

			return new Vector3(x, y, z);
		}

		public SgtBeltCustom MakeEditableCopy(int layer = 0, Transform parent = null)
		{
			return MakeEditableCopy(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public SgtBeltCustom MakeEditableCopy(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
#if UNITY_EDITOR
			SgtHelper.BeginUndo("Create Editable Belt Copy");
#endif
			var gameObject = SgtHelper.CreateGameObject(name + " (Editable Copy)", layer, parent, localPosition, localRotation, localScale);
			var customBelt = SgtHelper.AddComponent<SgtBeltCustom>(gameObject, false);
			var asteroids  = customBelt.Asteroids;
			var quadCount  = BeginQuads();

			for (var i = 0; i < quadCount; i++)
			{
				var asteroid = SgtPoolClass<SgtBeltAsteroid>.Pop() ?? new SgtBeltAsteroid();

				NextQuad(ref asteroid, i);

				asteroids.Add(asteroid);
			}

			EndQuads();

			// Copy common settings
			customBelt.Color             = Color;
			customBelt.Brightness        = Brightness;
			customBelt.mainTex           = mainTex;
			customBelt.layout            = layout;
			customBelt.layoutColumns     = layoutColumns;
			customBelt.layoutRows        = layoutRows;
			customBelt.layoutRects       = new List<Rect>(layoutRects);
			customBelt.orbitOffset       = orbitOffset;
			customBelt.orbitSpeed        = orbitSpeed;
			customBelt.lit               = lit;
			customBelt.maxLights         = maxLights;
			customBelt.maxSphereShadows  = maxSphereShadows;
			customBelt.ambientColor      = ambientColor;
			customBelt.ambientBrightness = ambientBrightness;
			customBelt.lightingTex       = lightingTex;
			customBelt.powerRgb          = powerRgb;

			return customBelt;
		}

#if UNITY_EDITOR
		[ContextMenu("Make Editable Copy")]
		public void MakeEditableCopyContext()
		{
			var customBelt = MakeEditableCopy(gameObject.layer, transform.parent, transform.localPosition, transform.localRotation, transform.localScale);

			customBelt.DirtyMaterial();
			customBelt.DirtyMesh();

			SgtHelper.SelectAndPing(customBelt);
		}
#endif

		protected override void LateUpdate()
		{
			base.LateUpdate();

			if (Application.isPlaying == true)
			{
				orbitOffset += Time.deltaTime * orbitSpeed;
			}

			material.SetFloat(SgtShader._Age, orbitOffset);

			// Write lights and shadows
			SgtHelper.SetTempMaterial(material);

			var mask   = 1 << gameObject.layer;
			var lights = SgtLight.Find(lit, mask, transform.position);

			SgtShadow.Find(lit, mask, lights);
			SgtShadow.FilterOutRing(transform.position);
			SgtShadow.WriteSphere(2);
			SgtShadow.WriteRing(1);

			SgtLight.Write(transform.position, transform, null, 1.0f, 2);
		}

		protected override void HandleCameraDraw(Camera camera)
		{
			if (SgtHelper.CanDraw(gameObject, camera) == false) return;

			var properties = shaderProperties.GetProperties(material, camera);
			var sgtCamera  = default(SgtCamera);

			if (SgtCamera.TryFind(camera, ref sgtCamera) == true)
			{
				properties.SetFloat(SgtShader._CameraRollAngle, sgtCamera.RollAngle * Mathf.Deg2Rad);
			}
			else
			{
				properties.SetFloat(SgtShader._CameraRollAngle, 0.0f);
			}

			Graphics.DrawMesh(mesh, transform.localToWorldMatrix, material, gameObject.layer, camera, 0, properties);
		}

		private string GetShaderName()
		{
			switch (blendMode)
			{
				case BlendModeType.Opaque:   return SgtHelper.ShaderNamePrefix + "Belt_Opaque";
				case BlendModeType.Additive: return SgtHelper.ShaderNamePrefix + "Belt_Additive";
			}

			return default(string);
		}

		protected override void UpdateMaterial()
		{
			var shaderName = GetShaderName();

			if (material == null)
			{
				material = SgtHelper.CreateTempMaterial("Starfield (Generated)", shaderName);
			}

			if (material.shader.name != shaderName)
			{
				material.shader = Shader.Find(shaderName);
			}

			base.UpdateMaterial();

			material.SetColor(SgtShader._Color, SgtHelper.Brighten(Color, Color.a * Brightness, false));
			material.SetFloat(SgtShader._Age, orbitOffset);

			if (lit == true)
			{
				material.SetTexture(SgtShader._LightingTex, lightingTex);
				material.SetColor(SgtShader._AmbientColor, SgtHelper.Brighten(ambientColor, ambientBrightness));

				SgtHelper.EnableKeyword("_LIT", material);
			}
			else
			{
				SgtHelper.DisableKeyword("_LIT", material);
			}

			if (powerRgb == true)
			{
				SgtHelper.EnableKeyword("_POWER_RGB", material);
			}
			else
			{
				SgtHelper.DisableKeyword("_POWER_RGB", material);
			}

			material.SetTexture(SgtShader._LightingTex, lightingTex);
		}

		protected abstract void NextQuad(ref SgtBeltAsteroid quad, int starIndex);

		protected override void BuildMesh(Mesh mesh, int count)
		{
			var positions = new Vector3[count * 4];
			var colors    = new Color[count * 4];
			var normals   = new Vector3[count * 4];
			var tangents  = new Vector4[count * 4];
			var coords1   = new Vector2[count * 4];
			var coords2   = new Vector2[count * 4];
			var indices   = new int[count * 6];
			var maxWidth  = 0.0f;
			var maxHeight = 0.0f;

			for (var i = 0; i < count; i++)
			{
				NextQuad(ref SgtBeltAsteroid.Temp, i);

				var offV     = i * 4;
				var offI     = i * 6;
				var radius   = SgtBeltAsteroid.Temp.Radius;
				var distance = SgtBeltAsteroid.Temp.OrbitDistance;
				var height   = SgtBeltAsteroid.Temp.Height;
				var uv       = tempCoords[SgtHelper.Mod(SgtBeltAsteroid.Temp.Variant, tempCoords.Count)];

				maxWidth  = Mathf.Max(maxWidth , distance + radius);
				maxHeight = Mathf.Max(maxHeight, height   + radius);

				positions[offV + 0] =
				positions[offV + 1] =
				positions[offV + 2] =
				positions[offV + 3] = new Vector3(SgtBeltAsteroid.Temp.OrbitAngle, distance, SgtBeltAsteroid.Temp.OrbitSpeed);

				colors[offV + 0] =
				colors[offV + 1] =
				colors[offV + 2] =
				colors[offV + 3] = SgtBeltAsteroid.Temp.Color;

				normals[offV + 0] = new Vector3(-1.0f,  1.0f, 0.0f);
				normals[offV + 1] = new Vector3( 1.0f,  1.0f, 0.0f);
				normals[offV + 2] = new Vector3(-1.0f, -1.0f, 0.0f);
				normals[offV + 3] = new Vector3( 1.0f, -1.0f, 0.0f);

				tangents[offV + 0] =
				tangents[offV + 1] =
				tangents[offV + 2] =
				tangents[offV + 3] = new Vector4(SgtBeltAsteroid.Temp.Angle / Mathf.PI, SgtBeltAsteroid.Temp.Spin / Mathf.PI, 0.0f, 0.0f);

				coords1[offV + 0] = new Vector2(uv.x, uv.y);
				coords1[offV + 1] = new Vector2(uv.z, uv.y);
				coords1[offV + 2] = new Vector2(uv.x, uv.w);
				coords1[offV + 3] = new Vector2(uv.z, uv.w);

				coords2[offV + 0] =
				coords2[offV + 1] =
				coords2[offV + 2] =
				coords2[offV + 3] = new Vector2(radius, height);

				indices[offI + 0] = offV + 0;
				indices[offI + 1] = offV + 1;
				indices[offI + 2] = offV + 2;
				indices[offI + 3] = offV + 3;
				indices[offI + 4] = offV + 2;
				indices[offI + 5] = offV + 1;
			}

			mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
			mesh.vertices    = positions;
			mesh.colors      = colors;
			mesh.normals     = normals;
			mesh.tangents    = tangents;
			mesh.uv          = coords1;
			mesh.uv2         = coords2;
			mesh.triangles   = indices;
			mesh.bounds      = new Bounds(Vector3.zero, new Vector3(maxWidth * 2.0f, maxHeight * 2.0f, maxWidth * 2.0f));
		}

		private void ObserverPreRender(SgtCamera observer)
		{
			if (material != null)
			{
				material.SetFloat(SgtShader._CameraRollAngle, observer.RollAngle * Mathf.Deg2Rad);
			}
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using UnityEditor;
	using TARGET = SgtBelt;

	public abstract class SgtBelt_Editor : SgtQuads_Editor
	{
		protected override void DrawMaterial(ref bool dirtyMaterial)
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			base.DrawMaterial(ref dirtyMaterial);

			Draw("blendMode", ref dirtyMaterial, "The blend mode used to render the material.");
			Draw("orbitOffset", "The amount of seconds this belt has been animating for."); // Updated automatically
			Draw("orbitSpeed", "The animation speed of this belt."); // Updated automatically
		}

		protected void DrawLighting(ref bool dirtyMaterial)
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Draw("powerRgb", ref dirtyMaterial, "Instead of just tinting the asteroids with the colors, should the RGB values be raised to the power of the color?");
			Draw("lit", ref dirtyMaterial, "If you enable this then nearby SgtLight and SgtShadow casters will be found and applied to the lighting calculations.");

			if (Any(tgts, t => t.Lit == true))
			{
				if (SgtLight.InstanceCount == 0)
				{
					Warning("You need to add the SgtLight component to your scene lights for them to work with SGT.");
				}

				BeginIndent();
					Draw("ambientColor", ref dirtyMaterial, "The belt will always be lit by this amount.");
					Draw("ambientBrightness", ref dirtyMaterial, "The <b>AmbientColor</b> will be multiplied by this.");
					Draw("maxLights", "The maximum amount of <b>SgtLight</b> components that can light this object."); // Updated automatically
					Draw("maxSphereShadows", "The maximum amount of <b>SgtShadowSphere</b> components that can shade this object."); // Updated automatically
					BeginError(Any(tgts, t => t.LightingTex == null));
						Draw("lightingTex", ref dirtyMaterial, "The look up table associating light angle with surface color. The left side is used on the dark side, the middle is used on the horizon, and the right side is used on the light side.");
					EndError();
				EndIndent();
			}

			if (Any(tgts, t => t.Lit == true && t.LightingTex == null && t.GetComponent<SgtBeltLightingTex>() == null))
			{
				Separator();

				if (Button("Add LightingTex") == true)
				{
					Each(tgts, t => SgtHelper.GetOrAddComponent<SgtBeltLightingTex>(t.gameObject));
				}
			}
		}
	}
}
#endif