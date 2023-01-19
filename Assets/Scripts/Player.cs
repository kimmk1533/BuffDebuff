using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Controller2D))]
public class Player : MonoBehaviour
{
	public float m_JumpHeight = 4;
	public float m_TimeToJumpApex = 0.4f;
	float m_AccelerationTimeAirborne = 0.2f;
	float m_AccelerationTimeGrounded = 0.1f;
	float m_MoveSpeed = 6;

	float m_Gravity;
	float m_JumpVelocity;
	Vector3 m_Velocity;
	float m_VelocityXSmoothing;

	Controller2D m_Controller;

	private void Start()
	{
		m_Controller = GetComponent<Controller2D>();

		{
			/*
			 *                                           acceleration * time昌
			 * deltaMovement = velocityInitial * time + 天天天天天天天天天天天天天天
			 *                                                     2
			 *               ⊿
			 *                  gravity * timeToJumpApex昌
			 *    jumpHeight = 天天天天天天天天天天天天天天天天天
			 *                              2
			 *               ⊿
			 *                  2 * jumpHeight
			 *       gravity = 天天天天天天天天天天
			 *                  timeToJumpApex昌
			 * 
			 */
		}
		m_Gravity = -(2 * m_JumpHeight) / Mathf.Pow(m_TimeToJumpApex, 2);
		{
			/*
			 * velocityFinal = velocityInitial + acceleration * time
			 * 
			 *  jumpVelocity = gravity * timeToJumpApex
			 * 
			 */
		}
		m_JumpVelocity = Mathf.Abs(m_Gravity) * m_TimeToJumpApex;
		print("Gravity: " + m_Gravity + "  Jump Velocity: " + m_JumpVelocity);
	}

	private void Update()
	{
		if (m_Controller.m_Collisions.above || m_Controller.m_Collisions.below)
		{
			m_Velocity.y = 0;
		}

		Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

		if (Input.GetKeyDown(KeyCode.Space) && m_Controller.m_Collisions.below)
		{
			m_Velocity.y = m_JumpVelocity;
		}

		float targetVelocityX = input.x * m_MoveSpeed;
		m_Velocity.x = Mathf.SmoothDamp(m_Velocity.x, targetVelocityX, ref m_VelocityXSmoothing, (m_Controller.m_Collisions.below) ? m_AccelerationTimeGrounded: m_AccelerationTimeAirborne);
		m_Velocity.y += m_Gravity * Time.deltaTime;
		m_Controller.Move(m_Velocity * Time.deltaTime);
	}
}