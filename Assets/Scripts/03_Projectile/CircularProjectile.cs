using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircularProjectile : Projectile
{
	[SerializeField]
	protected GameObject m_Target;
	[SerializeField]
	protected float m_AngularSpeed;
	protected float m_AngularMaxSpeed;
	protected float m_AngularAcceleration;

	// 타겟 위치
	protected Vector3 targetPos => (m_Target == null ? transform.position : m_Target.transform.position);
	// 타겟까지의 방향
	protected Vector3 targetDir => (targetPos - transform.position).normalized;
	// 타겟까지의 거리
	protected float distanceToTarget => Vector2.Distance(transform.position, targetPos);

	public override void Initialize(float moveSpeed, float lifeTime)
	{
		base.Initialize(moveSpeed, lifeTime);

		m_Target = null;
		m_AngularSpeed = moveSpeed;
		m_AngularMaxSpeed = moveSpeed * 2f;
		m_AngularAcceleration = moveSpeed * 0.5f;
	}
	public void Initialize(GameObject target, float moveSpeed, float lifeTime)
	{
		if (target == null)
			return;

		Initialize(moveSpeed, lifeTime);
		m_Target = target;
	}

	protected override void DeSpawn()
	{
		M_Project.DeSpawn("Circular", this);
	}
	protected override void RotateToTarget()
	{
		if (null == m_Target)
			return;

		Vector2 direction = m_Target.transform.position - transform.position;
		direction.Normalize();

		float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
		Quaternion angleAxis = Quaternion.AngleAxis(angle - 90f, Vector3.forward);

		if (m_AngularSpeed < m_AngularMaxSpeed)
			m_AngularSpeed += m_AngularAcceleration * Time.deltaTime;
		else
			m_AngularSpeed = m_AngularMaxSpeed;

		transform.rotation = Quaternion.Slerp(transform.rotation, angleAxis, m_AngularSpeed * Time.deltaTime);
	}
	protected override void Move()
	{
		if (m_Target == null)
			return;

		transform.position += transform.rotation * Vector2.up * Mathf.Min(m_MoveSpeed * Time.deltaTime, distanceToTarget);
	}
}