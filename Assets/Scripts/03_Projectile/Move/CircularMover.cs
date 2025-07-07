using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuffDebuff
{
	public class CircularMover : ProjectileMover
	{
		[SerializeField]
		protected GameObject m_Target;
		[SerializeField]
		protected float m_AngularSpeed;
		protected float m_AngularAcceleration;
		protected float m_RandomAngle;
		protected Quaternion m_InitRotation;
		private bool m_IsFirst;

		public CircularMover(GameObject target, float angularAcceleration, float randomAngle = 0f)
		{
			m_Target = target;
			m_AngularSpeed = 0f;
			m_AngularAcceleration = angularAcceleration;
			m_RandomAngle = randomAngle;
			m_IsFirst = true;
		}

		public override Vector2 CalculateVelocity(Projectile projectile)
		{
			if (m_Target == null)
			{
				return Vector2.zero;
			}

			Quaternion rotation = projectile.transform.rotation;
			Vector3 eulerAngles = rotation.eulerAngles;

			if (m_IsFirst == true)
			{
				m_IsFirst = false;

				eulerAngles.z += Random.Range(-m_RandomAngle, m_RandomAngle);
				rotation.eulerAngles = eulerAngles;
				projectile.transform.rotation = rotation;
				m_InitRotation = rotation;
			}

			m_AngularSpeed += m_AngularAcceleration * Time.deltaTime;

			Vector2 target = (m_Target.transform.position - projectile.transform.position);

			int sign = 1;

			Vector3 cross = Vector3.Cross(projectile.transform.up, target.normalized);
			if (cross.z < 0)
				sign = -1;

			float dot = Vector2.Dot(projectile.transform.up, target.normalized);
			float theta = Mathf.Acos(Mathf.Clamp(dot, -1f, 1f)) * Mathf.Rad2Deg;

			eulerAngles.z += sign * Mathf.Min(m_AngularSpeed, theta);
			rotation.eulerAngles = eulerAngles;

			Vector2 dir = rotation * Vector2.up;

			if (theta <= m_AngularSpeed)
				m_AngularSpeed = 0f;

			float distanceToTarget = target.magnitude;

			float speed = projectile.movementSpeed;

			if (projectile.movementSpeed * Time.deltaTime > distanceToTarget)
				speed = distanceToTarget;

			return dir.normalized * speed;
		}
	}
}