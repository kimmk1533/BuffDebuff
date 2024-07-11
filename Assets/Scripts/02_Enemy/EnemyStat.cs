using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuffDebuff
{
	[System.Serializable]
	public class EnemyStat : CharacterStat
	{
		public EnemyStat() : base()
		{

		}
		public EnemyStat(EnemyStat other) : base(other)
		{

		}
	}
}