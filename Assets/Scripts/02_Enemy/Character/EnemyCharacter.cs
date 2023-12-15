using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCharacter : Character<EnemyCharacterStat, EnemyController2D, EnemyAnimator>
{
	const float cBotMaxPositionError = 0.0625f; // 1 ÷ 16(pixels per unit)

	#region Enum
	public enum E_EnemyState
	{
		Idle,
		Chase,
		Attack,

		Max
	}
	public enum E_KeyInput
	{
		None = -1,

		Left,
		Right,
		Down,
		Up,

		Max
	}
	#endregion

	protected E_EnemyState m_State;

	[SerializeField, ChildComponent("TargetFinder")]
	protected EnemyTargetFinder m_TargetFinder;
	protected Vector2Int m_TargetPos;

	protected bool[] m_Inputs;
	protected bool[] m_PrevInputs;

	protected List<Vector2Int> m_PathFinding_Path;
	protected int m_PathFinding_NodeIndex;

	protected float m_JumpHeight;

	[SerializeField]
	protected int m_MoveDir;
	[SerializeField]
	protected UtilClass.Timer m_MoveDirTimer;

	[SerializeField]
	protected UtilClass.Timer m_PathFinding_ReSearchTimer;

	public GameObject target => m_TargetFinder.target;
	public Collider2D targetCollider => m_TargetFinder.targetCollider;
	protected Vector3 targetPos => (target == null) ? throw new System.Exception("targetPos: target is null.") : target.transform.position;
	protected float distanceToTarget => (target == null) ? throw new System.Exception("distanceToTarget: target is null.") : Vector2.Distance(transform.position, target.transform.position);

	protected int moveDir
	{
		get => m_MoveDir;
		set
		{
			m_MoveDir = System.Math.Sign(value);

			if (m_MoveDir == 0)
				return;

			Vector3 scale = transform.localScale;
			scale.x = Mathf.Abs(scale.x) * m_MoveDir;
			transform.localScale = scale;
		}
	}
	protected Bounds bounds => m_Controller.collider.bounds;

	private static GridManager M_Grid => GridManager.Instance;

	public override void Initialize()
	{
		base.Initialize();

		m_Inputs = new bool[(int)E_KeyInput.Max];
		m_PrevInputs = new bool[(int)E_KeyInput.Max];

		m_TargetFinder.Initialize();
		m_TargetFinder.onTargetEnter += () =>
		{
			SetState(E_EnemyState.Chase);
		};
		m_TargetFinder.onTargetLost += () =>
		{
			SetState(E_EnemyState.Idle);
		};

		m_PathFinding_Path = null;
		m_PathFinding_NodeIndex = -1;

		#region Timer
		m_MoveDirTimer = new UtilClass.Timer();
		m_MoveDirTimer.interval = Random.Range(0.2f, 1.0f);
		m_MoveDirTimer.Clear();

		m_PathFinding_ReSearchTimer = new UtilClass.Timer();
		m_PathFinding_ReSearchTimer.interval = 1f;
		m_PathFinding_ReSearchTimer.Clear();

		m_HealTimer = new UtilClass.Timer(m_CurrentStat.HpRegenTime);
		m_AttackTimer = new UtilClass.Timer(1f / m_CurrentStat.AttackSpeed, true);
		#endregion
	}
	protected override void Update()
	{
		base.Update();

		if (target != null && CanAttack())
			AnimEvent_Attacking(); // 임시. 공격 애니메이션(Attack 함수)으로 수정해야 함

		if (m_IsSimulating == false)
			return;

		Move();
	}

	private void ResetInput()
	{
		for (int i = 0; i < (int)E_KeyInput.Max; ++i)
		{
			m_PrevInputs[i] = m_Inputs[i];
			m_Inputs[i] = false;
		}
	}

	public void SetState(E_EnemyState state)
	{
		m_State = state;
	}

	#region 이동
	// 이동
	protected override void Move()
	{
		// 속도 계산
		CalculateVelocity();

		// 이동
		m_Controller.Move(m_Velocity * Time.deltaTime, m_Inputs);

		if (m_Controller.collisions.above || m_Controller.collisions.below)
		{
			m_Velocity.y = 0;
		}

		m_Animator.Anim_SetVelocity(m_Velocity);
		m_Animator.Anim_SetIsGround(m_Controller.collisions.grounded);
	}
	// 속도 계산
	protected override void CalculateVelocity()
	{
		switch (m_State)
		{
			// 목표가 없을 때
			case E_EnemyState.Idle:
				// 랜덤 움직임
				CalculateVelocity_Random();
				break;
			// 목표가 있을 때
			case E_EnemyState.Chase:
				// 목표로 향하는 움직임
				CalculateVelocity_ToTarget();
				break;
		}

		m_Velocity.x = moveDir * m_CurrentStat.MoveSpeed;
		// 중력 추가
		m_Velocity.y += m_Controller.gravity * Time.deltaTime;
	}

	// 랜덤 움직임
	private void CalculateVelocity_Random()
	{
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
	private void CalculateVelocity_ToTarget()
	{
		moveDir = 0;

		m_PathFinding_ReSearchTimer.Update();

		// 콜라이더 좌하단의 칸 중앙 위치
		Vector2Int start = new Vector2Int((int)(bounds.center.x - bounds.extents.x + 0.5f), (int)(bounds.center.y - bounds.extents.y + 0.5f));
		Vector2Int end = new Vector2Int(Mathf.FloorToInt(targetCollider.bounds.center.x), Mathf.FloorToInt(targetPos.y + 0.5f));

		if (m_PathFinding_Path == null ||
			m_PathFinding_Path.Count <= 1 ||
			m_PathFinding_Path.Contains(m_TargetPos) == false ||
			m_PathFinding_ReSearchTimer.TimeCheck() == true)
		{
			if (M_Grid.map.IsEmpty(start.x, start.y - 1) == false &&
				M_Grid.map.IsEmpty(end.x, end.y - 1) == false)
			{
				PathFinding_FindPath(start, end);
			}
		}

		if (m_PathFinding_Path == null
			|| m_PathFinding_Path.Count <= 1)
			return;

		Vector2 pathPosition;
		Vector2 prevDest, currentDest, nextDest;
		bool reachedX, reachedY, destOnGround;

		PathFinding_GetContext(out pathPosition, out prevDest, out currentDest, out nextDest, out destOnGround, out reachedX, out reachedY);

		ResetInput();

		if (pathPosition.y - currentDest.y > cBotMaxPositionError
			&& m_Controller.collisions.isOnOneWayPlatform == true)
			m_Inputs[(int)E_KeyInput.Down] = true;

		if (reachedX && reachedY)
		{
			PathFinding_NextPath(ref pathPosition, ref reachedX, ref reachedY);
			return;
		}
		else if (reachedX == false)
		{
			if (currentDest.x - pathPosition.x > cBotMaxPositionError)
			{
				m_Inputs[(int)E_KeyInput.Right] = true;
			}
			else if (pathPosition.x - currentDest.x > cBotMaxPositionError)
			{
				m_Inputs[(int)E_KeyInput.Left] = true;
			}
		}
		else if (reachedY == false && m_PathFinding_Path.Count > m_PathFinding_NodeIndex + 1 && destOnGround == false)
		{
			int checkedX = 0;
			int tileX = Mathf.RoundToInt(pathPosition.x);
			int tileY = Mathf.RoundToInt(pathPosition.y);

			if (nextDest.x != currentDest.x)
			{
				//M_Grid.map.GetMapTileAtPoint(pathPosition, out tileX, out tileY);

				if (nextDest.x > currentDest.x)
					checkedX = Mathf.RoundToInt(tileX + bounds.extents.x);
				else
					checkedX = Mathf.RoundToInt(tileX - bounds.extents.x);
			}

			if (checkedX != 0 && M_Grid.map.AnySolidBlockInStripe(checkedX, tileY, Mathf.RoundToInt(nextDest.y)) == false)
			{
				if (nextDest.x - pathPosition.x > cBotMaxPositionError)
					m_Inputs[(int)E_KeyInput.Right] = true;
				else if (pathPosition.x - nextDest.x > cBotMaxPositionError)
					m_Inputs[(int)E_KeyInput.Left] = true;

				bool temp_reachedX = PathFinding_ReachedNodeOnXAxis(ref pathPosition, ref prevDest, ref currentDest);
				bool temp_reachedY = PathFinding_ReachedNodeOnYAxis(ref pathPosition, ref prevDest, ref currentDest);

				if (reachedX && reachedY)
				{
					PathFinding_NextPath(ref pathPosition, ref temp_reachedX, ref temp_reachedY);
					return;
				}
			}
		}

		PathFinding_MoveX(ref pathPosition, ref currentDest, ref reachedX);
		PathFinding_MoveY(ref pathPosition, ref currentDest, ref destOnGround, ref reachedX, ref reachedY);

	}
	/// <summary>
	/// 패스파인딩 하는 함수
	/// </summary>
	/// <param name="start">출발점</param>
	/// <param name="end">도착점</param>
	private void PathFinding_FindPath(Vector2Int start, Vector2Int end)
	{
		// 땅에 붙어 있는지 확인
		if (m_Controller.collisions.grounded == false)
			return;

		m_PathFinding_ReSearchTimer.Clear();

		int width = Mathf.CeilToInt(bounds.size.x);
		int height = Mathf.CeilToInt(bounds.size.y);
		short maxJumpHeight = (short)((short)m_Controller.maxJumpHeight - 1);

		m_PathFinding_Path = M_Grid.map.FindPath(start, end, width, height, maxJumpHeight);

		if (m_PathFinding_Path == null
			|| m_PathFinding_Path.Count <= 1)
			return;

		m_PathFinding_NodeIndex = 1;
		m_TargetPos = m_PathFinding_Path[m_PathFinding_NodeIndex];

		// 점프 높이 계산
		m_JumpHeight = PathFinding_GetJumpHeight(m_PathFinding_NodeIndex - 1);

		#region Debug_Text
		for (int i = 0; i < textObjectList.Count; ++i)
		{
			GameObject.Destroy(textObjectList[i].gameObject);
		}
		textObjectList.Clear();

		Vector3 offset = Vector3.one * 0.5f;
		for (int i = 0; i < m_PathFinding_Path.Count; ++i)
		{
			Vector3 textPos = (Vector2)m_PathFinding_Path[i];
			textPos += offset;

			var text = UtilClass.CreateWorldText(i + 1, textObjectParent.transform, textPos, 0.1f, 40, textColor, TextAnchor.MiddleCenter);

			textObjectList.Add(text);
		}
		#endregion
	}
	/// <summary>
	/// 목적지 관련 변수들 값 받아오는 함수
	/// </summary>
	/// <param name="pathPosition">현재 길 위 위치</param>
	/// <param name="prevDest">이전 목적지</param>
	/// <param name="currentDest">현재 목적지</param>
	/// <param name="nextDest">다음 목적지</param>
	/// <param name="destOnGround">목적지가 지상에 있는지</param>
	/// <param name="reachedX">x축 도달 여부</param>
	/// <param name="reachedY">y축 도달 여부</param>
	private void PathFinding_GetContext(out Vector2 pathPosition, out Vector2 prevDest, out Vector2 currentDest, out Vector2 nextDest, out bool destOnGround, out bool reachedX, out bool reachedY)
	{
		// 목적지 (이전, 현재, 다음)
		prevDest = m_PathFinding_Path[m_PathFinding_NodeIndex - 1];
		currentDest = m_PathFinding_Path[m_PathFinding_NodeIndex];
		if (m_PathFinding_Path.Count > m_PathFinding_NodeIndex + 1)
			nextDest = m_PathFinding_Path[m_PathFinding_NodeIndex + 1];
		else
			nextDest = currentDest;

		// 목적지 (지상 여부)
		destOnGround = false;
		for (int x = (int)currentDest.x; x < (int)currentDest.x + Mathf.CeilToInt(bounds.size.x); ++x)
		{
			if (M_Grid.map.IsGround(x, (int)currentDest.y - 1))
			{
				destOnGround = true;
				break;
			}
		}

		Vector2 offset = Vector2.one * 0.5f /* * Map.c_tileSize*/;

		// 콜라이더 좌하단의 칸 중앙 위치
		//Vector2Int tempPos = new Vector2Int((int)(bounds.center.x - bounds.extents.x), (int)(bounds.center.y - bounds.extents.y));
		Vector2 tempPos = transform.position;
		pathPosition = tempPos + Vector2.up * 0.5f;

		// offset 적용
		prevDest += offset;
		currentDest += offset;
		nextDest += offset;

		// X축 도착 확인
		reachedX = PathFinding_ReachedNodeOnXAxis(ref pathPosition, ref prevDest, ref currentDest);
		// Y축 도착 확인
		reachedY = PathFinding_ReachedNodeOnYAxis(ref pathPosition, ref prevDest, ref currentDest);

		// X축 스냅
		if (reachedX
			&& Mathf.Abs(pathPosition.x - currentDest.x) > cBotMaxPositionError
			&& Mathf.Abs(pathPosition.x - currentDest.x) < cBotMaxPositionError * 3.0f
			&& m_PrevInputs[(int)E_KeyInput.Left] == false
			&& m_PrevInputs[(int)E_KeyInput.Right] == false)
		{
			pathPosition.x = currentDest.x;
			Vector3 pos = transform.position;
			pos.x = pathPosition.x;
			transform.position = pos;

			moveDir = 0;
		}

		if (destOnGround == true && (m_Controller.collisions.grounded == false && m_Controller.collisions.isOnOneWayPlatform == false))
			reachedY = false;
	}
	private bool PathFinding_ReachedNodeOnXAxis(ref Vector2 pathPosition, ref Vector2 prevDest, ref Vector2 currentDest)
	{
		return (prevDest.x < currentDest.x && currentDest.x < pathPosition.x)
			|| (prevDest.x > currentDest.x && currentDest.x > pathPosition.x)
			|| Mathf.Abs(pathPosition.x - currentDest.x) <= cBotMaxPositionError;
	}
	private bool PathFinding_ReachedNodeOnYAxis(ref Vector2 pathPosition, ref Vector2 prevDest, ref Vector2 currentDest)
	{
		return (prevDest.y < currentDest.y && currentDest.y < pathPosition.y)
			|| (prevDest.y > currentDest.y && currentDest.y > pathPosition.y)
			|| (Mathf.Abs(pathPosition.y - currentDest.y) <= cBotMaxPositionError);
	}
	/// <summary>
	/// X축 움직임 함수
	/// </summary>
	/// <param name="pathPosition">현재 길 위 위치</param>
	/// <param name="prevDest">이전 목적지</param>
	/// <param name="currentDest">현재 목적지</param>
	/// <param name="nextDest">다음 목적지</param>
	/// <param name="destOnGround">목적지가 지상에 있는지</param>
	/// <param name="reachedX">x축 도달 여부</param>
	private void PathFinding_MoveX(ref Vector2 pathPosition, ref Vector2 currentDest, ref bool reachedX)
	{
		if (reachedX == true)
			return;

		if (m_Inputs[(int)E_KeyInput.Left])
		{
			moveDir = -1;
		}
		else if (m_Inputs[(int)E_KeyInput.Right])
		{
			moveDir = 1;
		}
	}
	/// <summary>
	/// Y축 움직임 함수
	/// </summary>
	/// <param name="pathPosition">현재 길 위 위치</param>
	/// <param name="prevDest">이전 목적지</param>
	/// <param name="currentDest">현재 목적지</param>
	/// <param name="nextDest">다음 목적지</param>
	/// <param name="destOnGround">목적지가 지상에 있는지</param>
	/// <param name="reachedY">y축 도달 여부</param>
	private void PathFinding_MoveY(ref Vector2 pathPosition, ref Vector2 currentDest, ref bool destOnGround, ref bool reachedX, ref bool reachedY)
	{
		if (reachedY)
			return;

		if (pathPosition.y - currentDest.y > cBotMaxPositionError
			&& m_Controller.collisions.isOnOneWayPlatform == true)
			m_Inputs[(int)E_KeyInput.Down] = true;

		if (m_JumpHeight > 0 &&
				(m_Controller.collisions.grounded == false ||
				(reachedX == true && destOnGround == false) ||
				(m_Controller.collisions.grounded == true && destOnGround == true))) // 1칸 짜리 점프
			m_Inputs[(int)E_KeyInput.Up] = true;

		if (m_Inputs[(int)E_KeyInput.Up] == true)
		{
			float g = Mathf.Abs(m_Controller.gravity);
			float Vy = Mathf.Sqrt(2 * g * m_JumpHeight);
			m_Velocity.y = Vy;

			m_JumpHeight = 0;
		}
	}
	/// <summary>
	/// Y축으로 움직일 때 얼마나 높게 점프해야하는지 계산하는 함수
	/// </summary>
	/// <param name="prevDest"></param>
	/// <param name="currentDest"></param>
	private int PathFinding_GetJumpHeight(int prevNodeId)
	{
		int currentNodeId = prevNodeId + 1;

		Vector2Int currentNode = m_PathFinding_Path[currentNodeId];
		Vector2Int prevNode = m_PathFinding_Path[prevNodeId];

		if (currentNode.y <= prevNode.y
			|| m_Controller.collisions.grounded == false)
			return 0;

		int jumpHeight = 1;
		for (int i = currentNodeId; i < m_PathFinding_Path.Count; ++i)
		{
			int dy = m_PathFinding_Path[i].y - prevNode.y;
			if (dy >= jumpHeight)
				jumpHeight = dy;

			if (dy < jumpHeight || M_Grid.map.IsGround(m_PathFinding_Path[i].x, m_PathFinding_Path[i].y - 1) == true)
				break;
		}

		++jumpHeight;

		if (jumpHeight > m_Controller.maxJumpHeight)
			Debug.LogError("JumpHeight: " + jumpHeight);

		Debug.Log("JumpHeight: " + jumpHeight);

		return Mathf.Clamp(jumpHeight, 1, (int)m_Controller.maxJumpHeight);
	}
	private bool PathFinding_NextPath_IsJumpable(ref Vector2 pathPosition)
	{
		if (m_JumpHeight <= 0)
			return false;

		bool jumpFlag = true;
		for (int y = (int)pathPosition.y; y < pathPosition.y + Mathf.CeilToInt(bounds.size.y) + m_JumpHeight; ++y)
		{
			if (M_Grid.map.IsBlock((int)pathPosition.x, y) == true)
			{
				jumpFlag = false;
				break;
			}
		}

		// X축 이동속도가 충분한 지 확인 해야 함.

		return jumpFlag;
	}
	/// <summary>
	/// 현재 목적지에 도착해서 목적지를 다음 목적지로 바꾸는 함수
	/// </summary>
	/// <param name="reachedX">X축 도착 여부</param>
	/// <param name="reachedY">Y축 도착 여부</param>
	/// <returns>다음 목적지로 바꾸었는가?</returns>
	private bool PathFinding_NextPath(ref Vector2 pathPosition, ref bool reachedX, ref bool reachedY)
	{
		if (reachedX == false
			|| reachedY == false)
			return false;

		// 현재 목적지 도달. 다음 목적지로 인덱스 변경
		++m_PathFinding_NodeIndex;

		Debug.Log("Current Node Index: " + m_PathFinding_NodeIndex);

		// 최종 목적지 도달
		if (m_PathFinding_NodeIndex >= m_PathFinding_Path.Count)
		{
			m_PathFinding_Path.Clear();
			m_PathFinding_Path = null;
			m_PathFinding_NodeIndex = -1;
			return true;
		}

		// 현재 목적지 위치 저장
		m_TargetPos = m_PathFinding_Path[m_PathFinding_NodeIndex];

		// 점프 높이 계산
		if (m_Controller.collisions.grounded == true)
		{
			m_JumpHeight = PathFinding_GetJumpHeight(m_PathFinding_NodeIndex - 1);

			//if (PathFinding_NextPath_IsJumpable(ref pathPosition))
			//{
			//	float g = Mathf.Abs(m_Controller.gravity);
			//	float Vy = Mathf.Sqrt(2 * g * m_JumpHeight);
			//	m_Velocity.y = Vy;
			//}
		}

		return true;
	}
	#endregion

	[SerializeField]
	private bool showPath = true;
	[SerializeField]
	private Color textColor = Color.white;
	[SerializeField, ReadOnly(true)]
	private GameObject textObjectParent;
	[SerializeField, ReadOnly]
	private List<TextMesh> textObjectList = new List<TextMesh>();
	private void OnDrawGizmos()
	{
		if (textObjectList == null)
			return;

		if (showPath == false)
			return;

		Vector3 offset = Vector3.zero; //Vector3.one * 0.5f;
		Vector3 size = Vector3.one * 0.5f;

		var path = textObjectList;

		for (int i = 1; i < path.Count; ++i)
		{
			Vector3 start = new Vector3(path[i - 1].transform.position.x, path[i - 1].transform.position.y);
			Vector3 end = new Vector3(path[i].transform.position.x, path[i].transform.position.y);

			Debug.DrawLine(start + offset, end + offset, textColor);

			MyDebug.DrawRect(start + offset, size, textColor);

			if (i == path.Count - 1)
			{
				MyDebug.DrawRect(end + offset, size, textColor);
			}
		}
	}
	private void OnDisable()
	{
		int count = textObjectList.Count;
		for (int i = 0; i < count; ++i)
		{
			if (textObjectList[i] == null)
				continue;

			GameObject.Destroy(textObjectList[i].gameObject);
		}
		textObjectList.Clear();
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