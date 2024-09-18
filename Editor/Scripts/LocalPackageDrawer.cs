using UnityEditor;
using UnityEngine;

namespace LeonDrace.ProjectInitializer
{
	[CustomPropertyDrawer(typeof(LocalPackage))]
	public class LocalPackageDrawer : PropertyDrawer
	{
		private string[] m_DisplayOptions = new string[2] { "Unity Path", "Custom Path" };
		private int m_ActiveToggleWidth = 20;
		private int m_PathDropdownWidth = 100;
		private float m_PathPercentage = 0.85f;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			SerializedProperty activeProperty = property.FindPropertyRelative("m_Active");
			SerializedProperty pathProperty = property.FindPropertyRelative("m_Path");
			SerializedProperty tagProperty = property.FindPropertyRelative("m_Tag");
			SerializedProperty customPathProperty = property.FindPropertyRelative("m_HasCustomPath");

			int selection = customPathProperty.boolValue ? 1 : 0;

			EditorGUI.BeginChangeCheck();

			Rect rect = GetFullRect(position);
			float fullWidth = rect.width;
			float availableWidth = fullWidth;
			float x_Pos = rect.position.x;

			//Active
			rect.width = m_ActiveToggleWidth;
			availableWidth -= rect.width;
			EditorGUI.PropertyField(rect, activeProperty, new GUIContent());
			rect.position = new Vector2(rect.position.x, rect.position.y);
			x_Pos += rect.width;

			//Custom Path
			rect.position = new Vector2(x_Pos, rect.position.y);
			rect.width = m_PathDropdownWidth;
			availableWidth -= rect.width;
			selection = EditorGUI.Popup(rect, selection, m_DisplayOptions);
			customPathProperty.boolValue = selection == 1 ? true : false;
			x_Pos += rect.width;

			//Path
			float tagLabelWidth = EditorStyles.label.CalcSize(new GUIContent("Tag")).x;
			rect.width = availableWidth * m_PathPercentage - tagLabelWidth;
			availableWidth -= rect.width;
			rect.position = new Vector2(x_Pos, rect.position.y);
			EditorGUI.PropertyField(rect, pathProperty, new GUIContent());
			x_Pos += rect.width;

			//Tag
			rect.position = new Vector2(x_Pos, rect.position.y);
			rect.width = tagLabelWidth;
			availableWidth -= rect.width;
			EditorGUI.LabelField(rect, new GUIContent() { text = "Tag" });
			x_Pos += tagLabelWidth;
			rect.width = availableWidth;
			rect.position = new Vector2(x_Pos, rect.position.y);
			EditorGUI.PropertyField(rect, tagProperty, new GUIContent());

			if (EditorGUI.EndChangeCheck())
			{
				property.serializedObject.ApplyModifiedProperties();
			}
		}

		public Rect GetFullRect(Rect position)
		{
			Rect valueRect = GetFieldRect(position);
			Rect labelRect = GetLabelRect(position);
			Rect fullWidthRect = new Rect(labelRect);
			fullWidthRect.width += valueRect.width;
			return fullWidthRect;
		}

		public Rect GetFieldRect(Rect position)
		{
			position.width -= EditorGUIUtility.labelWidth;
			position.x += EditorGUIUtility.labelWidth;
			return position;
		}

		public Rect GetLabelRect(Rect position)
		{
			position.width = EditorGUIUtility.labelWidth - 2f;
			return position;
		}
	}
}
