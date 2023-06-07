using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MapGenerator))]
public class StageManager : Singleton<StageManager>
{
	[SerializeField]
	Transform m_RoomParent;
	[SerializeField, ReadOnly(true)]
	List<Room> m_RoomList;
	[SerializeField, Min(1)]
	int m_Stage;

	Room[,] m_GeneratedRooms;

	[SerializeField]
	Vector2Int m_MapSize;
	[SerializeField, ReadOnly]
	Vector2Int m_CurrentPos;

	MapGenerator m_MapGenerator;
	CameraFollow m_CameraFollow;

	public int stage => m_Stage;
	public Transform roomParent => m_RoomParent;
	public Vector2Int mapSize => m_MapSize;
	public int width => m_MapSize.x;
	public int height => m_MapSize.y;
	public Room currentRoom => m_GeneratedRooms[m_CurrentPos.y, m_CurrentPos.x];

	private void Awake()
	{
		m_MapGenerator = GetComponent<MapGenerator>();
		m_CameraFollow = Camera.main.GetComponent<CameraFollow>();

		m_GeneratedRooms = new Room[m_MapSize.y, m_MapSize.x];
	}
	private void Start()
	{
		int index = 0;
		for (int y = 0; y < m_MapSize.y; ++y)
		{
			for (int x = 0; x < m_MapSize.x; ++x)
			{
				if (m_MapGenerator.CheckMapGenerated(x, y))
					m_GeneratedRooms[y, x] = m_RoomParent.GetChild(index++).GetComponent<Room>();
			}
		}

		m_CurrentPos = m_MapSize / 2;
	}
	private void OnValidate()
	{
		GetComponent<MapGenerator>().OnValidate();
	}

	public Room GetRoom(int stage)
	{
		if (stage < 0 || stage >= m_RoomList.Count)
			return null;

		return m_RoomList[stage];
	}
	public Room GetRandomRoom()
	{
		return m_RoomList[Random.Range(0, m_RoomList.Count)];
	}
	public Transform GetSpawnPoint(Collider2D collider)
	{
		Room room = null;
		Transform warpPoint = null;

		switch (collider.transform.parent.name)
		{
			case "Left":
				if (m_CurrentPos.x - 1 < 0)
					break;

				room = m_GeneratedRooms[m_CurrentPos.y, m_CurrentPos.x - 1];

				if (room != null)
				{
					warpPoint = room.GetWarpPoint(Room.E_RoomDir.Right);
					--m_CurrentPos.x;
					m_CameraFollow.clampOffset += Vector2.left * room.clampAreaSize.x;
				}
				break;
			case "Right":
				if (m_CurrentPos.x + 1 >= m_MapSize.x)
					break;

				room = m_GeneratedRooms[m_CurrentPos.y, m_CurrentPos.x + 1];

				if (room != null)
				{
					warpPoint = room.GetWarpPoint(Room.E_RoomDir.Left);
					++m_CurrentPos.x;
					m_CameraFollow.clampOffset += Vector2.right * room.clampAreaSize.x;
				}
				break;
			case "Top":
				if (m_CurrentPos.y + 1 >= m_MapSize.y)
					break;

				room = m_GeneratedRooms[m_CurrentPos.y + 1, m_CurrentPos.x];

				if (room != null)
				{
					warpPoint = room.GetWarpPoint(Room.E_RoomDir.Bottom);
					++m_CurrentPos.y;
					m_CameraFollow.clampOffset += Vector2.up * room.clampAreaSize.y;
				}
				break;
			case "Bottom":
				if (m_CurrentPos.y - 1 < 0)
					break;

				room = m_GeneratedRooms[m_CurrentPos.y - 1, m_CurrentPos.x];

				if (room != null)
				{
					warpPoint = room.GetWarpPoint(Room.E_RoomDir.Top);
					--m_CurrentPos.y;
					m_CameraFollow.clampOffset += Vector2.down * room.clampAreaSize.y;
				}
				break;
		}

		if (warpPoint != null)
		{
			m_CameraFollow.transform.position = warpPoint.position + Vector3.up * 3f;
		}

		return warpPoint;
	}
}