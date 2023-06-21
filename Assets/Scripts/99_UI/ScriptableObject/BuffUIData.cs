using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuffUIData", menuName = "Scriptable Object/BuffUI Data", order = int.MinValue)]
public class BuffUIData : ScriptableObject
{
	[SerializeField, ReadOnly]
	private int m_Code;
	[SerializeField, ReadOnly]
	private string m_Title;
	[SerializeField, ReadOnly]
	private Sprite m_Sprite;
	[SerializeField, ReadOnly]
	private E_BuffGrade m_BuffGrade;
	[SerializeField, ReadOnly]
	private int m_MaxStack;
	[SerializeField, ReadOnly, TextArea]
	private string m_Description;

	public int code
	{
		get { return m_Code; }
		private set { m_Code = value; }
	}
	public string title
	{
		get { return m_Title; }
		private set { m_Title = value; }
	}
	public Sprite sprite
	{
		get { return m_Sprite; }
		private set { m_Sprite = value; }
	}
	public E_BuffGrade buffGrade
	{
		get { return m_BuffGrade; }
		private set { m_BuffGrade = value; }
	}
	public int maxStack
	{
		get { return m_MaxStack; }
	}
	public string description
	{
		get { return m_Description; }
		private set { m_Description = value; }
	}

	public BuffUIData(BuffData buffData)
	{
		m_Code = buffData.code;
		m_Title = buffData.title;
		m_Sprite = null;
		m_BuffGrade = buffData.grade;
		m_MaxStack = buffData.maxStack;
		m_Description = buffData.description;
	}
	public BuffUIData(int code, string title, Sprite sprite, E_BuffGrade buffGrade, int maxStack, string description)
	{
		m_Code = code;
		m_Title = title;
		m_Sprite = sprite;
		m_BuffGrade = buffGrade;
		m_MaxStack = maxStack;
		m_Description = description;
	}

	public static bool operator ==(BuffUIData a, BuffUIData b)
	{
		return a.code == b.code;
	}
	public static bool operator !=(BuffUIData a, BuffUIData b)
	{
		return a.code != b.code;
	}
}