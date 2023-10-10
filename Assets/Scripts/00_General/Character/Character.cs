using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Character<T> : MonoBehaviour where T : CharacterStat, new()
{
	// 최대 스탯
	[SerializeField]
	protected T m_MaxStat;
	// 현재 스탯
	[Space(10)]
	[SerializeField]
	protected T m_CurrentStat;

	[Header("===== 타이머 ====="), Space(10)]
	[SerializeField]
	protected UtilClass.Timer m_HealTimer;
	[SerializeField]
	protected UtilClass.Timer m_AttackTimer;

	public T maxStat => m_MaxStat;
	public T currentStat => m_CurrentStat;

	public abstract void Initialize();

	// Timer Func
	protected void HpRegenTimer()
	{
		if (Mathf.Abs(m_CurrentStat.HpRegen) <= float.Epsilon)
			return;
		if (m_CurrentStat.Hp >= m_MaxStat.Hp)
			return;

		if (m_HealTimer.Update(true))
		{
			float hp = m_CurrentStat.Hp + (m_CurrentStat.HpRegen * m_CurrentStat.HealScale) * m_CurrentStat.AntiHealScale;

			m_CurrentStat.Hp = Mathf.Clamp(hp, 0f, m_MaxStat.Hp);
		}
	}
	protected void AttackTimer()
	{
		m_AttackTimer.Update();
	}

	// Attack Func
	public virtual bool CanAttack()
	{
		return m_AttackTimer.timeIsUp;
	}
	public virtual void AttackStart()
	{
	}
	public virtual void Attack()
	{
	}
	public virtual void AttackEnd()
	{
		m_AttackTimer.Use();
	}
}