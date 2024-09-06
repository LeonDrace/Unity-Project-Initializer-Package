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
		private string m_DebugTitle = "Debug";

		//Data
		private ProjectInitializerData m_Data;
		private SerializedObject m_DataSerializedObject;
		private string m_SearchFilter = "l:architecture";

		//Index
		private int m_SelectedPresetIndex = 0;

		//Buttons
		private Color m_DefaultGuiBackgroundColor = Color.white;
		private string m_CreateFolderButton = "Create Folder Setup";
		private string m_DeletePresetButton = "Delete Preset";
		private string m_DeleteMessage = "Are You Sure You Want To Delete The Entire Preset?";

		private void OnEnable()
		{
			//m_Data = Resources.Load<ProjectInitializerData>(ProjectInitializerData.AssetName);
			m_Data = SearchForConfig<ProjectInitializerData>(m_SearchFilter);
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
				Importer.CreateFolderStructure();
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

		private static T SearchForConfig<T>(string filter) where T : ScriptableObject
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

		#endregion

		#endregion
	}
}
