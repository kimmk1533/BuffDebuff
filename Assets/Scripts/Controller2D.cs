using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller2D : RaycastController
{
	float m_MaxClimbAngle = 80;
	float m_MaxDescendAngle = 75;

	CollisionInfo m_Collisions;
	Vector2 m_PlayerInput;

	public CollisionInfo collisions => m_Collisions;
	public Vector2 playerInput => m_PlayerInput;

	protected override void Start()
	{
		base.Start();

		m_Collisions.faceDir = 1;
	}

	public void Move(Vector3 velocity, bool standingOnPlatform)
	{
		Move(velocity, Vector2.zero, standingOnPlatform);
	}

	public void Move(Vector3 velocity, Vector2 input, bool standingOnPlatform = false)
	{
		UpdateRaycastOrigins();

		m_Collisions.Reset();
		m_Collisions.velocityOld = velocity;
		m_PlayerInput = input;

		if (velocity.x != 0)
		{
			m_Collisions.faceDir = (int)Mathf.Sign(velocity.x);
		}

		if (velocity.y < 0)
		{
			DescendSlope(ref velocity);
		}

		HorizontalCollisions(ref velocity);
		if (velocity.y != 0)
		{
			VerticalCollisions(ref velocity);
		}

		transform.Translate(velocity);

		if (standingOnPlatform)
		{
			m_Collisions.below = true;
		}
	}
	void HorizontalCollisions(ref Vector3 velocity)
	{
		float directionX = m_Collisions.faceDir;
		float rayLength = Mathf.Abs(velocity.x) + skinWidth;

		if (Mathf.Abs(velocity.x) < skinWidth)
		{
			rayLength = 2 * skinWidth;
		}

		for (int i = 0; i < m_HorizontalRayCount; ++i)
		{
			Vector2 rayOrigin = (directionX == -1) ? m_RaycastOrigins.bottomLeft : m_RaycastOrigins.bottomRight;
			rayOrigin += Vector2.up * (m_HorizontalRaySpacing * i);
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, m_CollisionMask);

			Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);

			if (hit)
			{
				// °ãÄ£ °æ¿ì
				if (hit.distance == 0)
				{
					continue;
				}

				float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

				if (i == 0 && slopeAngle <= m_MaxClimbAngle)
				{
					if (m_Collisions.descendingSlope)
					{
						m_Collisions.descendingSlope = false;
						velocity = m_Collisions.velocityOld;
					}
					float distanceToSlopeStart = 0;
					if (slopeAngle != m_Collisions.slopeAngleOld)
					{
						distanceToSlopeStart = hit.distance - skinWidth;
						velocity.x -= distanceToSlopeStart * directionX;
					}
					ClimbSlope(ref velocity, slopeAngle);
					velocity.x += distanceToSlopeStart * directionX;
				}

				if (!m_Collisions.climbingSlope || slopeAngle > m_MaxClimbAngle)
				{
					//velocity.x = (hit.distance - skinWidth) * directionX;
					//rayLength = hit.distance;
					velocity.x = Mathf.Min(Mathf.Abs(velocity.x), (hit.distance - skinWidth)) * directionX;
					rayLength = Mathf.Min(Mathf.Abs(velocity.x) + skinWidth, hit.distance);

					if (m_Collisions.climbingSlope)
					{
						velocity.y = Mathf.Tan(m_Collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x);
					}

					m_Collisions.left = directionX == -1;
					m_Collisions.right = directionX == 1;
				}
			}
		}
	}
	void VerticalCollisions(ref Vector3 velocity)
	{
		float directionY = Mathf.Sign(velocity.y);
		float rayLength = Mathf.Abs(velocity.y) + skinWidth;

		for (int i = 0; i < m_VerticalRayCount; ++i)
		{
			Vector2 rayOrigin = (directionY == -1) ? m_RaycastOrigins.bottomLeft : m_RaycastOrigins.topLeft;
			rayOrigin += Vector2.right * (m_VerticalRaySpacing * i + velocity.x);
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, m_CollisionMask);

			Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);

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

				velocity.y = (hit.distance - skinWidth) * directionY;
				rayLength = hit.distance;

				if (m_Collisions.climbingSlope)
				{
					velocity.x = velocity.y / Mathf.Tan(m_Collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(velocity.x);
				}

				m_Collisions.below = directionY == -1;
				m_Collisions.above = directionY == 1;
			}
		}

		if (m_Collisions.climbingSlope)
		{
			float directionX = Mathf.Sign(velocity.x);
			rayLength = Mathf.Abs(velocity.x) + skinWidth;
			Vector2 rayOrigin = ((directionX == -1) ? m_RaycastOrigins.bottomLeft : m_RaycastOrigins.bottomRight) + Vector2.up * velocity.y;
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, m_CollisionMask);

			if (hit)
			{
				float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
				if (slopeAngle != m_Collisions.slopeAngle)
				{
					velocity.x = (hit.distance - skinWidth) * directionX;
					m_Collisions.slopeAngle = slopeAngle;
				}
			}
		}
	}
	void ClimbSlope(ref Vector3 velocity, float slopeAngle)
	{
		float moveDistance = Mathf.Abs(velocity.x);
		float climbVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

		if (velocity.y > climbVelocityY)
		{
			print("Jumping on slope");
		}
		else
		{
			velocity.y = climbVelocityY;
			velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
			m_Collisions.below = true;
			m_Collisions.climbingSlope = true;
			m_Collisions.slopeAngle = slopeAngle;
		}
	}
	void DescendSlope(ref Vector3 velocity)
	{
		float directionX = Mathf.Sign(velocity.x);
		Vector2 rayOrigin = (directionX == -1) ? m_RaycastOrigins.bottomRight : m_RaycastOrigins.bottomLeft;
		RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, m_CollisionMask);

		if (hit)
		{
			float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
			if (slopeAngle != 0 && slopeAngle <= m_MaxDescendAngle)
			{
				if (Mathf.Sign(hit.normal.x) == directionX)
				{
					if (hit.distance - skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x))
					{
						float moveDistance = Mathf.Abs(velocity.x);
						float descendVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
						velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
						velocity.y -= descendVelocityY;

						m_Collisions.slopeAngle = slopeAngle;
						m_Collisions.descendingSlope = true;
						m_Collisions.below = true;
					}
				}
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
		public float slopeAngle, slopeAngleOld;
		public Vector3 velocityOld;
		public int faceDir;
		public Collider2D fallingThroughPlatform;

		public void Reset()
		{
			above = below = false;
			left = right = false;
			climbingSlope = false;
			descendingSlope = false;

			slopeAngleOld = slopeAngle;
			slopeAngle = 0;
		}
	}
}