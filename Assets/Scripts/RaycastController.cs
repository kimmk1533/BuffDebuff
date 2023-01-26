using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class RaycastController : MonoBehaviour
{
	public LayerMask m_CollisionMask;

	public const float skinWidth = 0.015f;
	public int m_HorizontalRayCount = 4;
	public int m_VerticalRayCount = 4;

	protected float m_HorizontalRaySpacing;
	protected float m_VerticalRaySpacing;

	protected Collider2D m_Collider;
	public RaycastOrigins m_RaycastOrigins;

	public new Collider2D collider => m_Collider;

	protected virtual void Start()
	{
		m_Collider = GetComponent<Collider2D>();

		CalculateRaySpacing();
	}

	public void UpdateRaycastOrigins()
	{
		Bounds bounds = m_Collider.bounds;
		bounds.Expand(skinWidth * -2);

		m_RaycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
		m_RaycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
		m_RaycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
		m_RaycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
	}
	public void CalculateRaySpacing()
	{
		Bounds bounds = m_Collider.bounds;
		bounds.Expand(skinWidth * -2);

		m_HorizontalRayCount = Mathf.Clamp(m_HorizontalRayCount, 2, int.MaxValue);
		m_VerticalRayCount = Mathf.Clamp(m_VerticalRayCount, 2, int.MaxValue);

		m_HorizontalRaySpacing = bounds.size.y / (m_HorizontalRayCount - 1);
		m_VerticalRaySpacing = bounds.size.x / (m_VerticalRayCount - 1);
	}

	public struct RaycastOrigins
	{
		public Vector2 topLeft, topRight;
		public Vector2 bottomLeft, bottomRight;
	}
}