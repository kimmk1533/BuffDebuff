using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuffDebuff
{
	public struct DamageArg<DamageGiver, DamageTaker>
	{
		public DamageGiver damageGiver;
		public DamageTaker damageTaker;
		public Projectile projectile;
	}
}
