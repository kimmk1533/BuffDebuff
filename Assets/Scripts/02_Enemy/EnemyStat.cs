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
		// public static EnemyStat Clone(EnemyData data) 제작 예정
	}
}