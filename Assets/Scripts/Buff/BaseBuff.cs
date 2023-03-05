using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif
using static BuffData;

[System.Serializable]
public struct BuffData
{
	[SerializeField, ReadOnly]
	private string m_Name;
	[SerializeField, ReadOnly]
	private int m_Code;
	[SerializeField, ReadOnly]
	private E_Grade m_Grade;
	[SerializeField, ReadOnly]
	private E_Condition m_Condition;
	[SerializeField, ReadOnly]
	private string m_Description;

	public string name
	{
		get { return m_Name; }
	}
	public int code
	{
		get { return m_Code; }
	}
	public E_Grade grade
	{
		get { return m_Grade; }
	}
	public E_Condition condition
	{
		get { return m_Condition; }
	}
	public string description
	{
		get { return m_Description; }
	}

	public BuffData(string name, int code, E_Grade grade, E_Condition condition, string description)
	{
		m_Name = name;
		m_Code = code;
		m_Grade = grade;
		m_Condition = condition;
		m_Description = description;
	}

	public enum E_Grade
	{
		Normal = 0,
		Uncommon,
		Rare,
		Unique,
		Epic,
		Legendary,
		GOD,

		Max
	}
	public enum E_Condition
	{
		All,
		Melee,
		Ranged,

		Max
	}
}
#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(BuffData))]
public class BuffDataDrawer : PropertyDrawer
{
	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		return base.GetPropertyHeight(property, label) * 10f;
	}
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		//base.OnGUI(position, property, label);
		var name = property.FindPropertyRelative("m_Name");
		var code = property.FindPropertyRelative("m_Code");
		var grade = property.FindPropertyRelative("m_Grade");
		var condition = property.FindPropertyRelative("m_Condition");
		var description = property.FindPropertyRelative("m_Description");

		EditorGUI.BeginProperty(position, label, property);
		{
			Rect pos = position;
			pos.height = EditorGUIUtility.singleLineHeight;

			name.stringValue = EditorGUI.TextField(pos, "명칭", name.stringValue);
			pos.y += EditorGUIUtility.singleLineHeight + 5f;

			code.intValue = EditorGUI.IntField(pos, "코드", code.intValue);
			pos.y += EditorGUIUtility.singleLineHeight + 5f;

			E_Grade e_Grade = (E_Grade)grade.enumValueIndex;
			e_Grade = (E_Grade)EditorGUI.EnumPopup(pos, "등급", e_Grade);
			grade.enumValueIndex = (int)e_Grade;
			pos.y += EditorGUIUtility.singleLineHeight + 5f;

			E_Condition e_Condition = (E_Condition)condition.enumValueIndex;
			e_Condition = (E_Condition)EditorGUI.EnumPopup(pos, "조건", e_Condition);
			condition.enumValueIndex = (int)e_Condition;
			pos.y += EditorGUIUtility.singleLineHeight + 5f;

			EditorGUI.LabelField(pos, "설명");
			pos.y += EditorGUIUtility.singleLineHeight + 5f;
			pos.height = (pos.height * 3f) + 5f;
			description.stringValue = EditorGUI.TextArea(pos, description.stringValue, "TextArea");
		}
		EditorGUI.EndProperty();
	}
}
#endif

[System.Serializable]
public class BaseBuff
{
	[SerializeField]
	private BuffData m_Data;
	private Dictionary<E_BuffInvokeCondition, BuffHandler> m_BuffList;

	public delegate void OnBuffHandler(ref Character character);
	public class BuffHandler
	{
		public event OnBuffHandler OnBuffEvent;
		public void OnBuffInvoke(ref Character character)
		{
			OnBuffEvent?.Invoke(ref character);
		}
	}

	public BaseBuff(string name, int code, E_Grade grade, E_Condition condition, string description)
	{
		m_Data = new BuffData(name, code, grade, condition, description);
		m_BuffList = new Dictionary<E_BuffInvokeCondition, BuffHandler>();

		// 버프 리스트 초기화
		for (E_BuffInvokeCondition i = E_BuffInvokeCondition.Initialize; i < E_BuffInvokeCondition.Max; ++i)
		{
			m_BuffList.Add(i, new BuffHandler());
		}
	}

	#region BuffHandler
	public BuffHandler OnBuffInitialize => m_BuffList[E_BuffInvokeCondition.Initialize];
	public BuffHandler OnBuffFinalize => m_BuffList[E_BuffInvokeCondition.Finalize];
	public BuffHandler OnBuffUpdate => m_BuffList[E_BuffInvokeCondition.Update];
	public BuffHandler OnBuffJump => m_BuffList[E_BuffInvokeCondition.Jump];
	public BuffHandler OnBuffDash => m_BuffList[E_BuffInvokeCondition.Dash];
	public BuffHandler OnBuffGetDamage => m_BuffList[E_BuffInvokeCondition.GetDamage];
	public BuffHandler OnBuffAttackStart => m_BuffList[E_BuffInvokeCondition.AttackStart];
	public BuffHandler OnBuffGiveDamage => m_BuffList[E_BuffInvokeCondition.GiveDamage];
	public BuffHandler OnBuffAttackEnd => m_BuffList[E_BuffInvokeCondition.AttackEnd];
	#endregion

	public void Add(E_BuffInvokeCondition condition, OnBuffHandler handler)
	{
		m_BuffList[condition].OnBuffEvent += handler;
	}
	public void Remove(E_BuffInvokeCondition condition, OnBuffHandler handler)
	{
		m_BuffList[condition].OnBuffEvent -= handler;
	}
}