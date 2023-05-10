using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class RaycastController : MonoBehaviour
{
	[SerializeField]
	protected LayerMask m_CollisionMask;

	public const float skinWidth = 0.015f;
	const float dstBetweenRays = 0.125f;

	protected int m_HorizontalRayCount;
	protected int m_VerticalRayCount;

	protected float m_HorizontalRaySpacing;
	protected float m_VerticalRaySpacing;

	protected Collider2D m_Collider;
	public RaycastOrigins m_RaycastOrigins;

	public new Collider2D collider => m_Collider;

	private void CalculateRaySpacing()
	{
		Bounds bounds = m_Collider.bounds;
		bounds.Expand(skinWidth * -2);

		float boundsWidth = bounds.size.x;
		float boundsHeight = bounds.size.y;

		m_HorizontalRayCount = Mathf.RoundToInt(boundsHeight / dstBetweenRays);
		m_VerticalRayCount = Mathf.RoundToInt(boundsWidth / dstBetweenRays);

		m_HorizontalRaySpacing = bounds.size.y / (m_HorizontalRayCount - 1);
		m_VerticalRaySpacing = bounds.size.x / (m_VerticalRayCount - 1);
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

	public virtual void Initialize()
	{
		m_Collider = GetComponent<Collider2D>();

		CalculateRaySpacing();
	}

	public struct RaycastOrigins
	{
		public Vector2 topLeft, topRight;
		public Vector2 bottomLeft, bottomRight;
	}
}