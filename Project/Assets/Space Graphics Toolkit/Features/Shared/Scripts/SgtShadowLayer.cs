using UnityEngine;
using System.Collections.Generic;
using FSA = UnityEngine.Serialization.FormerlySerializedAsAttribute;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to add shadows cast from an SgtShadow___ component to any opaque renderer in your scene.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtShadowLayer")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Shadow Layer")]
	public class SgtShadowLayer : MonoBehaviour
	{
		/// <summary>The radius of this shadow receiver.</summary>
		public float Radius { set { radius = value; } get { return radius; } } [FSA("Radius")] [SerializeField] private float radius = 1.0f;

		/// <summary>The renderers you want the shadows to be applied to.</summary>
		public List<MeshRenderer> Renderers { get { if (renderers == null) renderers = new List<MeshRenderer>(); return renderers; } } [FSA("Renderers")] [SerializeField] private List<MeshRenderer> renderers;

		// The material added to all spacetime renderers
		[System.NonSerialized]
		private Material material;

		[ContextMenu("Apply Material")]
		public void ApplyMaterial()
		{
			if (renderers != null)
			{
				for (var i = renderers.Count - 1; i >= 0; i--)
				{
					SgtHelper.AddMaterial(renderers[i], material);
				}
			}
		}

		[ContextMenu("Remove Material")]
		public void RemoveMaterial()
		{
			if (renderers != null)
			{
				for (var i = renderers.Count - 1; i >= 0; i--)
				{
					SgtHelper.RemoveMaterial(renderers[i], material);
				}
			}
		}

		public void AddRenderer(MeshRenderer renderer)
		{
			if (renderer != null)
			{
				if (renderers == null)
				{
					renderers = new List<MeshRenderer>();
				}

				if (renderers.Contains(renderer) == false)
				{
					renderers.Add(renderer);

					SgtHelper.AddMaterial(renderer, material);
				}
			}
		}

		public void RemoveRenderer(MeshRenderer renderer)
		{
			if (renderer != null && renderers != null)
			{
				if (renderers.Remove(renderer) == true)
				{
					SgtHelper.RemoveMaterial(renderer, material);
				}
			}
		}

		protected virtual void OnEnable()
		{
			SgtCamera.OnCameraPreRender += CameraPreRender;

			if (material == null)
			{
				material = SgtHelper.CreateTempMaterial("Shadow Layer (Generated)", SgtHelper.ShaderNamePrefix + "ShadowLayer");
			}

			if (renderers == null)
			{
				AddRenderer(GetComponent<MeshRenderer>());
			}

			ApplyMaterial();
		}

#if UNITY_EDITOR
		protected virtual void OnDrawGizmosSelected()
		{
			Gizmos.DrawWireSphere(transform.position, SgtHelper.UniformScale(transform.lossyScale) * radius);
		}
#endif

		protected virtual void OnDisable()
		{
			SgtCamera.OnCameraPreRender -= CameraPreRender;

			RemoveMaterial();
		}

		protected virtual void CameraPreRender(Camera camera)
		{
			if (material != null)
			{
				SgtHelper.SetTempMaterial(material);

				var mask   = 1 << gameObject.layer;
				var lights = SgtLight.Find(true, mask, transform.position);

				SgtShadow.Find(true, mask, lights);
				SgtShadow.FilterOutSphere(transform.position);
				SgtShadow.FilterOutMiss(transform.position, SgtHelper.UniformScale(transform.lossyScale) * radius);
				SgtShadow.WriteSphere(2);
				SgtShadow.WriteRing(1);
			}
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using TARGET = SgtShadowLayer;

	[UnityEditor.CanEditMultipleObjects]
	[UnityEditor.CustomEditor(typeof(TARGET))]
	public class SgtShadowLayer_Editor : SgtEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Draw("radius", "The radius of this shadow receiver.");

			Separator();

			Each(tgts, t => { if (t.isActiveAndEnabled == true) t.RemoveMaterial(); });
				BeginError(Any(tgts, t => t.Renderers != null && t.Renderers.Exists(s => s == null)));
					Draw("renderers", "The renderers you want the shadows to be applied to.");
				EndError();
			Each(tgts, t => { if (t.isActiveAndEnabled == true) t.ApplyMaterial(); });
		}
	}
}
#endif