using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using E_Direction = WarpPoint.E_Direction;

public class RoomManager : Singleton<RoomManager>
{
	[SerializeField, ReadOnly(true)]
	private List<Room> m_RoomList;

	private Dictionary<(E_Direction direction, int count), List<Room>> m_RoomMap;

	public void Initialize()
	{
		m_RoomMap = new Dictionary<(E_Direction direction, int count), List<Room>>();
		foreach (Room room in m_RoomList)
		{
			room.Initialize();

			for (E_Direction direction = 0; direction < E_Direction.Max; ++direction)
			{
				int count = room.GetWarpPointCount(direction);
				if (count <= 0)
					continue;

				if (m_RoomMap.ContainsKey((direction, count)) == false)
					m_RoomMap.Add((direction, count), new List<Room>());

				m_RoomMap[(direction, count)].Add(room);
			}
		}
	}

	public Room GetRandomRoom()
	{
		List<Room> roomList = m_RoomList;

		if (roomList == null ||
			roomList.Count < 0)
			return null;

		return roomList[Random.Range(0, roomList.Count)];
	}
	public Room GetRandomRoom(E_Direction direction, int count)
	{
		List<Room> roomList = m_RoomMap[(direction, count)];

		if (roomList == null ||
			roomList.Count < 0)
			return null;

		return roomList[Random.Range(0, roomList.Count)];
	}
	public Room GetRandomRoom(params (E_Direction direction, int count)[] conditions)
	{
		List<Room> roomList = new List<Room>();

		for (int i = 0; i < conditions.Length; ++i)
		{
			// 현재 조건
			var currentCondition = conditions[i];

			// 현재 조건의 방들 추가
			roomList.AddRange(m_RoomMap[currentCondition]);

			for (int j = 0; j < i; ++j)
			{
				// 이전 조건
				var prevCondition = conditions[j];

				// 이전 조건들을 반복하며 충족하지 않는 방들 제외
				roomList.RemoveAll((room) =>
				{
					int count = room.GetWarpPointCount(prevCondition.direction);
					if (count != prevCondition.count)
						return true;

					return false;
				});
			}
		}

		if (roomList.Count <= 0)
			return null;

		return roomList[Random.Range(0, roomList.Count)];
	}
}