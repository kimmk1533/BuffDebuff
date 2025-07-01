using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BuffDebuff
{
	public class Enemy : Character<Enemy, EnemyStat, EnemyController2D, EnemyAnimator>, IDamageGiver, IDamageTaker
	{
		const float c_BotMaxPositionError = 0.0625f; // 1 ÷ 16 (pixels per unit)

		#region Enum
		public enum E_EnemyState
		{
			Idle,
			Chase,
			Attack,

			Max
		}
		public enum E_EnemyKeyInput
		{
			None = -1,

			Left,
			Right,
			Down,
			Up,

			Max
		}
		#endregion

		#region 변수
		protected E_EnemyState m_State;

		[SerializeField, ChildComponent("TargetFinder")]
		protected EnemyTargetFinder m_TargetFinder;
		protected Vector2Int m_TargetPos;

		protected bool[] m_Inputs;
		protected bool[] m_PrevInputs;

		protected float m_JumpHeight;

		protected List<Vector2Int> m_PathFinding_Path;
		protected int m_PathFinding_NodeIndex;

		[SerializeField, ReadOnly]
		[FoldoutGroup("타이머"), LabelText("랜덤 이동 타이머")]
		protected UtilClass.Timer m_MoveDirTimer;
		[SerializeField, ReadOnly]
		[FoldoutGroup("타이머"), LabelText("재탐색 타이머")]
		protected UtilClass.Timer m_PathFinding_ReSearchTimer;

		[SerializeField]
		private Transform m_AttackSpot;
		#endregion

		#region 프로퍼티
		protected Room currentRoom => M_Stage.currentStage.currentRoom;
		public GameObject target => m_TargetFinder.target;
		public Collider2D targetCollider => m_TargetFinder.targetCollider;
		protected Vector3 targetPos => (target == null) ? throw new System.Exception("targetPos: target is null.") : target.transform.position;

		protected Bounds bounds => m_Controller.collider.bounds;
		#endregion

		#region 매니저
		private static PlayerManager M_Player => PlayerManager.Instance;
		private static EnemyManager M_Enemy => EnemyManager.Instance;
		private static ProjectileManager M_Projectile => ProjectileManager.Instance;
		private static StageManager M_Stage => StageManager.Instance;
		#endregion

		// 초기화
		public override void InitializePoolItem()
		{
			base.InitializePoolItem();

			m_Inputs = new bool[(int)E_EnemyKeyInput.Max];
			m_PrevInputs = new bool[(int)E_EnemyKeyInput.Max];

			if (m_AttackSpot == null)
				m_AttackSpot = transform.FindInChildren("AttackSpot");

			m_TargetFinder.Initialize();
			m_TargetFinder.onTargetEnter2D += (target) =>
			{
				SetState(E_EnemyState.Chase);
			};
			m_TargetFinder.onTargetLost2D += (target) =>
			{
				SetState(E_EnemyState.Idle);
			};

			m_PathFinding_Path = null;
			m_PathFinding_NodeIndex = -1;

			#region Timer
			if (m_MoveDirTimer == null)
			{
				m_MoveDirTimer = new UtilClass.Timer()
				{
					autoClear = true,
				};
			}
			m_MoveDirTimer.interval = Random.Range(0.2f, 1.0f);
			m_MoveDirTimer.Clear();

			if (m_PathFinding_ReSearchTimer != null)
				m_PathFinding_ReSearchTimer.Clear();
			else
				m_PathFinding_ReSearchTimer = new UtilClass.Timer();
			m_PathFinding_ReSearchTimer.interval = 1f;
			#endregion

			m_Animator.Initialize();
		}
		public override void FinallizePoolItem()
		{
			base.FinallizePoolItem();

			m_State = E_EnemyState.Idle;

			m_TargetFinder.Finallize();

			m_MoveDirTimer.Clear();
			m_PathFinding_ReSearchTimer.Clear();

			if (m_PathFinding_Path != null)
				m_PathFinding_Path.Clear();
			m_PathFinding_NodeIndex = -1;
		}

		protected override void Update()
		{
			base.Update();
		}

		private void ResetInput()
		{
			for (int i = 0; i < (int)E_EnemyKeyInput.Max; ++i)
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

			int moveDir;

			if (m_Inputs[(int)E_EnemyKeyInput.Left] == true &&
				m_Inputs[(int)E_EnemyKeyInput.Right] == false)
				moveDir = -1;
			else if (m_Inputs[(int)E_EnemyKeyInput.Right] == true &&
				m_Inputs[(int)E_EnemyKeyInput.Left] == false)
				moveDir = 1;
			else
				moveDir = 0;

			if (moveDir != 0)
			{
				Vector3 scale = transform.localScale;
				scale.x = Mathf.Abs(scale.x) * moveDir;
				transform.localScale = scale;
			}

			m_Velocity.x = moveDir * m_Stat.MoveSpeed;

			if (m_Inputs[(int)E_EnemyKeyInput.Up] == true)
			{
				float g = Mathf.Abs(m_Controller.gravity);
				float Vy = Mathf.Sqrt(2 * g * m_JumpHeight);
				m_Velocity.y = Vy;

				m_JumpHeight = 0;
			}
			// 중력 추가
			m_Velocity.y += m_Controller.gravity * Time.deltaTime;
		}

		// 랜덤 움직임
		private void CalculateVelocity_Random()
		{
			// 타이머 작동
			m_MoveDirTimer.Update();

			// 타이머 시간 확인
			if (m_MoveDirTimer.TimeCheck() == false)
				return;

			// 다음 움직임까지의 시간
			m_MoveDirTimer.interval = Random.Range(0.2f, 1.0f);

			// 랜덤 방향 움직임
			int moveDir = Random.Range(-1, 2);
			if (moveDir == -1)
				m_Inputs[(int)E_EnemyKeyInput.Left] = true;
			else if (moveDir == 1)
				m_Inputs[(int)E_EnemyKeyInput.Right] = true;
			else
			{
				m_Inputs[(int)E_EnemyKeyInput.Left] = false;
				m_Inputs[(int)E_EnemyKeyInput.Right] = false;
			}
		}
		private void CalculateVelocity_ToTarget()
		{
			ResetInput();

			m_PathFinding_ReSearchTimer.Update();

			Vector2 offset = -currentRoom.transform.position;

			// 콜라이더 좌하단의 칸 중앙 위치
			Vector2Int start = new Vector2Int((int)(bounds.center.x - bounds.extents.x + 0.5f + offset.x), (int)(bounds.center.y - bounds.extents.y + 0.5f + offset.y));
			Vector2Int end = new Vector2Int(Mathf.FloorToInt(targetCollider.bounds.center.x + offset.x), Mathf.FloorToInt(targetPos.y + 0.5f + offset.y));

			if (m_PathFinding_Path == null ||
				m_PathFinding_Path.Count <= 1 ||
				m_PathFinding_Path.Contains(m_TargetPos) == false ||
				m_PathFinding_ReSearchTimer.TimeCheck() == true)
			{
				if (currentRoom.pathFindingMap.IsEmpty(start.x, start.y - 1) == false &&
					currentRoom.pathFindingMap.IsEmpty(end.x, end.y - 1) == false)
				{
					Vector2Int offset_int = new Vector2Int((int)offset.x, (int)offset.y);

					PathFinding_FindPath(start, end, -offset_int);
				}
			}

			if (m_PathFinding_Path == null
				|| m_PathFinding_Path.Count <= 1)
				return;

			Vector2 pathPosition;
			Vector2 prevDest, currentDest, nextDest;
			bool reachedX, reachedY, destOnGround;

			PathFinding_GetContext(out pathPosition, out prevDest, out currentDest, out nextDest, out destOnGround, out reachedX, out reachedY);

			if (pathPosition.y - currentDest.y > c_BotMaxPositionError
				&& m_Controller.collisions.isOnOneWayPlatform == true)
				m_Inputs[(int)E_EnemyKeyInput.Down] = true;

			if (reachedX && reachedY)
			{
				PathFinding_NextPath(ref reachedX, ref reachedY);
				return;
			}
			else if (reachedX == false)
			{
				PathFinding_MoveX(ref pathPosition, ref currentDest);
			}
			else if (reachedY == false && m_PathFinding_Path.Count > m_PathFinding_NodeIndex + 1 && destOnGround == false)
			{
				int checkedX = 0;
				int tileX = Mathf.RoundToInt(pathPosition.x);
				int tileY = Mathf.RoundToInt(pathPosition.y);

				if (nextDest.x != currentDest.x)
				{
					//currentRoom.pathFindingMap.GetMapTileAtPoint(pathPosition, out tileX, out tileY);

					if (nextDest.x > currentDest.x)
						checkedX = Mathf.RoundToInt(tileX + bounds.extents.x);
					else
						checkedX = Mathf.RoundToInt(tileX - bounds.extents.x);
				}

				if (checkedX != 0 && currentRoom.pathFindingMap.AnySolidBlockInStripe(checkedX, tileY, Mathf.RoundToInt(nextDest.y)) == false)
				{
					PathFinding_MoveX(ref pathPosition, ref nextDest);

					bool temp_reachedX = PathFinding_ReachedNodeOnXAxis(ref pathPosition, ref prevDest, ref currentDest);
					bool temp_reachedY = PathFinding_ReachedNodeOnYAxis(ref pathPosition, ref prevDest, ref currentDest);

					if (reachedX && reachedY)
					{
						PathFinding_NextPath(ref temp_reachedX, ref temp_reachedY);
						return;
					}
				}
			}

			PathFinding_MoveY(ref pathPosition, ref currentDest, ref destOnGround, ref reachedX, ref reachedY);
		}
		/// <summary>
		/// 패스파인딩 하는 함수
		/// </summary>
		/// <param name="start">출발점</param>
		/// <param name="end">도착점</param>
		private void PathFinding_FindPath(Vector2Int start, Vector2Int end, Vector2Int offset)
		{
			// 땅에 붙어 있는지 확인
			if (m_Controller.collisions.grounded == false)
				return;

			m_PathFinding_ReSearchTimer.Clear();

			int width = Mathf.CeilToInt(bounds.size.x);
			int height = Mathf.CeilToInt(bounds.size.y);
			short maxJumpHeight = (short)((short)m_Controller.maxJumpHeight - 1);

			m_PathFinding_Path = currentRoom.pathFindingMap.FindPath(start, end, width, height, maxJumpHeight);

			if (m_PathFinding_Path == null
				|| m_PathFinding_Path.Count <= 1)
				return;

			m_PathFinding_NodeIndex = 1;
			m_TargetPos = m_PathFinding_Path[m_PathFinding_NodeIndex];

			// 점프 높이 계산
			m_JumpHeight = PathFinding_GetJumpHeight(m_PathFinding_NodeIndex - 1);

			#region Debug_Text
			//for (int i = 0; i < textObjectList.Count; ++i)
			//{
			//	GameObject.Destroy(textObjectList[i].gameObject);
			//}
			//textObjectList.Clear();

			//Vector3 debug_offset = Vector3.one * 0.5f;
			//for (int i = 0; i < m_PathFinding_Path.Count; ++i)
			//{
			//	Vector3 debug_textPos = (Vector2)(m_PathFinding_Path[i] + offset);
			//	debug_textPos += debug_offset;

			//	var debug_text = UtilClass.CreateWorldText(i + 1, null, debug_textPos, 0.1f, 40, textColor, TextAnchor.MiddleCenter);

			//	textObjectList.Add(debug_text);
			//}
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
				if (currentRoom.pathFindingMap.IsGround(x, (int)currentDest.y - 1))
				{
					destOnGround = true;
					break;
				}
			}

			Vector2 offset = (Vector2)currentRoom.transform.position + Vector2.one * 0.5f /* * Map.c_tileSize*/;

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
				&& Mathf.Abs(pathPosition.x - currentDest.x) > c_BotMaxPositionError
				&& Mathf.Abs(pathPosition.x - currentDest.x) < c_BotMaxPositionError * 3.0f
				&& m_PrevInputs[(int)E_EnemyKeyInput.Left] == false
				&& m_PrevInputs[(int)E_EnemyKeyInput.Right] == false)
			{
				pathPosition.x = currentDest.x;
				Vector3 pos = transform.position;
				pos.x = pathPosition.x;
				transform.position = pos;
			}

			if (destOnGround == true && (m_Controller.collisions.grounded == false && m_Controller.collisions.isOnOneWayPlatform == false))
				reachedY = false;
		}
		private bool PathFinding_ReachedNodeOnXAxis(ref Vector2 pathPosition, ref Vector2 prevDest, ref Vector2 currentDest)
		{
			return (prevDest.x < currentDest.x && currentDest.x < pathPosition.x)
				|| (prevDest.x > currentDest.x && currentDest.x > pathPosition.x)
				|| Mathf.Abs(pathPosition.x - currentDest.x) <= c_BotMaxPositionError;
		}
		private bool PathFinding_ReachedNodeOnYAxis(ref Vector2 pathPosition, ref Vector2 prevDest, ref Vector2 currentDest)
		{
			return (prevDest.y < currentDest.y && currentDest.y < pathPosition.y)
				|| (prevDest.y > currentDest.y && currentDest.y > pathPosition.y)
				|| (Mathf.Abs(pathPosition.y - currentDest.y) <= c_BotMaxPositionError);
		}
		/// <summary>
		/// X축 움직임 함수
		/// </summary>
		/// <param name="reachedX">x축 도달 여부</param>
		private void PathFinding_MoveX(ref Vector2 pathPosition, ref Vector2 currentDest)
		{
			if (currentDest.x - pathPosition.x > c_BotMaxPositionError)
				m_Inputs[(int)E_EnemyKeyInput.Right] = true;

			else if (pathPosition.x - currentDest.x > c_BotMaxPositionError)
				m_Inputs[(int)E_EnemyKeyInput.Left] = true;
		}
		/// <summary>
		/// Y축 움직임 함수
		/// </summary>
		/// <param name="pathPosition">현재 길 위 위치</param>
		/// <param name="currentDest">현재 목적지</param>
		/// <param name="destOnGround">목적지가 지상에 있는지</param>
		/// <param name="reachedY">y축 도달 여부</param>
		private void PathFinding_MoveY(ref Vector2 pathPosition, ref Vector2 currentDest, ref bool destOnGround, ref bool reachedX, ref bool reachedY)
		{
			if (reachedY)
				return;

			if (pathPosition.y - currentDest.y > c_BotMaxPositionError
				&& m_Controller.collisions.isOnOneWayPlatform == true)
				m_Inputs[(int)E_EnemyKeyInput.Down] = true;

			if (m_JumpHeight > 0 &&
					(m_Controller.collisions.grounded == false ||
					(reachedX == true && destOnGround == false) ||
					(m_Controller.collisions.grounded == true && destOnGround == true))) // 1칸 짜리 점프
				m_Inputs[(int)E_EnemyKeyInput.Up] = true;
		}
		/// <summary>
		/// Y축으로 움직일 때 얼마나 높게 점프해야하는지 계산하는 함수
		/// </summary>
		/// <param name="prevNodeId">이전 노드 인덱스</param>
		private int PathFinding_GetJumpHeight(int prevNodeId)
		{
			int currentNodeId = prevNodeId + 1;

			Vector2Int checkNode;
			Vector2Int currentNode = m_PathFinding_Path[currentNodeId];
			Vector2Int prevNode = m_PathFinding_Path[prevNodeId];

			if (currentNode.y <= prevNode.y
				|| m_Controller.collisions.grounded == false)
				return 0;

			int jumpHeight = 1;
			for (int i = currentNodeId; i < m_PathFinding_Path.Count; ++i)
			{
				checkNode = m_PathFinding_Path[i];

				int dy = checkNode.y - prevNode.y;
				if (dy >= jumpHeight)
					jumpHeight = dy;

				if (dy < jumpHeight || currentRoom.pathFindingMap.IsGround(checkNode.x, checkNode.y - 1) == true)
					break;
			}

			++jumpHeight;

			if (jumpHeight > m_Controller.maxJumpHeight)
				Debug.LogError("JumpHeight: " + jumpHeight);

			return Mathf.Clamp(jumpHeight, 1, (int)m_Controller.maxJumpHeight);
		}
		/// <summary>
		/// 현재 목적지에 도착해서 목적지를 다음 목적지로 바꾸는 함수
		/// </summary>
		/// <param name="reachedX">X축 도착 여부</param>
		/// <param name="reachedY">Y축 도착 여부</param>
		/// <returns>다음 목적지로 바꾸었는가?</returns>
		private bool PathFinding_NextPath(ref bool reachedX, ref bool reachedY)
		{
			if (reachedX == false
				|| reachedY == false)
				return false;

			// 현재 목적지 도달. 다음 목적지로 인덱스 변경
			++m_PathFinding_NodeIndex;

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
			}

			return true;
		}
		#endregion

		protected virtual bool CanAttack()
		{
			return target != null &&
				m_TargetFinder.state == EnemyTargetFinder.E_TargetFinderState.Chasing &&
				m_Controller.collisions.isair == false;
		}
		public override void Attack()
		{
			if (CanAttack() == false)
				return;

			base.Attack();
		}
		protected override void CreateProjectile()
		{
			Vector3 position = m_AttackSpot.position;

			Vector3 targetPos = targetCollider.bounds.center;
			float angle = position.GetAngle(targetPos);
			Quaternion quaternion = Quaternion.AngleAxis(angle, Vector3.forward);

			Projectile projectile = M_Projectile.GetBuilder("Projectile")
				.SetActive(true)
				.SetAutoInit(true)
				.SetParent(null)
				.SetPosition(position)
				.SetRotation(quaternion)
				.SetMoveSpeed(m_Stat.ShotSpeed)
				.SetLifeTime(m_Stat.AttackRange)
				.SetMoveType(new StraightMove())
				.Spawn();

			projectile["Player"].onEnter2D += (Collider2D collider) =>
			{
				Player player = collider.GetComponent<Player>();

				DamageArg<IDamageGiver, IDamageTaker> damageArg = new DamageArg<IDamageGiver, IDamageTaker>(
					m_Stat.AttackPower,
					this,
					player,
					projectile);

				GiveDamage(damageArg);

				M_Projectile.Despawn(projectile);
			};
			projectile["Obstacle"].onEnter2D += (Collider2D collider) =>
			{
				M_Projectile.Despawn(projectile);
			};
		}
		public virtual void GiveDamage(DamageArg<IDamageGiver, IDamageTaker> arg)
		{
			Player player = arg.damageTaker as Player;
			Projectile projectile = arg.projectile;

			if (projectile == null || player == null)
				return;

			if (projectile.gameObject.activeSelf == false ||
				player.gameObject.activeSelf == false)
				return;

			player.TakeDamage(arg.damage);
		}
		public void TakeDamage(float damage)
		{
			StatValue<float> newHp = m_Stat.Hp;
			newHp.current -= damage;

			if (newHp.current <= 0f)
				Death();

			m_Stat.Hp = newHp;
		}
		protected void Death()
		{
			float xp = m_Stat.Xp.current * m_Stat.XpScale;
			M_Player.AddXp(xp);

			M_Enemy.Despawn(this);
		}

		// Anim Event
		public override void AnimEvent_AttackStart()
		{
			base.AnimEvent_AttackStart();

			m_IsSimulating = false;
			m_Velocity.x = System.MathF.Sign(m_Velocity.x) * float.Epsilon;
		}
		public override void AnimEvent_Attacking()
		{
			base.AnimEvent_Attacking();

			CreateProjectile();
		}
		public override void AnimEvent_AttackEnd()
		{
			base.AnimEvent_AttackEnd();

			m_IsSimulating = true;
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
}