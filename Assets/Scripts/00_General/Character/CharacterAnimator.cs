using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
	#region 변수
	protected SpriteRenderer m_SpriteRenderer;
	protected Animator m_Animator;
	#endregion

	public virtual void Initialize()
	{
		if (m_SpriteRenderer == null)
			m_SpriteRenderer = GetComponent<SpriteRenderer>();
		if (m_Animator == null)
			m_Animator = GetComponent<Animator>();
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
	public virtual void Anim_SetAttackSpeed(float attackSpeed)
	{
		m_Animator.SetFloat("Attack Speed", attackSpeed);
	}
	public virtual void Anim_Attack()
	{
		m_Animator.SetTrigger("Attack");
	}
	public virtual void Anim_Jump()
	{
		m_Animator.SetTrigger("Jump");
	}
	public virtual void Anim_Death()
	{
		m_Animator.SetTrigger("Death");
	}
}