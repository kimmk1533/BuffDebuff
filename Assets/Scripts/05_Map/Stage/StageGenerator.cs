using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using E_Condition = RoomManager.E_Condition;
using E_Direction = WarpPoint.E_Direction;

// 아이작 방식 맵 생성
// 참고: https://minpro-tech.tistory.com/106
public class StageGenerator : MonoBehaviour
{
	// 워프 이후의 방향
	private static readonly E_Direction[] c_DirectionAfterWarp = { E_Direction.Down, E_Direction.Up, E_Direction.Right, E_Direction.Left };
	// WarpPoint.E_Direction 의 순서로 고정
	private static readonly Vector2Int[] c_Direction = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

	[SerializeField, Min(1.0f)]
	private float m_StageMagnification = 2.6f;

	[SerializeField]
	private bool m_AutoClear = true;

	private int m_MaxRoomCount;
	private int m_StageRoomCount;
	private int m_GeneratedRoomCount;

	private Vector2Int m_StageSize;

	private bool[,] m_StageRoomCheck;
	private Queue<Vector2Int> m_StageRoomQueue;
	private Dictionary<Vector2Int, Room> m_GeneratedRoomMap;

	private CameraFollow m_CameraFollow;

	private StageManager M_Stage => StageManager.Instance;
	private RoomManager M_Room => RoomManager.Instance;

	public void Initialize(int width, int height)
	{
		Initialize(new Vector2Int(width, height));
	}
	public void Initialize(Vector2Int size)
	{
		m_StageSize = size;

		m_StageRoomCount = 0;
		m_GeneratedRoomCount = 0;

		m_StageRoomCheck = new bool[size.y, size.x];
		m_StageRoomQueue = new Queue<Vector2Int>();
		m_GeneratedRoomMap = new Dictionary<Vector2Int, Room>();

		m_CameraFollow = Camera.main.GetComponent<CameraFollow>();

		ClearStage();
	}

	[ContextMenu("스테이지 생성")]
	public Stage GenerateStage()
	{
		if (m_AutoClear)
			ClearStage();

		Stage stage = new GameObject("Stage " + M_Stage.currentStageLevel.ToString("00")).AddComponent<Stage>();
		stage.transform.SetParent(M_Stage.stageParent);

		Vector2Int checkPos;
		Vector2Int current;

		ResetRoomCheck(M_Stage.currentStageLevel, ref stage);

		while (m_StageRoomCount < m_MaxRoomCount)
		{
			if (m_StageRoomQueue.Count == 0)
			{
				ResetRoomCheck(M_Stage.currentStageLevel, ref stage);
				Debug.Log("맵 재생성");
			}

			current = m_StageRoomQueue.Dequeue();

			for (int i = 0; i < c_Direction.Length; ++i)
			{
				checkPos = current + c_Direction[i];

				UpdateRoomCheck(checkPos);
			}

			InfiniteLoopDetector.Run();
		}

		#region Debug
		string debugingLine = "";

		for (int i = 0; i < m_StageRoomCheck.GetLength(1) + 2; ++i)
			debugingLine += "＝";
		debugingLine += "\n";

		for (int y = m_StageRoomCheck.GetLength(0) - 1; y >= 0; --y)
		{
			debugingLine += "＝";
			for (int x = 0; x < m_StageRoomCheck.GetLength(1); ++x)
			{
				debugingLine += (m_StageRoomCheck[y, x] == true) ? "ㅁ" : "　";
			}
			debugingLine += "＝\n";
		}

		for (int i = 0; i < m_StageRoomCheck.GetLength(1) + 2; ++i)
			debugingLine += "＝";
		Debug.Log(debugingLine);
		#endregion

		for (int y = 0; y < m_StageRoomCheck.GetLength(0); ++y)
		{
			for (int x = 0; x < m_StageRoomCheck.GetLength(1); ++x)
			{
				if (m_StageRoomCheck[y, x] == false)
					continue;

				current = new Vector2Int(x, y);

				GenerateRoom(current, ref stage);
			}
		}

		Vector2Int center = m_StageSize / 2;

		Room room = m_GeneratedRoomMap[center];

		m_CameraFollow.clampOffset = room.offset;
		m_CameraFollow.clampAreaSize = room.roomSize;

		stage.Initialize(M_Stage.currentStageLevel, M_Stage.stageSize, m_GeneratedRoomMap);
		return stage;
	}
	[ContextMenu("스테이지 제거")]
	public void ClearStage()
	{
		int count = M_Stage.stageParent.childCount;
		for (int i = 0; i < count; ++i)
		{
			GameObject.Destroy(M_Stage.stageParent.GetChild(0).gameObject);
		}
	}

	public bool CheckRoomGenerated(int x, int y)
	{
		return m_StageRoomCheck[y, x];
	}
	public bool CheckRoomGenerated(Vector2Int check)
	{
		return m_StageRoomCheck[check.y, check.x];
	}

	private bool CheckRoomCondition(Vector2Int roomPos)
	{
		// 이미 충분한 방이 채워졌으면 포기
		if (m_StageRoomCount >= m_MaxRoomCount)
			return false;

		// 인덱스 체크
		if (roomPos.x < 0 || roomPos.x >= M_Stage.stageSize.x ||
			roomPos.y < 0 || roomPos.y >= M_Stage.stageSize.y)
			return false;

		// 이미 채워져 있으면 포기
		if (m_StageRoomCheck[roomPos.y, roomPos.x])
			return false;

		// 인접 칸이 둘 이상 채워져 있으면 포기
		int count = 0;
		Vector2Int check = roomPos + Vector2Int.left;
		if (!(
			check.x < 0 || check.x >= M_Stage.stageSize.x ||
			check.y < 0 || check.y >= M_Stage.stageSize.y
			) &&
			m_StageRoomCheck[check.y, check.x])
		{
			if (++count > 2)
				return false;
		}
		check = roomPos + Vector2Int.right;
		if (!(
			check.x < 0 || check.x >= M_Stage.stageSize.x ||
			check.y < 0 || check.y >= M_Stage.stageSize.y
			) &&
			m_StageRoomCheck[check.y, check.x])
		{
			if (++count > 2)
				return false;
		}
		check = roomPos + Vector2Int.up;
		if (!(
			check.x < 0 || check.x >= M_Stage.stageSize.x ||
			check.y < 0 || check.y >= M_Stage.stageSize.y
			) &&
			m_StageRoomCheck[check.y, check.x])
		{
			if (++count > 2)
				return false;
		}
		check = roomPos + Vector2Int.down;
		if (!(
			check.x < 0 || check.x >= M_Stage.stageSize.x ||
			check.y < 0 || check.y >= M_Stage.stageSize.y
			) &&
			m_StageRoomCheck[check.y, check.x])
		{
			if (++count > 2)
				return false;
		}

		// 50% 확률로 포기
		if (Random.Range(0, 2) == 0)
			return false;

		return true;
	}
	private void UpdateRoomCheck(Vector2Int roomPos)
	{
		if (CheckRoomCondition(roomPos) == false)
			return;

		int x = roomPos.x;
		int y = roomPos.y;

		++m_StageRoomCount;

		m_StageRoomCheck[y, x] = true;
		m_StageRoomQueue.Enqueue(roomPos);
		m_GeneratedRoomMap.Add(roomPos, null);
	}
	private void GenerateRoom(Vector2Int roomPos, ref Stage stage)
	{
		// 이미 생성한 곳
		if (m_GeneratedRoomMap.TryGetValue(roomPos, out Room curRoom) == true &&
			curRoom != null)
			return;

		#region 조건 확인
		List<(E_Condition condition, E_Direction direction, int count)> conditionList = new List<(E_Condition condition, E_Direction direction, int count)>();

		for (int i = 0; i < c_Direction.Length; ++i)
		{
			E_Condition condition;
			E_Direction direction = (E_Direction)i;
			int count;

			Vector2Int nearRoomPos = roomPos + c_Direction[i];

			if (m_GeneratedRoomMap.TryGetValue(nearRoomPos, out Room nearRoom) == false)
			{
				condition = E_Condition.Equal;

				count = 0;
			}
			else if (nearRoom != null)
			{
				condition = E_Condition.Equal;

				E_Direction nearRoomWarpDir = c_DirectionAfterWarp[i];
				count = nearRoom.GetWarpPointCount(nearRoomWarpDir);
			}
			else
			{
				condition = E_Condition.More;

				count = 1;
			}

			conditionList.Add((condition, direction, count));
		}

		for (int i = 0; i < conditionList.Count; ++i)
		{
			Debug.Log((m_GeneratedRoomCount + 1).ToString("00_") + i.ToString("00: ") + conditionList[i].ToString());
		}
		#endregion

		Room room = M_Room.SpawnRandomRoom(conditionList.ToArray());
		if (room == null)
			throw new System.Exception("Error: 조건에 맞는 방이 없음");

		++m_GeneratedRoomCount;

		int x = roomPos.x;
		int y = roomPos.y;

		Vector2Int center = M_Stage.stageSize / 2;
		Vector3 pos = new Vector3((x - center.x) * 100, (y - center.y) * 100);

		room.transform.SetParent(stage.transform);
		room.transform.position = pos;
		room.name = m_GeneratedRoomCount.ToString("00_") + room.name;
		room.gameObject.SetActive(true);
		room.Initialize();

		m_GeneratedRoomMap[roomPos] = room;
	}
	private void ResetRoomCheck(int stageLevel, ref Stage stage)
	{
		m_MaxRoomCount = Random.Range(0, 2) + 5 + (int)(stageLevel * m_StageMagnification);

		Vector2Int center = m_StageSize / 2;

		m_StageRoomQueue.Clear();
		m_StageRoomQueue.Enqueue(center);

		for (int y = 0; y < m_StageSize.y; ++y)
		{
			for (int x = 0; x < m_StageSize.x; ++x)
			{
				m_StageRoomCheck[y, x] = false;
			}
		}
		m_StageRoomCheck[center.y, center.x] = true;
		m_StageRoomCount = 1;
		m_GeneratedRoomCount = 0;

		var rooms = stage.GetComponentsInChildren<Room>();
		for (int i = 0; i < rooms.Length; ++i)
		{
			M_Room.Despawn(rooms[i]);

			string[] splitedName = rooms[i].name.Split('_');
			rooms[i].name = splitedName[splitedName.Length - 1];
		}

		m_GeneratedRoomMap.Clear();
		m_GeneratedRoomMap.Add(center, null);
	}

	#region Debuging Info
	[Space(10)]
	[SerializeField, ReadOnly]
	private Info m_Info;

	public void OnValidate()
	{
		m_Info.MinExpectedRoomCount = 0 + 5 + (int)(M_Stage.currentStageLevel * m_StageMagnification);
		m_Info.MaxExpectedRoomCount = 1 + 5 + (int)(M_Stage.currentStageLevel * m_StageMagnification);
		m_Info.MaxRoomCount = m_MaxRoomCount;
	}

	[System.Serializable]
	struct Info
	{
		public int MinExpectedRoomCount;
		public int MaxExpectedRoomCount;
		public int MaxRoomCount;
	}
	#endregion
}