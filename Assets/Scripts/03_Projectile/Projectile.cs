using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ProjectileController))]
public sealed class Projectile : MonoBehaviour
{
	private ProjectileController m_Controller;
	private IMovingStrategy m_MovingStrategy;
	private HashSet<ICollisionStrategy> m_CollisionStrategy;

	[SerializeField]
	private float m_MoveSpeed;
	[SerializeField]
	private float m_LifeTime;

	private Vector2 m_Velocity;

	public float moveSpeed => m_MoveSpeed;

	private ProjectileManager M_Projectile => ProjectileManager.Instance;

	private void OnEnable()
	{
		Invoke("DeSpawn", m_LifeTime);
	}
	private void OnDisable()
	{
		CancelInvoke("DeSpawn");
	}
	private void Update()
	{
		m_Velocity = m_MovingStrategy.CalculateVelocity(this);

		#region Rotate
		Vector2 direction = m_Velocity.normalized;

		float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
		Quaternion angleAxis = Quaternion.AngleAxis(angle - 90f, Vector3.forward);

		transform.rotation = angleAxis;
		#endregion

		#region Move
		m_Controller.Move(m_Velocity * Time.deltaTime);
		#endregion
	}
	public void OnTriggerEnter2D(Collider2D collider)
	{
		foreach (var item in m_CollisionStrategy)
		{
			item.OnTriggerEnter2D(collider);
		}
	}
	public void OnTriggerStay2D(Collider2D collider)
	{
		foreach (var item in m_CollisionStrategy)
		{
			item.OnTriggerStay2D(collider);
		}
	}
	public void OnTriggerExit2D(Collider2D collider)
	{
		foreach (var item in m_CollisionStrategy)
		{
			item.OnTriggerExit2D(collider);
		}
	}

	public void Initialize(float moveSpeed, float lifeTime)
	{
		m_MoveSpeed = moveSpeed;
		m_LifeTime = lifeTime;

		if (m_Controller == null)
		{
			m_Controller = GetComponent<ProjectileController>();
			m_Controller.Initialize(this);
		}

		m_CollisionStrategy = new HashSet<ICollisionStrategy>();
	}
	public void Initialize(float moveSpeed, float lifeTime, IMovingStrategy movingStrategy)
	{
		Initialize(moveSpeed, lifeTime);

		m_MovingStrategy = movingStrategy;
	}
	public void SetMovingStrategy(IMovingStrategy movingStrategy)
	{
		m_MovingStrategy = movingStrategy;
	}
	public void AddCollisionStrategy(ICollisionStrategy collisionStrategy)
	{
		m_CollisionStrategy.Add(collisionStrategy);
		m_Controller.collisionMask |= collisionStrategy.GetLayerMask();
	}

	private void DeSpawn()
	{
		M_Projectile.DeSpawn(this);
	}

	#region Moving Strategy
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

		public GuidedMove(GameObject target = null)
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
	}
	public class CircularMove : IMovingStrategy
	{
		[SerializeField]
		protected GameObject m_Target;
		[SerializeField]
		protected float m_AngularSpeed;
		protected float m_AngularMaxSpeed;
		protected float m_AngularAcceleration;

		public CircularMove(GameObject target = null, float angularSpeed = 5.0f)
		{
			m_Target = target;
			m_AngularSpeed = angularSpeed;
			m_AngularMaxSpeed = angularSpeed * 2f;
			m_AngularAcceleration = angularSpeed * 0.5f;
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

			if (m_AngularSpeed < m_AngularMaxSpeed)
				m_AngularSpeed += m_AngularAcceleration * Time.deltaTime;
			else
				m_AngularSpeed = m_AngularMaxSpeed;

			// t 수정 필요
			Vector2 dir = Quaternion.Slerp(projectile.transform.rotation, angleAxis, m_AngularSpeed * Time.deltaTime) * Vector2.up;

			float speed = projectile.moveSpeed;

			if (projectile.moveSpeed * Time.deltaTime > distanceToTarget)
				speed = distanceToTarget;

			return dir.normalized * speed;
		}
	}
	#endregion

	#region Collision Strategy
	public interface ICollisionStrategy
	{
		public LayerMask GetLayerMask();
		public void OnTriggerEnter2D(Collider2D collider);
		public void OnTriggerStay2D(Collider2D collider);
		public void OnTriggerExit2D(Collider2D collider);
	}

	public class PlayerCollision : ICollisionStrategy
	{
		public LayerMask GetLayerMask()
		{
			return LayerMask.GetMask("Player");
		}
		public void OnTriggerEnter2D(Collider2D collider)
		{
			int layer = LayerMask.NameToLayer("Player");
			if (collider.gameObject.layer == layer)
			{
				Debug.Log("Projectile: Player 충돌 Enter!");
			}
		}
		public void OnTriggerStay2D(Collider2D collider)
		{

		}
		public void OnTriggerExit2D(Collider2D collider)
		{
			int layer = LayerMask.NameToLayer("Player");
			if (collider.gameObject.layer == layer)
			{
				Debug.Log("Projectile: Player 충돌 Exit!");
			}
		}
	}
	public class ObstacleCollision : ICollisionStrategy
	{
		public LayerMask GetLayerMask()
		{
			return LayerMask.GetMask("Obstacle");
		}
		public void OnTriggerEnter2D(Collider2D collider)
		{
			int layer = LayerMask.NameToLayer("Obstacle");
			if (collider.gameObject.layer == layer)
			{
				Debug.Log("Projectile: Obstacle 충돌 Enter!");
			}
		}
		public void OnTriggerStay2D(Collider2D collider)
		{

		}
		public void OnTriggerExit2D(Collider2D collider)
		{
			int layer = LayerMask.NameToLayer("Obstacle");
			if (collider.gameObject.layer == layer)
			{
				Debug.Log("Projectile: Obstacle 충돌 Exit!");
			}
		}
	}
	public class EnemyCollision : ICollisionStrategy
	{
		public LayerMask GetLayerMask()
		{
			return LayerMask.GetMask("Enemy");
		}
		public void OnTriggerEnter2D(Collider2D collider)
		{
			int layer = LayerMask.NameToLayer("Enemy");
			if (collider.gameObject.layer == layer)
			{
				Debug.Log("Projectile: Enemy 충돌 Enter!");
			}
		}
		public void OnTriggerStay2D(Collider2D collider)
		{

		}
		public void OnTriggerExit2D(Collider2D collider)
		{
			int layer = LayerMask.NameToLayer("Enemy");
			if (collider.gameObject.layer == layer)
			{
				Debug.Log("Projectile: Enemy 충돌 Exit!");
			}
		}
	}
	#endregion
}