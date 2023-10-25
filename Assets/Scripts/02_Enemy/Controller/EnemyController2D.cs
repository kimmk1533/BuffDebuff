using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class EnemyController2D : Controller2D
{
	bool[] m_EnemyInput;
	Collider2D m_FallingOneWayPlatform;

	public new void Move(Vector2 moveAmount, bool standingOnPlatform = false)
	{
		Move(moveAmount, null, standingOnPlatform);
	}
	public void Move(Vector2 moveAmount, bool[] input, bool standingOnPlatform = false)
	{
		UpdateRaycastOrigins();

		m_Collisions.Reset();
		m_Collisions.moveAmountOld = moveAmount;
		m_EnemyInput = input;

		if (moveAmount.y < 0)
		{
			DescendSlope(ref moveAmount);
		}

		if (moveAmount.x != 0)
		{
			m_Collisions.faceDir = (int)Mathf.Sign(moveAmount.x);
		}

		HorizontalCollisions(ref moveAmount);
		if (moveAmount.y != 0)
		{
			VerticalCollisions(ref moveAmount);
		}

		transform.Translate(moveAmount, Space.World);

		if (standingOnPlatform)
		{
			m_Collisions.below = true;
		}
	}
	protected override void VerticalCollisions(ref Vector2 moveAmount)
	{
		float directionY = Mathf.Sign(moveAmount.y);
		float rayLength = Mathf.Abs(moveAmount.y) + skinWidth * 2.5f; // 레이가 안 닿는 경우가 생김 임시로 2.5배 곱해줌으로 해결

		bool grounded = false;
		bool throughFlag = false;

		for (int i = 0; i < m_VerticalRayCount; ++i)
		{
			Vector2 rayOrigin = (directionY == -1) ? m_RaycastOrigins.bottomLeft : m_RaycastOrigins.topLeft;
			rayOrigin += Vector2.right * (m_VerticalRaySpacing * i + moveAmount.x);
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, m_CollisionMask);

			Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);

			if (hit)
			{
				if (hit.collider.CompareTag("OneWay"))
				{
					//grounded = false;
					throughFlag = true;
					if (directionY == -1)
					{
						m_Collisions.isOnOneWayPlatform = true;
					}
					if (directionY == 1 || hit.distance == 0)
					{
						continue;
					}
					if (hit.collider == m_FallingOneWayPlatform)
					{
						continue;
					}
					if (m_EnemyInput != null
						&& m_EnemyInput[(int)EnemyCharacter.E_KeyInput.Down] == true)
					{
						m_FallingOneWayPlatform = hit.collider;
						continue;
					}
				}

				//moveAmount.y = (hit.distance - skinWidth) * directionY;
				//rayLength = hit.distance;
				moveAmount.y = Mathf.Min(Mathf.Abs(moveAmount.y), (hit.distance - skinWidth)) * directionY;
				rayLength = Mathf.Min(Mathf.Abs(moveAmount.y) + skinWidth, hit.distance);

				if (m_Collisions.climbingSlope)
				{
					moveAmount.x = moveAmount.y / Mathf.Tan(m_Collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(moveAmount.x);
				}

				m_Collisions.below = directionY == -1;
				m_Collisions.above = directionY == 1;

				grounded = true;
			}
		}

		m_Collisions.isair = !grounded;
		m_Collisions.grounded = grounded;

		if (!throughFlag)
		{
			m_FallingOneWayPlatform = null;
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
}