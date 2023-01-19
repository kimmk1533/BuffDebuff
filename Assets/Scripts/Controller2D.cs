using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Controller2D : MonoBehaviour
{
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
	}

	private void Update()
	{
		UpdateRaycastOrigins();
		CalculateRaySpacing();

		for (int i = 0; i < m_VerticalRayCount; ++i)
		{
			Debug.DrawRay(m_RaycastOrigins.bottomLeft + Vector2.right * m_VerticalRaySpacing * i, Vector2.up * -2, Color.red);
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