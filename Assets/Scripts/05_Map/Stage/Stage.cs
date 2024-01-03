using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enum;
using System;

public class Stage : MonoBehaviour
{
	#region 변수
	[SerializeField]
	private Vector2Int m_StageSize;

	private Vector2Int m_CurrentRoomIndex;
	private Dictionary<Vector2Int, Room> m_GeneratedRooms;

	private CameraFollow m_CameraFollow;
	#endregion

	#region 프로퍼티
	public Room currentRoom => m_GeneratedRooms[m_CurrentRoomIndex];
	#endregion

	public void Initialize(Vector2Int stageSize, Dictionary<Vector2Int, Room> generatedRooms)
	{
		m_StageSize = stageSize;

		m_CurrentRoomIndex = m_StageSize / 2;

		m_GeneratedRooms = new Dictionary<Vector2Int, Room>();
		foreach (var item in generatedRooms)
		{
			m_GeneratedRooms.Add(item.Key, item.Value);
		}

		//currentRoom.gameObject.SetActive(true);
		currentRoom.isSimulating = true;

		Camera.main.Safe_GetComponent<CameraFollow>(ref m_CameraFollow);
	}

	public void MoveRoom(Vector2Int moveIndex)
	{
		//currentRoom.gameObject.SetActive(false);
		currentRoom.isSimulating = false;
		m_CurrentRoomIndex += moveIndex;
		//currentRoom.gameObject.SetActive(true);
		currentRoom.isSimulating = true;

		Vector2 offset = (Vector2)currentRoom.transform.position + currentRoom.offset;
		Vector2 size = currentRoom.roomSize;

		m_CameraFollow.UpdateClamp(offset, size);
	}
}