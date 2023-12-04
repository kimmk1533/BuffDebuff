using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Character<TStat, TController, TAnimator> : MonoBehaviour where TStat : CharacterStat, new() where TController : Controller2D where TAnimator : CharacterAnimator
{
	protected TController m_Controller;
	[SerializeField, ChildComponent("Renderer")]
	protected TAnimator m_Animator;

	protected Dictionary<int, AbstractBuff> m_BuffList;

	[SerializeField]
	protected bool m_IsSimulating = true;
	[SerializeField, ReadOnly]
	protected Vector2 m_Velocity;
	protected float m_VelocityXSmoothing;
	protected float m_AccelerationTimeAirborne = 0.2f;
	protected float m_AccelerationTimeGrounded = 0.1f;

	#region Stat
	// 최대 스탯
	[SerializeField]
	protected TStat m_MaxStat;
	// 현재 스탯
	[Space(10)]
	[SerializeField]
	protected TStat m_CurrentStat;
	#endregion

	#region Timer
	[Header("===== 타이머 ====="), Space(10)]
	[SerializeField]
	protected UtilClass.Timer m_HealTimer;
	[SerializeField]
	protected UtilClass.Timer m_AttackTimer;
	#endregion

	public TStat maxStat => m_MaxStat;
	public TStat currentStat => m_CurrentStat;

	private BuffManager M_Buff => BuffManager.Instance;

	public virtual void Initialize()
	{
		m_Controller = GetComponent<TController>();
		m_Controller.Initialize();

		if (m_Animator == null)
			throw new Exception("Child Component(Animator) is null!");
		m_Animator.Initialize();

		// BuffList Init
		m_BuffList = new Dictionary<int, AbstractBuff>();
	}

	protected virtual void Update()
	{
		HpRegenTimer();
		AttackTimer();

		OnBuffUpdate();
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

	// Buff Func
	public bool AddBuff(string name)
	{
		if (name == null || name == string.Empty)
			return false;

		BuffData buffData = M_Buff.GetBuffData(name);

		return this.AddBuff(buffData);
	}
	public bool AddBuff(BuffData buffData)
	{
		if (buffData == null)
			return false;

		if (m_BuffList.TryGetValue(buffData.code, out AbstractBuff buff) &&
			buff != null)
		{
			if (buff.count < buffData.maxStack)
			{
				++buff.count;
				(buff as IOnBuffAdded)?.OnBuffAdded(this);
			}
			else
			{
				Debug.Log("Buff Count is Max. title =" + buffData.title + ", maxStack = " + buffData.maxStack.ToString());

				return false;
			}

			return true;
		}

		buff = M_Buff.CreateBuff(buffData);

		m_BuffList.Add(buffData.code, buff);

		(buff as IOnBuffAdded)?.OnBuffAdded(this);

		return true;
	}
	public bool RemoveBuff(string name)
	{
		if (name == null || name == string.Empty)
			return false;

		BuffData buff = M_Buff.GetBuffData(name);

		return this.RemoveBuff(buff);
	}
	public bool RemoveBuff(BuffData buffData)
	{
		if (buffData == null)
			return false;

		if (m_BuffList.TryGetValue(buffData.code, out AbstractBuff buff) &&
			buff != null)
		{
			if (buff.count > 0)
			{
				--buff.count;
			}
			else
			{
				m_BuffList.Remove(buffData.code);
			}

			(buff as IOnBuffRemoved)?.OnBuffRemoved(this);

			return true;
		}

		Debug.Log("버프 없는데 제거");

		return false;
	}

	// BuffEvent
	protected virtual void OnBuffUpdate()
	{
		foreach (AbstractBuff item in m_BuffList.Values)
		{
			(item as IOnBuffUpdate)?.OnBuffUpdate(this);
		}
	}
	protected virtual void OnBuffAttackStart()
	{
		foreach (AbstractBuff item in m_BuffList.Values)
		{
			(item as IOnBuffAttackStart)?.OnBuffAttackStart(this);
		}
	}
	protected virtual void OnBuffAttack()
	{
		foreach (AbstractBuff item in m_BuffList.Values)
		{
			(item as IOnBuffAttack)?.OnBuffAttack(this);
		}
	}
	protected virtual void OnBuffAttackEnd()
	{
		foreach (AbstractBuff item in m_BuffList.Values)
		{
			(item as IOnBuffAttackEnd)?.OnBuffAttackEnd(this);
		}
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

	// Attack Func
	public virtual void Attack()
	{
		if (CanAttack() == false)
			return;

		m_Animator.Anim_Attack();
	}
	protected virtual bool CanAttack()
	{
		return m_AttackTimer.TimeCheck();
	}

	// AnimEvent
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

		m_AttackTimer.TimeCheck();
	}

}