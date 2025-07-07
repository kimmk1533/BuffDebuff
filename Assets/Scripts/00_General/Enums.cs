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
	public enum E_BuffWeaponType : byte
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
	// 버프 값 적용 방식
	public enum E_BuffValueType : byte
	{
		// 사용안함
		None,
		// 합연산
		Plus,
		// 곱연산
		Multiply,

		Max
	}

	public static class BuffEnumUtil
	{
		public static bool TryParseKorStr<TEnum>(string korStr, out TEnum result) where TEnum : struct, System.Enum
		{
			string enumStr = ToString(korStr);

			return System.Enum.TryParse<TEnum>(enumStr, out result);
		}
		private static string ToString(string korStr)
		{
			// 한글 -> 영어 전환
			switch (korStr)
			{
				#region Buff Type
				case "버프":
					return "Buff";
				case "디버프":
					return "Debuff";
				case "양면버프":
					return "BothBuff";
				#endregion

				#region Buff Effect Type
				case "스탯형":
					return "Stat";
				case "무기형":
					return "Weapon";
				case "전투형":
					return "Combat";
				#endregion

				#region Buff Weapon Type
				case "공통":
					return "All";
				case "근거리 무기":
					return "Melee";
				case "원거리 무기":
					return "Ranged";
				#endregion

				#region Buff Invoke Condition
				case "버프를 얻을 때":
					return "Added";
				case "버프를 잃을 때":
					return "Removed";
				case "매 프레임마다":
					return "Update";
				case "일정 시간마다":
					return "Timer";
				case "점프 시":
					return "Jump";
				case "대쉬 시":
					return "Dash";
				case "타격 시":
					return "GiveDamage";
				case "피격 시":
					return "TakeDamage";
				case "공격 시작 시":
					return "AttackStart";
				case "공격 시":
					return "Attack";
				case "공격 종료 시":
					return "AttackEnd";
				case "적 처치 시":
					return "KillEnemy";
				case "사망 시":
					return "Death";
				case "스테이지를 넘어갈 시":
					return "NextStage";
				#endregion

				#region Buff Value Type
				case "사용 안함":
					return "None";
				case "합연산":
					return "Plus";
				case "곱연산":
					return "Multiply";
					#endregion
			}

			return string.Empty;
		}

		public static string ToKorString<TEnum>(TEnum value) where TEnum : System.Enum
		{
			switch (typeof(TEnum).Name)
			{
				#region Buff Type
				case "E_BuffType":
					if (System.Enum.TryParse<E_BuffType>(value.ToString(), out E_BuffType buffType) == false)
						break;

					return ToKorString_BuffType(buffType);
				#endregion
				#region Buff Effect Type
				case "E_BuffEffectType":
					if (System.Enum.TryParse<E_BuffEffectType>(value.ToString(), out E_BuffEffectType buffEffectType) == false)
						break;

					return ToKorString_BuffEffectType(buffEffectType);
				#endregion
				#region Buff Value Type
				case "E_BuffValueType":
					if (System.Enum.TryParse<E_BuffValueType>(value.ToString(), out E_BuffValueType buffValueType) == false)
						break;

					return ToKorString_BuffValueType(buffValueType);
					#endregion
			}

			return string.Empty;
		}
		private static string ToKorString_BuffType(E_BuffType buffType)
		{
			switch (buffType)
			{
				case E_BuffType.Buff:
					return "버프";
				case E_BuffType.Debuff:
					return "디버프";
			}

			return string.Empty;
		}
		private static string ToKorString_BuffEffectType(E_BuffEffectType buffEffectType)
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

			return string.Empty;
		}
		private static string ToKorString_BuffValueType(E_BuffValueType buffValueType)
		{
			switch (buffValueType)
			{
				case E_BuffValueType.None:
					return "사용 안함";
				case E_BuffValueType.Plus:
					return "합연산";
				case E_BuffValueType.Multiply:
					return "곱연산";
			}

			return string.Empty;
		}
	}
	#endregion
}