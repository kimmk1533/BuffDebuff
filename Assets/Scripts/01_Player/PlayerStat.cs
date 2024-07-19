using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuffDebuff
{
	[System.Serializable]
	public class PlayerStat : CharacterStat
	{
		[field: Header("레벨")]
		// 레벨
		[field: SerializeField]
		public StatValue<int> Level { get; set; }

		[field: Header("대쉬")]
		// 대쉬 속도
		[field: SerializeField]
		public float DashSpeed { get; set; }
		// 대쉬 횟수
		[field: SerializeField]
		public StatValue<int> DashCount { get; set; }
		// 대쉬 충전 속도
		[field: SerializeField]
		public float DashRechargeTime { get; set; }

		protected PlayerStat() : base()
		{
			// 레벨
			Level = new StatValue<int>(0);

			// 대쉬 속도
			DashSpeed = 0f;
			// 대쉬 횟수
			DashCount = new StatValue<int>(0);
			// 대쉬 충전 속도
			DashRechargeTime = 0.0f;
		}
		protected PlayerStat(CharacterStat other) : base(other)
		{
			PlayerStat otherStat = other as PlayerStat;
			if (otherStat == null)
				Debug.LogError("PlayerStat 복사생성자 오류");

			// 레벨
			Level = otherStat.Level;

			// 대쉬 속도
			DashSpeed = otherStat.DashSpeed;
			// 대쉬 횟수
			DashCount = otherStat.DashCount;
			// 대쉬 충전 속도
			DashRechargeTime = otherStat.DashRechargeTime;
		}

		public override CharacterStat Clone()
		{
			return new PlayerStat(this);
		}
		public static PlayerStat Clone(PlayerData data)
		{
			PlayerStat stat = new PlayerStat();

			// 체력
			stat.Hp = new StatValue<float>(data.hp);

			// 체력 회복량
			stat.HpRegen = data.hpRegen;
			// 체력 재생 쿨타임
			stat.HpRegenTime = data.hpRegenTime;
			// 힐 배율
			stat.HealScale = data.healScale;
			// 치유 감소 배율
			stat.AntiHealScale = data.antiHealScale;

			// 방어력
			stat.Armor = data.armor;

			// 공격력
			stat.AttackPower = data.attackPower;
			// 공격 속도
			stat.AttackSpeed = data.attackSpeed;
			// 공격 크기
			stat.AttackSize = data.attackSize;
			// 투사체 이동 속도
			stat.ShotSpeed = data.shotSpeed;
			// 공격 사거리(투사체 생존 시간)
			stat.AttackRange = data.attackRange;
			// 타격 수
			stat.MultiHitCount = data.multiHitCount;

			// 치명타 확률
			stat.CriticalRate = data.criticalRate;
			// 치명타 대미지 배율
			stat.CriticalDamageScale = data.criticalDamageScale;

			// 회피율
			stat.Avoidability = data.avoidability;

			// 이동 속도
			stat.MoveSpeed = data.moveSpeed;

			// 시야 거리
			stat.Sight = data.sight;

			// 얻은 경험치
			stat.Xp = new StatValue<float>();
			// 경험치 배율
			stat.XpScale = data.xpScale;

			// 레벨
			stat.Level = new StatValue<int>(0, 100);

			// 대쉬 속도
			stat.DashSpeed = data.dashSpeed;
			// 대쉬 횟수
			stat.DashCount = new StatValue<int>(data.dashCount);
			// 대쉬 충전 속도
			stat.DashRechargeTime = data.dashRechargeTime;

			return stat;
		}
	}
}