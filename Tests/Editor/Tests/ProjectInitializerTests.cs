using NUnit.Framework;
using System.IO;
using System.Linq;
using UnityEditor;

namespace LeonDrace.ProjectInitializer.Tests
{
	public class ProjectInitializerTests
	{

		[Test]
		public void ProjectInitializerDataExists()
		{
			var data = AssetInitializer.SearchForConfig<ProjectInitializerData>(AssetInitializer.ArchitectureFilter);
			Assert.That(data != null);
		}

		[Test]
		public void DefaultPresetsExist()
		{
			var data = AssetInitializer.SearchForConfig<ProjectInitializerData>(AssetInitializer.ArchitectureFilter);

			var smallProjectPreset = data.Presets.First(x => x.Name == "Small Project");
			var packagePreset = data.Presets.First(x => x.Name == "Package");

			Assert.That(smallProjectPreset != null && packagePreset != null);
		}

		[Test]
		public void CreatePreset()
		{
			var data = AssetInitializer.SearchForConfig<ProjectInitializerData>(AssetInitializer.ArchitectureFilter);

			int count = data.Presets.Length;
			data.AddNewPreset();

			Assert.That(data.Presets.Length, Is.EqualTo(count + 1));

			data.RemovePresetAt(data.Presets.Length - 1);
		}

		[Test]
		public void CreateAndDeleteFolders()
		{
			string root = "Temp";
			string subFolder = "Temp1";
			var path = AssetInitializer.GetFullRootPath(root);

			AssetInitializer.CreateFolders(root, subFolder);
			AssetDatabase.Refresh();
			Assert.That(AssetInitializer.DirectoryExists(path) && AssetInitializer.DirectoryExists(path + "/" + subFolder));

			AssetInitializer.DeleteFolders(root);
			AssetDatabase.Refresh();
			Assert.That(!AssetInitializer.DirectoryExists(path));
		}

		[Test]
		public void MoveFile()
		{
			string root1 = "Temp";
			string root2 = "TempMaterial.mat";
			var rootPath = AssetInitializer.GetFullRootPath(root1);
			var currentPath = "Assets/" + root2;
			var targetPath = "Assets/" + root1 + "/" + root2;

			AssetInitializer.CreateFolders(root1);
			AssetDatabase.CreateAsset(new UnityEngine.Material(UnityEngine.Shader.Find("Standard")), currentPath);
			AssetDatabase.Refresh();

			AssetInitializer.Move(root1, currentPath, targetPath);
			AssetDatabase.Refresh();
			Assert.That(File.Exists(rootPath + "/" + root2));

			AssetInitializer.DeleteFolders(root1);
			AssetDatabase.Refresh();
			Assert.That(!AssetInitializer.DirectoryExists(rootPath));
		}

		[Test]
		public void MoveFolder()
		{
			string root1 = "Temp";
			string root2 = "Temp1";
			var rootPath = AssetInitializer.GetFullRootPath(root1);
			var currentPath = "Assets/" + root2;
			var targetPath = "Assets/" + root1 + "/" + root2;

			AssetInitializer.CreateFolders(root1);
			AssetInitializer.CreateFolders(root2);
			AssetDatabase.Refresh();

			AssetInitializer.Move(root1, currentPath, targetPath);
			AssetDatabase.Refresh();
			Assert.That(AssetInitializer.DirectoryExists(rootPath + "/" + root2));

			AssetInitializer.DeleteFolders(root1);
			AssetDatabase.Refresh();
			Assert.That(!AssetInitializer.DirectoryExists(rootPath));
		}

		[Test]
		public void ImportExportJson()
		{
			var data = AssetInitializer.SearchForConfig<ProjectInitializerData>(AssetInitializer.ArchitectureFilter);

			string fileName = "Temp.json";
			string path = UnityEngine.Application.dataPath + "/" + fileName;

			AssetInitializer.ExportJson(path, data);
			AssetDatabase.Refresh();
			Assert.That(File.Exists(path));

			var testData = UnityEngine.ScriptableObject.CreateInstance<ProjectInitializerData>();
			Assert.That(testData.Presets, Is.EqualTo(null));
			AssetInitializer.ImportJson(path, testData);
			Assert.That(testData.Presets.Length, Is.EqualTo(data.Presets.Length));

			AssetDatabase.DeleteAsset("Assets/" + fileName);
			AssetDatabase.Refresh();
			Assert.That(!File.Exists(path));
		}
	}
}