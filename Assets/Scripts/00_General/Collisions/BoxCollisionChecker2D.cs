using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxCollisionChecker2D : CollisionChecker2D
{
	#region 변수
	[SerializeField, Min(0.0001f)]
	protected Vector2 m_Size = Vector2.one;
	#endregion

	#region 프로퍼티
	public Vector2 size
	{
		get => m_Size;
		set => m_Size = value;
	}
	#endregion

	public override void Initialize()
	{
		base.Initialize();

		m_ColliderType = E_ColliderType.BoxCollider2D;
	}

	protected override Collider2D[] GetCollisionColliders()
	{
		float scaleX = transform.lossyScale.x;
		float scaleY = transform.lossyScale.y;
		Vector2 offset = new Vector2(
			  Mathf.Sign(scaleX) * m_Offset.x,
			  Mathf.Sign(scaleY) * m_Offset.y);
		Vector2 origin = (Vector2)transform.position + offset;

		return Physics2D.OverlapBoxAll(origin, size, 0f, m_LayerMask);
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

		Gizmos.DrawWireCube(origin, m_Size);

		Gizmos.color = color;
	}
}