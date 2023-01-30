using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region enum
public enum E_MovingType
{
	None,       // 이동 금지
	Straight,   // 직선 이동
	Circular,   // 원형 이동(회전 속도 제한 넣어 안맞을 수 있음)
	Direct,     // 목표에게 직진(100% 맞는 유도탄)
}
#endregion

public class Projectile : MonoBehaviour
{
	[ReadOnly(true)]
	public GameObject m_Target;
	public Vector2 m_InitPos;
	public Vector2? m_TargetInitPos;
	public float m_MoveSpeed;
	public float m_AngularMaxSpeed;
	public float m_AngularAcceleration;
	public float m_AngularSpeed;
	public E_MovingType m_MovingType;

	#region property
	// 스킬 이동 속도
	protected float moveSpeed => m_MoveSpeed * Time.deltaTime;
	// 타겟 위치
	protected Vector3 targetPos => (m_Target == null ? transform.position : m_Target.transform.position);
	// 타겟까지의 방향
	protected Vector3 targetDir => (targetPos - transform.position).normalized;
	// 타겟까지의 거리
	protected float distanceToTarget => Vector3.Distance(transform.position, targetPos);
	#endregion
	#region manager
	ProjectileManager M_Project => ProjectileManager.Instance;
	#endregion

	private void OnEnable()
	{
		m_InitPos = transform.position;
	}

	private void Update()
	{
		Rotate();

		Move();

		#region Temp

		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			m_MovingType = E_MovingType.None;
		}
		if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			m_MovingType = E_MovingType.Straight;
		}
		if (Input.GetKeyDown(KeyCode.Alpha3))
		{
			m_MovingType = E_MovingType.Circular;
		}
		if (Input.GetKeyDown(KeyCode.Alpha4))
		{
			m_MovingType = E_MovingType.Direct;
		}

		#endregion
	}

	protected void Rotate()
	{
		if (null == m_Target || m_MovingType == E_MovingType.None)
			return;

		Vector2 direction = m_Target.transform.position - transform.position;
		direction.Normalize();

		float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
		Quaternion angleAxis = Quaternion.AngleAxis(angle - 90f, Vector3.forward);
		Quaternion rotation = new Quaternion();

		Vector2 axis = transform.position + transform.up;
		axis.Normalize();

		float theta = Mathf.Acos(Vector2.Dot(direction, axis)) * Mathf.Rad2Deg;

		//Debug.Log(theta);

		//if (p_DistanceToTarget < 1f)
		//	m_AngularSpeed = 0f;

		if (m_AngularSpeed < m_AngularMaxSpeed)
			m_AngularSpeed += m_AngularAcceleration * Time.deltaTime;
		else
			m_AngularSpeed = m_AngularMaxSpeed;

		switch (m_MovingType)
		{
			case E_MovingType.Straight:
				rotation = transform.rotation;
				break;
			case E_MovingType.Circular:
				rotation = Quaternion.Slerp(transform.rotation, angleAxis, m_AngularSpeed * Time.deltaTime);
				break;
			case E_MovingType.Direct:
				rotation = angleAxis;
				break;
		}

		transform.rotation = rotation;

		//if (m_MovingType == E_MovingType.Circular)
		//{
		//	float rotateAmount = Vector3.Cross(direction, transform.rotation * Vector2.up).z;
		//	angle = -rotateAmount * m_AngularSpeed * Time.deltaTime;

		//	transform.Rotate(Vector3.forward, angle);
		//}
	}
	protected void Move()
	{
		if (m_MovingType != E_MovingType.Straight && null == m_Target)
			return;

		if (m_MovingType == E_MovingType.None)
			return;

		transform.position += transform.rotation * Vector2.up * ((distanceToTarget != 0) ? Mathf.Min(moveSpeed, distanceToTarget) : moveSpeed);
	}
}