using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class EnemyCharacter : Character<EnemyCharacterStat, EnemyController2D, EnemyAnimator>
{
	[SerializeField, ChildComponent("TargetFinder")]
	protected EnemyTargetFinder m_TargetFinder;
	protected Vector2 m_TargetPos;

	protected List<Vector2Int> m_PathFinding_Path;
	protected int m_PathFinding_NodeIndex;

	protected bool m_Jumping;
	protected Vector2Int? m_JumpEndPos = null;

	[SerializeField]
	protected int m_MoveDir;
	[SerializeField]
	protected UtilClass.Timer m_MoveDirTimer;

	[SerializeField]
	protected UtilClass.Timer m_PathFinding_ReSearchTimer;

	public GameObject target => m_TargetFinder.target;
	protected Vector3 targetPos => (target == null) ? throw new System.Exception() : target.transform.position;
	protected float distanceToTarget => (target == null) ? 0f : Vector2.Distance(transform.position, target.transform.position);
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

	private GridManager M_Grid => GridManager.Instance;

	protected override void Update()
	{
		base.Update();

		if (target != null && CanAttack())
			AnimEvent_Attacking();

		Move();
	}

	public override void Initialize()
	{
		base.Initialize();

		m_TargetFinder.Initialize();

		m_PathFinding_Path = null;
		m_PathFinding_NodeIndex = -1;

		m_MoveDirTimer = new UtilClass.Timer();
		m_MoveDirTimer.interval = Random.Range(0.2f, 1.0f);
		m_MoveDirTimer.Clear();

		m_PathFinding_ReSearchTimer = new UtilClass.Timer();
		m_PathFinding_ReSearchTimer.interval = 3f;
		m_PathFinding_ReSearchTimer.Clear();

		// Timer Init
		m_HealTimer = new UtilClass.Timer(m_CurrentStat.HpRegenTime);
		m_AttackTimer = new UtilClass.Timer(1f / m_CurrentStat.AttackSpeed, true);
	}

	#region 이동
	// 이동
	protected override void Move()
	{
		// 이동 방향
		Vector2 dir = Vector2.zero;

		// 속도 계산
		CalculateVelocity();

		// 이동
		m_Controller.Move(m_Velocity * Time.deltaTime, dir);

		if (m_Controller.collisions.above || m_Controller.collisions.below)
		{
			m_Velocity.y = 0;
		}
	}
	// 속도 계산
	protected override void CalculateVelocity()
	{
		// 목표가 없을 때
		if (target == null)
		{
			// 랜덤 움직임
			CalculateVelocity_Random();
		}
		// 목표가 있을 때
		else
		{
			// 목표로 향하는 움직임
			CalculateVelocity_ToTarget();
		}

		m_Velocity.x = moveDir * m_CurrentStat.MoveSpeed;
		m_Velocity.y += m_Controller.gravity * Time.deltaTime;
	}

	#region 랜덤 움직임
	// 랜덤 움직임
	private void CalculateVelocity_Random()
	{
		// 목표가 존재하는 경우 랜덤하게 움직이면 안됨. 예외처리
		if (target != null)
			return;

		// 타이머 작동
		m_MoveDirTimer.Update();

		// 타이머 시간 확인
		if (m_MoveDirTimer.TimeCheck(true) == false)
			return;

		// 다음 움직임까지의 시간
		m_MoveDirTimer.interval = Random.Range(0.2f, 1.0f);

		// 랜덤 방향 움직임
		moveDir = Random.Range(-1, 2);
	}
	#endregion
	#region 목표로 향하는 움직임
	private void CalculateVelocity_ToTarget()
	{
		if (distanceToTarget <= m_CurrentStat.AttackRange)
		{
			moveDir = 0;
			return;
		}

		Vector2Int pos = new Vector2Int(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y));
		Vector2Int end = new Vector2Int(Mathf.FloorToInt(targetPos.x), Mathf.FloorToInt(targetPos.y));

		Vector2Int start_tempUp = pos + Vector2Int.up;
		Vector2Int end_tempUp = end + Vector2Int.up;

		m_PathFinding_ReSearchTimer.Update();

		if (m_PathFinding_Path == null
			|| m_PathFinding_Path.Count <= 0
			|| m_TargetPos != end
			|| m_PathFinding_ReSearchTimer.TimeCheck() == true)
		{
			m_TargetPos = end;

			PathFinding_FindPath(start_tempUp, end_tempUp);
		}

		if (m_PathFinding_Path == null
			|| m_PathFinding_Path.Count <= 1
			|| m_PathFinding_NodeIndex < 0)
			return;

		Vector2 prevDest, curDest, nextDest;
		bool reachedX, reachedY, destOnGround;

		PathFinding_GetContext(out prevDest, out curDest, out nextDest, out destOnGround, out reachedX, out reachedY);

		Vector2 dir = curDest - prevDest;

		moveDir = System.MathF.Sign(dir.x);
	}
	private void PathFinding_FindPath(Vector2Int start, Vector2Int end)
	{
		// 땅에 붙어 있는지 확인
		if (m_Controller.collisions.grounded == false)
			return;

		m_PathFinding_ReSearchTimer.Clear();

		Bounds bounds = m_Controller.collider.bounds;

		m_PathFinding_Path = M_Grid.FindPath(start, end, Mathf.CeilToInt(bounds.size.x), Mathf.CeilToInt(bounds.size.y), (short)m_Controller.maxJumpHeight);
		m_PathFinding_NodeIndex = 1;
	}
	private void PathFinding_GetContext(out Vector2 prevDest, out Vector2 currentDest, out Vector2 nextDest, out bool destOnGround, out bool reachedX, out bool reachedY)
	{
		// 목적지 (이전, 현재, 다음)
		prevDest = m_PathFinding_Path[m_PathFinding_NodeIndex - 1] + Vector2.down;
		currentDest = m_PathFinding_Path[m_PathFinding_NodeIndex] + Vector2Int.down;
		if (m_PathFinding_Path.Count > m_PathFinding_NodeIndex + 1)
			nextDest = m_PathFinding_Path[m_PathFinding_NodeIndex + 1] + Vector2Int.down;
		else
			nextDest = currentDest;

		Bounds bounds = m_Controller.collider.bounds;

		// 목적지 (지상 여부)
		destOnGround = false;
		for (int x = m_PathFinding_Path[m_PathFinding_NodeIndex].x; x < m_PathFinding_Path[m_PathFinding_NodeIndex].x + Mathf.CeilToInt(bounds.size.x); ++x)
		{
			if (M_Grid.map.IsGround(x, m_PathFinding_Path[m_PathFinding_NodeIndex].y - 1))
			{
				destOnGround = true;
				break;
			}
		}

		Vector2 pathPosition = bounds.center - bounds.extents;

		reachedX = (prevDest.x <= currentDest.x && currentDest.x <= pathPosition.x)
			|| (prevDest.x >= currentDest.x && currentDest.x >= pathPosition.x);

		// 스냅 부분 스킵

		reachedY = (prevDest.y <= currentDest.y && currentDest.y <= pathPosition.y)
			|| (prevDest.y >= currentDest.y && currentDest.y >= pathPosition.y)
			|| (Mathf.Abs(pathPosition.y - currentDest.y) <= 1);
	}
	private void PathFinding_NextPath(bool reachedX, bool reachedY)
	{
		if (reachedX == false)
			return;
		if (reachedY == false)
			return;

		// 현재 목적지 도달. 다음 목적지로 인덱스 변경
		++m_PathFinding_NodeIndex;

		// 최종 목적지 도달
		if (m_PathFinding_NodeIndex >= m_PathFinding_Path.Count)
		{
			m_PathFinding_Path.Clear();
			m_PathFinding_Path = null;
			m_PathFinding_NodeIndex = -1;
		}
	}
	//// 목표로 향하는 움직임
	//private void CalculateVelocity_ToTarget(ref Vector2 dir)
	//{
	//	#region 변수 생성
	//	// 현재 타일 맨 위 칸과 겹치는 상황이므로 임시로 검색할 땐 한 칸 위로 검색함(임시)
	//	// 현재 위치
	//	Vector3 pos = transform.position + Vector3.up * (1f + float.Epsilon);
	//	// 목표 위치
	//	Vector3 targetPos = target.transform.position + Vector3.up * (1f + float.Epsilon);
	//	// 현재 위치 정수 버전
	//	Vector2Int posInt = new Vector2Int((int)pos.x, (int)pos.y);
	//	// 목표 위치 정수 버전
	//	Vector2Int targetPosInt = new Vector2Int((int)targetPos.x, (int)targetPos.y);
	//	#endregion

	//	// 경로 검색
	//	FindPathToTarget(ref pos, ref targetPos, ref posInt, ref targetPosInt);

	//	// 현재 타일 맨 위 칸과 겹치는 상황이므로 임시로 검색할 땐 한 칸 위로 검색함(임시)
	//	// 한 칸 위로 설정한 것 다시 빼줌
	//	pos -= Vector3.up;
	//	posInt -= Vector2Int.up;

	//	// 경로가 없는 경우
	//	if (m_RoadToTarget == null)
	//	{
	//		// x 방향 움직임 봉인
	//		moveDir = 0;
	//		// 종료
	//		return;
	//	}

	//	// 다음 경로로의 방향 계산
	//	dir = new Vector3(m_RoadToTarget.x + 0.5f, m_RoadToTarget.y) - pos;

	//	// x 방향 움직임
	//	moveDir = System.MathF.Sign(dir.x);

	//	// 착지1
	//	// 땅에 닿은 경우
	//	if (m_Controller.collisions.grounded)
	//	{
	//		// 점프 중 아님 설정
	//		m_Jumping = false;
	//	}

	//	// 착지 조건2
	//	if (m_Jumping &&                        // 점프 중인 경우
	//		m_JumpEndPos.HasValue == true)      // 착지 지점이 있는 경우
	//	{
	//		Vector2 jumpEndPos = m_JumpEndPos.Value + Vector2.right * 0.5f - (Vector2)pos;
	//		Vector2Int dirInt = m_JumpEndPos.Value - posInt;

	//		// 착지 지점과 같은 칸에 도달한 경우
	//		if (dirInt == Vector2Int.zero &&
	//			Mathf.Abs(jumpEndPos.x) <= 0.15f)
	//		{
	//			// x 방향 움직임 봉인
	//			moveDir = 0;

	//			// 착지
	//			// 땅에 닿은 경우
	//			if (m_Controller.collisions.grounded)
	//			{
	//				// 착지 지점 날리고
	//				m_JumpEndPos = null;

	//				// 점프 중 아님 설정
	//				m_Jumping = false;
	//			}

	//			if (jumpEndPos.y < -0.15f)
	//				return;
	//		}
	//	}

	//	// 점프
	//	Jump(ref dir, ref pos);

	//	// 다음 경로 찾는 조건
	//	if (Mathf.Abs(dir.x) <= 0.1f &&     // x 좌표가 충분히 가까워졌고
	//		(Mathf.Abs(dir.y) <= 0.1f ||    // y 좌표가 충분히 가까워졌거나
	//		(m_Jumping && dir.y <= 0f)))    // 점프중이고 y 좌표가 아래에 있으면
	//	{
	//		m_PathFindTimer.Use();

	//		m_RoadToTarget = m_RoadToTarget.parent as CustomNode;
	//	}
	//}

	//// 경로 찾기
	//private void FindPathToTarget(ref Vector3 pos, ref Vector3 targetPos, ref Vector2Int posInt, ref Vector2Int targetPosInt)
	//{
	//	m_PathFindTimer.Update();

	//	#region 새로운 경로 찾는 조건 확인(예외 처리)
	//	// 땅에 붙어 있는지 확인
	//	if (m_Controller.collisions.grounded == false)
	//		return;

	//	if (!(m_RoadToTarget == null || // 경로가 아직 존재하는지 확인
	//			Vector2Int.Distance(posInt, m_RoadToTarget.position) > 3f || // 찾은 경로와 너무 멀어지면 다시 찾는 임시 조건
	//			m_TargetPos != targetPosInt || // 목표가 움직였는지 확인
	//			m_PathFindTimer.timeIsUp == true)) // 정해진 시간이 지났는지 확인
	//		return;
	//	#endregion

	//	m_PathFindTimer.Clear();

	//	// 목표 위치 업데이트
	//	m_TargetPos = targetPosInt;

	//	// 경로 검색
	//	m_RoadToTarget = M_Grid.PathFinding(pos, targetPos, (int)m_Controller.maxJumpHeight);

	//	// 착지 지점 날리고
	//	m_JumpEndPos = null;
	//	// 점프 중 아님 설정
	//	m_Jumping = false;

	//	// 경로에 시작점(현재 위치)가 포함되어 시작점 다음 위치부터 검색하도록 수정
	//	if (m_RoadToTarget != null)
	//		m_RoadToTarget = m_RoadToTarget.parent as CustomNode;
	//}
	//// 점프
	//private void Jump(ref Vector2 dir, ref Vector3 pos)
	//{
	//	#region 점프 조건 확인(예외 처리)
	//	// 공중에 떠 있는 경우
	//	if (m_Controller.collisions.isair == true)
	//		return;
	//	// 이미 점프 중인 경우
	//	if (m_Jumping == true)
	//		return;
	//	#endregion

	//	// 위로 점프 조건
	//	// 방향이 위를 향하고 있으면
	//	if (dir.y > 0f)
	//	{
	//		// 위로 점프
	//		JumpToUp(ref pos);
	//	}
	//	// 아래로 점프 조건
	//	// 방향이 아래를 향하고 있으면
	//	else if (dir.y < 0f)
	//	{
	//		// 아래로 점프
	//		JumpToDown();
	//	}
	//}
	//// 위로 점프
	//private void JumpToUp(ref Vector3 pos)
	//{
	//	CustomNode jumpNode = m_RoadToTarget;

	//	if (jumpNode == null)
	//		return;

	//	if (jumpNode.jumpStartPos.HasValue == false)
	//	{
	//		m_Velocity.y = m_Controller.minJumpVelocity;
	//		//Debug.Log("위로 점프 시작 좌표 없음");
	//		return;
	//	}

	//	// 좌우 이동을 못하면 점프도 못함
	//	if (m_CurrentStat.MoveSpeed == 0f)
	//		return;

	//	Vector2Int jumpStartPos = jumpNode.jumpStartPos.Value;

	//	while (jumpNode != null)
	//	{
	//		if (jumpNode.jumpStartPos.HasValue == false)
	//			break;

	//		if (jumpStartPos != jumpNode.jumpStartPos)
	//			break;

	//		CustomNode jumpParent = jumpNode.parent as CustomNode;

	//		if (jumpParent == null)
	//			break;

	//		jumpNode = jumpParent;
	//	}

	//	m_JumpEndPos = jumpNode.position;

	//	{
	//		// 최고점 (높이 차 + 1)
	//		float height = jumpNode.y - (int)pos.y + 1;
	//		// 중력 가속도
	//		float g = Mathf.Abs(m_Controller.gravity);
	//		// x축 전체 거리
	//		float distanceX = jumpNode.x - pos.x;

	//		// 가장 적절한 점프 높이 찾기
	//		while (height <= m_Controller.maxJumpHeight)
	//		{
	//			// 최고점 x 위치
	//			float Cx = pos.x + m_CurrentStat.MoveSpeed * distanceX * Mathf.Sqrt(2 * height / g) / Mathf.Abs(distanceX);
	//			// 최고점 y 위치
	//			float Cy = pos.y + height;

	//			// 이동할 x 거리
	//			float moveX = Mathf.Abs(Cx - jumpNode.x);
	//			// 이동할 y 거리
	//			float moveY = Mathf.Abs(Cy - jumpNode.y);

	//			// 최고점에서 목표지점까지 x축 이동 시간
	//			float xTime = moveX / m_CurrentStat.MoveSpeed;
	//			// 최고점에서 목표지점까지 y축 이동 시간
	//			float yTime = moveY / height;

	//			// y축 이동 시간이 x축 이동 시간보다 길다면
	//			// 시간 안에 목표 점에 도달하지 못한 것이므로
	//			// 현재가 최적의 점프 높이
	//			if (yTime > xTime)
	//				break;

	//			++height;
	//		}

	//		Debug.Log("점프 높이: " + height);
	//		height = Mathf.Clamp(height, 0f, m_Controller.maxJumpHeight);
	//		float Vy = Mathf.Sqrt(2 * g * height);

	//		m_Velocity.y = Vy;
	//	}

	//	//m_Velocity.y = m_Controller.maxJumpVelocity;

	//	m_Jumping = true;
	//}
	//// 아래로 점프
	//private void JumpToDown()
	//{

	//}
	#endregion
	#endregion

	public bool showPath = true;
	public float pathShowTime = 1f;
	private void OnDrawGizmos()
	{
		if (m_PathFinding_Path == null)
			return;

		if (showPath == false)
			return;

		Vector3 offset = Vector3.one * 0.5f;

		var path = m_PathFinding_Path;

		for (int i = 0; i < path.Count - 1; ++i)
		{
			Vector3 start = new Vector3(path[i].x, path[i].y);
			Vector3 end = new Vector3(path[i + 1].x, path[i + 1].y);

			Debug.DrawLine(start + offset, end + offset, Color.white, pathShowTime);

			MyDebug.DrawRect(start + offset, offset, Color.white, pathShowTime);
			var text = UtilClass.CreateWorldText(i + 1, null, start + offset, 0.1f, 40, Color.white, TextAnchor.MiddleCenter);
			GameObject.Destroy(text.gameObject, pathShowTime);

			if (i == path.Count - 2)
			{
				MyDebug.DrawRect(end + offset, offset, Color.white, pathShowTime);

				text = UtilClass.CreateWorldText(path.Count, null, end + offset, 0.1f, 40, Color.white, TextAnchor.MiddleCenter);
				GameObject.Destroy(text.gameObject, pathShowTime);
			}
		}
	}
}