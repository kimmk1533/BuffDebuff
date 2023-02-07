using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D), typeof(PlayerController2D))]
public class Player : MonoBehaviour
{
	[SerializeField, ReadOnly(true)]
	float m_MaxJumpHeight = 4;
	[SerializeField, ReadOnly(true)]
	float m_MinJumpHeight = 1;
	[SerializeField, ReadOnly(true)]
	float m_TimeToJumpApex = 0.4f;
	float m_AccelerationTimeAirborne = 0.2f;
	float m_AccelerationTimeGrounded = 0.1f;
	[SerializeField]
	float m_MoveSpeed = 6;

	[SerializeField, ReadOnly]
	float m_Gravity;
	float m_MaxJumpVelocity;
	float m_MinJumpVelocity;
	[SerializeField, ReadOnly]
	Vector2 m_Velocity;
	float m_VelocityXSmoothing;

	PlayerController2D m_Controller;
	PlayerRenderer m_Renderer;

	Vector2 m_DirectionalInput;

	ProjectileManager M_Projectile => ProjectileManager.Instance;

	private void Start()
	{
		m_Controller = GetComponent<PlayerController2D>();
		m_Renderer = GetComponentInChildren<PlayerRenderer>();

		{
			/*
			 *                                           acceleration * time²
			 * deltaMovement = velocityInitial * time + ㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ
			 *                                                     2
			 *               ↓
			 *                  gravity * timeToJumpApex²
			 *    jumpHeight = ㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ
			 *                              2
			 *               ↓
			 *                  2 * jumpHeight
			 *       gravity = ㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ
			 *                  timeToJumpApex²
			 * 
			 */
		}
		m_Gravity = -(2 * m_MaxJumpHeight) / Mathf.Pow(m_TimeToJumpApex, 2);
		{
			/*
			 * 
			 * velocityFinal = velocityInitial + acceleration * time
			 * 
			 *  jumpVelocity = gravity * timeToJumpApex
			 * 
			 */
		}
		m_MaxJumpVelocity = Mathf.Abs(m_Gravity) * m_TimeToJumpApex;
		m_MinJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(m_Gravity) * m_MinJumpHeight);
	}
	private void Update()
	{
		CalculateVelocity();

		m_Controller.Move(m_Velocity * Time.deltaTime, m_DirectionalInput);

		if (m_Controller.collisions.above || m_Controller.collisions.below)
		{
			//if (m_Controller.collisions.slidingDownMaxSlope)
			//{
			//	m_Velocity.y += m_Controller.collisions.slopeNormal.y * -m_Gravity * Time.deltaTime;
			//}
			//else
			{
				m_Velocity.y = 0;
			}
		}

		m_Renderer.SetVelocity(m_Velocity);
		m_Renderer.SetIsGround(m_Controller.collisions.below);
	}

	public void SetDirectionalInput(Vector2 input)
	{
		m_DirectionalInput = input;
		m_Renderer.SetDirectionalInput(input);
	}
	public void OnJumpInputDown()
	{
		if (m_Controller.collisions.below && m_DirectionalInput.y != -1)
		{
			//if (m_Controller.collisions.slidingDownMaxSlope)
			//{
			//	if (m_DirectionalInput.x != -Mathf.Sign(m_Controller.collisions.slopeNormal.x)) // not jumping against max slope
			//	{
			//		m_Velocity.y = m_MaxJumpVelocity * m_Controller.collisions.slopeNormal.y;
			//		m_Velocity.x = m_MaxJumpVelocity * m_Controller.collisions.slopeNormal.x;
			//	}
			//}
			//else
			{
				m_Velocity.y = m_MaxJumpVelocity;
				m_Renderer.Jump();
			}
		}
	}
	public void OnJumpInputUp()
	{
		if (m_Velocity.y > m_MinJumpVelocity)
			m_Velocity.y = m_MinJumpVelocity;
	}
	public void DefaultAttack()
	{
		Projectile projectile = M_Projectile.Spawn("Projectile");
		projectile.transform.position = transform.position;
		projectile.m_MovingType = E_MovingType.Straight;

		Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		float angle = transform.position.GetAngle(mousePos);
		projectile.transform.rotation = Quaternion.AngleAxis(angle - 90, Vector3.forward);
		projectile.m_MoveSpeed = 5.0f;
	}

	void CalculateVelocity()
	{
		float targetVelocityX = m_DirectionalInput.x * m_MoveSpeed;

		m_Velocity.x = Mathf.SmoothDamp(m_Velocity.x, targetVelocityX, ref m_VelocityXSmoothing, (m_Controller.collisions.below) ? m_AccelerationTimeGrounded : m_AccelerationTimeAirborne);
		m_Velocity.y += m_Gravity * Time.deltaTime;
	}
}