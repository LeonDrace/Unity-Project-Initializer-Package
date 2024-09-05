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

		private Vector2 m_scrollPos;
		private string m_FolderCreationTitle = "Folder Structure";

		private void OnGUI()
		{
			m_scrollPos = EditorGUILayout.BeginScrollView(m_scrollPos);
			DrawFolderSetup();
			EditorGUILayout.EndScrollView();
		}

		private void DrawFolderSetup()
		{
			GUILayout.BeginVertical(m_FolderCreationTitle, "window");

			CreateFolderStructure();

			GUILayout.EndVertical();
			GUILayout.FlexibleSpace();
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
