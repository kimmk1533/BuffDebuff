using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class EnemyTargetFinder : MonoBehaviour
{
	[SerializeField]
	protected LayerMask m_LayerMask;

	[Space(10)]
	protected BoxCollider2D m_Collider;

	[SerializeField, ReadOnly]
	protected GameObject m_Target;

	protected bool m_isLostTarget;
	[SerializeField]
	protected UtilClass.Timer m_ForgetTargetTimer;

	private bool m_Finding;

	public GameObject target => m_Target;
	protected int moveDir => (int)Mathf.Sign(transform.parent.lossyScale.x);

	public virtual void Initialize()
	{
		m_Collider = GetComponent<BoxCollider2D>();
		m_isLostTarget = false;

		m_ForgetTargetTimer = new UtilClass.Timer(1.0f);
	}

	private void Update()
	{
		CollisionCheck();

		if (m_isLostTarget)
		{
			FindTarget();
		}
	}

	private void CollisionCheck()
	{
		Vector2 origin = (Vector2)transform.position + m_Collider.offset * moveDir;
		Vector2 size = m_Collider.size;
		RaycastHit2D hit = Physics2D.BoxCast(origin, size, 0f, Vector2.right * moveDir, 0f, m_LayerMask);

		if (m_Finding == false && hit)
		{
			m_Finding = true;
			TargetEnter2D(hit.collider);
		}
		else if (m_Finding == true && !hit)
		{
			m_Finding = false;
			TargetExit2D(hit.collider);
		}
	}
	private void TargetEnter2D(Collider2D collider2D)
	{
		m_Target = collider2D.gameObject;

		//if (m_isLostTarget)
		//	Debug.Log("타겟 다시 찾음!");

		m_isLostTarget = false;

		//Debug.Log("타겟 찾음!");
	}
	private void TargetExit2D(Collider2D collider2D)
	{
		m_isLostTarget = true;
		m_ForgetTargetTimer.Clear();

		//Debug.Log("타겟 놓침!");
	}

	protected virtual void FindTarget()
	{
		//Debug.Log("타겟 다시 찾는 중!");

		//m_ForgetTargetTimer.Update();
		if (m_ForgetTargetTimer.TimeCheck(true))
		{
			//Debug.Log("타겟 잃어버림!");
			m_Target = null;
			m_isLostTarget = false;
		}
	}
}