using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Buff
{
	[SerializeField]
	protected BuffData m_Data;
	protected Dictionary<E_BuffInvokeCondition, BuffHandler> m_BuffList;

	public string name
	{
		get { return m_Data.name; }
	}
	public int code
	{
		get { return m_Data.code; }
	}

	public delegate void OnBuffHandler(ref Character character);
	public class BuffHandler
	{
		public event OnBuffHandler OnBuffEvent;
		public void OnBuffInvoke(ref Character character)
		{
			OnBuffEvent?.Invoke(ref character);
		}
	}

	public Buff(BuffData buffData)
	{
		m_Data = buffData;
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
}