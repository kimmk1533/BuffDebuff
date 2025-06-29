using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Timer = UtilClass.Timer;
using TimerController = UtilClass.TimerController;

namespace BuffDebuff
{
	public abstract class Character<TSelf, TStat, TController, TAnimator> : ObjectPoolItem<TSelf> where TSelf : ObjectPoolItem<TSelf> where TStat : CharacterStat where TController : Controller2D where TAnimator : CharacterAnimator
	{
		#region 기본 템플릿
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
		// 초기 스탯
		protected TStat m_InitStat;
		// 현재 스탯
		[FoldoutGroup("스탯")]
		[SerializeField, InlineProperty, HideLabel]
		protected TStat m_Stat;
		#endregion

		#region 공격 관련
		// 공격 패턴 갯수
		[FoldoutGroup("스탯")]
		[Title("공격 패턴")]
		[SerializeField]
		protected int m_AttackPatternCount;
		// 공격 패턴 인덱스
		protected int m_AttackPatternIndex;
		#endregion

		#region 타이머 관련
		[SerializeField, ReadOnly, InlineProperty, HideLabel]
		protected TimerController m_TimerController = null;
		[SerializeField, ReadOnly]
		[FoldoutGroup("타이머"), LabelText("공격 타이머")]
		protected UtilClass.Timer m_AttackTimer;
		#endregion
		#endregion

		#region 프로퍼티
		public TStat initStat { get => m_InitStat; set => m_InitStat = value; }
		public TStat currentStat => m_Stat;

		public TController controller => m_Controller;
		public TAnimator animator => m_Animator;
		#endregion

		#region 이벤트

		#region 이벤트 함수
		// Timer Event
		protected virtual void OnHealTimer()
		{
			StatValue<float> HpStat = m_Stat.Hp;

			HpStat.current = m_Stat.Hp.current + (m_Stat.HpRegen * m_Stat.HealScale) * m_Stat.AntiHealScale;
			HpStat.current = Mathf.Clamp(HpStat.current, 0f, HpStat.max);

			m_Stat.Hp = HpStat;
		}
		protected virtual bool CheckHpRegenCondition()
		{
			// 체력 재생이 있는지 확인
			if (Mathf.Abs(m_Stat.HpRegen) <= float.Epsilon)
				return false;
			// 현재 체력이 최대 체력보다 높거나 같은지 확인
			if (m_Stat.Hp.current >= m_Stat.Hp.max)
				return false;

			return true;
		}
		protected virtual void OnAttackTimer()
		{

		}
		protected virtual bool CheckAttackCondition()
		{
			return true;
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
		#endregion
		#endregion

		#region 매니저
		#endregion

		#region 초기화 & 마무리화 함수
		/// <summary>
		/// 초기화 함수
		/// </summary>
		public override void InitializePoolItem()
		{
			base.InitializePoolItem();

			if (m_IsSimulating == false)
				m_IsSimulating = true;

			this.NullCheckGetComponent<TController>(ref m_Controller);
			m_Controller.Initialize();

			if (m_Animator == null)
				throw new System.Exception("Child Component(Animator) is null!");
			m_Animator.Initialize();

			m_Stat = m_InitStat.Clone() as TStat;

			#region Timer
			if (m_TimerController == null)
				m_TimerController = new UtilClass.TimerController();

			Timer healTimer = new Timer();
			healTimer.interval = m_Stat.HpRegenTime;
			healTimer.onTime += OnHealTimer;
			m_TimerController.AddTimer("Heal", CheckHpRegenCondition, healTimer);

			Timer attackTimer = new Timer();
			attackTimer.interval = 1f / m_Stat.AttackSpeed;
			attackTimer.onTime += OnAttackTimer;
			m_TimerController.AddTimer("Attack", CheckAttackCondition, attackTimer);

			if (m_AttackTimer != null)
				m_AttackTimer.Clear();
			else
				m_AttackTimer = new UtilClass.Timer();
			m_AttackTimer.interval = 1f / m_Stat.AttackSpeed;
			#endregion
		}
		/// <summary>
		/// 마무리화 함수
		/// </summary>
		public override void FinallizePoolItem()
		{
			base.FinallizePoolItem();

			m_TimerController.Clear();

			if (m_AttackTimer != null)
				m_AttackTimer.Clear();

			m_Controller.Finallize();
		}
		#endregion

		#region 유니티 콜백 함수
		protected virtual void Update()
		{
			m_TimerController.Update();
			m_AttackTimer.Update();

			OnBuffUpdate();
		}
		protected virtual void LateUpdate()
		{
			if (m_IsSimulating == false)
				return;

			Move();
		}
		#endregion
		#endregion

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
		protected virtual bool CanAttack()
		{
			return m_AttackTimer.TimeCheck();
		}
		public virtual void Attack()
		{
			m_AttackTimer.Clear();
			m_AttackPatternIndex = Random.Range(0, m_AttackPatternCount);
			m_Animator.Anim_Attack(m_AttackPatternIndex);
		}
	}
}