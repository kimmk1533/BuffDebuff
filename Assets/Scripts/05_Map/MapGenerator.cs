using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 아이작 방식 맵 생성
// 참고: https://minpro-tech.tistory.com/106
public class MapGenerator : MonoBehaviour
{
	[SerializeField, Min(1.0f)]
	float m_StageMagnification = 2.6f;

	[SerializeField]
	bool AutoClear = true;

	[Space(10)]
	[SerializeField, ReadOnly]
	Info info;

	[System.Serializable]
	struct Info
	{
		public int MinExpectedRoomCount;
		public int MaxExpectedRoomCount;
		public int MaxRoomCount;
	}

	bool[,] m_MapCheck;

	int m_MaxRoomCount;
	int m_RoomCount;
	Queue<Vector2Int> m_RoomQueue = new Queue<Vector2Int>();

	StageManager M_Stage => StageManager.Instance;

	public void Initialize()
	{
		m_MapCheck = new bool[M_Stage.height, M_Stage.width];

		ClearMap();
		GenerateMap();
	}

	public bool CheckMapGenerated(int x, int y)
	{
		return m_MapCheck[y, x];
	}
	public bool CheckMapGenerated(Vector2Int check)
	{
		return m_MapCheck[check.y, check.x];
	}

	[ContextMenu("맵 생성")]
	void GenerateMap()
	{
		if (AutoClear)
			ClearMap();

		RandomMap(M_Stage.stage);

		Vector2Int center = M_Stage.mapSize / 2;

		for (int y = 0; y < M_Stage.mapSize.y; ++y)
		{
			for (int x = 0; x < M_Stage.mapSize.x; ++x)
			{
				Room room = M_Stage.GetRandomRoom();

				Vector3 pos = new Vector3((x - center.x) * room.clampAreaSize.x, (y - center.y) * room.clampAreaSize.y);

				if (m_MapCheck[y, x])
					Instantiate(room, pos, Quaternion.identity, M_Stage.roomParent);
			}
		}
	}
	[ContextMenu("맵 제거")]
	void ClearMap()
	{
		int count = M_Stage.roomParent.childCount;
		for (int i = 0; i < count; ++i)
		{
			GameObject.DestroyImmediate(M_Stage.roomParent.GetChild(0).gameObject);
		}
	}

	void ResetRoomCount(int stage)
	{
		m_RoomCount = 0;
		m_MaxRoomCount = Random.Range(0, 2) + 5 + (int)(stage * m_StageMagnification);
	}
	void ResetMap(int stage)
	{
		ResetRoomCount(stage);

		Vector2Int center = M_Stage.mapSize / 2;

		m_RoomQueue.Clear();
		m_RoomQueue.Enqueue(center);

		m_MapCheck = new bool[M_Stage.mapSize.y, M_Stage.mapSize.x];
		m_MapCheck[center.y, center.x] = true;
		++m_RoomCount;
	}
	void RandomMap(int stage)
	{
		ResetMap(stage);

		Vector2Int current;
		Vector2Int check;

		while (m_RoomCount < m_MaxRoomCount)
		{
			if (m_RoomQueue.Count == 0)
			{
				ResetMap(stage);
			}

			current = m_RoomQueue.Dequeue();

			{
				// 좌
				check = current + Vector2Int.left;
				if (CheckRoom(check))
				{
					m_MapCheck[check.y, check.x] = true;
					m_RoomQueue.Enqueue(check);
					++m_RoomCount;
				}

				// 우
				check = current + Vector2Int.right;
				if (CheckRoom(check))
				{
					m_MapCheck[check.y, check.x] = true;
					m_RoomQueue.Enqueue(check);
					++m_RoomCount;
				}

				// 상
				check = current + Vector2Int.up;
				if (CheckRoom(check))
				{
					m_MapCheck[check.y, check.x] = true;
					m_RoomQueue.Enqueue(check);
					++m_RoomCount;
				}

				// 하
				check = current + Vector2Int.down;
				if (CheckRoom(check))
				{
					m_MapCheck[check.y, check.x] = true;
					m_RoomQueue.Enqueue(check);
					++m_RoomCount;
				}
			}
		}
	}
	bool CheckRoom(Vector2Int room)
	{
		// 이미 충분한 방이 채워졌으면 포기
		if (m_RoomCount >= m_MaxRoomCount)
			return false;

		// 인덱스 체크
		if (room.x < 0 || room.x >= M_Stage.mapSize.x ||
			room.y < 0 || room.y >= M_Stage.mapSize.y)
			return false;

		// 이미 채워져 있으면 포기
		if (m_MapCheck[room.y, room.x])
			return false;

		// 인접 칸이 둘 이상 채워져 있으면 포기
		int count = 0;
		Vector2Int check = room + Vector2Int.left;
		if (!(
			check.x < 0 || check.x >= M_Stage.mapSize.x ||
			check.y < 0 || check.y >= M_Stage.mapSize.y
			) &&
			m_MapCheck[check.y, check.x])
		{
			if (++count > 2)
				return false;
		}
		check = room + Vector2Int.right;
		if (!(
			check.x < 0 || check.x >= M_Stage.mapSize.x ||
			check.y < 0 || check.y >= M_Stage.mapSize.y
			) &&
			m_MapCheck[check.y, check.x])
		{
			if (++count > 2)
				return false;
		}
		check = room + Vector2Int.up;
		if (!(
			check.x < 0 || check.x >= M_Stage.mapSize.x ||
			check.y < 0 || check.y >= M_Stage.mapSize.y
			) &&
			m_MapCheck[check.y, check.x])
		{
			if (++count > 2)
				return false;
		}
		check = room + Vector2Int.down;
		if (!(
			check.x < 0 || check.x >= M_Stage.mapSize.x ||
			check.y < 0 || check.y >= M_Stage.mapSize.y
			) &&
			m_MapCheck[check.y, check.x])
		{
			if (++count > 2)
				return false;
		}

		// 50% 확률로 포기
		if (Random.Range(0, 2) == 0)
			return false;

		return true;
	}

	public void OnValidate()
	{
		info.MinExpectedRoomCount = /*Random.Range(0, 2)*/0 + 5 + (int)(M_Stage.stage * m_StageMagnification);
		info.MaxExpectedRoomCount = /*Random.Range(0, 2)*/1 + 5 + (int)(M_Stage.stage * m_StageMagnification);
		info.MaxRoomCount = M_Stage.mapSize.x * M_Stage.mapSize.y;
	}
}