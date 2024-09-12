using Codice.Utils;
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
				var defaultPath = Combine(GetFolderPath(SpecialFolder.ApplicationData), "Unity");
				var assetsFolder = Combine(EditorPrefs.GetString("AssetStoreCacheRootPath", defaultPath), "Asset Store-5.x");
				path = path.EndsWith(".unitypackage") ? path : path + ".unitypackage";
				path = Combine(assetsFolder, path);
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
	}
}
