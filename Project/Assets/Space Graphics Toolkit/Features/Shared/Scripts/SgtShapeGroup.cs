using UnityEngine;
using System.Collections.Generic;
using FSA = UnityEngine.Serialization.FormerlySerializedAsAttribute;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to group multiple SgtShape___ components and treat them as a single large volume.</summary>
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtShapeGroup")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Shape Group")]
	public class SgtShapeGroup : MonoBehaviour
	{
		/// <summary>The shapes associated with this group.</summary>
		public List<SgtShape> Shapes { get { if (shapes == null) shapes = new List<SgtShape>(); return shapes; } } [FSA("Shapes")] [SerializeField] private List<SgtShape> shapes;

		public static SgtShapeGroup Create(int layer = 0, Transform parent = null)
		{
			return Create(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public static SgtShapeGroup Create(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			return SgtHelper.CreateGameObject("Shape Group", layer, parent, localPosition, localRotation, localScale).AddComponent<SgtShapeGroup>();
		}

#if UNITY_EDITOR
		[UnityEditor.MenuItem(SgtHelper.GameObjectMenuPrefix + "Shape/Group", false, 10)]
		public static void CreateMenuItem()
		{
			var parent     = SgtHelper.GetSelectedParent();
			var shapeGroup = Create(parent != null ? parent.gameObject.layer : 0, parent);

			SgtHelper.SelectAndPing(shapeGroup);
		}
#endif

		// Returns a 0..1 value, where 1 is full density
		public float GetDensity(Vector3 worldPosition)
		{
			var highestDensity = 0.0f;

			if (shapes != null)
			{
				for (var i = shapes.Count - 1; i >= 0; i--)
				{
					var shape = shapes[i];

					if (shape != null)
					{
						var density = shape.GetDensity(worldPosition);

						if (density > highestDensity)
						{
							highestDensity = density;
						}
					}
				}
			}

			return highestDensity;
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using TARGET = SgtShapeGroup;

	[UnityEditor.CanEditMultipleObjects]
	[UnityEditor.CustomEditor(typeof(TARGET))]
	public class SgtShapeGroup_Editor : SgtEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			BeginError(Any(tgts, t => t.Shapes == null || t.Shapes.Count == 0 || t.Shapes.Exists(s => s == null) == true));
				Draw("shapes", "The shapes associated with this group.");
			EndError();
		}
	}
}
#endif