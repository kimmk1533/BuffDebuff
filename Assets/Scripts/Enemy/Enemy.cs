using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D), typeof(Controller2D))]
public class Enemy : MonoBehaviour
{
	[Space(10)]
	[SerializeField, ReadOnly]
	protected CircleCollider2D m_Collider2D;

	[Space(10)]
	#region Move Variables

	[SerializeField]
	protected int m_MoveDir;
	[SerializeField, ReadOnly]
	protected float m_MoveDirTimeLimit;
	[SerializeField, ReadOnly]
	protected float m_MoveDirTimer;
	[SerializeField]
	protected float m_MoveSpeed;

	Vector3 m_Velocity;

	#endregion

	[Space(10)]
	[SerializeField, ReadOnly]
	protected EnemyVisualRange m_VisualRange;

	private void Awake()
	{
		Init();
	}
	private void Update()
	{
		#region ChangeMoveDir
		m_MoveDirTimer += Time.deltaTime;
		if (m_MoveDirTimer >= m_MoveDirTimeLimit)
		{
			m_MoveDirTimer -= m_MoveDirTimeLimit;
			m_MoveDirTimeLimit = Random.Range(0.5f, 1.5f);

			ChangeMoveDir();
		}
		#endregion

		#region Attack

		#endregion
	}

	protected virtual void Init()
	{
		if (this.AddOneComponent<CircleCollider2D>(out m_Collider2D))
		{
			m_Collider2D.offset = new Vector2(0.06f, 0.5f);
		}
		//if (this.AddOneComponent<Rigidbody2D>(out m_Rigidbody2D))
		//{
		//	m_Rigidbody2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
		//	m_Rigidbody2D.constraints = RigidbodyConstraints2D.FreezeRotation;
		//}

		if (m_MoveSpeed == 0.0f)
			m_MoveSpeed = 2.0f;

		m_MoveDirTimeLimit = Random.Range(0.5f, 1.5f);
	}
	// 코루틴 사용 안하게 된 이유 https://dhy948.tistory.com/16
	protected virtual void ChangeMoveDir()
	{
		m_MoveDir = Random.Range(-1, 2);

		if (m_MoveDir != 0)
			transform.localScale = new Vector3(m_MoveDir, 1.0f, 1.0f);
	}
	protected virtual void Move()
	{
		//m_Rigidbody2D.velocity = new Vector2(m_MoveDir * m_MoveSpeed, m_Rigidbody2D.velocity.y);

		//Vector2 frontVector = m_Rigidbody2D.position + new Vector2(m_MoveDir * m_Collider2D.radius, 0.0f);

		//Debug.DrawRay(frontVector, Vector2.down, new Color(1, 0, 0));

		//RaycastHit2D rayhit = Physics2D.Raycast(frontVector, Vector2.down, 1, LayerMask.GetMask("TileMap"));

		//if (rayhit.collider == null)
		//{
		//	m_MoveDir = 0;
		//	m_Rigidbody2D.velocity = new Vector2(m_MoveDir * m_MoveSpeed, m_Rigidbody2D.velocity.y);
		//}
	}
}