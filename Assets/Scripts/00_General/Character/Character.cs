using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuffDebuff
{
	public abstract class Character<TStat, TController, TAnimator> : MonoBehaviour where TStat : CharacterStat, new() where TController : Controller2D where TAnimator : CharacterAnimator
	{
		#region 변수
		protected TController m_Controller;
		[SerializeField, ChildComponent("Renderer")]
		protected TAnimator m_Animator;

		#region 이동 관련
		[SerializeField]
		protected bool m_IsSimulating = true;
		[SerializeField, ReadOnly]
		protected Vector2 m_Velocity;
		protected float m_VelocityXSmoothing;
		protected float m_AccelerationTimeAirborne = 0.2f;
		protected float m_AccelerationTimeGrounded = 0.1f;
		#endregion

		#region 스탯 관련
		[Header("===== 스탯 ====="), Space(10)]
		protected TStat m_InitMaxStat;
		protected TStat m_InitStat;
		// 최대 스탯
		[SerializeField]
		protected TStat m_MaxStat;
		// 현재 스탯
		[Space(10)]
		[SerializeField]
		protected TStat m_CurrentStat;
		#endregion

		#region 타이머 관련
		[Header("===== 타이머 ====="), Space(10)]
		[SerializeField, ReadOnly]
		protected UtilClass.Timer m_HealTimer;
		[SerializeField, ReadOnly]
		protected UtilClass.Timer m_AttackTimer;
		#endregion
		#endregion

		#region 프로퍼티
		public TStat maxStat => m_MaxStat;
		public TStat currentStat => m_CurrentStat;

		public TController controller => m_Controller;
		public TAnimator animator => m_Animator;
		#endregion

		public virtual void Initialize()
		{
			this.Safe_GetComponent<TController>(ref m_Controller);
			m_Controller.Initialize();

			if (m_Animator == null)
				throw new System.Exception("Child Component(Animator) is null!");
			m_Animator.Initialize();

			#region Stat
			m_InitMaxStat = m_MaxStat;
			m_InitStat = m_CurrentStat;
			#endregion

			#region Timer
			if (m_HealTimer == null)
				m_HealTimer = new UtilClass.Timer();
			m_HealTimer.interval = m_CurrentStat.HpRegenTime;

			if (m_AttackTimer == null)
				m_AttackTimer = new UtilClass.Timer();
			m_AttackTimer.interval = 1f / m_CurrentStat.AttackSpeed;
			#endregion
		}
		public virtual void Finallize()
		{
			m_MaxStat = m_InitMaxStat;
			m_CurrentStat = m_InitStat;

			if (m_HealTimer != null)
				m_HealTimer.Clear();

			if (m_AttackTimer != null)
				m_AttackTimer.Clear();
		}

		protected virtual void Update()
		{
			HpRegenTimer();
			AttackTimer();

			OnBuffUpdate();
		}
		protected virtual void LateUpdate()
		{
			if (m_IsSimulating == false)
				return;

			Move();
		}

		// Move Func
		protected abstract void CalculateVelocity();
		protected virtual void Move()
		{
			CalculateVelocity();

			m_Controller.Move(m_Velocity * Time.deltaTime);

			if (m_Controller.collisions.above || m_Controller.collisions.below)
			{
				m_Velocity.y = 0;
			}

			m_Animator.Anim_SetVelocity(m_Velocity);
			m_Animator.Anim_SetIsGround(m_Controller.collisions.grounded);
		}

		// Attack Func
		public virtual bool Attack()
		{
			if (CanAttack() == false)
				return false;

			//m_Animator.Anim_Attack();

			return true;
		}
		protected virtual bool CanAttack()
		{
			return m_AttackTimer.TimeCheck();
		}

		// Timer Func
		protected void HpRegenTimer()
		{
			if (Mathf.Abs(m_CurrentStat.HpRegen) <= float.Epsilon)
				return;
			if (m_CurrentStat.Hp >= m_MaxStat.Hp)
				return;

			m_HealTimer.Update();

			if (m_HealTimer.TimeCheck(true))
			{
				float hp = m_CurrentStat.Hp + (m_CurrentStat.HpRegen * m_CurrentStat.HealScale) * m_CurrentStat.AntiHealScale;

				m_CurrentStat.Hp = Mathf.Clamp(hp, 0f, m_MaxStat.Hp);
			}
		}
		protected void AttackTimer()
		{
			m_AttackTimer.Update();
		}

		// Buff Event
		protected virtual void OnBuffUpdate()
		{
			//foreach (AbstractBuff item in m_BuffList.Values)
			//{
			//	(item as IOnBuffUpdate)?.OnBuffUpdate(this);
			//}
		}
		protected virtual void OnBuffAttackStart()
		{
			//foreach (AbstractBuff item in m_BuffList.Values)
			//{
			//	(item as IOnBuffAttackStart)?.OnBuffAttackStart(this);
			//}
		}
		protected virtual void OnBuffAttack()
		{
			//foreach (AbstractBuff item in m_BuffList.Values)
			//{
			//	(item as IOnBuffAttack)?.OnBuffAttack(this);
			//}
		}
		protected virtual void OnBuffAttackEnd()
		{
			//foreach (AbstractBuff item in m_BuffList.Values)
			//{
			//	(item as IOnBuffAttackEnd)?.OnBuffAttackEnd(this);
			//}
		}

		// Anim Event
		public virtual void AnimEvent_AttackStart()
		{
			OnBuffAttackStart();
		}
		public virtual void AnimEvent_Attacking()
		{
			OnBuffAttack();
		}
		public virtual void AnimEvent_AttackEnd()
		{
			OnBuffAttackEnd();

			m_AttackTimer.Clear();
		}
	}
}