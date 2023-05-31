using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuffData", menuName = "Scriptable Object/Buff Data", order = int.MinValue)]
public class BuffData : ScriptableObject
{
	[SerializeField, ReadOnly]
	private int m_Code;
	[SerializeField, ReadOnly]
	private string m_Title;
	[SerializeField, ReadOnly]
	private E_BuffType m_Type;
	[SerializeField, ReadOnly]
	private E_BuffEffectType m_EffectType;
	[SerializeField, ReadOnly]
	private E_BuffGrade m_Grade;
	[SerializeField, ReadOnly]
	private int m_MaxStack;
	[SerializeField, ReadOnly]
	private E_BuffWeapon m_Weapon;
	[SerializeField, ReadOnly, TextArea]
	private string m_Description;

	public int code
	{
		get { return m_Code; }
	}
	public string title
	{
		get { return m_Title; }
	}
	public E_BuffType buffType
	{
		get { return m_Type; }
	}
	public E_BuffEffectType effectType
	{
		get { return m_EffectType; }
	}
	public E_BuffGrade grade
	{
		get { return m_Grade; }
	}
	public int maxStack
	{
		get { return m_MaxStack; }
	}
	public E_BuffWeapon weapon
	{
		get { return m_Weapon; }
	}
	public string description
	{
		get { return m_Description; }
	}

	public BuffData(string _title, int _code, E_BuffType _type, E_BuffEffectType _effectType, E_BuffGrade _grade, int _maxStack, E_BuffWeapon _weapon, string _description)
	{
		m_Title = _title;
		m_Code = _code;
		m_Type = _type;
		m_EffectType = _effectType;
		m_Grade = _grade;
		m_MaxStack = _maxStack;
		m_Weapon = _weapon;
		m_Description = _description;
	}
}