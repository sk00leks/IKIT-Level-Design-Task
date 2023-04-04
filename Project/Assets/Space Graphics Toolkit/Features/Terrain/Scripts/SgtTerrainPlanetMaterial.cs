using UnityEngine;
using Unity.Mathematics;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to render the <b>SgtTerrain</b> component using the <b>SGT Planet</b> shader.</summary>
	[ExecuteInEditMode]
	[DefaultExecutionOrder(200)]
	[RequireComponent(typeof(SgtTerrain))]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtTerrainPlanetMaterial")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Terrain Planet Material")]
	public class SgtTerrainPlanetMaterial : MonoBehaviour
	{
		/// <summary>The planet material that will be rendered.</summary>
		public Material Material { set { material = value; } get { return material; } } [SerializeField] private Material material;

		/// <summary>Normals bend incorrectly on high detail planets, so it's a good idea to fade them out. This allows you to set the camera distance at which the normals begin to fade out in local space.</summary>
		public double NormalFadeRange { set { normalFadeRange = value; } get { return normalFadeRange; } } [SerializeField] private double normalFadeRange;

		/// <summary>This allows you to specify the terrain used for the water surface. This is used to control where the beaches appear, if you enable that material feature.</summary>
		public SgtTerrain Water { set { if (water != value) { water = value; MarkAsDirty(); } } get { return water; } } [SerializeField] private SgtTerrain water;

		protected SgtTerrain cachedTerrain;

		private float bumpScale;

		protected int bakedDetailTilingA;
		protected int bakedDetailTilingB;
		protected int bakedDetailTilingC;

		public void MarkAsDirty()
		{
			if (cachedTerrain != null)
			{
				cachedTerrain.MarkAsDirty();
			}
		}

		protected virtual void OnEnable()
		{
			cachedTerrain = GetComponent<SgtTerrain>();

			cachedTerrain.OnDrawQuad += HandleDrawQuad;
		}

		protected virtual void OnDisable()
		{
			cachedTerrain.OnDrawQuad -= HandleDrawQuad;

			MarkAsDirty();
		}

		protected virtual void OnDidApplyAnimationProperties()
		{
			MarkAsDirty();
		}

		protected virtual void Update()
		{
			if (normalFadeRange > 0.0 && SgtHelper.Enabled(cachedTerrain) == true)
			{
				var localPosition = cachedTerrain.GetObserverLocalPosition();
				var localAltitude = math.length(localPosition);
				var localHeight   = cachedTerrain.GetLocalHeight(localPosition);

				bumpScale = (float)math.saturate((localAltitude - localHeight) / normalFadeRange);
			}
			else
			{
				bumpScale = 1.0f;
			}

			var cachedTerrainPlanet = cachedTerrain as SgtTerrainPlanet;

			if (cachedTerrainPlanet != null)
			{
				bakedDetailTilingA = cachedTerrainPlanet.BakedDetailTilingA;
				bakedDetailTilingB = cachedTerrainPlanet.BakedDetailTilingB;
				bakedDetailTilingC = cachedTerrainPlanet.BakedDetailTilingC;
			}
		}

		private void HandleDrawQuad(Camera camera, SgtTerrainQuad quad, Matrix4x4 matrix, int layer)
		{
			if (material != null)
			{
				var properties = quad.Properties;

				PreRenderMeshes(properties);

				foreach (var mesh in quad.CurrentMeshes)
				{
					Graphics.DrawMesh(mesh, matrix, material, gameObject.layer, camera, 0, properties);
				}
			}
		}

		protected virtual void PreRenderMeshes(SgtProperties properties)
		{
			properties.SetFloat(SgtShader._BumpScale, bumpScale);

			properties.SetInt(Shader.PropertyToID("_BakedDetailTilingA"), bakedDetailTilingA);
			properties.SetFloat(Shader.PropertyToID("_BakedDetailTilingAMul"), bakedDetailTilingA / 64.0f);
			properties.SetInt(Shader.PropertyToID("_BakedDetailTilingB"), bakedDetailTilingB);
			properties.SetInt(Shader.PropertyToID("_BakedDetailTilingC"), bakedDetailTilingC);

			if (water != null)
			{
				properties.SetFloat(Shader.PropertyToID("_ShoreHeight"), (float)(water.Radius - cachedTerrain.Radius));
			}
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using TARGET = SgtTerrainPlanetMaterial;

	[UnityEditor.CanEditMultipleObjects]
	[UnityEditor.CustomEditor(typeof(TARGET))]
	public class SgtTerrainPlanetMaterial_Editor : SgtEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			var markAsDirty = false;

			BeginError(Any(tgts, t => t.Material == null));
				Draw("material", "The planet material that will be rendered.");
			EndError();
			Draw("normalFadeRange", "Normals bend incorrectly on high detail planets, so it's a good idea to fade them out. This allows you to set the camera distance at which the normals begin to fade out in local space.");
			Draw("water", "This allows you to specify the terrain used for the water surface. This is used to control where the beaches appear, if you enable that material feature.");

			if (markAsDirty == true)
			{
				Each(tgts, t => t.MarkAsDirty());
			}
		}
	}
}
#endif