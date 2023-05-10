using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(BuffData))]
public class BuffDataDrawer : PropertyDrawer
{
	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		float height = EditorGUIUtility.singleLineHeight * 13f;

		return base.GetPropertyHeight(property, label) + height;
	}
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		var name = property.FindPropertyRelative("m_Name");
		var code = property.FindPropertyRelative("m_Code");
		var type = property.FindPropertyRelative("m_Type");
		var effectType = property.FindPropertyRelative("m_EffectType");
		var grade = property.FindPropertyRelative("m_Grade");
		var maxStack = property.FindPropertyRelative("m_MaxStack");
		var weapon = property.FindPropertyRelative("m_Weapon");
		var description = property.FindPropertyRelative("m_Description");

		Rect pos = position;
		pos.height = EditorGUIUtility.singleLineHeight;

		float labelWidth = EditorGUIUtility.labelWidth;
		EditorGUIUtility.labelWidth = 100f;

		EditorGUI.BeginProperty(position, label, property);
		{
			name.stringValue = EditorGUI.TextField(pos, "명칭", name.stringValue);
			pos.y += EditorGUIUtility.singleLineHeight + 5f;

			code.intValue = EditorGUI.IntField(pos, "코드", code.intValue);
			pos.y += EditorGUIUtility.singleLineHeight + 5f;

			E_BuffType e_Type = (E_BuffType)type.enumValueIndex;
			e_Type = (E_BuffType)EditorGUI.EnumPopup(pos, "종류", e_Type);
			type.enumValueIndex = (int)e_Type;
			pos.y += EditorGUIUtility.singleLineHeight + 5f;

			E_BuffEffectType e_EffectType = (E_BuffEffectType)effectType.enumValueIndex;
			e_EffectType = (E_BuffEffectType)EditorGUI.EnumPopup(pos, "효과 종류", e_EffectType);
			effectType.enumValueIndex = (int)e_EffectType;
			pos.y += EditorGUIUtility.singleLineHeight + 5f;

			E_BuffGrade e_Grade = (E_BuffGrade)grade.enumValueIndex;
			e_Grade = (E_BuffGrade)EditorGUI.EnumPopup(pos, "등급", e_Grade);
			grade.enumValueIndex = (int)e_Grade;
			pos.y += EditorGUIUtility.singleLineHeight + 5f;

			maxStack.intValue = EditorGUI.IntField(pos, "최대 스택", maxStack.intValue);
			pos.y += EditorGUIUtility.singleLineHeight + 5f;

			E_BuffWeapon e_Weapon = (E_BuffWeapon)weapon.enumValueIndex;
			e_Weapon = (E_BuffWeapon)EditorGUI.EnumPopup(pos, "발동 무기", e_Weapon);
			weapon.enumValueIndex = (int)e_Weapon;
			pos.y += EditorGUIUtility.singleLineHeight + 5f;

			EditorGUI.LabelField(pos, "설명");
			pos.y += EditorGUIUtility.singleLineHeight + 5f;
			pos.height = (pos.height * 3f) + 5f;
			description.stringValue = EditorGUI.TextArea(pos, description.stringValue, "TextArea");
		}
		EditorGUI.EndProperty();

		EditorGUIUtility.labelWidth = labelWidth;
	}
}

[CustomPropertyDrawer(typeof(Buff))]
public class BuffDrawer : PropertyDrawer
{
	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		float height =
			property.isExpanded ?
			EditorGUIUtility.singleLineHeight * 14f :
			0f;

		return base.GetPropertyHeight(property, label) + height;
	}
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		var data = property.FindPropertyRelative("m_Data");
		var name = data.FindPropertyRelative("m_Name");
		var description = data.FindPropertyRelative("m_Description");

		string newTitle = name.stringValue;
		if (string.IsNullOrEmpty(newTitle))
			newTitle = label.text;

		string newToolTip = description.stringValue;
		if (string.IsNullOrEmpty(newToolTip))
			newToolTip = label.tooltip;

		EditorGUI.PropertyField(position, property, new GUIContent(newTitle, newToolTip), true);
	}
}
#endif