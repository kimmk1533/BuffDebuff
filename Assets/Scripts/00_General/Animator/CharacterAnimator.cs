using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuffDebuff
{
	public class CharacterAnimator : MonoBehaviour, IAnim_Movable, IAnim_IsGround, IAnim_Jump, IAnim_Attack, IAnim_Death
	{
		#region 변수
		protected SpriteRenderer m_SpriteRenderer;
		protected Animator m_Animator;
		#endregion

		public virtual void Initialize()
		{
			this.NullCheckGetComponent<SpriteRenderer>(ref m_SpriteRenderer);
			this.NullCheckGetComponent<Animator>(ref m_Animator);
		}

		public virtual void Anim_SetVelocity(float x, float y)
		{
			m_Animator.SetFloat("VelocityX", x);
			m_Animator.SetFloat("VelocityY", y);
		}
		public virtual void Anim_SetVelocity(Vector2 speed)
		{
			Anim_SetVelocity(speed.x, speed.y);
		}
		public virtual void Anim_SetIsGround(bool isGround)
		{
			m_Animator.SetBool("isGround", isGround);
		}

		public virtual void Anim_Jump()
		{
			m_Animator.SetTrigger("Jump");
		}

		public void Anim_SetAttackSpeed(float attackSpeed)
		{
			m_Animator.SetFloat("Attack Speed", attackSpeed);
		}
		public void Anim_Attack(int patternIndex)
		{
			m_Animator.SetInteger("Attack Pattern", patternIndex);
			m_Animator.SetTrigger("Attack");
		}

		public virtual void Anim_Death()
		{
			m_Animator.SetTrigger("Death");
		}
	}
}