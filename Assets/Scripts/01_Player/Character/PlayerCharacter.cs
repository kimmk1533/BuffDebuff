using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class PlayerCharacter : Character<PlayerCharacterStat>
{
	[SerializeField]
	private UtilClass.Timer m_DashTimer;

	private Dictionary<int, AbstractBuff> m_BuffList;

	private BuffManager M_Buff => BuffManager.Instance;

	private void Update()
	{
		HpRegenTimer();
		AttackTimer();
		DashTimer();

		foreach (var item in m_BuffList.Values)
		{
			(item as IOnBuffUpdate)?.OnBuffUpdate(this);
		}
	}

	public override void Initialize()
	{
		// Stat Init
		m_CurrentStat = new PlayerCharacterStat(m_MaxStat);
		m_CurrentStat.Xp = 0.0f;
		m_CurrentStat.Level = 0;

		// Timer Init
		m_HealTimer = new UtilClass.Timer(m_CurrentStat.HpRegenTime);
		m_AttackTimer = new UtilClass.Timer(1f / m_CurrentStat.AttackSpeed, true);
		m_DashTimer = new UtilClass.Timer(m_CurrentStat.DashRechargeTime);

		// BuffList Init
		m_BuffList = new Dictionary<int, AbstractBuff>();
	}

	// Timer Func
	private void DashTimer()
	{
		if (m_CurrentStat.DashCount >= m_MaxStat.DashCount)
			return;

		if (m_DashTimer.Update(true))
		{
			++m_CurrentStat.DashCount;
		}
	}

	// Buff Func
	public bool AddBuff(string name)
	{
		if (name == null || name == string.Empty)
			return false;

		BuffData buffData = M_Buff.GetBuffData(name);

		return this.AddBuff(buffData);
	}
	public bool AddBuff(BuffData buffData)
	{
		if (buffData == null)
			return false;

		if (m_BuffList.TryGetValue(buffData.code, out AbstractBuff buff) &&
			buff != null)
		{
			if (buff.count < buffData.maxStack)
			{
				++buff.count;
				(buff as IOnBuffAdded)?.OnBuffAdded(this);
			}
			else
			{
				Debug.Log("Buff Count is Max. title =" + buffData.title + ", maxStack = " + buffData.maxStack.ToString());

				return false;
			}

			return true;
		}

		buff = M_Buff.CreateBuff(buffData);

		m_BuffList.Add(buffData.code, buff);

		(buff as IOnBuffAdded)?.OnBuffAdded(this);

		return true;
	}
	public bool RemoveBuff(string name)
	{
		if (name == null || name == string.Empty)
			return false;

		BuffData buff = M_Buff.GetBuffData(name);

		return this.RemoveBuff(buff);
	}
	public bool RemoveBuff(BuffData buffData)
	{
		if (buffData == null)
			return false;

		if (m_BuffList.TryGetValue(buffData.code, out AbstractBuff buff) &&
			buff != null)
		{
			if (buff.count > 0)
			{
				--buff.count;
			}
			else
			{
				m_BuffList.Remove(buffData.code);
			}

			(buff as IOnBuffRemoved)?.OnBuffRemoved(this);

			return true;
		}

		Debug.Log("버프 없는데 제거");

		return false;
	}

	public override void AttackStart()
	{
		base.AttackStart();

		foreach (var item in m_BuffList.Values)
		{
			(item as IOnBuffAttackStart)?.OnBuffAttackStart(this);
		}
	}
	public override void Attack()
	{
		base.Attack();

		foreach (var item in m_BuffList.Values)
		{
			(item as IOnBuffAttack)?.OnBuffAttack(this);
		}
	}
	public override void AttackEnd()
	{
		base.AttackEnd();

		foreach (var item in m_BuffList.Values)
		{
			(item as IOnBuffAttackEnd)?.OnBuffAttackEnd(this);
		}
	}
	public void Jump()
	{
		foreach (var item in m_BuffList.Values)
		{
			(item as IOnBuffJump)?.OnBuffJump(this);
		}
	}
	public bool CanDash()
	{
		return m_CurrentStat.DashCount > 0;
	}
	public void Dash()
	{
		--m_CurrentStat.DashCount;

		foreach (var item in m_BuffList.Values)
		{
			(item as IOnBuffDash)?.OnBuffDash(this);
		}
	}

	private void OnValidate()
	{
		M_Buff.OnValidate();
	}
}