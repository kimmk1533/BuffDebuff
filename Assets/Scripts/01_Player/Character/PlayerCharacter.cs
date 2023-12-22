using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

[RequireComponent(typeof(PlayerController2D))]
public sealed class PlayerCharacter : Character<PlayerCharacterStat, PlayerController2D, PlayerAnimator>
{
	#region 변수
	[Header("===== 플레이어 전용 변수 ====="), Space(10)]
	private Vector2 m_DirectionalInput;

	[SerializeField, ReadOnly]
	private bool m_IsAttacking;
	[SerializeField, ReadOnly]
	private bool m_CanComboAttack;

	[SerializeField, ReadOnly]
	private UtilClass.Timer m_DashTimer;
	#endregion

	#region 프로퍼티
	public float attackSpeed
	{
		get
		{
			return m_CurrentStat.AttackSpeed;
		}
		set
		{
			m_CurrentStat.AttackSpeed = value;
			m_Animator.Anim_SetAttackSpeed(value);
		}
	}
	#endregion

	public override void Initialize()
	{
		base.Initialize();

		m_IsAttacking = false;
		m_CanComboAttack = false;

		// 스탯 초기화
		#region Stat
		attackSpeed = 2.0f;
		m_CurrentStat.Xp = 0.0f;
		m_CurrentStat.Level = 0;
		#endregion

		// 타이머 초기화
		if (m_DashTimer != null)
			m_DashTimer.Clear();
		else
			m_DashTimer = new UtilClass.Timer();
		m_DashTimer.interval = m_CurrentStat.DashRechargeTime;
	}
	public override void Finallize()
	{
		base.Finallize();

		m_DashTimer.Clear();
	}

	protected override void Update()
	{
		base.Update();

		DashTimer();
	}

	private void SetDirectionalInput(Vector2 input)
	{
		m_DirectionalInput = input;
		m_Animator.Anim_SetDirectionalInput(input);
	}
	protected override void CalculateVelocity()
	{
		float targetVelocityX = m_DirectionalInput.x * m_CurrentStat.MoveSpeed;
		Vector3 scale = transform.localScale;
		if (m_Velocity.x > 0)
			scale.x = Mathf.Abs(scale.x);
		else if (m_Velocity.x < 0)
			scale.x = -Mathf.Abs(scale.x);
		transform.localScale = scale;


		m_Velocity.x = Mathf.SmoothDamp(m_Velocity.x, targetVelocityX, ref m_VelocityXSmoothing, (m_Controller.collisions.grounded) ? m_AccelerationTimeGrounded : m_AccelerationTimeAirborne);
		m_Velocity.y += m_Controller.gravity * Time.deltaTime;
	}
	protected override void Move()
	{
		Vector2 directionalInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
		SetDirectionalInput(directionalInput);

		CalculateVelocity();

		m_Controller.Move(m_Velocity * Time.deltaTime, m_DirectionalInput);
		if (m_Controller.collisions.above || m_Controller.collisions.below)
		{
			m_Velocity.y = 0;
		}

		m_Animator.Anim_SetVelocity(m_Velocity);
		m_Animator.Anim_SetIsGround(m_Controller.collisions.grounded);
	}

	// Jump Func
	public void JumpInputDown()
	{
		if (m_IsSimulating == false)
			return;

		if ((m_Controller.collisions.below && m_DirectionalInput.y != -1) == false)
			return;

		m_Animator.Anim_Jump();
		OnBuffJump();

		m_Velocity.y = m_Controller.maxJumpVelocity;
	}
	public void JumpInputUp()
	{
		if (m_IsSimulating == false)
			return;

		if (m_Velocity.y > m_Controller.minJumpVelocity)
			m_Velocity.y = m_Controller.minJumpVelocity;
	}

	// Dash Func
	public bool CanDash()
	{
		return m_CurrentStat.DashCount > 0;
	}
	public void Dash()
	{
		if (CanDash() == false)
			return;

		--m_CurrentStat.DashCount;

		OnBuffDash();

		Vector2 dir = UtilClass.GetMouseWorldPosition() - transform.position;

		if (m_BuffList.ContainsSubKey("전방향 대쉬") == true)
		{
			// 마우스 대쉬
			m_Velocity = dir.normalized * m_CurrentStat.DashSpeed;
		}
		else
		{
			// 좌우 대쉬
			m_Velocity.x = Mathf.Sign(dir.x) * m_CurrentStat.DashSpeed;
		}
	}

	// Attack Func
	public override bool Attack()
	{
		if (base.Attack() == false)
			return false;

		m_IsAttacking = true;

		return true;
	}
	protected override bool CanAttack()
	{
		return base.CanAttack() && (m_IsAttacking ? m_CanComboAttack : true);
	}

	// Timer Func
	private void DashTimer()
	{
		if (m_CurrentStat.DashCount >= m_MaxStat.DashCount)
		{
			m_DashTimer.Clear();
			return;
		}

		m_DashTimer.Update();

		if (m_DashTimer.TimeCheck(true))
		{
			++m_CurrentStat.DashCount;
		}
	}

	// Buff Event
	public void OnBuffJump()
	{
		foreach (var item in m_BuffList.Values)
		{
			(item as IOnBuffJump)?.OnBuffJump(this);
		}
	}
	private void OnBuffDash()
	{
		foreach (var item in m_BuffList.Values)
		{
			(item as IOnBuffDash)?.OnBuffDash(this);
		}
	}

	// Anim Event
	public override void AnimEvent_AttackStart()
	{
		base.AnimEvent_AttackStart();

		m_IsSimulating = false;
		m_Velocity.x = System.MathF.Sign(m_Velocity.x) * float.Epsilon;
	}
	public override void AnimEvent_Attacking()
	{
		base.AnimEvent_Attacking();
	}
	public override void AnimEvent_AttackEnd()
	{
		base.AnimEvent_AttackEnd();

		m_IsSimulating = true;
		m_IsAttacking = false;
	}
	public void AnimEvent_AirAttackStart()
	{
		m_IsSimulating = false;

		m_Velocity = Vector2.zero;
	}
	public void AnimEvent_AirAttackEnd()
	{
		m_IsSimulating = true;
		m_IsAttacking = false;
	}
	public void AnimEvent_StartCombo()
	{
		m_CanComboAttack = true;
	}
	public void AnimEvent_EndCombo()
	{
		m_CanComboAttack = false;
	}
}