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
	#endregion

	protected List<CustomNode> m_RoadToPlayerList;

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
			Vector3 pos = transform.position + Vector3.up * 2;
			Vector3 targetPos = m_VisualRange.target.transform.position + Vector3.up * 2;

			if (m_RoadToPlayerList.Count == 0)
			{
				var list = M_Grid.PathFinding(pos, targetPos, (int)m_Controller.maxJumpHeight);

				if (list != null)
					m_RoadToPlayerList.AddRange(list);
			}

			//m_RoadToPlayerList = M_Grid.PathFinding(pos, targetPos, (int)m_Controller.maxJumpHeight);

			if (m_RoadToPlayerList != null)
			{
				Vector3 roadPos = new Vector3(m_RoadToPlayerList[0].position.x, m_RoadToPlayerList[0].position.y);

				if (Vector3.Distance(roadPos, pos) <= 1f)
				{
					m_RoadToPlayerList.RemoveAt(0);
				}
				dir = roadPos - pos;

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

	protected virtual void Initialize()
	{
		m_Controller = GetComponent<EnemyController2D>();
		m_Controller.Initialize();

		m_RoadToPlayerList = new List<CustomNode>();

		m_MoveDirTimer = new UtilClass.Timer(Random.Range(0.5f, 1.5f));
	}
	// 코루틴 사용 자제 이유 https://dhy948.tistory.com/16
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
}