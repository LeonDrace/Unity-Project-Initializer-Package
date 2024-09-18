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
	public class PackageImporter
	{
		private static AddRequest request;
		private static Queue<string> packagesToInstall = new Queue<string>();

		/// <summary>
		/// Import a Unity package by default from the default set asset store path.
		/// Must be downloaded to work.
		/// Can be set to use a custom path useful when installing them from a local device/server.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="unityPath"></param>
		public static void ImportLocalPackage(string path, bool unityPath = true)
		{
			if (unityPath)
			{
				path = GetUnityPackagePath(path);
			}

			if (File.Exists(path))
			{
				AssetDatabase.ImportPackage(path, false);
			}
			else
			{
				Debug.Log($"File At Path: {path} Was Not Found! Make Sure To Download.");
			}
		}

		/// <summary>
		/// Add url to queue and import package as a package manager request.
		/// </summary>
		/// <param name="packages"></param>
		public static void ImportUrlPackage(string url)
		{
			packagesToInstall.Enqueue(url);

			if (packagesToInstall.Count > 0)
			{
				StartNextPackageInstallation();
			}
		}

		/// <summary>
		///  Add packages to queue and import package as a package manager request.
		/// </summary>
		/// <param name="packages"></param>
		public static void ImportUrlPackages(string[] packages)
		{
			foreach (var package in packages)
			{
				packagesToInstall.Enqueue(package);
			}

			if (packagesToInstall.Count > 0)
			{
				StartNextPackageInstallation();
			}
		}

		private static async void StartNextPackageInstallation()
		{
			request = Client.Add(packagesToInstall.Dequeue());

			while (!request.IsCompleted) await Task.Delay(10);

			if (request.Status == StatusCode.Success) Debug.Log("Installed: " + request.Result.packageId);
			else if (request.Status >= StatusCode.Failure) Debug.LogError(request.Error.message);

			if (packagesToInstall.Count > 0)
			{
				await Task.Delay(1000);
				StartNextPackageInstallation();
			}
		}

		/// <summary>
		/// Get the path at which Unity downloads its asset packs.
		/// It takes a changes asset store cache into account.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static string GetUnityPackagePath(string path)
		{
			var defaultPath = Combine(GetFolderPath(SpecialFolder.ApplicationData), "Unity");
			var assetsFolder = Combine(EditorPrefs.GetString("AssetStoreCacheRootPath", defaultPath), "Asset Store-5.x");
			path = path.EndsWith(".unitypackage") ? path : path + ".unitypackage";
			path = Combine(assetsFolder, path);
			return path;
		}

		/// <summary>
		/// Validates if it is a proper URI https pattern.
		/// Does not check if package is still available.
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>
		public static bool IsValidURI(string url)
		{
			if (url.Contains("http"))
			{
				return Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult)
				&& (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
			}

			return true;
		}

		/// <summary>
		/// Validates if the package exists at the given path either Unity asset cache or a custom path.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="isCustomPath"></param>
		/// <returns></returns>
		public static bool IsValidPath(string path, bool isCustomPath)
		{
			return isCustomPath ? File.Exists(path) : File.Exists(GetUnityPackagePath(path));
		}
	}
}
