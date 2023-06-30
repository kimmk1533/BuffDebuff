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
			ChildComponentAttribute child = (attribute as System.Attribute) as ChildComponentAttribute;

			if (child.showInInspector == false)
				return 0f;

			return base.GetPropertyHeight(property, label);
		}
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			ChildComponentAttribute child = (attribute as System.Attribute) as ChildComponentAttribute;

			Component component = property.serializedObject.targetObject as Component;
			if (component == null)
			{
				return;
			}

			GameObject gameObject = component.gameObject;
			Transform transform = component.transform;
			Transform childTransform = null;

			string[] path = child.childName.Replace('\\', '/').Split('/');

			for (int i = 0; i < path.Length; ++i)
			{
				if (path[i] == string.Empty)
					continue;

				childTransform = transform.Find(path[i]);

				if (childTransform == null)
				{
					if (child.autoCreateChild == false)
					{
						childTransform = null;
						break;
					}

					GameObject childObject = new GameObject(path[i]);
					childObject.tag = gameObject.tag;
					childObject.layer = gameObject.layer;

					childTransform = childObject.transform;
					childTransform.SetParent(transform);
					childTransform.localPosition = Vector3.zero;
					childTransform.localScale = Vector3.one;
				}

				transform = childTransform;
			}

			if (childTransform != null)
			{
				System.Type type = fieldInfo.FieldType;
				property.objectReferenceValue = childTransform.GetOrAddComponent(type);
			}

			if (child.showInInspector == true)
			{
				EditorGUI.BeginDisabledGroup(true);
				EditorGUI.PropertyField(position, property, new GUIContent(label + " (Child)"));
				EditorGUI.EndDisabledGroup();
			}
		}
	}
}
#endif