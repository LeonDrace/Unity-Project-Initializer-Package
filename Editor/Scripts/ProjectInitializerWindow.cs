using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LeonDrace.ProjectInitializer
{
	public class ProjectInitializerWindow : EditorWindow
	{
		#region Static Open Window

		[MenuItem("Window/LeonDrace/Project Initializer")]
		public static void ShowWindow()
		{
			EditorWindow.GetWindow<ProjectInitializerWindow>("Project Initializer");
		}

		#endregion

		#region Window Drawing

		private Vector2 m_ScrollPos;

		//Containers
		private string m_FolderCreationTitle = "Folder Structure";
		private string m_PresetsTitle = "Presets";
		private string m_NoPresets = "Create a Preset!";
		private string m_LocalPackagesTitle = "Local Packages";
		private string m_UrlPackagesTitle = "Url Packages";
		private string m_DebugTitle = "Debug";
		private string m_ImportExportTitle = "Import/Export";

		//Data
		private ProjectInitializerData m_Data;
		private SerializedObject m_DataSerializedObject;

		//Index
		private int m_SelectedPresetIndex = 0;

		//Buttons
		private Color m_DefaultGuiBackgroundColor = Color.white;
		private Color m_DefaultGuiContentColor = Color.white;
		private string m_CreateFolderButton = "Create Folder Setup";
		private string m_DeletePresetButton = "Delete Preset";
		private string m_DeleteMessage = "Are You Sure You Want To Delete The Entire Preset?";

		//Import Export
		private string m_ExportName = "New Preset";
		private string m_ExportMessage = string.Empty;
		private string m_ImportMessage = string.Empty;

		//Packages
		private string m_defaultLocalPackageModeTitle = "Default";
		private string m_filteredLocalPackageModeTitle = "Filtered";
		private string m_InvalidPathMessage = "Invalid Path!. File does not exist at location.";
		private bool m_defaultLocalPackages = false;
		private bool m_defaultUrlPackages = false;
		private Color m_InvalidPackageColor = Color.red;
		private Color m_InvalidDisabledPackageColor = Color.yellow;

		private void OnEnable()
		{
			m_Data = AssetInitializer.SearchForConfig<ProjectInitializerData>(AssetInitializer.ArchitectureFilter);
			m_DefaultGuiBackgroundColor = GUI.backgroundColor;
			m_DefaultGuiContentColor = GUI.contentColor;
		}

		private void OnGUI()
		{
			//In case it has been updated on the scriptable itself.
			m_DataSerializedObject = new SerializedObject(m_Data);
			m_ScrollPos = EditorGUILayout.BeginScrollView(m_ScrollPos);

			DrawImporterExporter();
			GUILayout.Space(5);
			DrawPresets();
			GUILayout.Space(5);
			DrawFolderSetup();
			GUILayout.Space(5);
			DrawLocalPackages();
			GUILayout.Space(5);
			DrawUrlPackages();
			GUILayout.Space(5);
			DrawDebugOptions();

			EditorGUILayout.EndScrollView();
		}

		#region Preset - Tabs

		private void DrawPresets()
		{
			var presets = m_Data.Presets;
			CreateContainer(m_PresetsTitle, () =>
			{
				EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

				int counter = 0;
				foreach (var preset in presets)
				{
					int index = counter;

					if (m_SelectedPresetIndex == index)
					{
						GUI.backgroundColor = Color.grey;
					}

					if (GUILayout.Button(preset.Name, EditorStyles.toolbarButton))
					{
						m_SelectedPresetIndex = index;
					}

					if (m_SelectedPresetIndex == index)
					{
						GUI.backgroundColor = m_DefaultGuiBackgroundColor;
					}
					counter++;
				}

				if (GUILayout.Button("+", EditorStyles.toolbarButton))
				{
					m_Data.AddNewPreset();
				}

				EditorGUILayout.EndHorizontal();
			});
		}

		#endregion

		#region Folders

		private void DrawFolderSetup()
		{
			CreateContainer(m_FolderCreationTitle, () =>
			{
				ShowFolderStructureConfig();
				CreateFolderStructure();
				DeletePreset();
			});
		}

		private void ShowFolderStructureConfig()
		{
			if (m_Data.Presets.Length == 0)
			{
				EditorGUILayout.LabelField(m_NoPresets);
				return;
			}

			if (m_SelectedPresetIndex > m_Data.Presets.Length - 1)
			{
				m_SelectedPresetIndex = 0;
			}

			EditorGUI.BeginChangeCheck();

			SerializedProperty presetProperty = m_DataSerializedObject.FindProperty("m_Presets").GetArrayElementAtIndex(m_SelectedPresetIndex);
			EditorGUILayout.PropertyField(presetProperty.FindPropertyRelative("m_Name"));
			SerializedProperty folderStrucutre = presetProperty.FindPropertyRelative("m_FolderStructure");
			EditorGUILayout.PropertyField(folderStrucutre.FindPropertyRelative("m_RootFolder"));
			EditorGUILayout.PropertyField(folderStrucutre.FindPropertyRelative("m_CreatedFolders"));
			EditorGUILayout.PropertyField(folderStrucutre.FindPropertyRelative("m_MovedFiles"));
			EditorGUILayout.PropertyField(folderStrucutre.FindPropertyRelative("m_DeletedFolders"));

			if (EditorGUI.EndChangeCheck())
			{
				m_DataSerializedObject.ApplyModifiedProperties();
			}
		}

		private void CreateFolderStructure()
		{
			if (GUILayout.Button(m_CreateFolderButton))
			{
				AssetInitializer.CreateFolderStructure(m_Data.Presets[m_SelectedPresetIndex].FolderStructure);
			}
		}

		private void DeletePreset()
		{
			if (GUILayout.Button(m_DeletePresetButton))
			{
				if (EditorUtility.DisplayDialog(m_DeletePresetButton, m_DeleteMessage, "Yes", "Cancel"))
				{
					m_Data.RemovePresetAt(m_SelectedPresetIndex);
					m_SelectedPresetIndex = m_SelectedPresetIndex-- < 0 ? 0 : m_SelectedPresetIndex--;
				}
			}
		}

		#endregion

		#region Packages

		private void DrawPackagesTab(string title, ref bool state, bool inverse)
		{
			if (inverse ? !state : state)
			{
				GUI.backgroundColor = Color.grey;
			}

			if (GUILayout.Button(title, EditorStyles.toolbarButton) && ((inverse && state) || (!inverse && !state)))
			{
				state = !state;
			}

			if (inverse ? !state : state)
			{
				GUI.backgroundColor = m_DefaultGuiBackgroundColor;
			}
		}

		private void DrawLocalPackages()
		{
			CreateContainer(m_LocalPackagesTitle, () =>
			{
				SerializedProperty localPackagesProperty = m_DataSerializedObject.FindProperty("m_Presets").
				GetArrayElementAtIndex(m_SelectedPresetIndex).FindPropertyRelative("m_LocalPackages");

				DrawPackages(localPackagesProperty, m_defaultLocalPackageModeTitle, m_filteredLocalPackageModeTitle,
					ref m_defaultLocalPackages, LocalPackageValidation);
				ImportLocalPackages();
			});
		}

		private void ImportLocalPackages()
		{
			if (GUILayout.Button("Import"))
			{
				var localPackages = m_Data.Presets[m_SelectedPresetIndex].LocalPackages;
				foreach (var package in localPackages)
				{
					if (package.Active && package.IsValid)
					{
						PackageImporter.ImportLocalPackage(package.Path);
					}
				}
			}
		}

		private void DrawUrlPackages()
		{
			CreateContainer(m_UrlPackagesTitle, () =>
			{
				SerializedProperty localPackagesProperty = m_DataSerializedObject.FindProperty("m_Presets").
				GetArrayElementAtIndex(m_SelectedPresetIndex).FindPropertyRelative("m_UrlPackages");

				DrawPackages(localPackagesProperty, m_defaultLocalPackageModeTitle, m_filteredLocalPackageModeTitle,
					ref m_defaultUrlPackages, UrlPackageValidation);

				ImportUrlPackages();
			});
		}

		private void ImportUrlPackages()
		{
			if (GUILayout.Button("Import"))
			{
				var urlPackages = m_Data.Presets[m_SelectedPresetIndex].UrlPackages;
				foreach (var package in urlPackages)
				{
					if (package.Active && package.IsValid)
					{
						PackageImporter.ImportUrlPackage(package.Url);
					}
				}
			}
		}

		private void DrawPackages(SerializedProperty packagesProperty, string defaultTitle,
			string filteredTitle, ref bool drawDefault, Func<SerializedProperty, bool> validator)
		{
			EditorGUILayout.BeginHorizontal();
			DrawPackagesTab(defaultTitle, ref drawDefault, false);
			DrawPackagesTab(filteredTitle, ref drawDefault, true);
			EditorGUILayout.EndHorizontal();
			GUILayout.Space(5);

			if (drawDefault)
			{
				DrawDefaultPackages(packagesProperty);
			}
			else
			{
				DrawFilteredPackages(GetFilteredPackages(packagesProperty), validator);
			}
		}

		private SortedDictionary<string, List<SerializedProperty>> GetFilteredPackages(SerializedProperty packagesProperty)
		{
			var filtered = new SortedDictionary<string, List<SerializedProperty>>();

			for (int i = 0; i < packagesProperty.arraySize; i++)
			{
				var package = packagesProperty.GetArrayElementAtIndex(i);
				string tag = package.FindPropertyRelative("m_Tag").stringValue;
				if (filtered.ContainsKey(tag))
				{
					filtered[tag].Add(package);

				}
				else
				{
					filtered.Add(tag, new List<SerializedProperty>() { package });
				}
			}

			return filtered;
		}

		private void DrawFilteredPackages(SortedDictionary<string, List<SerializedProperty>> filteredPackages, Func<SerializedProperty, bool> validator)
		{
			EditorGUI.BeginChangeCheck();

			foreach (var filter in filteredPackages)
			{
				CreateContainer(filter.Key, () =>
				{
					for (int i = 0; i < filter.Value.Count; i++)
					{
						bool isValid = validator(filter.Value[i]);
						bool isActive = filter.Value[i].FindPropertyRelative("m_Active").boolValue;

						filter.Value[i].FindPropertyRelative("m_IsValid").boolValue = isValid;

						if (!isValid)
						{
							GUI.backgroundColor = isActive ? m_InvalidPackageColor : m_InvalidDisabledPackageColor;
							CreateMessage(m_InvalidPathMessage, MessageType.Warning);
						}

						EditorGUILayout.PropertyField(filter.Value[i]);

						if (!isValid)
						{
							GUI.backgroundColor = m_DefaultGuiBackgroundColor;
						}
					}
				});
				GUILayout.Space(2);
			}

			if (EditorGUI.EndChangeCheck())
			{
				m_DataSerializedObject.ApplyModifiedProperties();
			}
		}

		private bool LocalPackageValidation(SerializedProperty serializedProperty)
		{
			bool isCustomPath = serializedProperty.FindPropertyRelative("m_HasCustomPath").boolValue;
			return IsValidPath(serializedProperty.FindPropertyRelative("m_Path").stringValue, isCustomPath);
		}

		private bool UrlPackageValidation(SerializedProperty serializedProperty)
		{
			//Will be validated on import by package manager.
			return true;
		}

		private void DrawDefaultPackages(SerializedProperty packagesProperty)
		{
			EditorGUI.BeginChangeCheck();

			EditorGUILayout.PropertyField(packagesProperty);

			if (EditorGUI.EndChangeCheck())
			{
				m_DataSerializedObject.ApplyModifiedProperties();
			}
		}

		#endregion

		#region Importer/Exporter

		private void DrawImporterExporter()
		{
			CreateContainer(m_ImportExportTitle, () =>
			{
				ImportJson();
				ExportJson();

				if (!string.IsNullOrEmpty(m_ImportMessage))
				{
					CreateMessage(m_ImportMessage);
				}

				if (!string.IsNullOrEmpty(m_ExportMessage))
				{
					CreateMessage(m_ExportMessage);
				}
			});
		}

		private void ImportJson()
		{
			if (GUILayout.Button("Import Presets As .json"))
			{
				var path = EditorUtility.OpenFilePanel("Importer", "", "json");
				m_ImportMessage = AssetInitializer.ImportJson(path, m_Data);
				EditorUtility.SetDirty(m_Data);
				m_DataSerializedObject = new SerializedObject(m_Data);
				m_DataSerializedObject.ApplyModifiedProperties();
			}
		}

		private void ExportJson()
		{
			EditorGUILayout.BeginHorizontal();
			m_ExportName = EditorGUILayout.TextField(m_ExportName);
			if (GUILayout.Button("Export Presets As .json"))
			{
				var path = EditorUtility.OpenFolderPanel("Exporter", "", "json");
				m_ExportMessage = AssetInitializer.ExportJson($"{path}/{m_ExportName}.json", m_Data);
			}
			EditorGUILayout.EndHorizontal();
		}

		#endregion

		#region Debug

		private void DrawDebugOptions()
		{
			CreateContainer(m_DebugTitle, () =>
			{
				EditorGUI.BeginChangeCheck();

				EditorGUILayout.PropertyField(m_DataSerializedObject.FindProperty("m_Show"));

				if (m_Data.Show)
				{
					SerializedProperty iterator = m_DataSerializedObject.GetIterator();
					iterator.NextVisible(true);
					do
					{
						if (iterator.name == "m_FirstField") break;
						EditorGUILayout.PropertyField(iterator);

					} while (iterator.NextVisible(false));
				}

				if (EditorGUI.EndChangeCheck())
				{
					m_DataSerializedObject.ApplyModifiedProperties();
				}
			}, true);
		}

		#endregion

		#region Helper
		private void CreateContainer(string title, System.Action onDraw, bool addFlexibleSpace = false)
		{
			GUILayout.BeginVertical(title, "window");
			onDraw?.Invoke();
			GUILayout.EndVertical();

			if (addFlexibleSpace) GUILayout.FlexibleSpace();
		}

		private void CreateMessage(string message, MessageType messageType = MessageType.Info)
		{
			EditorGUILayout.HelpBox(message, messageType);
		}

		private bool IsValidPath(string path, bool isCustomPath)
		{
			return PackageImporter.IsValidPath(path, isCustomPath);
		}

		#endregion

		#endregion
	}
}
