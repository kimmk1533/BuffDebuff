using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuffDebuff
{
	//[CreateAssetMenu(fileName = "New Player Data", menuName = "Scriptable Object/Player Data", order = int.MinValue)]
	public class PlayerData : SOData
	{
		#region 변수
		[Header("체력")]
		// 체력
		[SerializeField, ReadOnly]
		private float m_Hp;

		[Header("치유")]
		// 체력 회복량
		[SerializeField, ReadOnly]
		private float m_HpRegen;
		// 체력 재생 쿨타임
		[SerializeField, ReadOnly, Tooltip("HpRegenTime초 마다 HpRegen만큼 회복")]
		private float m_HpRegenTime;
		// 힐 배율
		[SerializeField, ReadOnly]
		private float m_HealScale;
		// 치유 감소 배율
		[SerializeField, ReadOnly]
		private float m_AntiHealScale;

		[Header("방어")]
		// 방어력
		[SerializeField, ReadOnly]
		private float m_Armor;

		[Header("공격")]
		// 공격력
		[SerializeField, ReadOnly]
		private float m_AttackPower;
		// 공격 속도
		[SerializeField, ReadOnly, Tooltip("1초에 공격 가능한 횟수")]
		private float m_AttackSpeed;
		// 공격 크기
		[SerializeField, ReadOnly, Tooltip("투사체의 크기")]
		private float m_AttackSize;
		// 투사체 이동 속도
		[SerializeField, ReadOnly, Tooltip("투사체의 이동 속도")]
		private float m_ShotSpeed;
		// 공격 사거리(투사체 생존 시간)
		[SerializeField, ReadOnly, Tooltip("투사체의 생존 시간")]
		private float m_AttackRange;
		// 타격 수
		[SerializeField, ReadOnly, Tooltip("공격 한 번의 타수")]
		private int m_MultiHitCount;

		[Header("치명타")]
		// 치명타 확률
		[SerializeField, ReadOnly]
		private float m_CriticalRate;
		// 치명타 대미지 배율
		[SerializeField, ReadOnly]
		private float m_CriticalDamageScale;

		[Header("회피")]
		// 회피율
		[SerializeField, ReadOnly]
		private float m_Avoidability;

		[Header("이동")]
		// 이동 속도
		[SerializeField, ReadOnly]
		private float m_MoveSpeed;

		[Header("시야")]
		// 시야 거리
		[SerializeField, ReadOnly]
		private float m_Sight;

		[Header("경험치")]
		// 경험치 배율
		[SerializeField, ReadOnly]
		private float m_XpScale;

		[Header("대쉬")]
		// 대쉬 속도
		[SerializeField, ReadOnly]
		private float m_DashSpeed;
		// 대쉬 횟수
		[SerializeField, ReadOnly]
		private int m_DashCount;
		// 대쉬 충전 속도
		[SerializeField, ReadOnly]
		private float m_DashRechargeTime;
		#endregion

		#region 프로퍼티
		// 체력
		public float hp => m_Hp;

		// 체력 회복량
		public float hpRegen => m_HpRegen;
		// 체력 재생 쿨타임
		public float hpRegenTime => m_HpRegenTime;
		// 힐 배율
		public float healScale => m_HealScale;
		// 치유 감소 배율
		public float antiHealScale => m_AntiHealScale;

		// 방어력
		public float armor => m_Armor;

		// 공격력
		public float attackPower => m_AttackPower;
		// 공격 속도
		public float attackSpeed => m_AttackSpeed;
		// 공격 크기
		public float attackSize => m_AttackSize;
		// 투사체 이동 속도
		public float shotSpeed => m_ShotSpeed;
		// 공격 사거리(투사체 생존 시간)
		public float attackRange => m_AttackRange;
		// 타격 수
		public int multiHitCount => m_MultiHitCount;

		// 치명타 확률
		public float criticalRate => m_CriticalRate;
		// 치명타 대미지 배율
		public float criticalDamageScale => m_CriticalDamageScale;

		// 회피율
		public float avoidability => m_Avoidability;

		// 이동 속도
		public float moveSpeed => m_MoveSpeed;

		// 시야 거리
		public float sight => m_Sight;

		// 경험치 배율
		public float xpScale => m_XpScale;

		// 대쉬 속도
		public float dashSpeed => m_DashSpeed;
		// 대쉬 횟수
		public int dashCount => m_DashCount;
		// 대쉬 충전 속도
		public float dashRechargeTime => m_DashRechargeTime;
		#endregion

		public void Initialize(string _assetPath, int _code, string _title, float _hp, float _hpRegen, float _hpRegenTime,
			float _healScale, float _antiHealScale, float _armor, float _attackPower,
			float _attackSpeed, float _attackSize, float _shotSpeed, float _attackRange,
			int _multiHitCount, float _criticalRate, float _criticalDamageScale,
			float _avoidability, float _moveSpeed, float _sight, float _xpScale,
			float _dashSpeed, int _dashCount, float _dashRechargeTime)
		{
			// 에셋 경로
			m_AssetPath = _assetPath;
			// 코드
			m_Code = _code;
			// 명칭
			m_Title = _title;

			// 체력
			m_Hp = _hp;

			// 체력 회복량
			m_HpRegen = _hpRegen;
			// 체력 재생 쿨타임
			m_HpRegenTime = _hpRegenTime;
			// 힐 배율
			m_HealScale = _healScale;
			// 치유 감소 배율
			m_AntiHealScale = _antiHealScale;

			// 방어력
			m_Armor = _armor;

			// 공격력
			m_AttackPower = _attackPower;
			// 공격 속도
			m_AttackSpeed = _attackSpeed;
			// 공격 크기
			m_AttackSize = _attackSize;
			// 투사체 이동 속도
			m_ShotSpeed = _shotSpeed;
			// 공격 사거리(투사체 생존 시간)
			m_AttackRange = _attackRange;
			// 타격 수
			m_MultiHitCount = _multiHitCount;

			// 치명타 확률
			m_CriticalRate = _criticalRate;
			// 치명타 대미지 배율
			m_CriticalDamageScale = _criticalDamageScale;

			// 회피율
			m_Avoidability = _avoidability;

			// 이동 속도
			m_MoveSpeed = _moveSpeed;

			// 시야 거리
			m_Sight = _sight;

			// 경험치 배율
			m_XpScale = _xpScale;

			// 대쉬 속도
			m_DashSpeed = _dashSpeed;
			// 대쉬 횟수
			m_DashCount = _dashCount;
			// 대쉬 충전 속도
			m_DashRechargeTime = _dashRechargeTime;
		}
	}
}