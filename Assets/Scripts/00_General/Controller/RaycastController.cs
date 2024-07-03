using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class RaycastController : MonoBehaviour
{
	public const float c_SkinWidth = 0.015f;
	private const float c_DstBetweenRays = 0.25f;

	#region 변수
	protected int m_HorizontalRayCount;
	protected int m_VerticalRayCount;

	protected float m_HorizontalRaySpacing;
	protected float m_VerticalRaySpacing;

	protected Collider2D m_Collider;
	public RaycastOrigins m_RaycastOrigins;
	#endregion

	#region 프로퍼티
	public new Collider2D collider => m_Collider;
	#endregion

	public virtual void Initialize()
	{
		this.NullCheckGetComponent<Collider2D>(ref m_Collider);

		CalculateRaySpacing();
	}

	private void CalculateRaySpacing()
	{
		Bounds bounds = m_Collider.bounds;
		bounds.Expand(c_SkinWidth * -2);

		float boundsWidth = bounds.size.x;
		float boundsHeight = bounds.size.y;

		m_HorizontalRayCount = Mathf.RoundToInt(boundsHeight / c_DstBetweenRays);
		m_VerticalRayCount = Mathf.RoundToInt(boundsWidth / c_DstBetweenRays);

		m_HorizontalRaySpacing = bounds.size.y / (m_HorizontalRayCount - 1);
		m_VerticalRaySpacing = bounds.size.x / (m_VerticalRayCount - 1);
	}
	public void UpdateRaycastOrigins()
	{
		Bounds bounds = m_Collider.bounds;
		bounds.Expand(c_SkinWidth * -2);

		m_RaycastOrigins.bottomLeft.Set(bounds.min.x, bounds.min.y);
		m_RaycastOrigins.bottomRight.Set(bounds.max.x, bounds.min.y);
		m_RaycastOrigins.topLeft.Set(bounds.min.x, bounds.max.y);
		m_RaycastOrigins.topRight.Set(bounds.max.x, bounds.max.y);
	}

	public struct RaycastOrigins
	{
		public Vector2 topLeft, topRight;
		public Vector2 bottomLeft, bottomRight;
	}
}