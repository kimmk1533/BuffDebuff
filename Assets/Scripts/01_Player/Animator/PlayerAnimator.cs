using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuffDebuff
{
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

		#region 변수
		private Player m_Player;
		private PlayerCharacter m_PlayerCharacter;
		#endregion

		public override void Initialize()
		{
			base.Initialize();

			this.Safe_GetComponentInParent<Player>(ref m_Player);
			this.Safe_GetComponentInParent<PlayerCharacter>(ref m_PlayerCharacter);
		}

		public void Anim_SetDirectionalInput(Vector2 input)
		{
			if (input.x != 0f)
				m_Animator.SetInteger("AnimState", (int)E_AnimState.Run);
			else
				m_Animator.SetInteger("AnimState", (int)E_AnimState.Idle);
		}

		// Anim Event
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
		private void AnimEvent_Attack_ComboStart()
		{
			m_PlayerCharacter.AnimEvent_StartCombo();
		}
		private void AnimEvent_Attack_ComboEnd()
		{
			m_PlayerCharacter.AnimEvent_EndCombo();
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
}