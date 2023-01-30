using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller2D : RaycastController
{
	public float m_MaxSlopeAngle = 50;

	CollisionInfo m_Collisions;
	Vector2 m_PlayerInput;

	public CollisionInfo collisions => m_Collisions;
	public Vector2 playerInput => m_PlayerInput;

	protected override void Start()
	{
		base.Start();

		m_Collisions.faceDir = 1;
	}

	public void Move(Vector2 moveAmount, bool standingOnPlatform)
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

		if (moveAmount.x != 0)
		{
			m_Collisions.faceDir = (int)Mathf.Sign(moveAmount.x);
		}

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
	void HorizontalCollisions(ref Vector2 moveAmount)
	{
		float directionX = m_Collisions.faceDir;
		float rayLength = Mathf.Abs(moveAmount.x) + skinWidth;

		if (Mathf.Abs(moveAmount.x) < skinWidth)
		{
			rayLength = 2 * skinWidth;
		}

		for (int i = 0; i < m_HorizontalRayCount; ++i)
		{
			Vector2 rayOrigin = (directionX == -1) ? m_RaycastOrigins.bottomLeft : m_RaycastOrigins.bottomRight;
			rayOrigin += Vector2.up * (m_HorizontalRaySpacing * i);
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, m_CollisionMask);

			Debug.DrawRay(rayOrigin, Vector2.right * directionX, Color.red);

			if (hit)
			{
				// °ãÄ£ °æ¿ì
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
						distanceToSlopeStart = hit.distance - skinWidth;
						moveAmount.x -= distanceToSlopeStart * directionX;
					}
					ClimbSlope(ref moveAmount, slopeAngle, hit.normal);
					moveAmount.x += distanceToSlopeStart * directionX;
				}

				if (!m_Collisions.climbingSlope || slopeAngle > m_MaxSlopeAngle)
				{
					//moveAmount.x = (hit.distance - skinWidth) * directionX;
					//rayLength = hit.distance;
					moveAmount.x = Mathf.Min(Mathf.Abs(moveAmount.x), (hit.distance - skinWidth)) * directionX;
					rayLength = Mathf.Min(Mathf.Abs(moveAmount.x) + skinWidth, hit.distance);

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
	void VerticalCollisions(ref Vector2 moveAmount)
	{
		float directionY = Mathf.Sign(moveAmount.y);
		float rayLength = Mathf.Abs(moveAmount.y) + skinWidth;

		for (int i = 0; i < m_VerticalRayCount; ++i)
		{
			Vector2 rayOrigin = (directionY == -1) ? m_RaycastOrigins.bottomLeft : m_RaycastOrigins.topLeft;
			rayOrigin += Vector2.right * (m_VerticalRaySpacing * i + moveAmount.x);
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, m_CollisionMask);

			Debug.DrawRay(rayOrigin, Vector2.up * directionY, Color.red);

			if (hit)
			{
				if (hit.collider.CompareTag("Through"))
				{
					if (directionY == 1 || hit.distance == 0)
					{
						continue;
					}
					if (hit.collider == m_Collisions.fallingThroughPlatform)
					{
						continue;
					}
					if (m_PlayerInput.y == -1 && Input.GetKeyDown(KeyCode.Space))
					{
						m_Collisions.fallingThroughPlatform = hit.collider;
						Invoke("ResetFallingThroughPlatform", 0.1f);
						continue;
					}
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
	void ClimbSlope(ref Vector2 moveAmount, float slopeAngle, Vector2 slopeNormal)
	{
		float moveDistance = Mathf.Abs(moveAmount.x);
		float climbMoveAmountY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

		if (moveAmount.y <= climbMoveAmountY)
		{
			moveAmount.y = climbMoveAmountY;
			moveAmount.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(moveAmount.x);
			m_Collisions.below = true;
			m_Collisions.climbingSlope = true;
			m_Collisions.slopeAngle = slopeAngle;
			m_Collisions.slopeNormal = slopeNormal;
		}
	}
	void DescendSlope(ref Vector2 moveAmount)
	{
		RaycastHit2D maxSlopeHitLeft = Physics2D.Raycast(m_RaycastOrigins.bottomLeft, Vector2.down, Mathf.Abs(moveAmount.y) + skinWidth, m_CollisionMask);
		RaycastHit2D maxSlopeHitRight = Physics2D.Raycast(m_RaycastOrigins.bottomRight, Vector2.down, Mathf.Abs(moveAmount.y) + skinWidth, m_CollisionMask);

		//if (maxSlopeHitLeft ^ maxSlopeHitRight)
		{
			SlideDownMaxSlope(maxSlopeHitLeft, ref moveAmount);
			SlideDownMaxSlope(maxSlopeHitRight, ref moveAmount);
		}

		if (!m_Collisions.slidingDownMaxSlope)
		{
			float directionX = Mathf.Sign(moveAmount.x);
			Vector2 rayOrigin = (directionX == -1) ? m_RaycastOrigins.bottomRight : m_RaycastOrigins.bottomLeft;
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, m_CollisionMask);

			if (hit)
			{
				float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
				if (slopeAngle != 0 && slopeAngle <= m_MaxSlopeAngle)
				{
					if (Mathf.Sign(hit.normal.x) == directionX)
					{
						if (hit.distance - skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(moveAmount.x))
						{
							float moveDistance = Mathf.Abs(moveAmount.x);
							float descendMoveAmountY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
							moveAmount.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(moveAmount.x);
							moveAmount.y -= descendMoveAmountY;

							m_Collisions.slopeAngle = slopeAngle;
							m_Collisions.descendingSlope = true;
							m_Collisions.below = true;
							m_Collisions.slopeNormal = hit.normal;
						}
					}
				}
			}
		}
	}
	void SlideDownMaxSlope(RaycastHit2D hit, ref Vector2 moveAmount)
	{
		if (hit)
		{
			float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
			if (slopeAngle > m_MaxSlopeAngle)
			{
				moveAmount.x = Mathf.Sign(hit.normal.x) * (Mathf.Abs(moveAmount.y) - hit.distance) / Mathf.Tan(slopeAngle * Mathf.Deg2Rad);

				m_Collisions.slopeAngle = slopeAngle;
				m_Collisions.slidingDownMaxSlope = true;
				m_Collisions.slopeNormal = hit.normal;
			}
		}
	}
	void ResetFallingThroughPlatform()
	{
		m_Collisions.fallingThroughPlatform = null;
	}

	public struct CollisionInfo
	{
		public bool above, below;
		public bool left, right;

		public bool climbingSlope;
		public bool descendingSlope;
		public bool slidingDownMaxSlope;

		public float slopeAngle, slopeAngleOld;
		public Vector2 slopeNormal;
		public Vector2 moveAmountOld;
		public int faceDir;
		public Collider2D fallingThroughPlatform;

		public void Reset()
		{
			above = below = false;
			left = right = false;
			climbingSlope = false;
			descendingSlope = false;
			slidingDownMaxSlope = false;
			slopeNormal = Vector2.zero;

			slopeAngleOld = slopeAngle;
			slopeAngle = 0;
		}
	}
}