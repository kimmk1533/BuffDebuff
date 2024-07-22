using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuffDebuff
{
	[System.Serializable]
	public class EnemyStat : CharacterStat
	{
		protected EnemyStat() : base()
		{

		}
		protected EnemyStat(CharacterStat other) : base(other)
		{

		}

		public override CharacterStat Clone()
		{
			return new EnemyStat(this);
		}
		public static EnemyStat Clone(EnemyData data)
		{
			EnemyStat stat = new EnemyStat();

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
			stat.Xp = new StatValue<float>(data.xp);
			// 경험치 배율
			stat.XpScale = data.xpScale;

			return stat;
		}
	}
}