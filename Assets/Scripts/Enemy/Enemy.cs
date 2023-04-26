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
	[SerializeField/*, ReadOnly*/]
	protected float m_MoveSpeed;

	[SerializeField, ReadOnly]
	Vector2 m_Velocity;

	Vector2Int m_TargetPos;

	bool m_Jumping;
	Vector2Int? m_JumpEndPos = null;
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
		m_MoveSpeed = m_Controller.maxJumpHeight;
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
		Vector2Int dirInt = Vector2Int.zero;

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

			Vector2Int posInt = new Vector2Int((int)pos.x, (int)pos.y);
			Vector2Int targetPosInt = new Vector2Int((int)targetPos.x, (int)targetPos.y);

			if (m_RoadToPlayer == null ||
				Vector2Int.Distance(posInt, m_RoadToPlayer.position) > 3f || // 찾은 경로와 너무 멀어지면 다시 찾는 임시 조건
				m_TargetPos != targetPosInt)
			{
				m_TargetPos = targetPosInt;

				m_RoadToPlayer = M_Grid.PathFinding(pos, targetPos, (int)m_Controller.maxJumpHeight);

				CustomNode node = m_RoadToPlayer;

				while (node != null)
				{
					node.position.y = node.position.y - 1;
					node.start.y = node.start.y - 1;
					node.end.y = node.end.y - 1;
					if (node.jumpStartPos.HasValue == true)
					{
						Vector2Int jumpStartPos = node.jumpStartPos.Value;
						--jumpStartPos.y;
						node.jumpStartPos = jumpStartPos;
					}
					node = node.parent as CustomNode;
				}

				if (m_RoadToPlayer != null)
					m_RoadToPlayer = m_RoadToPlayer.parent as CustomNode;
			}

			pos -= Vector3.up;
			posInt -= Vector2Int.up;

			if (m_RoadToPlayer != null)
			{
				dirInt = m_RoadToPlayer.position - posInt;

				ChangeMoveDir(System.MathF.Sign(dirInt.x));

				if (m_JumpEndPos.HasValue == true &&
					m_JumpEndPos.Value == posInt)
				{
					ChangeMoveDir(0);

					if (m_Controller.collisions.grounded)
						m_JumpEndPos = null;
				}

				if (m_Jumping == false &&
					m_Controller.collisions.grounded && dirInt.y > 0)
				{
					m_Jumping = true;
				}

				CustomNode jumpNode = m_RoadToPlayer.parent as CustomNode;

				if (m_Jumping && dirInt.x == 0 &&
					jumpNode != null && jumpNode.jumpStartPos.HasValue)
				{
					Vector2Int jumpStartPos = jumpNode.jumpStartPos.Value;

					while (jumpNode != null)
					{
						if (jumpNode.jumpStartPos.HasValue == false)
							break;

						if (jumpStartPos != jumpNode.jumpStartPos)
							break;

						CustomNode jumpParent = jumpNode.parent as CustomNode;

						if (jumpParent == null)
							break;

						jumpNode = jumpParent;
					}

					m_JumpEndPos = jumpNode.position;

					int height = jumpNode.y - posInt.y;
					height = Mathf.Clamp(height + 1, 0, Mathf.RoundToInt(m_Controller.maxJumpHeight));
					float g = Mathf.Abs(m_Controller.gravity);
					float Vy = Mathf.Sqrt(2 * g * height);

					m_Velocity.y = Vy;

					m_Jumping = false;
				}

				if (dirInt.x == 0)
				{
					if (dirInt.y == 0 ||
						(m_Controller.collisions.isair && Mathf.Abs(dirInt.y) == 1))
					{
						m_RoadToPlayer = m_RoadToPlayer.parent as CustomNode;
					}
				}
			}
			else
			{
				ChangeMoveDir(0);
			}
		}

		CalculateVelocity();

		m_Controller.Move(m_Velocity * Time.deltaTime, dirInt);

		if (m_Controller.collisions.above || m_Controller.collisions.below)
		{
			m_Velocity.y = 0;
		}
	}

	private void OnDrawGizmos()
	{
		if (m_RoadToPlayer == null ||
			Application.isPlaying == false)
			return;

		Vector3 pos = new Vector3(m_RoadToPlayer.x, m_RoadToPlayer.y);
		Vector3 half = Vector3.one * 0.5f;

		// 시작점
		Gizmos.color = Color.green;
		Gizmos.DrawCube(pos + half, half);
	}
}