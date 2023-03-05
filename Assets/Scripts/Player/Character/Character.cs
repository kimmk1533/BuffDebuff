using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Character
{
	public CharacterStat m_CurrentStat;
	public CharacterStat m_BuffStat;
	//public Dictionary<E_BuffInvokeCondition, List<BaseBuff>> m_BuffList;
	public HashSet<BaseBuff> m_BuffList;

	public Character()
	{
		m_CurrentStat = new CharacterStat();
		m_BuffStat = new CharacterStat();
		m_BuffList = new HashSet<BaseBuff>();
	}

	public void AddBuff(BaseBuff buff)
	{
		if (buff == null)
			return;

		m_BuffList.Add(buff);

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

	[System.Serializable]
	public struct CharacterStat
	{
		public float MaxHP;                 // 최대 체력
		public float HP;                    // 현재 체력
		public float HPregen;               // 체력 재생력
		public float HealScale;             // 힐 배율
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
		public float MoveSpeed;             // 이동속도
		public float Sight;                 // 시야거리
	}
}