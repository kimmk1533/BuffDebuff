using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller2D : RaycastController
{
	[SerializeField]
	protected LayerMask m_CollisionMask;

	protected float m_MaxSlopeAngle = 50;

	[SerializeField, ReadOnly(true)]
	protected float m_MaxJumpHeight = 4;
	[SerializeField, ReadOnly(true)]
	protected float m_MinJumpHeight = 1;
	[SerializeField, ReadOnly(true)]
	protected float m_TimeToJumpApex = 0.4f;

	[SerializeField, ReadOnly]
	protected float m_Gravity;
	protected float m_MaxJumpVelocity;
	protected float m_MinJumpVelocity;

	public float timeToJumpApex => m_TimeToJumpApex;
	public float gravity => m_Gravity;
	public float maxJumpHeight => m_MaxJumpHeight;
	public float minJumpHeight => m_MinJumpHeight;
	public float maxJumpVelocity => m_MaxJumpVelocity;
	public float minJumpVelocity => m_MinJumpVelocity;

	protected CollisionInfo m_Collisions;
	public CollisionInfo collisions => m_Collisions;

	public override void Initialize()
	{
		base.Initialize();

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

		m_Collisions.faceDir = 1;
	}

	public void Move(Vector2 moveAmount, bool standingOnPlatform = false)
	{
		UpdateRaycastOrigins();

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

		transform.Translate(moveAmount);

		if (standingOnPlatform)
		{
			m_Collisions.below = true;
		}
	}
	protected virtual void HorizontalCollisions(ref Vector2 moveAmount)
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

			Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength * 10.0f, Color.red);

			if (hit)
			{
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
	protected virtual void VerticalCollisions(ref Vector2 moveAmount)
	{
		float directionY = Mathf.Sign(moveAmount.y);
		float rayLength = Mathf.Abs(moveAmount.y) + skinWidth;

		bool grounded = false;

		for (int i = 0; i < m_VerticalRayCount; ++i)
		{
			Vector2 rayOrigin = (directionY == -1) ? m_RaycastOrigins.bottomLeft : m_RaycastOrigins.topLeft;
			rayOrigin += Vector2.right * (m_VerticalRaySpacing * i + moveAmount.x);
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, m_CollisionMask);

			Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength * 10.0f, Color.red);

			if (hit)
			{
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
	protected void ClimbSlope(ref Vector2 moveAmount, float slopeAngle, Vector2 slopeNormal)
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
	protected void DescendSlope(ref Vector2 moveAmount)
	{
		float directionX = m_Collisions.faceDir;
		Vector2 rayOrigin = (directionX == -1) ? m_RaycastOrigins.bottomRight : m_RaycastOrigins.bottomLeft;
		RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, Mathf.Infinity, m_CollisionMask);

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

	public struct CollisionInfo
	{
		public bool above, below;
		public bool left, right;

		public bool isair, grounded;

		public bool isOnOneWayPlatform;

		public bool climbingSlope;
		public bool descendingSlope;

		public float slopeAngle, slopeAngleOld;
		public Vector2 slopeNormal;
		public Vector2 moveAmountOld;
		public int faceDir;

		public void Reset()
		{
			above = below = false;
			left = right = false;
			isair = grounded = false;
			climbingSlope = false;
			descendingSlope = false;

			isOnOneWayPlatform = false;

			slopeAngleOld = slopeAngle;
			slopeAngle = 0;
			slopeNormal = Vector2.zero;
		}
	}
}