using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyController2D))]
public class Enemy : MonoBehaviour
{
	protected EnemyController2D m_Controller;

	[SerializeField]
	protected int m_MoveDir;
	protected UtilClass.Timer m_MoveDirTimer;

	[SerializeField, ReadOnly]
	Vector2 m_Velocity;

	protected CustomNode m_RoadToTarget;
	Vector2Int m_TargetPos;

	bool m_Jumping;
	Vector2Int? m_JumpEndPos = null;

	UtilClass.Timer m_PathFindTimer;

	[SerializeField, ChildComponent("Renderer")]
	protected EnemyRenderer m_Renderer;
	[SerializeField, ChildComponent("VisualRange")]
	protected EnemyVisualRange m_VisualRange;

	protected EnemyCharacter m_Character;

	protected int moveDir
	{
		get { return m_MoveDir; }
		set
		{
			m_MoveDir = value;

			if (value != 0)
				transform.localScale = new Vector3(value, 1.0f, 1.0f);
		}
	}
	protected bool checkDeath => m_Character.currentStat.Hp <= 0f;

	protected PlayerManager M_Player => PlayerManager.Instance;
	protected EnemyManager M_Enemy => EnemyManager.Instance;
	protected GridManager M_Grid => GridManager.Instance;

	private void Awake()
	{
		Initialize();
	}
	private void Update()
	{
		//Move();
	}

	// 초기화
	protected virtual void Initialize()
	{
		m_Controller = GetComponent<EnemyController2D>();
		m_Controller.Initialize();

		m_MoveDirTimer = new UtilClass.Timer(Random.Range(0.5f, 1.5f));
		m_PathFindTimer = new UtilClass.Timer(1f);

		m_Renderer.Initialize();
		m_VisualRange.Initialize();

		m_Character = GetComponent<EnemyCharacter>();
		m_Character.Initialize();
	}

	#region 이동
	// 이동
	protected virtual void Move()
	{
		// 이동 방향
		Vector2 dir = Vector2.zero;

		// 속도 계산
		CalculateVelocity(ref dir);

		// 이동
		m_Controller.Move(m_Velocity * Time.deltaTime, dir);

		if (m_Controller.collisions.above || m_Controller.collisions.below)
		{
			m_Velocity.y = 0;
		}
	}
	// 속도 계산
	protected virtual void CalculateVelocity(ref Vector2 dir)
	{
		// 목표가 없을 때
		if (m_VisualRange.target == null)
		{
			// 랜덤 움직임
			RandomVelocity();
		}
		// 목표가 있을 때
		else
		{
			// 목표를 향하는 움직임
			TargetVelocity(ref dir);
		}

		m_Velocity.x = moveDir * m_Character.currentStat.MoveSpeed;
		m_Velocity.y += m_Controller.gravity * Time.deltaTime;
	}
	#region 랜덤 움직임
	// 랜덤 움직임
	private void RandomVelocity()
	{
		// 목표가 존재하는 경우 랜덤하게 움직이면 안됨. 예외처리
		if (m_VisualRange.target != null)
			return;

		// 타이머
		if (m_MoveDirTimer.Update(true) == false)
			return;

		// 다음 움직임까지의 시간
		m_MoveDirTimer.interval = Random.Range(0.5f, 1.5f);

		// 랜덤 방향 움직임
		moveDir = Random.Range(-1, 2);
	}
	#endregion
	#region 목표로 향하는 움직임
	// 목표로 향하는 움직임
	private void TargetVelocity(ref Vector2 dir)
	{
		#region 변수 생성
		// 현재 타일 맨 위 칸과 겹치는 상황이므로 임시로 검색할 땐 한 칸 위로 검색함(임시)
		// 현재 위치
		Vector3 pos = transform.position + Vector3.up * (1f + float.Epsilon);
		// 목표 위치
		Vector3 targetPos = m_VisualRange.target.transform.position + Vector3.up * (1f + float.Epsilon);
		// 현재 위치 정수 버전
		Vector2Int posInt = new Vector2Int((int)pos.x, (int)pos.y);
		// 목표 위치 정수 버전
		Vector2Int targetPosInt = new Vector2Int((int)targetPos.x, (int)targetPos.y);
		#endregion

		// 경로 검색
		FindPathToTarget(ref pos, ref targetPos, ref posInt, ref targetPosInt);

		// 현재 타일 맨 위 칸과 겹치는 상황이므로 임시로 검색할 땐 한 칸 위로 검색함(임시)
		// 한 칸 위로 설정한 것 다시 빼줌
		pos -= Vector3.up;
		posInt -= Vector2Int.up;

		// 경로가 없는 경우
		if (m_RoadToTarget == null)
		{
			// x 방향 움직임 봉인
			moveDir = 0;
			// 종료
			return;
		}

		// 다음 경로로의 방향 계산
		dir = new Vector3(m_RoadToTarget.x + 0.5f, m_RoadToTarget.y) - pos;

		// x 방향 움직임
		moveDir = System.MathF.Sign(dir.x);

		// 착지1
		// 땅에 닿은 경우
		if (m_Controller.collisions.grounded)
		{
			// 점프 중 아님 설정
			m_Jumping = false;
		}

		// 착지 조건2
		if (m_Jumping &&                        // 점프 중인 경우
			m_JumpEndPos.HasValue == true)      // 착지 지점이 있는 경우
		{
			Vector2 jumpEndPos = m_JumpEndPos.Value + Vector2.right * 0.5f - (Vector2)pos;
			Vector2Int dirInt = m_JumpEndPos.Value - posInt;

			// 착지 지점과 같은 칸에 도달한 경우
			if (dirInt == Vector2Int.zero &&
				Mathf.Abs(jumpEndPos.x) <= 0.15f)
			{
				// x 방향 움직임 봉인
				moveDir = 0;

				// 착지
				// 땅에 닿은 경우
				if (m_Controller.collisions.grounded)
				{
					// 착지 지점 날리고
					m_JumpEndPos = null;

					// 점프 중 아님 설정
					m_Jumping = false;
				}

				if (jumpEndPos.y < -0.15f)
					return;
			}
		}

		// 점프
		Jump(ref dir, ref pos);

		// 다음 경로 찾는 조건
		if (Mathf.Abs(dir.x) <= 0.1f &&     // x 좌표가 충분히 가까워졌고
			(Mathf.Abs(dir.y) <= 0.1f ||    // y 좌표가 충분히 가까워졌거나
			(m_Jumping && dir.y <= 0f)))    // 점프중이고 y 좌표가 아래에 있으면
		{
			m_PathFindTimer.Use();

			m_RoadToTarget = m_RoadToTarget.parent as CustomNode;
		}
	}

	// 경로 찾기
	private void FindPathToTarget(ref Vector3 pos, ref Vector3 targetPos, ref Vector2Int posInt, ref Vector2Int targetPosInt)
	{
		m_PathFindTimer.Update();

		#region 새로운 경로 찾는 조건 확인(예외 처리)
		// 땅에 붙어 있는지 확인
		if (m_Controller.collisions.grounded == false)
			return;

		if (!(m_RoadToTarget == null || // 경로가 아직 존재하는지 확인
				Vector2Int.Distance(posInt, m_RoadToTarget.position) > 3f || // 찾은 경로와 너무 멀어지면 다시 찾는 임시 조건
				m_TargetPos != targetPosInt || // 목표가 움직였는지 확인
				m_PathFindTimer.timeIsUp == true))
			return;
		#endregion

		m_PathFindTimer.Clear();

		// 목표 위치 업데이트
		m_TargetPos = targetPosInt;

		// 경로 검색
		m_RoadToTarget = M_Grid.PathFinding(pos, targetPos, (int)m_Controller.maxJumpHeight);

		// 착지 지점 날리고
		m_JumpEndPos = null;
		// 점프 중 아님 설정
		m_Jumping = false;

		// 경로에 시작점(현재 위치)가 포함되어 시작점 다음 위치부터 검색하도록 수정
		if (m_RoadToTarget != null)
			m_RoadToTarget = m_RoadToTarget.parent as CustomNode;
	}
	// 점프
	private void Jump(ref Vector2 dir, ref Vector3 pos)
	{
		#region 점프 조건 확인(예외 처리)
		// 공중에 떠 있는 경우
		if (m_Controller.collisions.isair == true)
			return;
		// 이미 점프 중인 경우
		if (m_Jumping == true)
			return;
		#endregion

		// 위로 점프 조건
		// 방향이 위를 향하고 있으면
		if (dir.y > 0f)
		{
			// 위로 점프
			JumpToUp(ref pos);
		}
		// 아래로 점프 조건
		// 방향이 아래를 향하고 있으면
		else if (dir.y < 0f)
		{
			// 아래로 점프
			JumpToDown();
		}
	}
	// 위로 점프
	private void JumpToUp(ref Vector3 pos)
	{
		CustomNode jumpNode = m_RoadToTarget as CustomNode;

		if (jumpNode == null)
			return;

		if (jumpNode.jumpStartPos.HasValue == false)
		{
			m_Velocity.y = m_Controller.minJumpVelocity;
			//Debug.Log("위로 점프 시작 좌표 없음");
			return;
		}

		// 좌우 이동을 못하면 점프도 못함
		if (m_Character.currentStat.MoveSpeed == 0f)
			return;

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

		{
			// 최고점 (높이 차 + 1)
			float height = jumpNode.y - (int)pos.y + 1;
			// 중력 가속도
			float g = Mathf.Abs(m_Controller.gravity);
			// x축 전체 거리
			float distanceX = jumpNode.x - pos.x;

			// 가장 적절한 점프 높이 찾기
			while (height <= m_Controller.maxJumpHeight)
			{
				// 최고점 x 위치
				float Cx = pos.x + m_Character.currentStat.MoveSpeed * distanceX * Mathf.Sqrt(2 * height / g) / Mathf.Abs(distanceX);
				// 최고점 y 위치
				float Cy = pos.y + height;

				// 이동할 x 거리
				float moveX = Mathf.Abs(Cx - jumpNode.x);
				// 이동할 y 거리
				float moveY = Mathf.Abs(Cy - jumpNode.y);

				// 최고점에서 목표지점까지 x축 이동 시간
				float xTime = moveX / m_Character.currentStat.MoveSpeed;
				// 최고점에서 목표지점까지 y축 이동 시간
				float yTime = moveY / height;

				// y축 이동 시간이 x축 이동 시간보다 길다면
				// 시간 안에 목표 점에 도달하지 못한 것이므로
				// 현재가 최적의 점프 높이
				if (yTime > xTime)
					break;

				++height;
			}

			Debug.Log("점프 높이: " + height);
			height = Mathf.Clamp(height, 0f, m_Controller.maxJumpHeight);
			float Vy = Mathf.Sqrt(2 * g * height);

			m_Velocity.y = Vy;
		}

		//m_Velocity.y = m_Controller.maxJumpVelocity;

		m_Jumping = true;
	}
	// 아래로 점프
	private void JumpToDown()
	{

	}
	#endregion
	#endregion

	public void TakeDamage(float damage)
	{
		m_Character.currentStat.Hp -= damage;

		if (checkDeath == true)
			Death();
	}
	protected void Death()
	{
		float xp = m_Character.currentStat.Xp * m_Character.currentStat.XpScale;
		M_Player.AddXp(xp);

		M_Enemy.Despawn(this);
	}

	private void OnDrawGizmos()
	{
		if (m_RoadToTarget == null ||
			Application.isPlaying == false)
			return;

		Vector3 pos = new Vector3(m_RoadToTarget.x, m_RoadToTarget.y);
		Vector3 half = Vector3.one * 0.5f;

		// 시작점
		Gizmos.color = Color.green;
		Gizmos.DrawCube(pos + half, half);
	}
}