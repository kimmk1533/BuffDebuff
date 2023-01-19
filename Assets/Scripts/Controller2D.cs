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

	float m_HorizontalRaySpacing;
	float m_VerticalRaySpacing;

	BoxCollider2D m_Collider;
	RaycastOrigins m_RaycastOrigins;

	private void Start()
	{
		m_Collider = GetComponent<BoxCollider2D>();

		CalculateRaySpacing();
	}

	public void Move(Vector3 velocity)
	{
		UpdateRaycastOrigins();

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
				velocity.x = (hit.distance - skinWidth) * directionX;
				rayLength = hit.distance;
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
			}
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
}