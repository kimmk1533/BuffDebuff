using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Controller2D : MonoBehaviour
{
	public LayerMask m_CollisionMask;

	const float skinWidth = 0.015f;
	public int m_HorizontalRayCount = 4;
	public int m_VerticalRayCount = 4;

	float m_MaxClimbAngle = 80;

	float m_HorizontalRaySpacing;
	float m_VerticalRaySpacing;

	BoxCollider2D m_Collider;
	RaycastOrigins m_RaycastOrigins;
	public CollisionInfo m_Collisions;

	private void Start()
	{
		m_Collider = GetComponent<BoxCollider2D>();

		CalculateRaySpacing();
	}

	public void Move(Vector3 velocity)
	{
		UpdateRaycastOrigins();

		m_Collisions.Reset();

		if (velocity.x != 0)
		{
			HorizontalCollisions(ref velocity);
		}
		if (velocity.y != 0)
		{
			VerticalCollisions(ref velocity);
		}

		transform.Translate(velocity);
	}
	void HorizontalCollisions(ref Vector3 velocity)
	{
		float directionX = Mathf.Sign(velocity.x);
		float rayLength = Mathf.Abs(velocity.x) + skinWidth;

		for (int i = 0; i < m_HorizontalRayCount; ++i)
		{
			Vector2 rayOrigin = (directionX == -1) ? m_RaycastOrigins.bottomLeft : m_RaycastOrigins.bottomRight;
			rayOrigin += Vector2.up * (m_HorizontalRaySpacing * i);
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, m_CollisionMask);

			Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);

			if (hit)
			{
				float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

				if (i == 0 && slopeAngle <= m_MaxClimbAngle)
				{
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
	void UpdateRaycastOrigins()
	{
		Bounds bounds = m_Collider.bounds;
		bounds.Expand(skinWidth * -2);

		m_RaycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
		m_RaycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
		m_RaycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
		m_RaycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
	}
	void CalculateRaySpacing()
	{
		Bounds bounds = m_Collider.bounds;
		bounds.Expand(skinWidth * -2);

		m_HorizontalRayCount = Mathf.Clamp(m_HorizontalRayCount, 2, int.MaxValue);
		m_VerticalRayCount = Mathf.Clamp(m_VerticalRayCount, 2, int.MaxValue);

		m_HorizontalRaySpacing = bounds.size.y / (m_HorizontalRayCount - 1);
		m_VerticalRaySpacing = bounds.size.x / (m_VerticalRayCount - 1);
	}

	struct RaycastOrigins
	{
		public Vector2 topLeft, topRight;
		public Vector2 bottomLeft, bottomRight;
	}
	public struct CollisionInfo
	{
		public bool above, below;
		public bool left, right;

		public bool climbingSlope;
		public float slopeAngle, slopeAngleOld;

		public void Reset()
		{
			above = below = false;
			left = right = false;
			climbingSlope = false;

			slopeAngleOld = slopeAngle;
			slopeAngle = 0;
		}
	}
}