using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollisionChecker2D))]
public class EnemyTargetFinder : MonoBehaviour
{
	public enum E_TargetFinderState
	{
		Finding, // 탐색중
		Chasing, // 추격중
		ReSearching, // 재탐색중
	}

	#region 변수
	[Space(10)]
	[SerializeField, ReadOnly]
	protected Collider2D m_Target;
	protected E_TargetFinderState m_TargetFinderState;

	[SerializeField, ReadOnly]
	protected BoxCollisionChecker2D m_Finder;
	[SerializeField]
	protected UtilClass.Timer m_ForgetTargetTimer;
	protected bool m_Finding;
	#endregion

	#region 프로퍼티
	public E_TargetFinderState state => m_TargetFinderState;
	public GameObject target => (m_Target == null) ? null : m_Target.gameObject;
	public Collider2D targetCollider => m_Target;
	protected int moveDir => (int)Mathf.Sign(transform.parent.lossyScale.x);
	#endregion

	#region 이벤트
	public event CollisionChecker2D.OnTriggerHandler onTargetEnter2D
	{
		add
		{
			m_Finder["Player"].onEnter2D += value;
		}
		remove
		{
			m_Finder["Player"].onEnter2D -= value;
		}
	}
	public event CollisionChecker2D.OnTriggerHandler onTargetStay2D
	{
		add
		{
			m_Finder["Player"].onStay2D += value;
		}
		remove
		{
			m_Finder["Player"].onStay2D -= value;
		}
	}
	public event CollisionChecker2D.OnTriggerHandler onTargetExit2D
	{
		add
		{
			m_Finder["Player"].onExit2D += value;
		}
		remove
		{
			m_Finder["Player"].onExit2D -= value;
		}
	}
	public event CollisionChecker2D.OnTriggerHandler onTargetLost2D;
	#endregion

	public virtual void Initialize()
	{
		m_TargetFinderState = E_TargetFinderState.Finding;

		#region SAFE_INIT
		this.NullCheckGetComponent<BoxCollisionChecker2D>(ref m_Finder);
		m_Finder.Initialize();
		m_Finder["Player"].onEnter2D += OnTargetEnter2D;
		m_Finder["Player"].onExit2D += OnTargetExit2D;

		if (m_ForgetTargetTimer != null)
		{
			m_ForgetTargetTimer.interval = 3.0f;
			m_ForgetTargetTimer.Clear();
		}
		else
			m_ForgetTargetTimer = new UtilClass.Timer(3.0f);
		#endregion
	}
	public virtual void Finallize()
	{
		m_Target = null;

		m_Finder.Finallize();
		onTargetLost2D = null;

		m_ForgetTargetTimer.Clear();
	}

	private void Update()
	{
		if (m_Target == null)
			return;
		if (m_TargetFinderState != E_TargetFinderState.ReSearching)
			return;

		ResearchTarget();
	}

	protected virtual void ResearchTarget()
	{
		m_ForgetTargetTimer.Update();
		if (m_ForgetTargetTimer.TimeCheck(true))
		{
			onTargetLost2D?.Invoke(m_Target);

			m_Target = null;
			m_TargetFinderState = E_TargetFinderState.Finding;
		}
	}

	private void OnTargetEnter2D(Collider2D collider2D)
	{
		m_Target = collider2D;

		m_TargetFinderState = E_TargetFinderState.Chasing;
		m_ForgetTargetTimer.Clear();
	}
	private void OnTargetExit2D(Collider2D collider2D)
	{
		m_TargetFinderState = E_TargetFinderState.ReSearching;
	}
}