using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enum;
using System;

public class Stage : MonoBehaviour
{
	[SerializeField, ReadOnly]
	private int m_StageLevel;
	[SerializeField]
	private Vector2Int m_StageSize;

	private Vector2Int m_CurrentRoomIndex;

	private Dictionary<Vector2Int, Room> m_GeneratedRooms;

	public Room currentRoom => m_GeneratedRooms[m_CurrentRoomIndex];

	private StageManager M_Stage => StageManager.Instance;

	public void Initialize(int stageLevel, Vector2Int stageSize, Dictionary<Vector2Int, Room> generatedRooms)
	{
		m_StageLevel = stageLevel;
		m_StageSize = stageSize;

		m_GeneratedRooms = new Dictionary<Vector2Int, Room>();
		foreach (var item in generatedRooms)
		{
			m_GeneratedRooms.Add(item.Key, item.Value);
		}

		m_CurrentRoomIndex = M_Stage.stageSize / 2;
	}
	public void MoveRoom(Vector2Int moveIndex)
	{
		m_CurrentRoomIndex += moveIndex;
	}
}