using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuffDebuff.Enum
{
	#region 방향
	public enum E_Direction : sbyte
	{
		None = -1,

		Up,
		Down,
		Left,
		Right,

		Max
	}
	[System.Flags]
	public enum E_DirectionFlag : byte
	{
		None = 0,

		Up = 1 << 0,
		Down = 1 << 1,
		Left = 1 << 2,
		Right = 1 << 3,

		Max = 1 << 4,
		All = byte.MaxValue
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
		public static string ToString(string korStr)
		{
			// 한글 -> 영어 전환
			switch (korStr)
			{
				case "스탯형":
					korStr = "Stat";
					break;
				case "무기형":
					korStr = "Weapon";
					break;
				case "전투형":
					korStr = "Combat";
					break;

				case "공통":
					korStr = "All";
					break;
				case "근거리 무기":
					korStr = "Melee";
					break;
				case "원거리 무기":
					korStr = "Ranged";
					break;

				case "버프를 얻을 때":
					korStr = "Added";
					break;
				case "버프를 잃을 때":
					korStr = "Removed";
					break;
				case "매 프레임마다":
					korStr = "Update";
					break;
				case "일정 시간마다":
					korStr = "Timer";
					break;
				case "점프 시":
					korStr = "Jump";
					break;
				case "대쉬 시":
					korStr = "Dash";
					break;
				case "타격 시":
					korStr = "GiveDamage";
					break;
				case "피격 시":
					korStr = "TakeDamage";
					break;
				case "공격 시작 시":
					korStr = "AttackStart";
					break;
				case "공격 시":
					korStr = "Attack";
					break;
				case "공격 종료 시":
					korStr = "AttackEnd";
					break;
				case "적 처치 시":
					korStr = "KillEnemy";
					break;
				case "사망 시":
					korStr = "Death";
					break;
				case "스테이지를 넘어갈 시":
					korStr = "NextStage";
					break;
			}

			return korStr;
		}

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