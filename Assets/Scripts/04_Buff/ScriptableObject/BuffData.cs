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
	private E_BuffEffectType m_BuffEffectType;
	[SerializeField, ReadOnly]
	private E_BuffGrade m_BuffGrade;
	[SerializeField, ReadOnly]
	private int m_MaxStack;
	[SerializeField, ReadOnly]
	private E_BuffWeapon m_BuffWeapon;
	[SerializeField, ReadOnly, TextArea]
	private string m_Description;
	[SerializeField, ReadOnly]
	private Sprite m_Sprite;

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
	public E_BuffEffectType buffEffectType
	{
		get { return m_BuffEffectType; }
	}
	public E_BuffGrade buffGrade
	{
		get { return m_BuffGrade; }
	}
	public int maxStack
	{
		get { return m_MaxStack; }
	}
	public E_BuffWeapon buffWeapon
	{
		get { return m_BuffWeapon; }
	}
	public string description
	{
		get { return m_Description; }
	}
	public Sprite sprite
	{
		get { return m_Sprite; }
	}

	public BuffData(string _title, int _code, E_BuffType _buffType, E_BuffEffectType _buffEffectType, E_BuffGrade _buffGrade, int _maxStack, E_BuffWeapon _buffWeapon, string _description, Sprite _sprite)
	{
		m_Title = _title;
		m_Code = _code;
		m_Type = _buffType;
		m_BuffEffectType = _buffEffectType;
		m_BuffGrade = _buffGrade;
		m_MaxStack = _maxStack;
		m_BuffWeapon = _buffWeapon;
		m_Description = _description;
		m_Sprite = _sprite;
	}
}