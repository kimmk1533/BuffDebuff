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
		// 최대 대쉬 횟수
		// 대쉬 횟수
		[field: SerializeField]
		public StatValue<int> DashCount { get; set; }
		// 대쉬 충전 속도
		[field: SerializeField]
		public float DashRechargeTime { get; set; }

		public PlayerStat() : base()
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
		public PlayerStat(PlayerStat other) : base(other)
		{
			// 레벨
			Level = other.Level;

			// 대쉬 속도
			DashSpeed = other.DashSpeed;
			// 대쉬 횟수
			DashCount = other.DashCount;
			// 대쉬 충전 속도
			DashRechargeTime = other.DashRechargeTime;
		}
	}
}