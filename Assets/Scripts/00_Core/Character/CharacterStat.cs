using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterStat
{
	// 체력
	[field: SerializeField]
	public float Hp { get; set; }

	// 공격력
	[field: Space(10), SerializeField]
	public float Attack { get; set; }
	// 공격 속도
	[field: SerializeField]
	public float AttackSpeed { get; set; }
	// 공격 크기
	[field: SerializeField]
	public float AttackScale { get; set; }
	// 공격 사거리(투사체 생존 시간)
	[field: SerializeField]
	public float AttackRange { get; set; }
	// 타격 수
	[field: SerializeField]
	public int MultiHitCount { get; set; }
	// 치명타 확률
	[field: SerializeField]
	public float CriticalRate { get; set; }
	// 치명타 대미지 배율
	[field: SerializeField]
	public float CriticalDamageScale { get; set; }

	// 방어력
	[field: Space(10), SerializeField]
	public float Armor { get; set; }
	// 회피율
	[field: SerializeField]
	public float Avoidability { get; set; }

	// 이동 속도
	[field: Space(10), SerializeField]
	public float MoveSpeed { get; set; }

	// 시야 거리
	[field: Space(10), SerializeField]
	public float Sight { get; set; }

	public CharacterStat()
	{
		// 체력
		Hp = 0.0f;

		// 공격력
		Attack = 0.0f;
		// 공격 속도
		AttackSpeed = 0.0f;
		// 근접 공격 범위
		AttackScale = 0.0f;
		// 투사체 공격 사거리
		AttackRange = 0.0f;
		// 타격 수
		MultiHitCount = 0;
		// 치명타 확률
		CriticalRate = 0.0f;
		// 치명타 대미지 배율
		CriticalDamageScale = 0.0f;

		// 방어력
		Armor = 0.0f;
		// 회피율
		Avoidability = 0.0f;

		// 이동 속도
		MoveSpeed = 0.0f;

		// 시야 거리
		Sight = 0.0f;
	}
	public CharacterStat(CharacterStat other)
	{
		// 체력
		Hp = other.Hp;

		// 공격력
		Attack = other.Attack;
		// 공격 속도
		AttackSpeed = other.AttackSpeed;
		// 근접 공격 범위
		AttackScale = other.AttackScale;
		// 투사체 공격 사거리
		AttackRange = other.AttackRange;
		// 타격 수
		MultiHitCount = other.MultiHitCount;
		// 치명타 확률
		CriticalRate = other.CriticalRate;
		// 치명타 대미지 배율
		CriticalDamageScale = other.CriticalDamageScale;

		// 방어력
		Armor = other.Armor;
		// 회피율
		Avoidability = other.Avoidability;

		// 이동 속도
		MoveSpeed = other.MoveSpeed;

		// 시야 거리
		Sight = other.Sight;
	}

	//private static readonly CharacterStat m_ZeroStat = new CharacterStat()
	//{
	//	// 체력
	//	Hp = 0.0f,
	//	// 체력 재생량 (체력 재생 시간마다 한 번 재생)
	//	HpRegen = 0.0f,
	//	// 체력 재생 시간
	//	HpRegenTime = 0.0f,
	//	// 힐 배율
	//	HealScale = 0.0f,

	//	// 공격력
	//	Attack = 0.0f,
	//	// 공격 속도
	//	AttackSpeed = 0.0f,
	//	// 근접 공격 범위
	//	AttackScale = 0.0f,
	//	// 투사체 공격 사거리
	//	AttackRange = 0.0f,
	//	// 타격 수
	//	MultiHitCount = 0,
	//	// 치명타 확률
	//	CriticalRate = 0.0f,
	//	// 치명타 대미지 배율
	//	CriticalDamageScale = 0.0f,

	//	// 방어력
	//	Armor = 0.0f,
	//	// 회피율
	//	Avoidability = 0.0f,

	//	// 이동 속도
	//	MoveSpeed = 0.0f,

	//	// 시야 거리
	//	Sight = 0.0f,
	//};
	//private static readonly CharacterStat m_OneStat = new CharacterStat()
	//{
	//	// 체력
	//	Hp = 1.0f,
	//	// 체력 재생량 (체력 재생 시간마다 한 번 재생)
	//	HpRegen = 1.0f,
	//	// 체력 재생 시간
	//	HpRegenTime = 1.0f,
	//	// 힐 배율
	//	HealScale = 1.0f,

	//	// 공격력
	//	Attack = 1.0f,
	//	// 공격 속도
	//	AttackSpeed = 1.0f,
	//	// 근접 공격 범위
	//	AttackScale = 1.0f,
	//	// 투사체 공격 사거리
	//	AttackRange = 1.0f,
	//	// 타격 수
	//	MultiHitCount = 1,
	//	// 치명타 확률
	//	CriticalRate = 1.0f,
	//	// 치명타 대미지 배율
	//	CriticalDamageScale = 1.0f,

	//	// 방어력
	//	Armor = 1.0f,
	//	// 회피율
	//	Avoidability = 1.0f,

	//	// 이동 속도
	//	MoveSpeed = 1.0f,

	//	// 시야 거리
	//	Sight = 1.0f,
	//};
	//public static CharacterStat zero => m_ZeroStat;
	//public static CharacterStat one => m_OneStat;

	public static CharacterStat operator +(CharacterStat s1, CharacterStat s2)
	{
		CharacterStat result = new CharacterStat();

		// 체력
		result.Hp = s1.Hp + s2.Hp;

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

		return result;
	}
}