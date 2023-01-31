using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRenderer : MonoBehaviour
{
	SpriteRenderer m_SpriteRenderer;
	Animator m_Animator;

	private int direction
	{
		set
		{
			Vector3 scale = transform.localScale;

			if (value > 0)
				scale.x = 1;
			else if (value < 0)
				scale.x = -1;

			transform.localScale = scale;
		}
	}

	void Start()
	{
		m_SpriteRenderer = GetComponent<SpriteRenderer>();
		m_Animator = GetComponent<Animator>();

		if (m_SpriteRenderer == null)
			Debug.LogError("Player doesn`t have SpriteRenderer with PlayerRenderer!");
		if (m_Animator == null)
			Debug.LogError("Player doesn`t have Animator with PlayerRenderer!");
	}

	public void SetIsGround(bool isGround)
	{
		m_Animator.SetBool("isGround", isGround);
	}
	public void SetDirectionalInput(Vector2 input)
	{
		if (input.x == 0)
			m_Animator.SetInteger("AnimState", (int)E_AnimState.Idle);
		else
			m_Animator.SetInteger("AnimState", (int)E_AnimState.Run);
	}
	public void SetVelocity(float x, float y)
	{
		direction = (int)x;

		m_Animator.SetFloat("VelocityX", x);
		m_Animator.SetFloat("VelocityY", y);
	}
	public void SetVelocity(Vector2 speed)
	{
		SetVelocity(speed.x, speed.y);
	}
	public void Jump()
	{
		m_Animator.SetTrigger("Jump");
	}

	private enum E_AnimState
	{
		None = 0,

		Idle = 0,
		Run = 1,

		Max
	}
}