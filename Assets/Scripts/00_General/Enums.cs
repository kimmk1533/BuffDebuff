using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuffDebuff.Enum
{
	#region 방향
	public enum E_Direction : byte
	{
		Up,
		Down,
		Left,
		Right,

		Max
	}

	public static class DirEnumUtil
	{
		// 워프 이후의 방향 (E_Direction의 반대 방향)
		private static readonly E_Direction[] c_DirectionAfterWarp = { E_Direction.Down, E_Direction.Up, E_Direction.Right, E_Direction.Left };
		// E_Direction -> Vector2Int
		private static readonly Vector2Int[] c_Direction = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

		public static E_Direction GetOtherDir(E_Direction direction)
		{
			return c_DirectionAfterWarp[(int)direction];
		}
		public static Vector2Int ConvertToVector2Int(E_Direction direction)
		{
			return c_Direction[(int)direction];
		}
	}
	#endregion

	#region 버프
	// 버프 종류
	public enum E_BuffType : byte
	{
		// 버프
		Buff,
		// 디버프
		Debuff,
		// 양면버프
		Bothbuff,

		Max
	}
	// 버프 효과 종류
	public enum E_BuffEffectType : byte
	{
		// 스탯형
		Stat,
		// 무기형
		Weapon,
		// 전투형
		Combat,

		Max
	}
	// 버프 등급
	public enum E_BuffGrade : byte
	{
		Normal = 0,
		Uncommon,
		Rare,
		Unique,
		Epic,
		Legendary,
		GOD,

		Max
	}
	// 적용되는 무기
	public enum E_BuffWeapon : byte
	{
		// 모두
		All,
		// 근거리만
		Melee,
		// 원거리만
		Ranged,

		Max
	}
	// 버프 발동 조건
	public enum E_BuffInvokeCondition : byte
	{
		// 버프를 얻을 때
		Added,
		// 버프를 잃을 때
		Removed,
		// 매 프레임마다
		Update,
		// 일정 시간마다
		Timer,
		// 점프 시
		Jump,
		// 대쉬 시
		Dash,
		// 타격 시
		GiveDamage,
		// 피격 시
		TakeDamage,
		// 공격 시작 시	(애니메이션 시작)
		AttackStart,
		// 공격 시		(애니메이션 도중)
		Attack,
		// 공격 종료 시	(애니메이션 종료)
		AttackEnd,
		// 적 처치 시
		KillEnemy,
		// 사망 시
		Death,
		// 스테이지를 넘어갈 시
		NextStage,

		Max
	}

	public static class BuffEnumUtil
	{
		public static string ToKorString<T>(T value) where T : struct, System.Enum
		{
			string str = value.ToString();

			if (E_BuffType.TryParse(str, out E_BuffType buffType))
			{
				switch (buffType)
				{
					case E_BuffType.Buff:
						return "버프";
					case E_BuffType.Debuff:
						return "디버프";
					case E_BuffType.Bothbuff:
						return "양면버프";
				}
			}
			else if (E_BuffEffectType.TryParse(str, out E_BuffEffectType buffEffectType))
			{
				switch (buffEffectType)
				{
					case E_BuffEffectType.Stat:
						return "스탯형";
					case E_BuffEffectType.Weapon:
						return "무기형";
					case E_BuffEffectType.Combat:
						return "전투형";
				}
			}

			return str;
		}
	}
	#endregion
}