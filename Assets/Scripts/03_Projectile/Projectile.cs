using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BuffDebuff
{
	[RequireComponent(typeof(ProjectileController))]
	public class Projectile : ObjectPoolItem<Projectile>
	{
		#region 변수
		private ProjectileController m_Controller;

		[SerializeField, ReadOnly]
		private CollisionChecker2D m_CollisionChecker2D;

		[SerializeField, ChildComponent("Renderer")]
		private ProjectileAnimator m_Animator;

		private ProjectileMover m_Mover;
		[SerializeField, ReadOnly]
		private float m_MovementSpeed;

		[SerializeField, ReadOnly]
		private Vector2 m_Velocity;

		[SerializeField, ReadOnly]
		private UtilClass.Timer m_DespawnTimer;
		#endregion

		#region 프로퍼티
		public float movementSpeed
		{
			get => m_MovementSpeed;
			set => m_MovementSpeed = value;
		}
		public float lifeTime
		{
			get => m_DespawnTimer.interval + m_Animator.deathAnimationDelay;
			set => m_DespawnTimer.interval = value - m_Animator.deathAnimationDelay;
		}
		public ProjectileMover mover
		{
			get => m_Mover;
			set => m_Mover = value;
		}

		#region 인덱서
		public CollisionChecker2D.Trigger this[int layer] => m_CollisionChecker2D[layer];
		public CollisionChecker2D.Trigger this[string layer] => m_CollisionChecker2D[layer];
		#endregion
		#endregion

		#region 매니저
		private static ProjectileManager M_Projectile => ProjectileManager.Instance;
		#endregion

		public override void InitializePoolItem()
		{
			base.InitializePoolItem();

			#region SAFE_INIT
			this.NullCheckGetComponent<ProjectileController>(ref m_Controller);
			m_Controller.Initialize();

			this.NullCheckGetComponent<CollisionChecker2D>(ref m_CollisionChecker2D);
			m_CollisionChecker2D.Initialize();

			m_Animator.Initialize();

			if (m_DespawnTimer == null)
				m_DespawnTimer = new UtilClass.Timer();
			#endregion
		}
		public override void FinallizePoolItem()
		{
			base.FinallizePoolItem();

			m_Animator.Finallize();

			m_CollisionChecker2D.Finallize();

			if (m_DespawnTimer != null)
				m_DespawnTimer.Clear();
		}

		private void Update()
		{
			m_DespawnTimer.Update();

			if (m_DespawnTimer.TimeCheck() == true)
			{
				Death();
				return;
			}

			Move();
		}

		private void Move()
		{
			if (m_Mover == null)
				return;

			m_Velocity = m_Mover.CalculateVelocity(this);

			// Rotate
			Vector2 direction = m_Velocity.normalized;
			float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
			if (Mathf.Abs(angle) > 90f)
			{
				m_Animator.FlipY(true);
			}
			transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

			// Move
			m_Controller.Move(m_Velocity * Time.deltaTime);
		}
		public void Death()
		{
			m_Animator.Anim_Death();
		}
	}
}