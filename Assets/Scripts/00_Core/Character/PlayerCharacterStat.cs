using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerCharacterStat : CharacterStat
{
	// 체력 재생량 (체력 재생 시간마다 한 번 재생)
	[field: Space(10), SerializeField]
	public float HpRegen { get; set; }
	// 체력 재생 시간
	[field: SerializeField]
	public float HpRegenTime { get; set; }
	// 힐 배율
	[field: SerializeField]
	public float HealScale { get; set; }

	// 대쉬 횟수
	[field: Space(10), SerializeField]
	public int DashCount { get; set; }
	// 대쉬 속도
	[field: SerializeField]
	public float DashSpeed { get; set; }
	// 대쉬 충전 속도
	[field: SerializeField]
	public float DashRechargeTime { get; set; }

	public PlayerCharacterStat() : base()
	{
		// 체력 재생량 (체력 재생 시간마다 한 번 재생)
		HpRegen = 0.0f;
		// 체력 재생 시간
		HpRegenTime = 0.0f;
		// 힐 배율
		HealScale = 0.0f;
		
		// 대쉬 횟수
		DashCount = 0;
		// 대쉬 속도
		DashSpeed = 0f;
		// 대쉬 충전 속도
		DashRechargeTime = 0.0f;
	}
	public PlayerCharacterStat(PlayerCharacterStat other) : base(other)
	{
		// 체력 재생량 (체력 재생 시간마다 한 번 재생)
		HpRegen = other.HpRegen;
		// 체력 재생 시간
		HpRegenTime = other.HpRegenTime;
		// 힐 배율
		HealScale = other.HealScale;

		// 대쉬 횟수
		DashCount = other.DashCount;
		// 대쉬 속도
		DashSpeed = other.DashSpeed;
		// 대쉬 충전 속도
		DashRechargeTime = other.DashRechargeTime;
	}

	public static PlayerCharacterStat operator +(PlayerCharacterStat s1, PlayerCharacterStat s2)
	{
		PlayerCharacterStat result = new PlayerCharacterStat();

		// 체력
		result.Hp = s1.Hp + s2.Hp;
		// 체력 재생량
		result.HpRegen = s1.HpRegen + s2.HpRegen;
		// 체력 재생 시간
		result.HpRegenTime = s1.HpRegenTime + s2.HpRegenTime;
		// 모든 힐 배율
		result.HealScale = s1.HealScale + s2.HealScale;

		// 공격력
		result.Attack = s1.Attack + s2.Attack;
		// 공격 속도
		result.AttackSpeed = s1.AttackSpeed + s2.AttackSpeed;
		// 근접 공격 범위
		result.AttackScale = s1.AttackScale + s2.AttackScale;
		// 투사체 공격 사거리
		result.AttackRange = s1.AttackRange + s2.AttackRange;
		// 타격 수
		result.MultiHitCount = s1.MultiHitCount + s2.MultiHitCount;
		// 치명타 확률
		result.CriticalRate = s1.CriticalRate + s2.CriticalRate;
		// 치명타 대미지 배율
		result.CriticalDamageScale = s1.CriticalDamageScale + s2.CriticalDamageScale;

		// 방어력
		result.Armor = s1.Armor + s2.Armor;
		// 회피율
		result.Avoidability = s1.Avoidability + s2.Avoidability;

		// 이동 속도
		result.MoveSpeed = s1.MoveSpeed + s2.MoveSpeed;

		// 시야 거리
		result.Sight = s1.Sight + s2.Sight;

		// 대쉬 횟수
		result.DashCount = s1.DashCount + s2.DashCount;
		// 대쉬 충전 속도
		result.DashRechargeTime = s1.DashRechargeTime + s2.DashRechargeTime;

		return result;
	}
}