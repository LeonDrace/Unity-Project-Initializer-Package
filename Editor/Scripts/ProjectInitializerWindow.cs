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
		private ProjectInitializerData m_Data;
		private SerializedObject m_DataSerializedObject;

		private void OnEnable()
		{
			m_Data = Resources.Load<ProjectInitializerData>(ProjectInitializerData.AssetName);
			m_DataSerializedObject = new SerializedObject(m_Data);
		}

		private void OnGUI()
		{
			m_ScrollPos = EditorGUILayout.BeginScrollView(m_ScrollPos);
			DrawFolderSetup();
			EditorGUILayout.EndScrollView();
		}

		private void DrawFolderSetup()
		{
			GUILayout.BeginVertical(m_FolderCreationTitle, "window");

			ShowFolderStructureConfig();
			CreateFolderStructure();

			GUILayout.EndVertical();
			GUILayout.FlexibleSpace();
		}

		private void ShowFolderStructureConfig()
		{
			EditorGUI.BeginChangeCheck();

			SerializedProperty iterator = m_DataSerializedObject.GetIterator();
			iterator.NextVisible(true);
			do
			{
				if (iterator.name == "m_FirstField") break;
				EditorGUILayout.PropertyField(iterator);

			} while (iterator.NextVisible(false));

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
	}
}
