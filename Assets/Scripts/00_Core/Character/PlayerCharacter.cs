using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacter : Character<PlayerCharacterStat>
{
	protected UtilClass.Timer m_HealTimer;
	private UtilClass.Timer m_DashTimer;

	protected Dictionary<int, AbstractBuff> m_BuffList;

	private BuffManager M_Buff => BuffManager.Instance;

	protected void Update()
	{
		HpRegenTimer();
		DashTimer();

		foreach (var item in m_BuffList.Values)
		{
			(item as IOnBuffUpdate)?.OnBuffUpdate(this);
		}
	}

	public override void Initialize()
	{
		m_CurrentStat = new PlayerCharacterStat(m_MaxStat);

		m_HealTimer = new UtilClass.Timer(m_MaxStat.HpRegenTime);
		m_DashTimer = new UtilClass.Timer(m_MaxStat.DashRechargeTime);

		m_BuffList = new Dictionary<int, AbstractBuff>();
	}

	protected void HpRegenTimer()
	{
		if (m_CurrentStat.Hp >= m_MaxStat.Hp)
			return;

		if (m_HealTimer.Update(true))
		{
			float hp = m_CurrentStat.Hp + m_CurrentStat.HpRegen;

			m_CurrentStat.Hp = Mathf.Clamp(hp, 0f, m_MaxStat.Hp);
		}
	}
	private void DashTimer()
	{
		if (m_CurrentStat.DashCount >= m_MaxStat.DashCount)
			return;

		if (m_DashTimer.Update(true))
		{
			++m_CurrentStat.DashCount;
		}
	}

	public bool AddBuff(int code)
	{
		BuffData buffData = M_Buff.GetBuffData(code);

		return this.AddBuff(buffData);
	}
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
	public bool RemoveBuff(int code)
	{
		BuffData buffData = M_Buff.GetBuffData(code);

		return this.RemoveBuff(buffData);
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

	public void Jump()
	{
		foreach (var item in m_BuffList.Values)
		{
			(item as IOnBuffJump)?.OnBuffJump(this);
		}
	}
	public void Dash()
	{
		foreach (var item in m_BuffList.Values)
		{
			(item as IOnBuffDash)?.OnBuffDash(this);
		}
	}
}