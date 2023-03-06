using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Character
{
	public CharacterStat m_CurrentStat;
	public CharacterStat m_BuffStat;
	//public Dictionary<E_BuffInvokeCondition, List<BaseBuff>> m_BuffList;
	public HashSet<Buff> m_BuffList;

	public Character()
	{
		m_CurrentStat = new CharacterStat();
		m_BuffStat = new CharacterStat();
		m_BuffList = new HashSet<Buff>();
	}

	public void AddBuff(Buff buff)
	{
		if (buff == null)
			return;

		m_BuffList.Add(buff);

		Character character = this;
		buff.OnBuffInitialize.OnBuffInvoke(ref character);

		//m_BuffList[E_BuffInvokeCondition.Initialize].Add(buff.OnBuffInitialize);
		//m_BuffList[E_BuffInvokeCondition.Finalize].Add(buff.OnBuffFinalize);
		//m_BuffList[E_BuffInvokeCondition.Update].Add(buff.OnBuffUpdate);
		//m_BuffList[E_BuffInvokeCondition.Jump].Add(buff.OnBuffJump);
		//m_BuffList[E_BuffInvokeCondition.Dash].Add(buff.OnBuffDash);
		//m_BuffList[E_BuffInvokeCondition.GetDamage].Add(buff.OnBuffGetDamage);
		//m_BuffList[E_BuffInvokeCondition.AttackStart].Add(buff.OnBuffAttackStart);
		//m_BuffList[E_BuffInvokeCondition.GiveDamage].Add(buff.OnBuffGiveDamage);
		//m_BuffList[E_BuffInvokeCondition.AttackEnd].Add(buff.OnBuffAttackEnd);
	}
	public void RemoveBuff(string name)
	{

	}
	public void RemoveBuff(int code)
	{

	}

	[System.Serializable]
	public struct CharacterStat
	{
		public float MaxHP;                 // 최대 체력
		public float HP;                    // 현재 체력
		public float HPregen;               // 체력 재생력
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
			// 체력 재생력
			result.HPregen = s1.HPregen + s2.HPregen;
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