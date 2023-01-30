using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController2D : Controller2D
{
	Vector2 m_PlayerInput;
	Collider2D m_FallingThroughPlatform;

	public Vector2 playerInput => m_PlayerInput;

	public new void Move(Vector2 moveAmount, bool standingOnPlatform)
	{
		Move(moveAmount, Vector2.zero, standingOnPlatform);
	}
	public void Move(Vector2 moveAmount, Vector2 input, bool standingOnPlatform = false)
	{
		UpdateRaycastOrigins();

		m_Collisions.Reset();
		m_Collisions.moveAmountOld = moveAmount;
		m_PlayerInput = input;

		if (moveAmount.y < 0)
		{
			DescendSlope(ref moveAmount);
		}

		//if (moveAmount.x != 0)
		//{
		//	m_Collisions.faceDir = (int)Mathf.Sign(moveAmount.x);
		//}

		HorizontalCollisions(ref moveAmount);
		if (moveAmount.y != 0)
		{
			VerticalCollisions(ref moveAmount);
		}

		transform.Translate(moveAmount);

		if (standingOnPlatform)
		{
			m_Collisions.below = true;
		}
	}

	protected override void VerticalCollisions(ref Vector2 moveAmount)
	{
		float directionY = Mathf.Sign(moveAmount.y);
		float rayLength = Mathf.Abs(moveAmount.y) + skinWidth;

		for (int i = 0; i < m_VerticalRayCount; ++i)
		{
			Vector2 rayOrigin = (directionY == -1) ? m_RaycastOrigins.bottomLeft : m_RaycastOrigins.topLeft;
			rayOrigin += Vector2.right * (m_VerticalRaySpacing * i + moveAmount.x);
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, m_CollisionMask);

			Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength * 10.0f, Color.red);

			if (hit)
			{
				if (hit.collider.CompareTag("Through"))
				{
					if (directionY == 1 || hit.distance == 0)
					{
						continue;
					}
					if (hit.collider == m_FallingThroughPlatform)
					{
						continue;
					}
				}
				if (m_PlayerInput.y == -1 && Input.GetKeyDown(KeyCode.Space))
				{
					m_FallingThroughPlatform = hit.collider;
					Invoke("ResetFallingThroughPlatform", 0.1f);
					continue;
				}

				moveAmount.y = (hit.distance - skinWidth) * directionY;
				rayLength = hit.distance;

				if (m_Collisions.climbingSlope)
				{
					moveAmount.x = moveAmount.y / Mathf.Tan(m_Collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(moveAmount.x);
				}

				m_Collisions.below = directionY == -1;
				m_Collisions.above = directionY == 1;
			}
		}

		if (m_Collisions.climbingSlope)
		{
			float directionX = Mathf.Sign(moveAmount.x);
			rayLength = Mathf.Abs(moveAmount.x) + skinWidth;
			Vector2 rayOrigin = ((directionX == -1) ? m_RaycastOrigins.bottomLeft : m_RaycastOrigins.bottomRight) + Vector2.up * moveAmount.y;
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, m_CollisionMask);

			if (hit)
			{
				float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
				if (slopeAngle != m_Collisions.slopeAngle)
				{
					moveAmount.x = (hit.distance - skinWidth) * directionX;
					m_Collisions.slopeAngle = slopeAngle;
					m_Collisions.slopeNormal = hit.normal;
				}
			}
		}
	}

	protected void ResetFallingThroughPlatform()
	{
		m_FallingThroughPlatform = null;
	}
}