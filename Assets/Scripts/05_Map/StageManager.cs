using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(StageGenerator))]
public class StageManager : Singleton<StageManager>
{
	#region Variables
	[SerializeField, Min(1)]
	private int m_Stage;

	[SerializeField]
	private Transform m_RoomParent;
	[SerializeField, ReadOnly(true)]
	private List<Room> m_RoomList;
	private Dictionary<(WarpPoint.E_WarpPointPos, WarpPoint.E_WarpPointPos), Room> m_RoomMap;

	private Room[,] m_GeneratedRooms;

	[SerializeField]
	private Vector2Int m_StageSize;
	[SerializeField, ReadOnly]
	private Vector2Int m_CurrentPos;

	private StageGenerator m_StageGenerator;
	private CameraFollow m_CameraFollow; 
	#endregion

	#region Property
	public int stage => m_Stage;
	public Transform roomParent => m_RoomParent;
	public Vector2Int stageSize => m_StageSize;
	public int width => m_StageSize.x;
	public int height => m_StageSize.y;
	public Room currentRoom => m_GeneratedRooms[m_CurrentPos.y, m_CurrentPos.x]; 
	#endregion

	public void Initialize()
	{
		m_RoomMap = new Dictionary<(WarpPoint.E_WarpPointPos, WarpPoint.E_WarpPointPos), Room>();

		m_StageGenerator = GetComponent<StageGenerator>();
		m_StageGenerator.Initialize();
		m_StageGenerator.GenerateStage();

		m_CameraFollow = Camera.main.GetComponent<CameraFollow>();

		m_GeneratedRooms = new Room[m_StageSize.y, m_StageSize.x];

		int index = 0;
		for (int y = 0; y < m_StageSize.y; ++y)
		{
			for (int x = 0; x < m_StageSize.x; ++x)
			{
				if (m_StageGenerator.CheckRoomGenerated(x, y))
				{
					m_GeneratedRooms[y, x] = m_RoomParent.GetChild(index++).GetComponent<Room>();
					m_GeneratedRooms[y, x].Initialize();
				}
			}
		}

		m_CurrentPos = m_StageSize / 2;
	}

	public Room GetRandomRoom()
	{
		return m_RoomList[Random.Range(0, m_RoomList.Count)];
	}
	//public Transform GetSpawnPoint(Collider2D collider)
	//{
	//	Room room = null;
	//	Transform warpPoint = null;

	//	switch (collider.transform.parent.name)
	//	{
	//		case "Left":
	//			if (m_CurrentPos.x - 1 < 0)
	//				break;

	//			room = m_GeneratedRooms[m_CurrentPos.y, m_CurrentPos.x - 1];

	//			if (room != null)
	//			{
	//				warpPoint = room.GetWarpPoint(Room.E_RoomDir.Right);
	//				--m_CurrentPos.x;
	//				m_CameraFollow.clampOffset += Vector2.left * room.clampAreaSize.x;
	//			}
	//			break;
	//		case "Right":
	//			if (m_CurrentPos.x + 1 >= m_MapSize.x)
	//				break;

	//			room = m_GeneratedRooms[m_CurrentPos.y, m_CurrentPos.x + 1];

	//			if (room != null)
	//			{
	//				warpPoint = room.GetWarpPoint(Room.E_RoomDir.Left);
	//				++m_CurrentPos.x;
	//				m_CameraFollow.clampOffset += Vector2.right * room.clampAreaSize.x;
	//			}
	//			break;
	//		case "Top":
	//			if (m_CurrentPos.y + 1 >= m_MapSize.y)
	//				break;

	//			room = m_GeneratedRooms[m_CurrentPos.y + 1, m_CurrentPos.x];

	//			if (room != null)
	//			{
	//				warpPoint = room.GetWarpPoint(Room.E_RoomDir.Bottom);
	//				++m_CurrentPos.y;
	//				m_CameraFollow.clampOffset += Vector2.up * room.clampAreaSize.y;
	//			}
	//			break;
	//		case "Bottom":
	//			if (m_CurrentPos.y - 1 < 0)
	//				break;

	//			room = m_GeneratedRooms[m_CurrentPos.y - 1, m_CurrentPos.x];

	//			if (room != null)
	//			{
	//				warpPoint = room.GetWarpPoint(Room.E_RoomDir.Top);
	//				--m_CurrentPos.y;
	//				m_CameraFollow.clampOffset += Vector2.down * room.clampAreaSize.y;
	//			}
	//			break;
	//	}

	//	if (warpPoint != null)
	//	{
	//		m_CameraFollow.transform.position = warpPoint.position + Vector3.up * 3f;
	//	}

	//	return warpPoint;
	//}

	private void OnValidate()
	{
		GetComponent<StageGenerator>().OnValidate();
	}
}