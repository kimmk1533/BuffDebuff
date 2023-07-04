using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class ProjectileController : RaycastController
{
	[SerializeField, ReadOnly]
	protected LayerMask m_CollisionMask;

	protected Projectile m_Projectile;

	protected List<Collider2D> m_CollisionList;
	protected List<Collider2D> m_OldCollisionList;

	public LayerMask collisionMask
	{
		get
		{
			return m_CollisionMask;
		}
		set
		{
			m_CollisionMask = value;
		}
	}

	private void Update()
	{
		m_OldCollisionList.Clear();
		if (m_CollisionList.Count > 0)
			m_OldCollisionList.AddRange(m_CollisionList);

		Collider2D[] hitColliders = null;

		BoxCollider2D boxCollider2D = m_Collider as BoxCollider2D;
		CircleCollider2D circleCollider2D = m_Collider as CircleCollider2D;

		Vector2 center = m_Collider.bounds.center;

		if (boxCollider2D != null)
		{
			Vector2 size = boxCollider2D.size * transform.lossyScale;
			float angle = transform.rotation.eulerAngles.z;

			hitColliders = Physics2D.OverlapBoxAll(center, size, angle, m_CollisionMask);

			#region Debug Ray
			//Vector2 extends = size * 0.5f;
			//Vector2 topLeft, topRight;
			//Vector2 bottomLeft, bottomRight;

			//topLeft = center + Vector2.left * extends.x + Vector2.up * extends.y;
			//topRight = center + Vector2.right * extends.x + Vector2.up * extends.y;
			//bottomLeft = center + Vector2.left * extends.x + Vector2.down * extends.y;
			//bottomRight = center + Vector2.right * extends.x + Vector2.down * extends.y;

			//Matrix4x4 matrix = new Matrix4x4();
			//matrix.m00 = Mathf.Cos(angle * Mathf.Deg2Rad);
			//matrix.m01 = -Mathf.Sin(angle * Mathf.Deg2Rad);
			//matrix.m10 = Mathf.Sin(angle * Mathf.Deg2Rad);
			//matrix.m11 = Mathf.Cos(angle * Mathf.Deg2Rad);

			//topLeft = center + (Vector2)(matrix * (center - topLeft));
			//topRight = center + (Vector2)(matrix * (center - topRight));
			//bottomLeft = center + (Vector2)(matrix * (center - bottomLeft));
			//bottomRight = center + (Vector2)(matrix * (center - bottomRight));

			//float duration = 0.001f;

			//Debug.DrawLine(topLeft, topRight, Color.red, duration);
			//Debug.DrawLine(topRight, bottomRight, Color.red, duration);
			//Debug.DrawLine(bottomRight, bottomLeft, Color.red, duration);
			//Debug.DrawLine(bottomLeft, topLeft, Color.red, duration);
			#endregion
		}
		else if (circleCollider2D != null)
		{
			hitColliders = Physics2D.OverlapCircleAll(center, circleCollider2D.radius, m_CollisionMask);
		}

		m_CollisionList.Clear();

		if (hitColliders == null)
			return;

		foreach (var item in hitColliders)
		{
			int layer = item.gameObject.layer;

			if (m_OldCollisionList.Contains(item) == false)
			{
				m_Projectile[layer].OnEnter2D?.Invoke(item);
			}
			else
			{
				m_Projectile[layer].OnStay2D?.Invoke(item);
			}

			m_CollisionList.Add(item);
		}

		foreach (var item in m_OldCollisionList)
		{
			int layer = item.gameObject.layer;

			if (m_CollisionList.Contains(item) == false)
			{
				m_Projectile[layer].OnExit2D?.Invoke(item);
			}
		}
	}

	public override void Initialize()
	{
		base.Initialize();

		// CollisionMask 초기화
		m_CollisionMask = LayerMask.GetMask();

		if (m_CollisionList == null)
			m_CollisionList = new List<Collider2D>();
		else
			m_CollisionList.Clear();

		if (m_OldCollisionList == null)
			m_OldCollisionList = new List<Collider2D>();
		else
			m_OldCollisionList.Clear();
	}
	public void Initialize(Projectile projectile)
	{
		Initialize();

		m_Projectile = projectile;
	}

	public void Move(Vector2 moveAmount)
	{
		UpdateRaycastOrigins();

		transform.Translate(moveAmount, Space.World);
	}
}