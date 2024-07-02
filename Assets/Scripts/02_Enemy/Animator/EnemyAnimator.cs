using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuffDebuff
{
	public class EnemyAnimator : CharacterAnimator
	{
		#region Enum
		private enum E_AnimState
		{
			None = 0,

			Idle = 0,
			Run = 1,

			Max
		}
		#endregion

		#region 변수
		private Enemy m_EnemyCharacter;
		#endregion

		public override void Initialize()
		{
			base.Initialize();

			this.Safe_GetComponentInParent<Enemy>(ref m_EnemyCharacter);
		}

		public override void Anim_SetVelocity(float x, float y)
		{
			base.Anim_SetVelocity(x, y);

			if (x != 0f)
				m_Animator.SetInteger("AnimState", 1);
			else
				m_Animator.SetInteger("AnimState", 0);
		}

		// Anim Event
		protected virtual void AnimEvent_Attack1()
		{
			m_EnemyCharacter.AnimEvent_Attacking();
		}
		protected virtual void AnimEvent_Attack2()
		{
			m_EnemyCharacter.AnimEvent_Attacking();
		}
		protected virtual void AnimEvent_Attack3()
		{
			m_EnemyCharacter.AnimEvent_Attacking();
		}
		private void AnimEvent_AttackStart()
		{
			m_EnemyCharacter.AnimEvent_AttackStart();
		}
		private void AnimEvent_AttackEnd()
		{
			m_EnemyCharacter.AnimEvent_AttackEnd();
		}
	}
}