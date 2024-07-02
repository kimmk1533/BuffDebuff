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
		#endregion

		public override void Initialize()
		{
			base.Initialize();

			this.Safe_GetComponentInParent<Player>(ref m_Player);
		}

		public void Anim_SetDirectionalInput(Vector2 input)
		{
			if (input.x != 0f)
				m_Animator.SetInteger("AnimState", (int)E_AnimState.Run);
			else
				m_Animator.SetInteger("AnimState", (int)E_AnimState.Idle);
		}

		// Anim Event
		private void AnimEvent_Attack1()
		{
			m_Player.AnimEvent_Attacking();
		}
		private void AnimEvent_Attack2()
		{
			m_Player.AnimEvent_Attacking();
		}
		private void AnimEvent_Attack3()
		{
			m_Player.AnimEvent_Attacking();
		}
		private void AnimEvent_Attack_Start()
		{
			m_Player.AnimEvent_AttackStart();
		}
		private void AnimEvent_Attack_End()
		{
			m_Player.AnimEvent_AttackEnd();
		}
		private void AnimEvent_Attack_ComboStart()
		{
			m_Player.AnimEvent_StartCombo();
		}
		private void AnimEvent_Attack_ComboEnd()
		{
			m_Player.AnimEvent_EndCombo();
		}
		private void AnimEvent_AirAttack_Start()
		{
			m_Player.AnimEvent_AirAttackStart();
		}
		private void AnimEvent_AirAttack_End()
		{
			m_Player.AnimEvent_AirAttackEnd();
		}
	}
}