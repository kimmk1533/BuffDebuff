using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleCollisionChecker2D : CollisionChecker2D
{
	#region 변수
	[SerializeField, Min(0.0001f)]
	protected float m_Radius = 0.5f;
	#endregion

	#region 프로퍼티
	public float radius
	{
		get => m_Radius;
		set => m_Radius = value;
	}
	#endregion

	public override void Initialize()
	{
		base.Initialize();

		m_ColliderType = E_ColliderType.CircleCollider2D;
	}

	protected override Collider2D[] GetCollisionColliders()
	{
		float scaleX = transform.lossyScale.x;
		float scaleY = transform.lossyScale.y;
		Vector2 offset = new Vector2(
			  Mathf.Sign(scaleX) * m_Offset.x,
			  Mathf.Sign(scaleY) * m_Offset.y);
		Vector2 origin = (Vector2)transform.position + offset;

		return Physics2D.OverlapCircleAll(origin, m_Radius, m_LayerMask);
	}

	protected override void OnDrawGizmosSelected()
	{
		Color color = Gizmos.color;

		float scaleX = transform.lossyScale.x;
		float scaleY = transform.lossyScale.y;
		Vector2 offset = new Vector2(
			  Mathf.Sign(scaleX) * m_Offset.x,
			  Mathf.Sign(scaleY) * m_Offset.y);
		Vector2 origin = (Vector2)transform.position + offset;

		Gizmos.color = c_ColliderColor;

		Gizmos.DrawWireSphere(origin, m_Radius);

		Gizmos.color = color;
	}
}