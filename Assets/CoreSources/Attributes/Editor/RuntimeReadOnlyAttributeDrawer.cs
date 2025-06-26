using UnityEngine;
using System;

#if UNITY_EDITOR
namespace UnityEditor
{
	[CustomPropertyDrawer(typeof(RuntimeReadOnlyAttribute), true)]
	public class RuntimeReadOnlyAttributeDrawer : PropertyDrawer
	{
		// Necessary since some properties tend to collapse smaller than their content
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUI.GetPropertyHeight(property, label, true);
		}

		// Draw a disabled property field
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (property == null)
				return;

			GUI.enabled = !Application.isPlaying && ((RuntimeReadOnlyAttribute)attribute).runtimeOnly;
			EditorGUI.PropertyField(position, property, label, true);
			GUI.enabled = true;
		}
	}
}
#endif