using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GreenerGames;

[System.Serializable]
public class Character
{
	public CharacterStat m_CurrentStat;
	public CharacterStat m_BuffStat;
	[SerializeField, ReadOnly]
	private CharacterStat m_FinalStat;

	public UtilClass.Timer m_HealTimer;

	public SecondaryKeyDictionary<int, string, Buff> m_BuffList;

	public CharacterStat finalStat
	{
		get { return m_FinalStat; }
	}

	public Character()
	{
		m_CurrentStat = new CharacterStat()
		{
			HPregenCooldown = 5f,
		};
		m_BuffStat = new CharacterStat();
		m_FinalStat = m_CurrentStat + m_BuffStat;

		m_HealTimer = new UtilClass.Timer(finalStat.HPregenCooldown);

		m_BuffList = new SecondaryKeyDictionary<int, string, Buff>();
	}

	BuffManager M_Buff => BuffManager.Instance;

	public void Update()
	{
		if (m_HealTimer.Update())
		{
			if (m_CurrentStat.HP < m_CurrentStat.MaxHP)
			{
				m_CurrentStat.HP += m_CurrentStat.HPregen;
				m_HealTimer.Use();

				if (m_CurrentStat.HP > m_CurrentStat.MaxHP)
				{
					m_CurrentStat.HP = m_CurrentStat.MaxHP;
				}
			}
		}
	}

	public void AddBuff(string name)
	{
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

		if (m_BuffList.TryGetValue(buff.code, out Buff newBuff) &&
			newBuff.stackCount < newBuff.data.maxStack)
		{
			++newBuff.stackCount;
			newBuff.OnBuffInitialize.OnBuffInvoke(this);
			return;
		}

		newBuff = new Buff(buff);

		m_BuffList.Add(buff.code, newBuff, buff.name);
		newBuff.OnBuffInitialize.OnBuffInvoke(this);
	}
	public bool RemoveBuff(string name)
	{
		if (name == null || name == "")
			return false;

		Buff buff = M_Buff.GetBuff(name);

		return RemoveBuff(buff);
	}
	public bool RemoveBuff(int code)
	{
		Buff buff = M_Buff.GetBuff(code);

		return RemoveBuff(buff);
	}
	public bool RemoveBuff(Buff buff)
	{
		if (buff == null)
			return false;

		if (m_BuffList.TryGetValue(buff.code, out Buff newBuff) &&
			newBuff.stackCount > 0)
		{
			--newBuff.stackCount;
			newBuff.OnBuffFinalize.OnBuffInvoke(this);
			return true;
		}

		return false;
	}

	[System.Serializable]
	public struct CharacterStat
	{
		public float MaxHP;                 // 최대 체력
		public float HP;                    // 현재 체력
		public float HPregen;               // 체력 재생량 (체력 재생 시간마다 한 번 재생)
		public float HPregenCooldown;       // 체력 재생 시간
		public float HealScale;             // 모든 힐 배율
		public float Defense;               // 방어력
		public float Avoidability;          // 회피율

		public float Attack;                // 공격력
		public float AttackSpeed;           // 공격 속도
		public float AttackRadius;          // 근접 공격 범위
		public float AttackRange;           // 투사체 공격 사거리
		public int MultiHitCount;           // 타격 수
		public float CriticalRate;          // 치명타 확률
		public float CriticalDamageScale;   // 치명타 대미지 배율

		public int DashCount;               // 대쉬 횟수
		public float DashRechargeTime;      // 대쉬 충전 속도
		public float MoveSpeed;             // 이동 속도
		public float Sight;                 // 시야 거리

		public static CharacterStat operator +(CharacterStat s1, CharacterStat s2)
		{
			CharacterStat result = new CharacterStat();

			// 최대 체력
			result.MaxHP = s1.MaxHP + s2.MaxHP;
			// 현재 체력
			result.HP = s1.HP + s2.HP;
			// 체력 재생량
			result.HPregen = s1.HPregen + s2.HPregen;
			// 체력 재생 시간
			result.HPregenCooldown = s1.HPregenCooldown + s2.HPregenCooldown;
			// 모든 힐 배율
			result.HealScale = s1.HealScale + s2.HealScale;
			// 방어력
			result.Defense = s1.Defense + s2.Defense;
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