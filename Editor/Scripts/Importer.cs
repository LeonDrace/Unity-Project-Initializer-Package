using System.IO;
using System.Linq;
using UnityEngine;

using static System.IO.Path;
using static UnityEditor.AssetDatabase;

namespace LeonDrace.ProjectInitializer
{
	public class Importer
	{
		public static void CreateFolderStructure()
		{
			var data = Resources.Load<ProjectInitializerData>("Project Initializer Data");

			if (data == null)
			{
				Debug.LogError("Failed To Load Project Initializer Data.");
				return;
			}

			Folders.CreateFolders(data.GetFolderStructure.RootFolder, data.GetFolderStructure.CreatedFolders);
			Refresh();
			var currentPaths = data.GetFolderStructure.MovedFiles.Select(x => x.CurrentPath).ToArray();
			var targetPaths = data.GetFolderStructure.MovedFiles.Select(x => x.TargetPath).ToArray();
			Folders.MoveAssets(currentPaths, targetPaths);
			Refresh();
			Folders.DeleteFolders(data.GetFolderStructure.DeletedFolders);
			Refresh();
		}

		private static class Folders
		{
			public static void CreateFolders(string root, params string[] folders)
			{
				var fullpath = Combine(Application.dataPath, root);
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

			public static void MoveAssets(string[] currentPath, string[] targetPath)
			{
				int count = currentPath.Length;
				for (int i = 0; i < count; i++)
				{
					Move(currentPath[i], targetPath[i]);
				}
			}

			public static void Move(string currentPath, string targetPath)
			{
				if (IsValidFolder(currentPath) || File.Exists(currentPath))
				{
					var destinationPath = $"{targetPath}";
					var error = MoveAsset(currentPath, destinationPath);

					if (!string.IsNullOrEmpty(error))
					{
						Debug.LogError($"Failed to move {targetPath}: {error}");
					}
				}
			}

			public static void DeleteFolders(string[] folders)
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
		}
	}
}
