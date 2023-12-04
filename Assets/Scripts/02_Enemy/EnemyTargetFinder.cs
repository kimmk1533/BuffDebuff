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
	protected Collider2D m_Target;

	protected bool m_isLostTarget;
	[SerializeField]
	protected UtilClass.Timer m_ForgetTargetTimer;

	private bool m_Finding;

	private event System.Action m_OnTargetEnter;
	private event System.Action m_OnTargetExit;
	private event System.Action m_OnTargetLost;

	public GameObject target => (m_Target == null) ? null : m_Target.gameObject;
	public Collider2D targetCollider => m_Target;
	protected int moveDir => (int)Mathf.Sign(transform.parent.lossyScale.x);

	public event System.Action onTargetEnter
	{
		add { m_OnTargetEnter += value; }
		remove { m_OnTargetEnter -= value; }
	}
	public event System.Action onTargetExit
	{
		add { m_OnTargetExit += value; }
		remove { m_OnTargetExit -= value; }
	}
	public event System.Action onTargetLost
	{
		add { m_OnTargetLost += value; }
		remove { m_OnTargetLost -= value; }
	}

	public virtual void Initialize()
	{
		m_Collider = GetComponent<BoxCollider2D>();
		m_isLostTarget = false;

		if (m_ForgetTargetTimer == null)
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
		Vector2 origin = m_Collider.bounds.center;
		Vector2 size = m_Collider.size;
		RaycastHit2D[] hit = Physics2D.BoxCastAll(origin, size, 0f, Vector2.right * moveDir, 0f, m_LayerMask);

		if (hit == null)
			return;

		if (hit.Length > 1)
		{
			Debug.LogError("Error: 적 공격 범위에 플레이어가 2명 이상 감지됨");
			return;
		}

		if (m_Finding == false && hit.Length > 0)
		{
			m_Finding = true;
			TargetEnter2D(hit[0].collider);
		}
		else if (m_Finding == true && hit.Length == 0)
		{
			m_Finding = false;
			TargetExit2D(null);
		}
	}
	private void TargetEnter2D(Collider2D collider2D)
	{
		m_Target = collider2D;

		//if (m_isLostTarget)
		//	Debug.Log("타겟 다시 찾음!");

		m_isLostTarget = false;

		m_OnTargetEnter?.Invoke();

		//Debug.Log("타겟 찾음!");
	}
	private void TargetExit2D(Collider2D collider2D)
	{
		m_isLostTarget = true;
		m_ForgetTargetTimer.Clear();

		m_OnTargetExit?.Invoke();

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

			m_OnTargetLost?.Invoke();
		}
	}
}