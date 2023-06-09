using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Projectile : MonoBehaviour
{
	[SerializeField]
	protected float m_MoveSpeed;
	[SerializeField]
	protected float m_LifeTime;
	[SerializeField]
	protected LayerMask m_CollisionLayerMask;

	protected ProjectileManager M_Project => ProjectileManager.Instance;

	protected virtual void OnEnable()
	{
		Invoke("DeSpawn", m_LifeTime);
	}
	protected virtual void OnDisable()
	{
		CancelInvoke("DeSpawn");
	}
	protected virtual void Update()
	{
		RotateToTarget();

		Move();
	}

	public virtual void Initialize(float moveSpeed, float lifeTime)
	{
		m_MoveSpeed = moveSpeed;
		m_LifeTime = lifeTime;
	}

	protected abstract void DeSpawn();
	protected abstract void Move();
	//protected void Move()
	//{
	//	if (m_MovingType == E_MovingType.None)
	//		return;

	//	if (m_MovingType == E_MovingType.Straight)
	//		transform.position += transform.rotation * Vector2.up * moveSpeed;
	//	else if (m_Target == null)
	//		return;

	//	transform.position += transform.rotation * Vector2.up * Mathf.Min(moveSpeed, distanceToTarget);
	//}
	protected abstract void RotateToTarget();
	//protected void RotateToTarget()
	//{
	//	if (null == m_Target || m_MovingType == E_MovingType.None)
	//		return;

	//	Vector2 direction = m_Target.transform.position - transform.position;
	//	direction.Normalize();

	//	float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
	//	Quaternion angleAxis = Quaternion.AngleAxis(angle - 90f, Vector3.forward);

	//	//Vector2 axis = transform.position + transform.up;
	//	//axis.Normalize();

	//	//float theta = Mathf.Acos(Vector2.Dot(direction, axis)) * Mathf.Rad2Deg;

	//	//Debug.Log(theta);

	//	//if (p_DistanceToTarget < 1f)
	//	//	m_AngularSpeed = 0f;

	//	Quaternion rotation = new Quaternion();
	//	switch (m_MovingType)
	//	{
	//		case E_MovingType.Straight:
	//			rotation = transform.rotation;
	//			break;
	//		//case E_MovingType.CircularToTarget:
	//		//	if (m_AngularSpeed < m_AngularMaxSpeed)
	//		//		m_AngularSpeed += m_AngularAcceleration * Time.deltaTime;
	//		//	else
	//		//		m_AngularSpeed = m_AngularMaxSpeed;

	//		//	rotation = Quaternion.Slerp(transform.rotation, angleAxis, m_AngularSpeed * Time.deltaTime);
	//		//	break;
	//		case E_MovingType.DirectToTarget:
	//			rotation = angleAxis;
	//			break;
	//	}

	//	transform.rotation = rotation;

	//	//if (m_MovingType == E_MovingType.Circular)
	//	//{
	//	//	float rotateAmount = Vector3.Cross(direction, transform.rotation * Vector2.up).z;
	//	//	angle = -rotateAmount * m_AngularSpeed * Time.deltaTime;

	//	//	transform.Rotate(Vector3.forward, angle);
	//	//}
	//}
}