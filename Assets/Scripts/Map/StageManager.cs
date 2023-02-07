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

	[Space(10)]
	public Transform[,] m_GeneratedRooms;

	[SerializeField]
	Vector2Int m_MapSize;
	[SerializeField, ReadOnly]
	Vector2Int m_CurrentPos;

	public int stage => m_Stage;
	public Transform roomParent => m_RoomParent;
	public Vector2Int mapSize => m_MapSize;

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

	private void OnValidate()
	{
		GetComponent<MapGenerator>().OnValidate();
	}
}