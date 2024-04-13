using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuffDebuff
{
	public class PlayerController2D : Controller2D
	{
		#region 변수
		private bool m_IsGroundedOld = false;
		private Collider2D m_FallingOneWayPlatform;
		#endregion

		#region 매니저
		private static InputManager M_Input => InputManager.Instance;
		#endregion

		public new void Move(Vector2 moveAmount, bool standingOnPlatform = false)
		{
			UpdateRaycastOrigins();

			m_IsGroundedOld = m_Collisions.grounded;
			m_Collisions.Reset();
			m_Collisions.moveAmountOld = moveAmount;

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
		protected override void HorizontalCollisions(ref Vector2 moveAmount)
		{
			float directionX = m_Collisions.faceDir;
			float rayLength = Mathf.Abs(moveAmount.x) + c_SkinWidth;

			if (Mathf.Abs(moveAmount.x) < c_SkinWidth)
			{
				rayLength = 2 * c_SkinWidth;
			}

			for (int i = 0; i < m_HorizontalRayCount; ++i)
			{
				Vector2 rayOrigin = (directionX == -1) ? m_RaycastOrigins.bottomLeft : m_RaycastOrigins.bottomRight;
				rayOrigin += Vector2.up * (m_HorizontalRaySpacing * i);
				RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, m_CollisionMask);

				Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength * 10.0f, Color.red);

				if (hit)
				{
					if (hit.collider.CompareTag("OneWay") && m_IsGroundedOld)
					{
						continue;
					}

					// 겹친 경우
					if (hit.distance == 0)
					{
						continue;
					}

					float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

					if (i == 0 && slopeAngle <= m_MaxSlopeAngle)
					{
						if (m_Collisions.descendingSlope)
						{
							m_Collisions.descendingSlope = false;
							moveAmount = m_Collisions.moveAmountOld;
						}

						float distanceToSlopeStart = 0;
						if (slopeAngle != m_Collisions.slopeAngleOld)
						{
							distanceToSlopeStart = hit.distance - c_SkinWidth;
							moveAmount.x -= distanceToSlopeStart * directionX;
						}

						ClimbSlope(ref moveAmount, slopeAngle, hit.normal);
						moveAmount.x += distanceToSlopeStart * directionX;
					}

					if (!m_Collisions.climbingSlope || slopeAngle > m_MaxSlopeAngle)
					{
						//moveAmount.x = (hit.distance - skinWidth) * directionX;
						//rayLength = hit.distance;
						moveAmount.x = Mathf.Min(Mathf.Abs(moveAmount.x), hit.distance - c_SkinWidth) * directionX;
						rayLength = Mathf.Min(Mathf.Abs(moveAmount.x) + c_SkinWidth, hit.distance);

						if (m_Collisions.climbingSlope)
						{
							moveAmount.y = Mathf.Tan(m_Collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(moveAmount.x);
						}

						m_Collisions.left = directionX == -1;
						m_Collisions.right = directionX == 1;
					}
				}
			}
		}
		protected override void VerticalCollisions(ref Vector2 moveAmount)
		{
			float directionY = Mathf.Sign(moveAmount.y);
			float rayLength = Mathf.Abs(moveAmount.y) + c_SkinWidth * 2.5f; // 레이가 안 닿는 경우가 생김 임시로 2.5배 곱해줌으로 해결

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
						if (M_Input.GetKey(E_InputType.PlayerMoveDown) && M_Input.GetKeyDown(E_InputType.PlayerJump))
						{
							m_FallingOneWayPlatform = hit.collider;
							continue;
						}
					}

					//moveAmount.y = (hit.distance - skinWidth) * directionY;
					//rayLength = hit.distance;
					moveAmount.y = Mathf.Min(Mathf.Abs(moveAmount.y), (hit.distance - c_SkinWidth)) * directionY;
					rayLength = Mathf.Min(Mathf.Abs(moveAmount.y) + c_SkinWidth, hit.distance);

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
				rayLength = Mathf.Abs(moveAmount.x) + c_SkinWidth;
				Vector2 rayOrigin = ((directionX == -1) ? m_RaycastOrigins.bottomLeft : m_RaycastOrigins.bottomRight) + Vector2.up * moveAmount.y;
				RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, m_CollisionMask);

				if (hit)
				{
					float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
					if (slopeAngle != m_Collisions.slopeAngle)
					{
						moveAmount.x = (hit.distance - c_SkinWidth) * directionX;
						m_Collisions.slopeAngle = slopeAngle;
						m_Collisions.slopeNormal = hit.normal;
					}
				}
			}
		}
	}
}