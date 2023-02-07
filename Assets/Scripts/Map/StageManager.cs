using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MapGenerator))]
public class StageManager : Singleton<StageManager>
{
	[SerializeField]
	Transform m_RoomParent;
	[SerializeField, ReadOnly(true)]
	List<GameObject> m_RoomList;
	[SerializeField, Min(1)]
	int m_Stage;

	Transform[,] m_GeneratedRooms;

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

	private void Awake()
	{
		m_MapGenerator = GetComponent<MapGenerator>();
		m_CameraFollow = Camera.main.GetComponent<CameraFollow>();
	}
	private void Start()
	{
		m_GeneratedRooms = new Transform[m_MapSize.y, m_MapSize.x];

		int index = 0;
		for (int y = 0; y < m_MapSize.y; ++y)
		{
			for (int x = 0; x < m_MapSize.x; ++x)
			{
				if (m_MapGenerator.CheckMapGenerated(x, y))
					m_GeneratedRooms[y, x] = m_RoomParent.GetChild(index++);
			}
		}

		m_CurrentPos = m_MapSize / 2;
	}
	private void OnValidate()
	{
		GetComponent<MapGenerator>().OnValidate();
	}

	public GameObject GetRoom(int index)
	{
		if (index < 0 || index >= m_RoomList.Count)
			return null;

		return m_RoomList[index];
	}
	public GameObject GetRandomRoom()
	{
		return m_RoomList[Random.Range(0, m_RoomList.Count)];
	}
	public Transform GetSpawnPoint(Collider2D collider)
	{
		Transform room = null;
		Transform spawnPoint = null;

		switch (collider.transform.parent.name)
		{
			case "Left":
				if (m_CurrentPos.x - 1 < 0)
					break;

				room = m_GeneratedRooms[m_CurrentPos.y, m_CurrentPos.x - 1];

				if (room != null)
				{
					spawnPoint = room.Find("Right").Find("SpawnPoint");
					--m_CurrentPos.x;
					m_CameraFollow.ChangeClampOffset(-50, 0);
					m_CameraFollow.transform.position = spawnPoint.position;
				}
				break;
			case "Right":
				if (m_CurrentPos.x + 1 >= m_MapSize.x)
					break;

				room = m_GeneratedRooms[m_CurrentPos.y, m_CurrentPos.x + 1];

				if (room != null)
				{
					spawnPoint = room.Find("Left").Find("SpawnPoint");
					++m_CurrentPos.x;
					m_CameraFollow.ChangeClampOffset(50, 0);
					m_CameraFollow.transform.position = spawnPoint.position;
				}
				break;
			case "Top":
				if (m_CurrentPos.y + 1 >= m_MapSize.y)
					break;

				room = m_GeneratedRooms[m_CurrentPos.y + 1, m_CurrentPos.x];

				if (room != null)
				{
					spawnPoint = room.Find("Bottom").Find("SpawnPoint");
					++m_CurrentPos.y;
					m_CameraFollow.ChangeClampOffset(0, 50);
					m_CameraFollow.transform.position = spawnPoint.position;
				}
				break;
			case "Bottom":
				if (m_CurrentPos.y - 1 < 0)
					break;

				room = m_GeneratedRooms[m_CurrentPos.y - 1, m_CurrentPos.x];

				if (room != null)
				{
					spawnPoint = room.Find("Top").Find("SpawnPoint");
					--m_CurrentPos.y;
					m_CameraFollow.ChangeClampOffset(0, -50);
					m_CameraFollow.transform.position = spawnPoint.position;
				}
				break;
		}

		return spawnPoint;
	}
}