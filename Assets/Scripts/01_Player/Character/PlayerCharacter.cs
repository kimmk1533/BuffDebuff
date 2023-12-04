using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

[RequireComponent(typeof(PlayerController2D))]
public sealed class PlayerCharacter : Character<PlayerCharacterStat, PlayerController2D, PlayerAnimator>
{
	private Vector2 m_DirectionalInput;

	[SerializeField]
	private UtilClass.Timer m_DashTimer;

	private BuffManager M_Buff => BuffManager.Instance;

	protected override void Update()
	{
		base.Update();

		DashTimer();

		if (m_IsSimulating == false)
			return;

		if (Input.GetMouseButtonDown(1))
		{
			Dash();
		}

		Move();

		if (Input.GetKeyDown(KeyCode.Space))
		{
			JumpInputDown();
		}
		if (Input.GetKeyUp(KeyCode.Space))
		{
			JumpInputUp();
		}
	}

	public override void Initialize()
	{
		base.Initialize();

		// Stat Init
		m_CurrentStat.Xp = 0.0f;
		m_CurrentStat.Level = 0;

		// Timer Init
		m_HealTimer = new UtilClass.Timer(m_CurrentStat.HpRegenTime);
		m_AttackTimer = new UtilClass.Timer(1f / m_CurrentStat.AttackSpeed, true);
		m_DashTimer = new UtilClass.Timer(m_CurrentStat.DashRechargeTime);
	}

	private void SetDirectionalInput(Vector2 input)
	{
		m_DirectionalInput = input;
		m_Animator.Anim_SetDirectionalInput(input);
	}
	protected override void CalculateVelocity()
	{
		float targetVelocityX = m_DirectionalInput.x * m_CurrentStat.MoveSpeed;

		m_Velocity.x = Mathf.SmoothDamp(m_Velocity.x, targetVelocityX, ref m_VelocityXSmoothing, (m_Controller.collisions.grounded) ? m_AccelerationTimeGrounded : m_AccelerationTimeAirborne);

		Vector3 scale = transform.localScale;
		if (m_Velocity.x > 0)
			scale.x = Mathf.Abs(scale.x);
		else if (m_Velocity.x < 0)
			scale.x = -Mathf.Abs(scale.x);
		transform.localScale = scale;

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

	public override void AnimEvent_AttackStart()
	{
		base.AnimEvent_AttackStart();

		m_IsSimulating = false;
	}
	public override void AnimEvent_Attacking()
	{
		base.AnimEvent_Attacking();
	}
	public override void AnimEvent_AttackEnd()
	{
		base.AnimEvent_AttackEnd();

		m_IsSimulating = true;
		m_Velocity.x = 0.0f;
	}
	public void AnimEvent_AirAttackStart()
	{
		m_IsSimulating = false;

		m_Velocity = Vector2.zero;
	}
	public void AnimEvent_AirAttackEnd()
	{
		m_IsSimulating = true;
	}

	private void JumpInputDown()
	{
		if ((m_Controller.collisions.below && m_DirectionalInput.y != -1) == false)
			return;

		m_Animator.Anim_Jump();
		Jump();

		m_Velocity.y = m_Controller.maxJumpVelocity;
	}
	private void JumpInputUp()
	{
		if (m_Velocity.y > m_Controller.minJumpVelocity)
			m_Velocity.y = m_Controller.minJumpVelocity;
	}
	public void Jump()
	{
		foreach (var item in m_BuffList.Values)
		{
			(item as IOnBuffJump)?.OnBuffJump(this);
		}
	}
	public bool CanDash()
	{
		return m_CurrentStat.DashCount > 0;
	}
	public void Dash()
	{
		if (CanDash() == false)
			return;

		--m_CurrentStat.DashCount;

		foreach (var item in m_BuffList.Values)
		{
			(item as IOnBuffDash)?.OnBuffDash(this);
		}

		Vector2 dir = UtilClass.GetMouseWorldPosition() - transform.position;

		//if (m_BuffList.ContainsKey(M_Buff.GetBuffData("마우스 대쉬").code) == true)
		//{
		//	// 마우스 대쉬
		//	m_Velocity = dir.normalized * m_CurrentStat.DashSpeed;
		//}
		//else
		{
			// 좌우 대쉬
			m_Velocity.x = Mathf.Sign(dir.x) * m_CurrentStat.DashSpeed;
		}
	}

	private void OnValidate()
	{
		M_Buff.OnValidate();
	}
}