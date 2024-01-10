using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ProjectileController))]
public sealed class Projectile : ObjectPoolItemBase
{
	#region 변수
	private ProjectileController m_Controller;

	[SerializeField, ReadOnly]
	private BoxCollisionChecker2D m_CollisionChecker2D;

	private IMovingStrategy m_MovingStrategy;
	[SerializeField, ReadOnly]
	private float m_MoveSpeed;

	[SerializeField, ReadOnly]
	private Vector2 m_Velocity;

	[SerializeField, ReadOnly]
	private UtilClass.Timer m_DespawnTimer;
	#endregion

	#region 프로퍼티
	public float moveSpeed => m_MoveSpeed;

	#region 인덱서
	public CollisionChecker2D.Trigger this[int layer] => m_CollisionChecker2D[layer];
	public CollisionChecker2D.Trigger this[string layer] => m_CollisionChecker2D[layer];
	#endregion
	#endregion

	#region 매니저
	private static ProjectileManager M_Projectile => ProjectileManager.Instance;
	#endregion

	public void Initialize(float moveSpeed, float lifeTime)
	{
		base.InitializePoolItem();

		m_MoveSpeed = moveSpeed;

		#region SAFE_INIT
		this.Safe_GetComponent<ProjectileController>(ref m_Controller);
		m_Controller.Initialize();

		this.Safe_GetComponent<BoxCollisionChecker2D>(ref m_CollisionChecker2D);
		m_CollisionChecker2D.Initialize();

		if (m_DespawnTimer == null)
			m_DespawnTimer = new UtilClass.Timer();
		m_DespawnTimer.interval = lifeTime;
		#endregion
	}
	public override void FinallizePoolItem()
	{
		m_CollisionChecker2D.Finallize();

		if (m_DespawnTimer != null)
		{
			m_DespawnTimer.Clear();
		}
	}

	private void Update()
	{
		m_DespawnTimer.Update();

		if (m_DespawnTimer.TimeCheck(true))
		{
			M_Projectile.Despawn(this);
			return;
		}

		Move();
	}

	private void Move()
	{
		if (m_MovingStrategy == null)
			return;

		m_Velocity = m_MovingStrategy.CalculateVelocity(this);

		if (m_Velocity.sqrMagnitude * Time.deltaTime < 0.01f)
			return;

		// Rotate
		Vector2 direction = m_Velocity.normalized;
		float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
		transform.rotation = Quaternion.AngleAxis(angle - 90f, Vector3.forward);

		// Move
		m_Controller.Move(m_Velocity * Time.deltaTime);
	}

	#region Moving Strategy
	public void SetMovingStrategy(IMovingStrategy movingStrategy)
	{
		m_MovingStrategy = movingStrategy;
	}

	public interface IMovingStrategy
	{
		public Vector2 CalculateVelocity(Projectile projectile);
	}

	public class StraightMove : IMovingStrategy
	{
		public Vector2 CalculateVelocity(Projectile projectile)
		{
			Vector2 dir = projectile.transform.rotation * Vector2.up;

			return dir.normalized * projectile.moveSpeed;
		}
	}
	public class GuidedMove : IMovingStrategy
	{
		[SerializeField]
		protected GameObject m_Target;

		public GuidedMove(GameObject target)
		{
			m_Target = target;
		}

		public Vector2 CalculateVelocity(Projectile projectile)
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
	public class CircularMove : IMovingStrategy
	{
		[SerializeField]
		protected GameObject m_Target;
		[SerializeField]
		protected float m_AngularSpeed;
		protected float m_AngularAcceleration;
		protected float m_RandomAngle;
		protected Quaternion m_InitRotation;
		private bool m_IsFirst;

		public CircularMove(GameObject target, float angularAcceleration, float randomAngle = 0f)
		{
			m_Target = target;
			m_AngularSpeed = 0f;
			m_AngularAcceleration = angularAcceleration;
			m_RandomAngle = randomAngle;
			m_IsFirst = true;
		}

		public Vector2 CalculateVelocity(Projectile projectile)
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

			float speed = projectile.moveSpeed;

			if (projectile.moveSpeed * Time.deltaTime > distanceToTarget)
				speed = distanceToTarget;

			return dir.normalized * speed;
		}
	}
	#endregion
}