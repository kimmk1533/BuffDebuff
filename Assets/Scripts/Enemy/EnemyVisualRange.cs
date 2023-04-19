using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class EnemyVisualRange : MonoBehaviour
{
	[SerializeField]
	protected LayerMask m_LayerMask;

	[Space(10)]
	protected BoxCollider2D m_Collider;

	[SerializeField, ReadOnly]
	protected GameObject m_Target;
	public GameObject target
	{
		get { return m_Target; }
		protected set { m_Target = value; }
	}

	protected bool m_isLostTarget;
	[SerializeField]
	protected float m_FindTargetTimeLimit;
	protected float m_FindTargetTimer;

	private bool m_Finding;
	protected int moveDir
	{
		get
		{
			return (int)Mathf.Sign(transform.parent.lossyScale.x);
		}
	}

	private void Awake()
	{
		Init();
	}
	protected virtual void Init()
	{
		m_Collider = GetComponent<BoxCollider2D>();
		m_isLostTarget = false;

		m_FindTargetTimer = 0.0f;
	}

	private void CollisionCheck()
	{
		Vector2 origin = (Vector2)transform.position + m_Collider.offset * moveDir;
		Vector2 size = m_Collider.size;
		RaycastHit2D hit = Physics2D.BoxCast(origin, size, 0f, Vector2.right * moveDir, 0f, m_LayerMask);

		if (m_Finding == false && hit)
		{
			m_Finding = true;
			TriggerEnter2D(hit.collider);
		}
		else if (m_Finding == true && !hit)
		{
			m_Finding = false;
			TriggerExit2D(hit.collider);
		}
	}
	private void TriggerEnter2D(Collider2D collider2D)
	{
		target = collider2D.gameObject;

		if (m_isLostTarget)
			Debug.Log("타겟 다시 찾음!");

		m_isLostTarget = false;

		Debug.Log("타겟 찾음!");
	}
	private void TriggerExit2D(Collider2D collider2D)
	{
		m_isLostTarget = true;
		m_FindTargetTimer = 0.0f;

		Debug.Log("타겟 놓침!");
	}

	private void Update()
	{
		CollisionCheck();

		if (m_isLostTarget)
		{
			FindTarget();
		}
	}

	protected virtual void FindTarget()
	{
		Debug.Log("타겟 다시 찾는 중!");
		m_FindTargetTimer += Time.deltaTime;

		if (m_FindTargetTimer >= m_FindTargetTimeLimit)
		{
			Debug.Log("타겟 잃어버림!");
			m_Target = null;
			m_isLostTarget = false;
		}
	}
}