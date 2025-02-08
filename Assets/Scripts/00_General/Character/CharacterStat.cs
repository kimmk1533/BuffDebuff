using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BuffDebuff
{
	[System.Serializable]
	public class CharacterStat
	{
		[field: Title("체력")]
		// 체력
		[field: SerializeField, InlineProperty(LabelWidth = 120), LabelWidth(20)]
		public StatValue<float> Hp { get; set; }

		[field: Title("치유")]
		// 체력 회복량
		[field: SerializeField]
		public float HpRegen { get; set; }
		// 체력 재생 쿨타임
		[field: SerializeField, Tooltip("HpRegenTime초 마다 HpRegen만큼 회복")]
		public float HpRegenTime { get; set; }
		// 힐 배율
		[field: SerializeField]
		public float HealScale { get; set; }
		// 치유 감소 배율
		[field: SerializeField]
		public float AntiHealScale { get; set; }

		[field: Title("방어")]
		// 방어력
		[field: SerializeField]
		public float Armor { get; set; }

		[field: Title("공격")]
		// 공격력
		[field: SerializeField]
		public float AttackPower { get; set; }
		// 공격 속도
		[field: SerializeField, Tooltip("1초에 공격 가능한 횟수")]
		public float AttackSpeed { get; set; }
		// 공격 크기
		[field: SerializeField, Tooltip("투사체의 크기")]
		public float AttackSize { get; set; }
		// 투사체 이동 속도
		[field: SerializeField, Tooltip("투사체의 이동 속도")]
		public float ShotSpeed { get; set; }
		// 공격 사거리(투사체 생존 시간)
		[field: SerializeField, Tooltip("투사체의 생존 시간")]
		public float AttackRange { get; set; }
		// 타격 수
		[field: SerializeField, Tooltip("공격 한 번의 타수")]
		public int MultiHitCount { get; set; }

		[field: Title("치명타")]
		// 치명타 확률
		[field: SerializeField]
		public float CriticalRate { get; set; }
		// 치명타 대미지 배율
		[field: SerializeField]
		public float CriticalDamageScale { get; set; }

		[field: Title("회피")]
		// 회피율
		[field: SerializeField]
		public float Avoidability { get; set; }

		[field: Title("이동")]
		// 이동 속도
		[field: SerializeField]
		public float MoveSpeed { get; set; }

		[field: Title("시야")]
		// 시야 거리
		[field: SerializeField]
		public float Sight { get; set; }

		// 플레이어:	얻은 경험치
		//		적:	얻을 경험치
		[field: Title("경험치")]
		// 경험치
		[field: SerializeField, InlineProperty(LabelWidth = 120), LabelWidth(20)]
		public StatValue<float> Xp { get; set; }
		// 경험치 배율
		[field: SerializeField]
		public float XpScale { get; set; }

		protected CharacterStat()
		{
			// 체력
			Hp = new StatValue<float>(0.0f);

			// 체력 회복량 (체력 재생 시간마다 한 번 재생)
			HpRegen = 0.0f;
			// 체력 재생 시간
			HpRegenTime = 0.0f;
			// 힐 배율
			HealScale = 1.0f;
			// 치유 감소 배율
			AntiHealScale = 1.0f;

			// 방어력
			Armor = 0.0f;

			// 공격력
			AttackPower = 0.0f;
			// 공격 속도
			AttackSpeed = 0.0f;
			// 근접 공격 범위
			AttackSize = 0.0f;
			// 투사체 이동 속도
			ShotSpeed = 1.0f;
			// 투사체 공격 사거리
			AttackRange = 0.0f;
			// 타격 수
			MultiHitCount = 0;
			// 치명타 확률
			CriticalRate = 0.0f;
			// 치명타 대미지 배율
			CriticalDamageScale = 1.0f;

			// 회피율
			Avoidability = 0.0f;

			// 이동 속도
			MoveSpeed = 0.0f;

			// 시야 거리
			Sight = 0.0f;

			// 경험치
			Xp = new StatValue<float>(0.0f);
			// 경험치 배율
			XpScale = 1.0f;
		}
		protected CharacterStat(CharacterStat other)
		{
			// 체력
			Hp = other.Hp;

			// 체력 회복량 (체력 재생 시간마다 한 번 재생)
			HpRegen = other.HpRegen;
			// 체력 재생 시간
			HpRegenTime = other.HpRegenTime;
			// 힐 배율
			HealScale = other.HealScale;
			// 치유 감소 배율
			AntiHealScale = other.AntiHealScale;

			// 방어력
			Armor = other.Armor;

			// 공격력
			AttackPower = other.AttackPower;
			// 공격 속도
			AttackSpeed = other.AttackSpeed;
			// 근접 공격 범위
			AttackSize = other.AttackSize;
			// 투사체 이동 속도
			ShotSpeed = other.ShotSpeed;
			// 투사체 공격 사거리
			AttackRange = other.AttackRange;
			// 타격 수
			MultiHitCount = other.MultiHitCount;
			// 치명타 확률
			CriticalRate = other.CriticalRate;
			// 치명타 대미지 배율
			CriticalDamageScale = other.CriticalDamageScale;

			// 회피율
			Avoidability = other.Avoidability;

			// 이동 속도
			MoveSpeed = other.MoveSpeed;

			// 시야 거리
			Sight = other.Sight;

			// 경험치
			Xp = other.Xp;
			// 경험치 배율
			XpScale = other.XpScale;
		}

		public virtual CharacterStat Clone()
		{
			CharacterStat stat = new CharacterStat();

			// 체력
			stat.Hp = this.Hp;

			// 체력 회복량 (체력 재생 시간마다 한 번 재생)
			stat.HpRegen = this.HpRegen;
			// 체력 재생 시간
			stat.HpRegenTime = this.HpRegenTime;
			// 힐 배율
			stat.HealScale = this.HealScale;
			// 치유 감소 배율
			stat.AntiHealScale = this.AntiHealScale;

			// 방어력
			stat.Armor = this.Armor;

			// 공격력
			stat.AttackPower = this.AttackPower;
			// 공격 속도
			stat.AttackSpeed = this.AttackSpeed;
			// 근접 공격 범위
			stat.AttackSize = this.AttackSize;
			// 투사체 이동 속도
			stat.ShotSpeed = this.ShotSpeed;
			// 투사체 공격 사거리
			stat.AttackRange = this.AttackRange;
			// 타격 수
			stat.MultiHitCount = this.MultiHitCount;
			// 치명타 확률
			stat.CriticalRate = this.CriticalRate;
			// 치명타 대미지 배율
			stat.CriticalDamageScale = this.CriticalDamageScale;

			// 회피율
			stat.Avoidability = this.Avoidability;

			// 이동 속도
			stat.MoveSpeed = this.MoveSpeed;

			// 시야 거리
			stat.Sight = this.Sight;

			// 경험치
			stat.Xp = this.Xp;
			// 경험치 배율
			stat.XpScale = this.XpScale;

			return stat;
		}
		//private static readonly CharacterStat m_ZeroStat = new CharacterStat()
		//{
		//	// 체력
		//	Hp = 0.0f,
		//	// 체력 회복량 (체력 재생 시간마다 한 번 재생)
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
		//	// 체력 회복량 (체력 재생 시간마다 한 번 재생)
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

		//public static CharacterStat operator +(CharacterStat s1, CharacterStat s2)
		//{
		//	CharacterStat result = new CharacterStat();

		//	// 체력
		//	result.Hp = s1.Hp + s2.Hp;

		//	// 체력 회복량 (체력 재생 시간마다 한 번 재생)
		//	result.HpRegen = s1.HpRegen + s2.HpRegen;
		//	// 체력 재생 시간
		//	result.HpRegenTime = s1.HpRegenTime + s2.HpRegenTime;
		//	// 모든 힐 배율
		//	result.HealScale = s1.HealScale + s2.HealScale;

		//	// 공격력
		//	result.Attack = s1.Attack + s2.Attack;
		//	// 공격 속도
		//	result.AttackSpeed = s1.AttackSpeed + s2.AttackSpeed;
		//	// 근접 공격 범위
		//	result.AttackSize = s1.AttackSize + s2.AttackSize;
		//	// 투사체 공격 사거리
		//	result.AttackRange = s1.AttackRange + s2.AttackRange;
		//	// 타격 수
		//	result.MultiHitCount = s1.MultiHitCount + s2.MultiHitCount;
		//	// 치명타 확률
		//	result.CriticalRate = s1.CriticalRate + s2.CriticalRate;
		//	// 치명타 대미지 배율
		//	result.CriticalDamageScale = s1.CriticalDamageScale + s2.CriticalDamageScale;

		//	// 방어력
		//	result.Armor = s1.Armor + s2.Armor;
		//	// 회피율
		//	result.Avoidability = s1.Avoidability + s2.Avoidability;

		//	// 이동 속도
		//	result.MoveSpeed = s1.MoveSpeed + s2.MoveSpeed;

		//	// 시야 거리
		//	result.Sight = s1.Sight + s2.Sight;

		//	// 경험치
		//	result.Xp = s1.Xp + s2.Xp;
		//	// 경험치 배율
		//	result.XpScale = s1.XpScale + s2.XpScale;

		//	return result;
		//}
	}
}