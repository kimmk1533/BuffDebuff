using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyCharacterStat : CharacterStat
{
	public EnemyCharacterStat() : base()
	{

	}
	public EnemyCharacterStat(EnemyCharacterStat other) : base(other)
	{

	}

	public static EnemyCharacterStat operator +(EnemyCharacterStat s1, EnemyCharacterStat s2)
	{
		EnemyCharacterStat result = new EnemyCharacterStat();

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