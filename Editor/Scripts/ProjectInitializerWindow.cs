using System.IO;
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
		private string m_DebugTitle = "Debug";
		private string m_ImportExportTitle = "Import/Export";

		//Data
		private ProjectInitializerData m_Data;
		private SerializedObject m_DataSerializedObject;

		//Index
		private int m_SelectedPresetIndex = 0;

		//Buttons
		private Color m_DefaultGuiBackgroundColor = Color.white;
		private string m_CreateFolderButton = "Create Folder Setup";
		private string m_DeletePresetButton = "Delete Preset";
		private string m_DeleteMessage = "Are You Sure You Want To Delete The Entire Preset?";

		//Import Export
		private string m_ExportName = "New Preset";
		private string m_ExportMessage = string.Empty;
		private string m_ImportMessage = string.Empty;

		private void OnEnable()
		{
			m_Data = AssetInitializer.SearchForConfig<ProjectInitializerData>(AssetInitializer.ArchitectureFilter);
			m_DefaultGuiBackgroundColor = GUI.backgroundColor;
		}

		private void OnGUI()
		{
			//In case it has been updated on the scriptable itself.
			m_DataSerializedObject = new SerializedObject(m_Data);
			m_ScrollPos = EditorGUILayout.BeginScrollView(m_ScrollPos);

			DrawPresets();
			GUILayout.Space(5);
			DrawFolderSetup();
			GUILayout.Space(5);
			DrawLocalPackages();
			GUILayout.Space(5);
			DrawImporterExporter();
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

		private void DrawLocalPackages()
		{
			CreateContainer(m_LocalPackagesTitle, () =>
			{
				EditorGUI.BeginChangeCheck();

				SerializedProperty localPackagesProperty = m_DataSerializedObject.FindProperty("m_Presets").
				GetArrayElementAtIndex(m_SelectedPresetIndex).FindPropertyRelative("m_LocalPackages");

				EditorGUILayout.PropertyField(localPackagesProperty);

				if (EditorGUI.EndChangeCheck())
				{
					m_DataSerializedObject.ApplyModifiedProperties();
				}
			});


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

		#endregion

		#endregion
	}
}
