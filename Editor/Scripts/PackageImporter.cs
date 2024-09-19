using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using static System.Environment;
using static System.IO.Path;

namespace LeonDrace.ProjectInitializer
{
	[InitializeOnLoad]
	public static class PackageImporter
	{
		private static AddRequest s_Request;
		private static Queue<string> s_RemotePackagesToInstall = new Queue<string>();
		private static Queue<string> s_LocalPackagesToInstall = new Queue<string>();
		private static readonly string s_TempImportDataPath = "Assets/TempInitializerData.asset";
		private static readonly string s_TempImportDataFullPath = Application.dataPath + "/TempInitializerData.asset";

		public static bool IsProcessing { get; private set; }

		[System.Serializable]
		private class ImportData
		{
			public string[] localPackages;
			public string[] remotePackages;
		}

		static PackageImporter()
		{
			AssetDatabase.importPackageStarted += OnImportPackageStarted;
			AssetDatabase.importPackageCompleted += OnImportPackageCompleted;
			AssetDatabase.importPackageFailed += OnImportPackageFailed;
			AssetDatabase.importPackageCancelled += OnImportPackageCancelled;
			ImportProcessor();
			if (s_LocalPackagesToInstall.Count == 0) ImportNextPackage();
		}

		/// <summary>
		/// Get the path at which Unity downloads its asset packs.
		/// It takes a changes asset store cache into account.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static string GetUnityAssetPath()
		{
			var defaultPath = Combine(GetFolderPath(SpecialFolder.ApplicationData), "Unity");
			var assetsFolder = Combine(EditorPrefs.GetString("AssetStoreCacheRootPath", defaultPath), "Asset Store-5.x");
			return assetsFolder;
		}

		/// <summary>
		/// Get the path at which Unity downloads its asset packs.
		/// It takes a changes asset store cache into account.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static string GetUnityPackageAtPath(string path)
		{
			var defaultPath = Combine(GetFolderPath(SpecialFolder.ApplicationData), "Unity");
			var assetsFolder = Combine(EditorPrefs.GetString("AssetStoreCacheRootPath", defaultPath), "Asset Store-5.x");
			path = path.EndsWith(".unitypackage") ? path : path + ".unitypackage";
			path = Combine(assetsFolder, path);
			return path;
		}

		/// <summary>
		/// Validates if the package exists at the given path either Unity asset cache or a custom path.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="isCustomPath"></param>
		/// <returns></returns>
		public static bool IsValidPath(string path, bool isCustomPath)
		{
			return isCustomPath ? File.Exists(path) : File.Exists(GetUnityPackageAtPath(path));
		}

		/// <summary>
		/// Import a Unity package by default from the default set asset store path.
		/// Must be downloaded to work.
		/// Can be set to use a custom path useful when installing them from a local device/server.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="unityPath"></param>
		public static void ImportLocalPackage(string path, bool hasCustomPath)
		{
			var package = new KeyValuePair<string, bool>[] { new KeyValuePair<string, bool>(path, hasCustomPath) };
			AddLocalPackagesToQueue(package);

			if (File.Exists(path))
			{
				ImportNextPackage();
			}
			else
			{
				Debug.Log($"File At Path: {path} Was Not Found! Make Sure To Download.");
			}
		}

		public static void ImportLocalPackages(KeyValuePair<string, bool>[] localPackages)
		{
			AddLocalPackagesToQueue(localPackages);
			ImportNextPackage();
		}

		/// <summary>
		///  Add packages to queue and import package as a package manager request.
		/// </summary>
		/// <param name="packages"></param>
		public static void ImportRemotePackage(string package)
		{
			s_RemotePackagesToInstall.Enqueue(package);
			ImportNextPackage();
		}

		/// <summary>
		///  Add packages to queue and import package as a package manager request.
		/// </summary>
		/// <param name="packages"></param>
		public static void ImportRemotePackages(string[] packages)
		{
			foreach (var package in packages)
			{
				s_RemotePackagesToInstall.Enqueue(package);
			}
			ImportNextPackage();
		}

		public static void ImportAll(KeyValuePair<string, bool>[] localPackages, string[] remotePackages)
		{
			AddLocalPackagesToQueue(localPackages);
			AddRemotePackagesToQueue(remotePackages);
			ImportNextPackage();
		}

		private static void AddLocalPackagesToQueue(KeyValuePair<string, bool>[] localPackages)
		{
			s_LocalPackagesToInstall = new Queue<string>();
			for (int i = 0; i < localPackages.Length; i++)
			{
				string key = localPackages[i].Key;
				bool customPath = localPackages[i].Value;
				string path = customPath ? key : GetUnityPackageAtPath(key);
				s_LocalPackagesToInstall.Enqueue(path);
			}
		}

		private static void AddRemotePackagesToQueue(string[] remotePackages)
		{
			s_RemotePackagesToInstall = new Queue<string>(remotePackages);
		}

		private static void ImportNextLocalPackage()
		{
			IsProcessing = true;
			string path = s_LocalPackagesToInstall.Dequeue();
			CreateTempImportDataAsset();

			if (File.Exists(path))
			{
				AssetDatabase.ImportPackage(path, false);
			}
			else
			{
				Debug.Log($"File At Path: {path} Was Not Found! Make Sure To Download.");
			}
		}

		private static async void ImportNextRemotePackage()
		{
			IsProcessing = true;
			await Task.Delay(1000);
			string path = s_RemotePackagesToInstall.Dequeue();
			Debug.Log($"Started importing remote package: {path}");
			s_Request = Client.Add(path);
			CreateTempImportDataAsset();

			while (!s_Request.IsCompleted) await Task.Delay(10);

			if (s_Request.Status == StatusCode.Success) Debug.Log("Imported package: " + s_Request.Result.packageId);
			else if (s_Request.Status >= StatusCode.Failure) Debug.LogError(s_Request.Error.message);

			Debug.Log($"{s_Request.Status} Installing Package: {path} - Packages Left: " + s_RemotePackagesToInstall.Count);
			IsProcessing = false;
			ImportNextPackage();
		}

		private static void CreateTempImportDataAsset()
		{
			if (s_LocalPackagesToInstall.Count == 0 && s_RemotePackagesToInstall.Count == 0)
			{
				DeleteTempData();
				return;
			}

			var data = new ImportData();
			data.localPackages = s_LocalPackagesToInstall.ToArray();
			data.remotePackages = s_RemotePackagesToInstall.ToArray();
			string json = JsonUtility.ToJson(data, true);
			var text = new TextAsset(json);
			AssetDatabase.CreateAsset(text, s_TempImportDataPath);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}

		private static void OnImportPackageCancelled(string packageName)
		{
			Debug.Log($"Cancelled the import of package: {packageName}");
			IsProcessing = false;
			s_LocalPackagesToInstall.Clear();
			s_RemotePackagesToInstall.Clear();
			DeleteTempData();
		}

		private static void OnImportPackageCompleted(string packagename)
		{
			Debug.Log($"Imported package: {packagename}");
			IsProcessing = false;
			ImportNextPackage();
		}

		private static void OnImportPackageFailed(string packagename, string errormessage)
		{
			Debug.LogWarning($"Failed importing package: {packagename} with error: {errormessage}");
			IsProcessing = false;
			ImportNextPackage();
		}

		private static void OnImportPackageStarted(string packagename)
		{
			Debug.Log($"Started importing package: {packagename}");
			IsProcessing = true;
		}

		private static void ImportProcessor()
		{
			if (!File.Exists(s_TempImportDataFullPath)) return;

			var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(s_TempImportDataPath);
			ImportData data = JsonUtility.FromJson<ImportData>(asset.text);
			s_LocalPackagesToInstall = new Queue<string>(data.localPackages);
			s_RemotePackagesToInstall = new Queue<string>(data.remotePackages);
		}

		private static void ImportNextPackage()
		{
			if (IsProcessing) return;

			if (s_LocalPackagesToInstall.Count > 0)
			{
				ImportNextLocalPackage();
			}
			else if (s_RemotePackagesToInstall.Count > 0)
			{
				ImportNextRemotePackage();
			}
			else
			{
				DeleteTempData();
			}
		}

		private static void DeleteTempData()
		{
			if (File.Exists(s_TempImportDataFullPath))
			{
				AssetDatabase.DeleteAsset(s_TempImportDataPath);
				AssetDatabase.Refresh();
			}
		}
	}
}
