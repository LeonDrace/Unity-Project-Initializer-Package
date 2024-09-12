using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LeonDrace.ProjectInitializer
{
	[CustomPropertyDrawer(typeof(ProjectInitializerData.Package))]
	public class PackageDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			SerializedProperty activeProperty = property.FindPropertyRelative("m_Active");
			SerializedProperty pathProperty = property.FindPropertyRelative("m_Path");
			SerializedProperty tagProperty = property.FindPropertyRelative("m_Tag");

			EditorGUI.BeginChangeCheck();

			Rect rect = GetFullRect(position);
			float fullWidth = rect.width;
			//Active
			rect.width = 20;
			EditorGUI.PropertyField(rect, activeProperty, new GUIContent());
			//Path
			rect.position = new Vector2(rect.position.x + rect.width, rect.position.y);
			rect.width = fullWidth / 4 - 20;
			EditorGUI.LabelField(rect, new GUIContent() { text = "Path" });
			rect.position = new Vector2(rect.position.x + rect.width, rect.position.y);
			EditorGUI.PropertyField(rect, pathProperty, new GUIContent());
			//Tag
			rect.position = new Vector2(rect.position.x + rect.width, rect.position.y);
			EditorGUI.LabelField(rect, new GUIContent() { text = "Tag" });
			rect.position = new Vector2(rect.position.x + rect.width, rect.position.y);
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
