using System;
using System.IO;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using static System.Environment;
using static System.IO.Path;
using static UnityEditor.AssetDatabase;

namespace LeonDrace.ProjectInitializer
{
	public class PackageImporter
	{
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
				ImportPackage(path, false);
			}
			else
			{
				Debug.Log($"File At Path: {path} Was Not Found! Make Sure To Download.");
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
		/// <param name="uri"></param>
		/// <returns></returns>
		public static bool IsValidURI(string uri)
		{
			return Uri.TryCreate(uri, UriKind.Absolute, out Uri uriResult)
				&& (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
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
