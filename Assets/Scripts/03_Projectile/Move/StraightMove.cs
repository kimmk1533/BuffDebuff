using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuffDebuff
{
	public class StraightMove : ProjectileMove
	{
		public override Vector2 CalculateVelocity(Projectile projectile)
		{
			Vector2 dir = projectile.transform.rotation * Vector2.up;

			return dir.normalized * projectile.moveSpeed;
		}
	}
}