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
		private string m_FolderCreationTitle = "Folder Structure";
		private string m_DebugTitle = "Debug";
		private ProjectInitializerData m_Data;
		private SerializedObject m_DataSerializedObject;
		private string m_SearchFilter = "l:architecture";

		private void OnEnable()
		{
			//m_Data = Resources.Load<ProjectInitializerData>(ProjectInitializerData.AssetName);
			m_Data = SearchForConfig<ProjectInitializerData>(m_SearchFilter);
		}

		private void OnGUI()
		{
			//In case it has been updated on the scriptable itself.
			m_DataSerializedObject = new SerializedObject(m_Data);
			m_ScrollPos = EditorGUILayout.BeginScrollView(m_ScrollPos);

			DrawFolderSetup();
			GUILayout.Space(5);
			DrawDebugOptions();

			EditorGUILayout.EndScrollView();
		}

		#region Folders

		private void DrawFolderSetup()
		{
			CreateContainer(m_FolderCreationTitle, () =>
			{
				ShowFolderStructureConfig();
				CreateFolderStructure();
			});
		}

		private void ShowFolderStructureConfig()
		{
			EditorGUI.BeginChangeCheck();

			SerializedProperty folderStrucutre = m_DataSerializedObject.FindProperty("m_FolderStructure");
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
			if (GUILayout.Button("Create Folder Setup"))
			{
				Importer.CreateFolderStructure();
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
					return (T)config;
				}
			}
			Debug.LogError($"The requested config was not found: {typeof(T)} it might be missing the config asset label.");
			return null;
		}

		#endregion

		#endregion
	}
}
