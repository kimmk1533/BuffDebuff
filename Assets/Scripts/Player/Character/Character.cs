using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
	public CharacterStat m_CurrentStat;
	public CharacterStat m_BuffStat;

	[System.Serializable]
	public struct CharacterStat
	{
		public float MaxHP;					// 최대 체력
		public float HP;					// 현재 체력
		public float HPregen;				// 체력 재생력
		public float HealScale;				// 힐 배율
		public float Defense;				// 방어력
		public float Avoidability;			// 회피율
		
		public float Attack;				// 공격력
		public float AttackSpeed;			// 공격 속도
		public float AttackRadius;			// 근접 공격 범위
		public float AttackRange;			// 투사체 공격 사거리
		public int MultiHitCount;			// 타격 수
		public float CriticalRate;			// 치명타 확률
		public float CriticalDamageScale;	// 치명타 대미지 배율

		public int DashCount;				// 대쉬 횟수
		public float DashRechargeTime;		// 대쉬 충전 속도
		public float MoveSpeed;				// 이동속도
		public float Sight;					// 시야거리
	}
}