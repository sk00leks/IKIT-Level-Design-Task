using UnityEngine;

namespace SpaceGraphicsToolkit
{
	/// <summary>All prefabs spawned from SgtFloatingLod and SgtFloatingSpawner___ will be attached to this GameObject.</summary>
	[ExecuteInEditMode]
	[DefaultExecutionOrder(-100)]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtFloatingRoot")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Floating Root")]
	public class SgtFloatingRoot : SgtLinkedBehaviour<SgtFloatingRoot>
	{
		public static Transform Root
		{
			get
			{
				if (InstanceCount > 0)
				{
					return FirstInstance.transform;
				}

				return null;
			}
		}

		public static Transform GetRoot()
		{
			if (InstanceCount == 0)
			{
				new GameObject("SgtFloatingRoot").AddComponent<SgtFloatingRoot>();
			}

			return FirstInstance.transform;
		}

		protected override void OnEnable()
		{
			if (InstanceCount > 0)
			{
				Debug.LogWarning("Your scene already contains an instance of SgtFloatingRoot!", FirstInstance);
			}

			base.OnEnable();
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using TARGET = SgtFloatingRoot;

	[UnityEditor.CanEditMultipleObjects]
	[UnityEditor.CustomEditor(typeof(TARGET))]
	public class SgtFloatingRoot_Editor : SgtEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Info("All prefabs spawned from SgtFloatingLod and SgtFloatingSpawner___ will be attached to this GameObject.");
		}
	}
}
#endif