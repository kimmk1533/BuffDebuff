using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyController2D))]
public class Enemy : MonoBehaviour
{
	[Space(10)]
	#region Move Variables
	[SerializeField]
	protected int m_MoveDir;
	protected UtilClass.Timer m_MoveDirTimer;
	[SerializeField]
	protected float m_MoveSpeed;

	[SerializeField, ReadOnly]
	Vector2 m_Velocity;

	Vector3 m_TargetPos;
	#endregion

	protected CustomNode m_RoadToPlayer;

	protected EnemyController2D m_Controller;
	[SerializeField, ChildComponent("VisualRange")]
	protected EnemyVisualRange m_VisualRange;

	protected GridManager M_Grid => GridManager.Instance;

	private void Awake()
	{
		Initialize();
	}
	private void Update()
	{
		Move();
	}

	protected virtual void Initialize()
	{
		m_Controller = GetComponent<EnemyController2D>();
		m_Controller.Initialize();

		m_MoveDirTimer = new UtilClass.Timer(Random.Range(0.5f, 1.5f));
	}
	protected virtual void ChangeMoveDir(int moveDir)
	{
		m_MoveDir = moveDir;

		if (m_MoveDir != 0)
			transform.localScale = new Vector3(m_MoveDir, 1.0f, 1.0f);
	}
	protected virtual void CalculateVelocity()
	{
		float targetVelocityX = m_MoveDir * m_MoveSpeed;

		m_Velocity.x = targetVelocityX;
		m_Velocity.y += m_Controller.gravity * Time.deltaTime;
	}
	protected virtual void Move()
	{
		Vector3 dir = Vector3.zero;

		if (m_VisualRange.target == null)
		{
			if (m_MoveDirTimer.Update())
			{
				m_MoveDirTimer.Use();
				m_MoveDirTimer.interval = Random.Range(0.5f, 1.5f);

				ChangeMoveDir(Random.Range(-1, 2));
			}
		}
		else
		{
			Vector3 pos = transform.position + Vector3.up;
			Vector3 targetPos = m_VisualRange.target.transform.position + Vector3.up;

			if (m_RoadToPlayer == null ||
				m_TargetPos != targetPos)
			{
				m_TargetPos = targetPos;

				m_RoadToPlayer = M_Grid.PathFinding(pos, targetPos, (int)m_Controller.maxJumpHeight);
			}

			//m_RoadToPlayerList = M_Grid.PathFinding(pos, targetPos, (int)m_Controller.maxJumpHeight);

			if (m_RoadToPlayer != null)
			{
				Vector3 nextPos = new Vector3(m_RoadToPlayer.parent.x, m_RoadToPlayer.parent.y);

				dir = nextPos - pos;
				if (Mathf.Abs(dir.x) <= 0.1f &&
					Mathf.Abs(dir.y) <= 1.5f)
				{
					m_RoadToPlayer = (CustomNode)m_RoadToPlayer.parent;
				}

				ChangeMoveDir((int)Mathf.Sign(dir.x));

				if (m_Controller.collisions.below && dir.y > 0f)
				{


					m_Velocity.y = m_Controller.maxJumpVelocity;
				}
			}
		}

		CalculateVelocity();

		m_Controller.Move(m_Velocity * Time.deltaTime, dir);

		if (m_Controller.collisions.above || m_Controller.collisions.below)
		{
			m_Velocity.y = 0;
		}
	}
}