using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuffDebuff
{
	[RequireComponent(typeof(ProjectileController))]
	public sealed class Projectile : ObjectPoolItemBase
	{
		#region 변수
		private ProjectileController m_Controller;

		[SerializeField, ReadOnly]
		private BoxCollisionChecker2D m_CollisionChecker2D;

		private ProjectileMove m_Mover;
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
			this.NullCheckGetComponent<ProjectileController>(ref m_Controller);
			m_Controller.Initialize();

			this.NullCheckGetComponent<BoxCollisionChecker2D>(ref m_CollisionChecker2D);
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
			if (m_Mover == null)
				return;

			m_Velocity = m_Mover.CalculateVelocity(this);

			if (m_Velocity.sqrMagnitude * Time.deltaTime < 0.01f)
				return;

			// Rotate
			Vector2 direction = m_Velocity.normalized;
			float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
			transform.rotation = Quaternion.AngleAxis(angle - 90f, Vector3.forward);

			// Move
			m_Controller.Move(m_Velocity * Time.deltaTime);
		}

		public void SetMovingStrategy(ProjectileMove mover)
		{
			m_Mover = mover;
		}
	}
}