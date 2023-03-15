using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

#if UNITY_EDITOR
namespace UnityEditor
{
	// 자식에 할당하는 컴포넌트
	[CustomPropertyDrawer(typeof(ChildComponentAttribute))]
	public class ChildComponentAttributeDrawer : PropertyDrawer
	{
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return base.GetPropertyHeight(property, label);
		}
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			ChildComponentAttribute child = attribute as ChildComponentAttribute;
			System.Type type = fieldInfo.FieldType;

			Component component = property.serializedObject.targetObject as Component;

			if (component == null)
			{
				return;
			}

			GameObject gameObject = component.gameObject;

			Transform childTransform = null;

			if (child.checkChildren)
			{
				childTransform = gameObject.transform.GetChild(child.childName);
			}
			else
			{
				int childCount = gameObject.transform.childCount;
				for (int i = 0; i < childCount; ++i)
				{
					childTransform = gameObject.transform.GetChild(i);
					if (childTransform.name == child.childName)
						break;
					childTransform = null;
				}
			}

			if (childTransform == null && child.autoCreateChild)
			{
				GameObject childObject = new GameObject(child.childName);
				childObject.tag = gameObject.tag;
				childObject.layer = gameObject.layer;

				childTransform = childObject.transform;
				childTransform.SetParent(gameObject.transform);
				childTransform.transform.localPosition = Vector3.zero;
			}

			if (childTransform != null)
			{
				property.objectReferenceValue = childTransform.GetOrAddComponent(type);
			}

			EditorGUI.BeginDisabledGroup(true);
			EditorGUI.PropertyField(position, property, new GUIContent(label + " (Child)"));
			EditorGUI.EndDisabledGroup();
		}
	}
}
#endif