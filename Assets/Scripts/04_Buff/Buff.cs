using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Buff : System.IEquatable<Buff>
{
	#region 변수
	[SerializeField]
	protected BuffData m_BuffData;

	[SerializeField]
	protected int m_Count;
	#endregion

	#region 프로퍼티
	public BuffData buffData => m_BuffData;
	public int count
	{
		get => m_Count;
		set => m_Count = value;
	}
	public int maxStack => m_BuffData.maxStack;
	#endregion

	public Buff(BuffData buffData)
	{
		m_BuffData = buffData;
		m_Count = 0;
	}

	public void Clear()
	{
		m_Count = 0;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
			return false;

		Buff buff = obj as Buff;
		if (buff == null)
			return false;

		return Equals(buff);
	}
	public bool Equals(Buff other)
	{
		if (other == null)
			return false;

		return this.m_BuffData.code == other.m_BuffData.code;
	}
	public override int GetHashCode()
	{
		return m_BuffData.code.GetHashCode();
	}
}