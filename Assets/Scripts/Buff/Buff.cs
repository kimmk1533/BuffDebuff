using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Buff : IEquatable<Buff>
{
	[SerializeField]
	protected BuffData m_Data;
	[SerializeField]
	protected int m_StackCount;
	protected Dictionary<E_BuffInvokeCondition, BuffHandler> m_BuffList;

	public string name
	{
		get { return m_Data.name; }
	}
	public int code
	{
		get { return m_Data.code; }
	}
	public int stackCount
	{
		get { return m_StackCount; }
		set { m_StackCount = value; }
	}

	public delegate void OnBuffHandler(Character character);
	public class BuffHandler
	{
		public event OnBuffHandler OnBuffEvent;
		public void OnBuffInvoke(Character character)
		{
			OnBuffEvent?.Invoke(character);
		}
	}

	public Buff(BuffData buffData)
	{
		m_Data = buffData;
		m_StackCount = 1;
		m_BuffList = new Dictionary<E_BuffInvokeCondition, BuffHandler>();

		// 버프 리스트 초기화
		for (E_BuffInvokeCondition i = E_BuffInvokeCondition.Initialize; i < E_BuffInvokeCondition.Max; ++i)
		{
			m_BuffList.Add(i, new BuffHandler());
		}
	}
	public Buff(Buff other)
	{
		m_Data = other.m_Data;
		m_StackCount = other.m_StackCount;
		m_BuffList = new Dictionary<E_BuffInvokeCondition, BuffHandler>(other.m_BuffList);
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
		if (other == null) return false;
		return this.code == other.code;
	}
	public override int GetHashCode()
	{
		return code.GetHashCode();
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
}