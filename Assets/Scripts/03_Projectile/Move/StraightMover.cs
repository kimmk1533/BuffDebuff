using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuffDebuff
{
	public class StraightMover : ProjectileMover
	{
		public override Vector2 CalculateVelocity(Projectile projectile)
		{
			Vector2 dir = projectile.transform.rotation * Vector2.right;

			return dir.normalized * projectile.movementSpeed;
		}
	}
}