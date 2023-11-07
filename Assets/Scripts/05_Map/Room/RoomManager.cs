using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using E_Direction = WarpPoint.E_Direction;

public class RoomManager : ObjectManager<RoomManager, Room>
{
	private Dictionary<(E_Direction direction, int count), List<ObjectPool<Room>>> m_Room_Dir_Count_Map;

	public override void Initialize()
	{
		base.Initialize();

		m_Room_Dir_Count_Map = new Dictionary<(E_Direction direction, int count), List<ObjectPool<Room>>>();
		foreach (var originInfo in m_Origins)
		{
			originInfo.origin.Initialize();

			for (E_Direction direction = 0; direction < E_Direction.Max; ++direction)
			{
				int count = originInfo.origin.GetWarpPointCount(direction);
				if (count < 0)
					continue;

				if (m_Room_Dir_Count_Map.ContainsKey((direction, count)) == false)
					m_Room_Dir_Count_Map.Add((direction, count), new List<ObjectPool<Room>>());

				m_Room_Dir_Count_Map[(direction, count)].Add(GetPool(originInfo.key));
			}
		}
	}

	/// <summary>
	/// 모든 방들 중 랜덤한 방을 리턴하는 함수
	/// </summary>
	/// <returns>모든 방들 중 랜덤한 방</returns>
	public Room SpawnRandomRoom()
	{
		List<ObjectPool<Room>> poolList = new List<ObjectPool<Room>>();

		foreach (var item in m_Pools)
		{
			poolList.Add(item.Value);
		}

		if (poolList == null ||
			poolList.Count <= 0)
			return null;

		var randomPool = poolList[Random.Range(0, poolList.Count)];

		return randomPool.Spawn();
	}
	/// <summary>
	/// 하나의 조건을 만족하는 방들 중 랜덤한 방을 리턴하는 함수
	/// </summary>
	/// <param name="direction">조건: 방향</param>
	/// <param name="count">조건: 포탈 갯수</param>
	/// <returns>조건을 만족하는 방들 중 랜덤한 방</returns>
	public Room GetRandomRoom(E_Direction direction, int count)
	{
		List<ObjectPool<Room>> poolList = m_Room_Dir_Count_Map[(direction, count)];

		if (poolList == null ||
			poolList.Count <= 0)
			return null;

		var randomPool = poolList[Random.Range(0, poolList.Count)];

		return randomPool.Spawn();
	}
	/// <summary>
	/// 여러 조건을 만족하는 방들 중 랜덤한 방을 리턴하는 함수
	/// </summary>
	/// <param name="conditions"></param>
	/// <returns>조건을 만족하는 방들 중 랜덤한 방</returns>
	public Room GetRandomRoom(params (E_Direction direction, int count)[] conditions)
	{
		List<ObjectPool<Room>> poolList = new List<ObjectPool<Room>>();

		for (int i = 0; i < conditions.Length; ++i)
		{
			// 현재 조건
			var curCondition = conditions[i];

			if (m_Room_Dir_Count_Map.TryGetValue(curCondition, out List<ObjectPool<Room>> curConditionRoomList) == false)
				return null;

			// 현재 조건의 방들 추가
			poolList.AddRange(curConditionRoomList);

			for (int j = 0; j < i; ++j)
			{
				// 이전 조건
				var prevCondition = conditions[j];

				// 이전 조건들을 반복하며 충족하지 않는 방들 제외
				// 임시로 현재 풀에서 하나 생성해서 확인하는 방식을 쓰는데
				// 최적화면에서 너무 안좋을 것 같음
				poolList.RemoveAll((pool) =>
				{
					Room room = pool.Spawn();
					room.Initialize();
					int count = room.GetWarpPointCount(prevCondition.direction);
					pool.Despawn(room);

					if (count != prevCondition.count)
						return true;

					return false;
				});
			}
		}

		if (poolList == null ||
			poolList.Count <= 0)
			return null;

		var randomPool = poolList[Random.Range(0, poolList.Count)];

		return randomPool.Spawn();
	}
}