using UnityEditor;

namespace LeonDrace.ProjectInitializer
{
	public class ProjectInitializerWindow : EditorWindow
	{
		[MenuItem("Window/LeonDrace/Project Initializer")]

		public static void ShowWindow()
		{
			EditorWindow.GetWindow(typeof(ProjectInitializerWindow));
		}
	}
}
