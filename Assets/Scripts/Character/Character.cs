using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Character
{
	protected readonly CharacterStat m_InitialStat;
	[SerializeField]
	protected CharacterStat m_CurrentStat;
	[SerializeField]
	protected CharacterStat m_BuffStat;

	public CharacterStat buffStat
	{
		get { return m_BuffStat; }
		set { m_BuffStat = value; }
	}
	public CharacterStat finalStat => m_InitialStat + m_BuffStat;

	protected Dictionary<int, (Buff buff, int count)> m_BuffList;
	protected UtilClass.Timer m_HealTimer;
	protected UtilClass.Timer m_DashTimer;

	private BuffManager M_Buff => BuffManager.Instance;

	public Character()
	{
		m_InitialStat = new CharacterStat()
		{
			HpRegen = 5f,
			HpRegenTime = 1f,

			DashCount = 3,
			DashRechargeTime = 1f,
		};

		m_CurrentStat = m_InitialStat;
		m_BuffStat = new CharacterStat();
		m_BuffList = new Dictionary<int, (Buff buff, int count)>();

		m_HealTimer = new UtilClass.Timer(m_InitialStat.HpRegenTime);
		m_DashTimer = new UtilClass.Timer(m_InitialStat.DashRechargeTime);
	}

	protected void HpRegenTimer()
	{
		if (m_CurrentStat.Hp >= finalStat.MaxHp)
			return;

		if (m_HealTimer.Update(true))
		{
			float hp = m_CurrentStat.Hp + finalStat.HpRegen;

			m_CurrentStat.Hp = Mathf.Clamp(hp, 0f, finalStat.MaxHp);
		}
	}
	protected void DashTimer()
	{
		if (m_CurrentStat.DashCount >= m_InitialStat.DashCount)
			return;

		if (m_DashTimer.Update(true))
		{
			++m_CurrentStat.DashCount;
		}
	}
	public void Update()
	{
		HpRegenTimer();

		DashTimer();

		foreach (var item in m_BuffList.Values)
		{
			item.buff.OnBuffUpdate.OnBuffInvoke(this);
		}
	}
	public void Jump()
	{
		foreach (var item in m_BuffList.Values)
		{
			item.buff.OnBuffJump.OnBuffInvoke(this);
		}
	}
	public bool Dash()
	{
		if (m_CurrentStat.DashCount <= 0)
			return false;

		--m_CurrentStat.DashCount;

		foreach (var item in m_BuffList.Values)
		{
			item.buff.OnBuffDash.OnBuffInvoke(this);
		}

		return true;
	}

	public void AddBuff(string name)
	{
		if (name == null || name == string.Empty)
			return;

		Buff buff = M_Buff.GetBuff(name);

		this.AddBuff(buff);
	}
	public void AddBuff(int code)
	{
		Buff buff = M_Buff.GetBuff(code);

		this.AddBuff(buff);
	}
	public void AddBuff(Buff buff)
	{
		if (buff == null)
			return;

		if (m_BuffList.TryGetValue(buff.code, out (Buff buff, int count) tuple))
		{
			if (tuple.count < tuple.buff.data.maxStack)
			{
				++tuple.count;
				tuple.buff.OnBuffInitialize.OnBuffInvoke(this);
			}
			else
				Debug.Log("버프 최대");

			return;
		}

		tuple.buff = new Buff(buff);

		m_BuffList.Add(tuple.buff.code, tuple);

		tuple.buff.OnBuffInitialize.OnBuffInvoke(this);
	}
	public bool RemoveBuff(string name)
	{
		if (name == null || name == string.Empty)
			return false;

		Buff buff = M_Buff.GetBuff(name);

		return RemoveBuff(buff);
	}
	public bool RemoveBuff(int code)
	{
		if (m_BuffList.TryGetValue(code, out (Buff buff, int count) tuple))
		{
			if (tuple.count > 0)
			{
				--tuple.count;

				tuple.buff.OnBuffFinalize.OnBuffInvoke(this);
			}

			return true;
		}

		return false;
	}
	public bool RemoveBuff(Buff buff)
	{
		if (buff == null)
			return false;

		return RemoveBuff(buff.code);
	}

	[System.Serializable]
	public struct CharacterStat
	{
		#region 체력
		// 최대 체력
		public float MaxHp;
		// 현재 체력
		public float Hp;
		#endregion

		#region 회복
		// 체력 재생량 (체력 재생 시간마다 한 번 재생)
		public float HpRegen;
		// 체력 재생 시간
		public float HpRegenTime;
		// 힐 배율
		public float HealScale;
		#endregion

		#region 공격
		public float Attack;                // 공격력
		public float AttackSpeed;           // 공격 속도
		public float AttackRadius;          // 근접 공격 범위
		public float AttackRange;           // 투사체 공격 사거리
		public int   MultiHitCount;         // 타격 수
		public float CriticalRate;          // 치명타 확률
		public float CriticalDamageScale;   // 치명타 대미지 배율 
		#endregion

		#region 수비
		public float Armor;                 // 방어력
		public float Avoidability;          // 회피율 
		#endregion

		#region 대쉬
		public int   DashCount;             // 대쉬 횟수
		public float DashRechargeTime;      // 대쉬 충전 속도 
		#endregion

		#region 이동
		public float MoveSpeed;             // 이동 속도 
		#endregion

		#region 시야
		public float Sight;                 // 시야 거리
		#endregion

		public static CharacterStat operator +(CharacterStat s1, CharacterStat s2)
		{
			CharacterStat result = new CharacterStat();

			// 최대 체력
			result.MaxHp = s1.MaxHp + s2.MaxHp;
			// 현재 체력
			result.Hp = s1.Hp + s2.Hp;
			// 체력 재생량
			result.HpRegen = s1.HpRegen + s2.HpRegen;
			// 체력 재생 시간
			result.HpRegenTime = s1.HpRegenTime + s2.HpRegenTime;
			// 모든 힐 배율
			result.HealScale = s1.HealScale + s2.HealScale;
			// 방어력
			result.Armor = s1.Armor + s2.Armor;
			// 회피율
			result.Avoidability = s1.Avoidability + s2.Avoidability;

			// 공격력
			result.Attack = s1.Attack + s2.Attack;
			// 공격 속도
			result.AttackSpeed = s1.AttackSpeed + s2.AttackSpeed;
			// 근접 공격 범위
			result.AttackRadius = s1.AttackRadius + s2.AttackRadius;
			// 투사체 공격 사거리
			result.AttackRange = s1.AttackRange + s2.AttackRange;
			// 타격 수
			result.MultiHitCount = s1.MultiHitCount + s2.MultiHitCount;
			// 치명타 확률
			result.CriticalRate = s1.CriticalRate + s2.CriticalRate;
			// 치명타 대미지 배율
			result.CriticalDamageScale = s1.CriticalDamageScale + s2.CriticalDamageScale;

			// 대쉬 횟수
			result.DashCount = s1.DashCount + s2.DashCount;
			// 대쉬 충전 속도
			result.DashRechargeTime = s1.DashRechargeTime + s2.DashRechargeTime;
			// 이동 속도
			result.MoveSpeed = s1.MoveSpeed + s2.MoveSpeed;
			// 시야 거리
			result.Sight = s1.Sight + s2.Sight;

			return result;
		}
	}
}