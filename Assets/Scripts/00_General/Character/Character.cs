using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BuffDebuff
{
	public abstract class Character<TStat, TController, TAnimator> : SerializedMonoBehaviour where TStat : CharacterStat where TController : Controller2D where TAnimator : CharacterAnimator, IAnim_Attack
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
		[SerializeField, ReadOnly]
		[FoldoutGroup("타이머"), LabelText("힐 타이머")]
		protected UtilClass.Timer m_HealTimer;
		[SerializeField, ReadOnly]
		[FoldoutGroup("타이머"), LabelText("공격 타이머")]
		protected UtilClass.Timer m_AttackTimer;
		#endregion
		#endregion

		#region 프로퍼티
		public TStat currentStat => m_Stat;

		public TController controller => m_Controller;
		public TAnimator animator => m_Animator;
		#endregion

		public virtual void Initialize()
		{
			this.NullCheckGetComponent<TController>(ref m_Controller);
			m_Controller.Initialize();

			if (m_Animator == null)
				throw new System.Exception("Child Component(Animator) is null!");
			m_Animator.Initialize();

			m_Stat = m_InitStat.Clone() as TStat;

			#region Timer
			if (m_HealTimer == null)
				m_HealTimer = new UtilClass.Timer();
			m_HealTimer.interval = m_Stat.HpRegenTime;

			if (m_AttackTimer == null)
				m_AttackTimer = new UtilClass.Timer();
			m_AttackTimer.interval = 1f / m_Stat.AttackSpeed;
			#endregion
		}
		public virtual void Finallize()
		{
			if (m_HealTimer != null)
				m_HealTimer.Clear();

			if (m_AttackTimer != null)
				m_AttackTimer.Clear();

			m_Controller.Finallize();
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

		public void SetInitStat(TStat initStat)
		{
			m_InitStat = initStat;
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

		// Timer Func
		protected void HpRegenTimer()
		{
			if (Mathf.Abs(m_Stat.HpRegen) <= float.Epsilon)
				return;
			if (m_Stat.Hp.current >= m_Stat.Hp.max)
				return;

			m_HealTimer.Update();

			if (m_HealTimer.TimeCheck(true))
			{
				StatValue<float> HpStat = m_Stat.Hp;

				HpStat.current = m_Stat.Hp.current + (m_Stat.HpRegen * m_Stat.HealScale) * m_Stat.AntiHealScale;
				HpStat.current = Mathf.Clamp(HpStat.current, 0f, HpStat.max);

				m_Stat.Hp = HpStat;
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
	public abstract class PoolCharacter<TStat, TController, TAnimator> : ObjectPoolItemBase where TStat : CharacterStat where TController : Controller2D where TAnimator : CharacterAnimator, IAnim_Attack
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
		[SerializeField, ReadOnly]
		[FoldoutGroup("타이머"), LabelText("힐 타이머")]
		protected UtilClass.Timer m_HealTimer;
		[SerializeField, ReadOnly]
		[FoldoutGroup("타이머"), LabelText("공격 타이머")]
		protected UtilClass.Timer m_AttackTimer;
		#endregion
		#endregion

		#region 프로퍼티
		public TStat currentStat => m_Stat;

		public TController controller => m_Controller;
		public TAnimator animator => m_Animator;
		#endregion

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
			if (m_HealTimer != null)
				m_HealTimer.Clear();
			else
				m_HealTimer = new UtilClass.Timer();
			m_HealTimer.interval = m_Stat.HpRegenTime;

			if (m_AttackTimer != null)
				m_AttackTimer.Clear();
			else
				m_AttackTimer = new UtilClass.Timer();
			m_AttackTimer.interval = 1f / m_Stat.AttackSpeed;
			#endregion
		}
		public override void FinallizePoolItem()
		{
			base.FinallizePoolItem();

			if (m_HealTimer != null)
				m_HealTimer.Clear();

			if (m_AttackTimer != null)
				m_AttackTimer.Clear();

			m_Controller.Finallize();
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

		public void SetInitStat(TStat initStat)
		{
			m_InitStat = initStat;
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

		// Timer Func
		protected void HpRegenTimer()
		{
			if (Mathf.Abs(m_Stat.HpRegen) <= float.Epsilon)
				return;
			if (m_Stat.Hp.current >= m_Stat.Hp.max)
				return;

			m_HealTimer.Update();

			if (m_HealTimer.TimeCheck(true))
			{
				float hp = m_Stat.Hp.current + (m_Stat.HpRegen * m_Stat.HealScale) * m_Stat.AntiHealScale;

				StatValue<float> newHp = m_Stat.Hp;
				newHp.current = Mathf.Clamp(hp, 0f, newHp.max);
				m_Stat.Hp = newHp;
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