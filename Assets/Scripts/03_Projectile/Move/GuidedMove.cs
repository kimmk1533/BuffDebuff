using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuffDebuff
{
	public class GuidedMove : ProjectileMove
	{
		[SerializeField]
		protected GameObject m_Target;

		public GuidedMove(GameObject target)
		{
			m_Target = target;
		}

		public override Vector2 CalculateVelocity(Projectile projectile)
		{
			if (m_Target == null)
			{
				return Vector2.zero;
			}

			Vector2 direction = m_Target.transform.position - projectile.transform.position;
			float distanceToTarget = direction.magnitude;
			direction.Normalize();

			float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
			Quaternion angleAxis = Quaternion.AngleAxis(angle - 90f, Vector3.forward);

			Vector2 dir = angleAxis * Vector2.up;

			float speed = projectile.moveSpeed;

			if (projectile.moveSpeed * Time.deltaTime > distanceToTarget)
				speed = distanceToTarget;

			return dir.normalized * speed;
		}
	};
}