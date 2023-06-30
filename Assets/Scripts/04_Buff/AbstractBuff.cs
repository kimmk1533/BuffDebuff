using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractBuff : IEquatable<AbstractBuff>
{
	[SerializeField]
	protected int m_Count;
	[SerializeField]
	protected BuffData m_DebugBuffData;
	protected readonly BuffData m_BuffData;

	public int count
	{
		get { return m_Count; }
		set { m_Count = value; }
	}
	public BuffData data
	{
		get { return m_BuffData; }
	}

	public AbstractBuff(BuffData buffData)
	{
		m_Count = 0;
		m_DebugBuffData = m_BuffData = buffData;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
			return false;

		AbstractBuff buff = obj as AbstractBuff;
		if (buff == null)
			return false;

		return Equals(buff);
	}
	public bool Equals(AbstractBuff other)
	{
		if (other == null)
			return false;
		return this.data.code == other.data.code;
	}
	public override int GetHashCode()
	{
		return data.code.GetHashCode();
	}
	public static bool operator==(AbstractBuff a, AbstractBuff b)
	{
		return a.Equals(b);
	}
	public static bool operator!=(AbstractBuff a, AbstractBuff b)
	{
		return !(a == b);
	}
}