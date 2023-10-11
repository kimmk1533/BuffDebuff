using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
	protected SpriteRenderer m_SpriteRenderer;
	protected Animator m_Animator;

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

	public virtual void Initialize()
	{
		m_SpriteRenderer = GetComponent<SpriteRenderer>();
		m_Animator = GetComponent<Animator>();
	}

	public virtual void Anim_SetVelocity(float x, float y)
	{
		direction = System.MathF.Sign(x);

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