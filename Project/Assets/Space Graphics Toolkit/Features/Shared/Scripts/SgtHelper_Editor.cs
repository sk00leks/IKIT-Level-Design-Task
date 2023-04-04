#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace SpaceGraphicsToolkit
{
	/// <summary>This class contains some useful editor-only features used by most other SGT code.</summary>
	public static partial class SgtHelper
	{
		private static string undoName;

		public static void BeginUndo(string newUndoName)
		{
			undoName = newUndoName;
		}

		public static Rect Reserve(float height = 16.0f)
		{
			var rect = default(Rect);

			rect = EditorGUILayout.BeginVertical();
			{
				EditorGUILayout.LabelField(string.Empty, GUILayout.Height(height));
			}
			EditorGUILayout.EndVertical();

			return rect;
		}

		public static void RequireCamera()
		{
			if (SgtCamera.InstanceCount == 0)
			{
				SgtEditor.Separator();

				if (SgtEditor.HelpButton("Your scene contains no SgtCameras", MessageType.Error, "Fix", 50.0f) == true)
				{
					ClearSelection();

					foreach (var camera in Camera.allCameras)
					{
						AddToSelection(camera.gameObject);

						GetOrAddComponent<SgtCamera>(camera.gameObject);
					}
				}
			}
		}

		public static void RequireDepth()
		{
			var found = false;

			foreach (var camera in Camera.allCameras)
			{
				var mask = camera.depthTextureMode;

				if (mask == DepthTextureMode.DepthNormals || ((int)mask & 1) != 0)
				{
					found = true; break;
				}
			}

			if (found == false)
			{
				SgtEditor.Separator();

				if (Camera.main != null)
				{
					if (WritesDepth(Camera.main) == false)
					{
						if (SgtEditor.HelpButton("This component requires your camera to render a Depth Texture, but it doesn't.", UnityEditor.MessageType.Error, "Fix", 50.0f) == true)
						{
							GetOrAddComponent<SgtDepthTextureMode>(Camera.main.gameObject).DepthMode = DepthTextureMode.Depth;

							SelectAndPing(Camera.main);
						}
					}
				}
				else
				{
					SgtEditor.Error("This component requires your camera to render a Depth Texture, but none of the cameras in your scene do. This can be fixed with the SgtDepthTextureMode component.");

					foreach (var camera in Camera.allCameras)
					{
						if (Enabled(camera) == true)
						{
							GetOrAddComponent<SgtDepthTextureMode>(camera.gameObject).DepthMode = DepthTextureMode.Depth;

							SelectAndPing(camera);
						}
					}
				}
			}
		}

		private static bool WritesDepth(Camera camera)
		{
			return camera != null && camera.depthTextureMode == DepthTextureMode.DepthNormals || ((int)camera.depthTextureMode & 1) != 0;
		}

		public static T LoadFirstAsset<T>(string pattern) // e.g. "Name t:mesh"
			where T : Object
		{
			var guids = UnityEditor.AssetDatabase.FindAssets(pattern);

			if (guids.Length > 0)
			{
				var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);

				return (T)UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(T));
			}

			return null;
		}

		public static T GetAssetImporter<T>(Object asset)
			where T : AssetImporter
		{
			return GetAssetImporter<T>((AssetDatabase.GetAssetPath(asset)));
		}

		public static T GetAssetImporter<T>(string path)
			where T : AssetImporter
		{
			return AssetImporter.GetAtPath(path) as T;
		}

		public static void ReimportAsset(Object asset)
		{
			ReimportAsset(AssetDatabase.GetAssetPath(asset));
		}

		public static void ReimportAsset(string path)
		{
			AssetDatabase.ImportAsset(path);
		}

		public static void MakeTextureReadable(Texture texture)
		{
			if (texture != null)
			{
				var importer = GetAssetImporter<TextureImporter>(texture);

				if (importer != null && importer.isReadable == false)
				{
					importer.isReadable = true;

					ReimportAsset(importer.assetPath);
				}
			}
		}

		public static void MakeTextureTruecolor(Texture2D texture)
		{
			if (texture != null)
			{
				var importer = GetAssetImporter<TextureImporter>(texture);

				if (importer != null)
				{
					if (importer.textureCompression != TextureImporterCompression.Uncompressed)
					{
						importer.textureCompression = TextureImporterCompression.Uncompressed;

						ReimportAsset(importer.assetPath);
					}
				}
			}
		}

		public static void ClearSelection()
		{
			Selection.objects = new Object[0];
		}

		public static void AddToSelection(Object o)
		{
			var os = new List<Object>(Selection.objects);

			os.Add(o);

			Selection.objects = os.ToArray();
		}

		public static void SelectAndPing(Object o)
		{
			Selection.activeObject = o;

			EditorApplication.delayCall += () => EditorGUIUtility.PingObject(o);
		}

		public static Transform GetSelectedParent()
		{
			if (Selection.activeGameObject != null)
			{
				return Selection.activeGameObject.transform;
			}

			return null;
		}

		public static void DestroyOldGameObjects(Transform parent, string name)
		{
			while (TryDestroyOldGameObject(parent, name))
			{
				Debug.Log("SGT Upgrade: Destroyed old " + name, parent);
			}
		}

		private static List<Component> tempComponents = new List<Component>();

		public static bool TryDestroyOldGameObject(Transform parent, string name)
		{
			foreach (Transform child in parent)
			{
				if (child.name == name)
				{
					child.GetComponents(tempComponents);

					foreach (var component in tempComponents)
					{
						if (component == null)
						{
							Undo.RecordObject(parent, "Removing " + name);

							Undo.DestroyObjectImmediate(child.gameObject);

							UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(parent.gameObject.scene);

							return true;
						}
					}
				}
			}

			return false;
		}

		public static TextureImporter ExportTextureDialog(Texture2D texture2D, string title)
		{
			if (texture2D != null)
			{
				var root = Application.dataPath;
				var path = EditorUtility.SaveFilePanel("Export " + title, root, title, "png");

				if (string.IsNullOrEmpty(path) == false)
				{
					var data = texture2D.EncodeToPNG();

					System.IO.File.WriteAllBytes(path, data);

					Debug.Log("Exported " + title + " Texture to " + path);

					if (path.StartsWith(root) == true)
					{
						var local = path.Substring(root.Length - "Assets".Length);

						AssetDatabase.ImportAsset(local);

						return GetAssetImporter<TextureImporter>(local);
					}
				}
			}

			return null;
		}

		public static AssetImporter ExportAssetDialog(Object asset, string title)
		{
			if (asset != null)
			{
				var root = Application.dataPath;
				var path = EditorUtility.SaveFilePanel("Export " + title, root, title, "asset");

				if (string.IsNullOrEmpty(path) == false)
				{
					if (path.StartsWith(root) == true)
					{
						var local = path.Substring(root.Length - "Assets".Length);

						Debug.Log("Exported " + title + " Asset to " + local);

						var clone = Object.Instantiate(asset);

						AssetDatabase.CreateAsset(clone, local);

						return GetAssetImporter<AssetImporter>(local);
					}
				}
			}

			return null;
		}
	}
}
#endif