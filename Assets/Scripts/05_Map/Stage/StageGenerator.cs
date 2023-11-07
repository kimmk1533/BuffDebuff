using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using E_Direction = WarpPoint.E_Direction;

// 아이작 방식 맵 생성
// 참고: https://minpro-tech.tistory.com/106
public class StageGenerator : MonoBehaviour
{
	[SerializeField, Min(1.0f)]
	private float m_StageMagnification = 2.6f;

	[SerializeField]
	private bool m_AutoClear = true;

	private bool[,] m_StageRoomCheck;
	private Dictionary<Vector2Int, Room> m_GeneratedRoomMap;
	// 워프 이후의 방향
	private readonly E_Direction[] m_DirectionAfterWarp = { E_Direction.Down, E_Direction.Up, E_Direction.Right, E_Direction.Left };
	// WarpPoint.E_Direction 의 순서로 고정
	private readonly Vector2Int[] m_Direction = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

	private int m_MaxRoomCount;
	private int m_StageRoomCount;
	private Queue<Vector2Int> m_StageRoomQueue = new Queue<Vector2Int>();

	private CameraFollow m_CameraFollow;

	private StageManager M_Stage => StageManager.Instance;
	private RoomManager M_Room => RoomManager.Instance;

	public void Initialize()
	{
		m_StageRoomCheck = new bool[M_Stage.stageHeight, M_Stage.stageWidth];
		m_GeneratedRoomMap = new Dictionary<Vector2Int, Room>();

		m_CameraFollow = Camera.main.GetComponent<CameraFollow>();

		ClearStage();
	}

	public void GenerateStage(out Stage stage)
	{
		if (m_AutoClear)
			ClearStage();

		stage = new GameObject("Stage " + M_Stage.currentStageLevel).AddComponent<Stage>();
		stage.transform.SetParent(M_Stage.stageParent);

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

			for (int i = 0; i < m_Direction.Length; ++i)
			{
				GenerateRoom(current + m_Direction[i], ref stage);
				//if (GenerateRoom(current + m_Direction_Vector[i]) == false)
				//	return false;
			}
		}

		stage.Initialize(M_Stage.currentStageLevel, M_Stage.stageSize, m_GeneratedRoomMap);
	}
	[ContextMenu("맵 제거")]
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

	private bool CheckRoom(Vector2Int room)
	{
		// 이미 충분한 방이 채워졌으면 포기
		if (m_StageRoomCount >= m_MaxRoomCount)
			return false;

		// 인덱스 체크
		if (room.x < 0 || room.x >= M_Stage.stageSize.x ||
			room.y < 0 || room.y >= M_Stage.stageSize.y)
			return false;

		// 이미 채워져 있으면 포기
		if (m_StageRoomCheck[room.y, room.x])
			return false;

		// 인접 칸이 둘 이상 채워져 있으면 포기
		int count = 0;
		Vector2Int check = room + Vector2Int.left;
		if (!(
			check.x < 0 || check.x >= M_Stage.stageSize.x ||
			check.y < 0 || check.y >= M_Stage.stageSize.y
			) &&
			m_StageRoomCheck[check.y, check.x])
		{
			if (++count > 2)
				return false;
		}
		check = room + Vector2Int.right;
		if (!(
			check.x < 0 || check.x >= M_Stage.stageSize.x ||
			check.y < 0 || check.y >= M_Stage.stageSize.y
			) &&
			m_StageRoomCheck[check.y, check.x])
		{
			if (++count > 2)
				return false;
		}
		check = room + Vector2Int.up;
		if (!(
			check.x < 0 || check.x >= M_Stage.stageSize.x ||
			check.y < 0 || check.y >= M_Stage.stageSize.y
			) &&
			m_StageRoomCheck[check.y, check.x])
		{
			if (++count > 2)
				return false;
		}
		check = room + Vector2Int.down;
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
	private void GenerateRoom(Vector2Int check, ref Stage stage)
	{
		if (CheckRoom(check) == false)
			return;

		List<(E_Direction direction, int count)> conditionList = new List<(E_Direction direction, int count)>();

		for (int i = 0; i < m_Direction.Length; ++i)
		{
			E_Direction direction = (E_Direction)i;

			if (m_GeneratedRoomMap.TryGetValue(check + m_Direction[i], out Room nearRoom) == false)
			{
				conditionList.Add((direction, 0));
			}
			else
			{
				E_Direction nearRoomWarpDir = m_DirectionAfterWarp[i];
				int count = nearRoom.GetWarpPointCount(nearRoomWarpDir);

				if (count < 0)
					continue;

				conditionList.Add((direction, count));
			}
		}

		if (conditionList.Count <= 0)
			return;

		for (int i = 0; i < conditionList.Count; ++i)
		{
			Debug.Log(m_StageRoomCount.ToString("00_") + i.ToString("00: ") + conditionList[i].ToString());
		}

		Room room = M_Room.GetRandomRoom(conditionList.ToArray());
		if (room == null)
		{
			throw new System.Exception("Error: 조건에 맞는 방이 없음");
		}

		int x = check.x;
		int y = check.y;

		m_StageRoomCheck[y, x] = true;
		m_StageRoomQueue.Enqueue(check);
		++m_StageRoomCount;

		Vector2Int center = M_Stage.stageSize / 2;
		Vector3 pos = new Vector3((x - center.x) * 100, (y - center.y) * 100);

		room.transform.SetParent(stage.transform);
		room.transform.position = (Vector2)pos;
		room.name = m_StageRoomCount.ToString("00_") + room.name;
		room.gameObject.SetActive(true);
		room.Initialize();

		m_GeneratedRoomMap.Add(check, room);
	}
	private void ResetRoomCheck(int stageLevel, ref Stage stage)
	{
		m_StageRoomCount = 0;
		m_MaxRoomCount = Random.Range(0, 2) + 5 + (int)(stageLevel * m_StageMagnification);

		Vector2Int center = M_Stage.stageSize / 2;

		m_StageRoomQueue.Clear();
		m_StageRoomQueue.Enqueue(center);

		m_StageRoomCheck = new bool[M_Stage.stageSize.y, M_Stage.stageSize.x];
		m_StageRoomCheck[center.y, center.x] = true;
		++m_StageRoomCount;

		var rooms = stage.GetComponentsInChildren<Room>();
		for (int i = 0; i < rooms.Length; ++i)
		{
			M_Room.Despawn(rooms[i]);

			string[] splitedName = rooms[i].name.Split('_');
			rooms[i].name = splitedName[splitedName.Length - 1];
		}

		m_GeneratedRoomMap.Clear();

		Room room = M_Room.SpawnRandomRoom();
		if (room == null)
			Debug.LogError("Error: 시작 방이 null 임");

		room.transform.SetParent(stage.transform);
		room.transform.position = Vector3.zero;
		room.name = "01_ " + room.name;
		room.gameObject.SetActive(true);
		room.Initialize();

		m_CameraFollow.clampOffset = room.offset;
		m_CameraFollow.clampAreaSize = room.roomSize;

		m_GeneratedRoomMap.Add(center, room);
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