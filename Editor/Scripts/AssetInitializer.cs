using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static System.IO.Path;
using static UnityEditor.AssetDatabase;

namespace LeonDrace.ProjectInitializer
{
	public class AssetInitializer
	{
		public static readonly string ArchitectureFilter = "l:architecture";
		public static bool IsProcessing { get; private set; }

		public static void CreateFolderStructure(FolderStructure folderStructure)
		{
			var data = SearchForConfig<ProjectInitializerData>(ArchitectureFilter);

			if (data == null)
			{
				Debug.LogError("Failed To Load Project Initializer Data.");
				return;
			}

			IsProcessing = true;

			CreateFolders(folderStructure.RootFolder, folderStructure.CreatedFolders);
			Refresh();
			var currentPaths = folderStructure.MovedFiles.Select(x => x.CurrentPath).ToArray();
			var targetPaths = folderStructure.MovedFiles.Select(x => x.TargetPath).ToArray();
			MoveAssets(folderStructure.RootFolder, currentPaths, targetPaths);
			Refresh();
			DeleteFolders(folderStructure.DeletedFolders);
			Refresh();

			IsProcessing = false;
		}

		public static string GetFullRootPath(string root)
		{
			return Combine(Application.dataPath, root);
		}

		public static bool DirectoryExists(string path)
		{
			return Directory.Exists(path);
		}

		public static void CreateFolders(string root, params string[] folders)
		{
			var fullpath = GetFullRootPath(root);
			if (!Directory.Exists(fullpath))
			{
				Directory.CreateDirectory(fullpath);
			}

			foreach (var folder in folders)
			{
				CreateSubFolders(fullpath, folder);
			}
		}

		private static void CreateSubFolders(string rootPath, string folderHierarchy)
		{
			var folders = folderHierarchy.Split('/');
			var currentPath = rootPath;

			foreach (var folder in folders)
			{
				currentPath = Combine(currentPath, folder);
				if (!Directory.Exists(currentPath))
				{
					Directory.CreateDirectory(currentPath);
				}
			}
		}

		public static void MoveAssets(string rootName, string[] currentPath, string[] targetPath)
		{
			int count = currentPath.Length;
			for (int i = 0; i < count; i++)
			{
				Move(rootName, currentPath[i], targetPath[i]);
			}
		}

		public static void Move(string rootName, string currentPath, string targetPath)
		{
			if (IsValidFolder(currentPath) || File.Exists(currentPath))
			{
				targetPath = targetPath.Replace("$root", rootName);
				var error = MoveAsset(currentPath, targetPath);

				if (!string.IsNullOrEmpty(error))
				{
					Debug.LogError($"Failed to move {targetPath}: {error}");
				}
			}
		}

		public static void DeleteFolders(params string[] folders)
		{
			foreach (var folder in folders)
			{
				Delete(folder);
			}
		}

		public static void Delete(string folderName)
		{
			var pathToDelete = $"Assets/{folderName}";

			if (IsValidFolder(pathToDelete))
			{
				DeleteAsset(pathToDelete);
			}
		}

		/// <summary>
		/// Import json file and apply it to the given object.
		/// </summary>
		/// <param name="updatedObject"></param>
		/// <returns></returns>
		public static string ImportJson(string path, object updatedObject)
		{
			if (path != null && path.Length != 0 && path.Contains(".json"))
			{
				JsonDeserialiserOverwrite(path, updatedObject);
				return string.Empty;
			}
			else
			{
				return "Nothing Selected Or Wrong File Format!";
			}
		}

		/// <summary>
		/// Will overwrite the data on an object.
		/// Useful for <see cref="ScriptableObject"/> which are instanced differently from normal objects.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="overwrittenObject"></param>
		public static void JsonDeserialiserOverwrite(string path, object overwrittenObject)
		{
			StreamReader reader = new StreamReader(path);
			string json = reader.ReadToEnd();
			reader.Close();
			JsonUtility.FromJsonOverwrite(json, overwrittenObject);
		}

		/// <summary>
		/// Export object to folder location with given file name.
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="exportedObject"></param>
		/// <returns></returns>
		public static string ExportJson(string path, object exportedObject)
		{
			if (path != null && path.Length != 0)
			{
				JsonSerialiser<ProjectInitializerData>(exportedObject, path);
				return $"Presets Have Been Exported To {path}";
			}
			else
			{
				return "Could Not Export To Location!";
			}
		}

		/// <summary>
		/// Create a json file at the given path.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="obj"></param>
		/// <param name="path"></param>
		public static void JsonSerialiser<T>(object obj, string path)
		{
			string json = JsonUtility.ToJson((T)obj, true);
			StreamWriter writer = new StreamWriter(path, false);
			writer.Write(json);
			writer.Close();
		}

		/// <summary>
		/// Search the asset database by the given type T and filter.
		/// A filter will improve search speed significantly.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="filter"></param>
		/// <returns></returns>
		public static T SearchForConfig<T>(string filter) where T : ScriptableObject
		{
			string[] guids = AssetDatabase.FindAssets(filter);
			foreach (string guid in guids)
			{
				T config = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
				if (config != null)
				{
					return config;
				}
			}
			Debug.LogError($"The requested config was not found: {typeof(T)} it might be missing the config asset label.");
			return null;
		}

	}
}
