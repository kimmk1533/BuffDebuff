using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : CharacterAnimator
{
	#region Enum
	private enum E_AnimState
	{
		None = 0,

		Idle = 0,
		Run = 1,
		Attack = 2,

		Max
	}
	#endregion

	private Player m_Player;

	private int direction
	{
		set
		{
			if (value == 0)
				return;

			Vector3 scale = transform.localScale;

			if (value > 0)
				scale.x = Mathf.Abs(scale.x);
			else if (value < 0)
				scale.x = -Mathf.Abs(scale.x);

			transform.localScale = scale;
		}
	}

	public override void Initialize()
	{
		base.Initialize();

		m_Player = GetComponentInParent<Player>();
	}

	public void Anim_SetDirectionalInput(Vector2 input)
	{
		if (input.x == 0)
			m_Animator.SetInteger("AnimState", (int)E_AnimState.Idle);
		else
			m_Animator.SetInteger("AnimState", (int)E_AnimState.Run);
	}
	public void Anim_SetVelocity(float x, float y)
	{
		direction = System.MathF.Sign(x);

		m_Animator.SetFloat("VelocityX", x);
		m_Animator.SetFloat("VelocityY", y);
	}
	public void Anim_SetVelocity(Vector2 speed)
	{
		Anim_SetVelocity(speed.x, speed.y);
	}

	private void AnimEvent_Attack1_CreateProjectile()
	{
		m_Player.Attack(0);
	}
	private void AnimEvent_Attack2_CreateProjectile()
	{
		m_Player.Attack(1);
	}
	private void AnimEvent_Attack3_CreateProjectile()
	{
		m_Player.Attack(2);
	}
	private void AnimEvent_Attack_Start()
	{
		m_Player.AttackStart();
	}
	private void AnimEvent_Attack_End()
	{
		m_Player.AttackEnd();
	}
	private void AnimEvent_AirAttack_Start()
	{
		m_Player.AirAttackStart();
	}
	private void AnimEvent_AirAttack_End()
	{
		m_Player.AirAttackEnd();
	}
}