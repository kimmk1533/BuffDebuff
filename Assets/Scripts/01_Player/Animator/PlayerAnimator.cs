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
	private PlayerCharacter m_PlayerCharacter;

	public override void Initialize()
	{
		base.Initialize();

		m_Player = GetComponentInParent<Player>();
		m_PlayerCharacter = GetComponentInParent<PlayerCharacter>();
	}

	public void Anim_SetDirectionalInput(Vector2 input)
	{
		if (input.x == 0)
			m_Animator.SetInteger("AnimState", (int)E_AnimState.Idle);
		else
			m_Animator.SetInteger("AnimState", (int)E_AnimState.Run);
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
		m_PlayerCharacter.AnimEvent_AttackStart();
	}
	private void AnimEvent_Attack_End()
	{
		m_PlayerCharacter.AnimEvent_AttackEnd();
	}
	private void AnimEvent_AirAttack_Start()
	{
		m_PlayerCharacter.AnimEvent_AirAttackStart();
	}
	private void AnimEvent_AirAttack_End()
	{
		m_PlayerCharacter.AnimEvent_AirAttackEnd();
	}
}