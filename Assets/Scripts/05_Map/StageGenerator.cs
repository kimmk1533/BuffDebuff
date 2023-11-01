using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 아이작 방식 맵 생성
// 참고: https://minpro-tech.tistory.com/106
public class StageGenerator : MonoBehaviour
{
	[SerializeField, Min(1.0f)]
	private float m_StageMagnification = 2.6f;

	[SerializeField]
	private bool m_AutoClear = true;

	[Space(10)]
	[SerializeField, ReadOnly]
	private Info m_Info;

	[System.Serializable]
	struct Info
	{
		public int MinExpectedRoomCount;
		public int MaxExpectedRoomCount;
		public int MaxRoomCount;
	}

	private bool[,] m_RoomCheck;

	private int m_MaxRoomCount;
	private int m_RoomCount;
	private Queue<Vector2Int> m_RoomQueue = new Queue<Vector2Int>();

	private StageManager M_Stage => StageManager.Instance;

	public void Initialize()
	{
		m_RoomCheck = new bool[M_Stage.height, M_Stage.width];

		ClearStage();
		GenerateStage();
	}

	public bool CheckRoomGenerated(int x, int y)
	{
		return m_RoomCheck[y, x];
	}
	public bool CheckRoomGenerated(Vector2Int check)
	{
		return m_RoomCheck[check.y, check.x];
	}

	[ContextMenu("맵 생성")]
	public void GenerateStage()
	{
		if (m_AutoClear)
			ClearStage();

		RandomMap(M_Stage.stage);

		Vector2Int center = M_Stage.stageSize / 2;

		for (int y = 0; y < M_Stage.stageSize.y; ++y)
		{
			for (int x = 0; x < M_Stage.stageSize.x; ++x)
			{
				Room room = M_Stage.GetRandomRoom();

				Vector3 pos = new Vector3((x - center.x) * room.clampAreaSize.x, (y - center.y) * room.clampAreaSize.y);

				if (m_RoomCheck[y, x])
					GameObject.Instantiate(room, pos, Quaternion.identity, M_Stage.roomParent);
			}
		}
	}
	[ContextMenu("맵 제거")]
	public void ClearStage()
	{
		int count = M_Stage.roomParent.childCount;
		for (int i = 0; i < count; ++i)
		{
			GameObject.DestroyImmediate(M_Stage.roomParent.GetChild(0).gameObject);
		}
	}

	private void ResetRoomCount(int stage)
	{
		m_RoomCount = 0;
		m_MaxRoomCount = Random.Range(0, 2) + 5 + (int)(stage * m_StageMagnification);
	}
	private void ResetMap(int stage)
	{
		ResetRoomCount(stage);

		Vector2Int center = M_Stage.stageSize / 2;

		m_RoomQueue.Clear();
		m_RoomQueue.Enqueue(center);

		m_RoomCheck = new bool[M_Stage.stageSize.y, M_Stage.stageSize.x];
		m_RoomCheck[center.y, center.x] = true;
		++m_RoomCount;
	}
	private void RandomMap(int stage)
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
					m_RoomCheck[check.y, check.x] = true;
					m_RoomQueue.Enqueue(check);
					++m_RoomCount;
				}

				// 우
				check = current + Vector2Int.right;
				if (CheckRoom(check))
				{
					m_RoomCheck[check.y, check.x] = true;
					m_RoomQueue.Enqueue(check);
					++m_RoomCount;
				}

				// 상
				check = current + Vector2Int.up;
				if (CheckRoom(check))
				{
					m_RoomCheck[check.y, check.x] = true;
					m_RoomQueue.Enqueue(check);
					++m_RoomCount;
				}

				// 하
				check = current + Vector2Int.down;
				if (CheckRoom(check))
				{
					m_RoomCheck[check.y, check.x] = true;
					m_RoomQueue.Enqueue(check);
					++m_RoomCount;
				}
			}
		}
	}
	private bool CheckRoom(Vector2Int room)
	{
		// 이미 충분한 방이 채워졌으면 포기
		if (m_RoomCount >= m_MaxRoomCount)
			return false;

		// 인덱스 체크
		if (room.x < 0 || room.x >= M_Stage.stageSize.x ||
			room.y < 0 || room.y >= M_Stage.stageSize.y)
			return false;

		// 이미 채워져 있으면 포기
		if (m_RoomCheck[room.y, room.x])
			return false;

		// 인접 칸이 둘 이상 채워져 있으면 포기
		int count = 0;
		Vector2Int check = room + Vector2Int.left;
		if (!(
			check.x < 0 || check.x >= M_Stage.stageSize.x ||
			check.y < 0 || check.y >= M_Stage.stageSize.y
			) &&
			m_RoomCheck[check.y, check.x])
		{
			if (++count > 2)
				return false;
		}
		check = room + Vector2Int.right;
		if (!(
			check.x < 0 || check.x >= M_Stage.stageSize.x ||
			check.y < 0 || check.y >= M_Stage.stageSize.y
			) &&
			m_RoomCheck[check.y, check.x])
		{
			if (++count > 2)
				return false;
		}
		check = room + Vector2Int.up;
		if (!(
			check.x < 0 || check.x >= M_Stage.stageSize.x ||
			check.y < 0 || check.y >= M_Stage.stageSize.y
			) &&
			m_RoomCheck[check.y, check.x])
		{
			if (++count > 2)
				return false;
		}
		check = room + Vector2Int.down;
		if (!(
			check.x < 0 || check.x >= M_Stage.stageSize.x ||
			check.y < 0 || check.y >= M_Stage.stageSize.y
			) &&
			m_RoomCheck[check.y, check.x])
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
		m_Info.MinExpectedRoomCount = 0 + 5 + (int)(M_Stage.stage * m_StageMagnification);
		m_Info.MaxExpectedRoomCount = 1 + 5 + (int)(M_Stage.stage * m_StageMagnification);
		m_Info.MaxRoomCount = M_Stage.stageSize.x * M_Stage.stageSize.y;
	}
}