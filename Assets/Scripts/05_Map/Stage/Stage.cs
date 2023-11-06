using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

		//int index = 0;
		//for (int y = 0; y < m_StageSize.y; ++y)
		//{
		//	for (int x = 0; x < m_StageSize.x; ++x)
		//	{
		//		if (CheckRoomGenerated(x, y))
		//		{
		//			m_GeneratedRooms[y, x] = m_RoomParent.GetChild(index++).GetComponent<Room>();
		//			m_GeneratedRooms[y, x].Initialize();
		//		}
		//	}
		//}

	}
}