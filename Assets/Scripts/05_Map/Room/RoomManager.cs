using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RoomPool = ObjectPool<Room>;
using Enum;

public class RoomManager : ObjectManager<RoomManager, Room>
{
	// E_Condition
	#region Enum
	public enum E_Condition
	{
		// 초과
		Over,
		// 이상
		More,
		// 항등
		Equal,
		// 
		NotEqual,
		// 이하
		Less,
		// 미만
		Under,

		Max
	}
	#endregion

	#region 변수
	private List<RoomPool> m_AllRoomPool;
	private Dictionary<(E_Direction direction, int count), List<RoomPool>> m_DirCountMap;
	#endregion

	public override void Initialize()
	{
		base.Initialize();

		m_AllRoomPool = new List<RoomPool>();
		foreach (var item in m_Pools)
		{
			m_AllRoomPool.Add(item.Value);
		}
		m_AllRoomPool = m_AllRoomPool.Distinct().ToList();

		m_DirCountMap = new Dictionary<(E_Direction direction, int count), List<RoomPool>>();
		foreach (var originInfo in m_Origins)
		{
			originInfo.origin.Initialize();

			for (E_Direction direction = 0; direction < E_Direction.Max; ++direction)
			{
				int count = originInfo.origin.GetWarpPointCount(direction);

				if (m_DirCountMap.ContainsKey((direction, count)) == false)
					m_DirCountMap.Add((direction, count), new List<RoomPool>());

				m_DirCountMap[(direction, count)].Add(GetPool(originInfo.key));
			}
		}
	}

	/// <summary>
	/// 모든 방들 중 랜덤한 방을 리턴하는 함수
	/// </summary>
	/// <returns>모든 방들 중 랜덤한 방</returns>
	public Room SpawnRandomRoom_All()
	{
		List<RoomPool> poolList = new List<RoomPool>(m_AllRoomPool);

		if (poolList.Count <= 0)
			return null;

		RoomPool randomPool = poolList[Random.Range(0, poolList.Count)];

		return randomPool.Spawn();
	}
	/// <summary>
	/// 여러 조건을 만족하는 방들 중 랜덤한 방을 리턴하는 함수
	/// </summary>
	/// <param name="conditions"></param>
	/// <returns>조건을 만족하는 방들 중 랜덤한 방</returns>
	public Room SpawnRandomRoom(params (E_Condition condition, E_Direction direction, int count)[] conditions)
	{
		if (conditions.Length <= 0)
			return SpawnRandomRoom_All();

		List<RoomPool> poolList = new List<RoomPool>(m_AllRoomPool);

		for (int i = 0; i < conditions.Length; ++i)
		{
			(E_Condition condition, E_Direction direction, int count) condition = conditions[i];

			for (int poolListIndex = 0; poolListIndex < poolList.Count; ++poolListIndex)
			{
				RoomPool pool = poolList[poolListIndex];

				Room room = pool.Spawn();
				room.Initialize();
				int count = room.GetWarpPointCount(condition.direction);
				pool.Despawn(room);

				bool flag;

				switch (condition.condition)
				{
					case E_Condition.Over:
						flag = !(count > condition.count);
						break;
					case E_Condition.More:
						flag = !(count >= condition.count);
						break;
					case E_Condition.Equal:
						flag = !(count == condition.count);
						break;
					case E_Condition.NotEqual:
						flag = !(count != condition.count);
						break;
					case E_Condition.Less:
						flag = !(count <= condition.count);
						break;
					case E_Condition.Under:
						flag = !(count < condition.count);
						break;
					default:
						throw new System.Exception("prevCondition.condition value is strange.");
				}

				if (flag)
				{
					poolList.RemoveAt(poolListIndex);
					--poolListIndex;
				}
			}
		}

		if (poolList == null ||
			poolList.Count <= 0)
			return null;

		RoomPool randomPool = poolList[Random.Range(0, poolList.Count)];

		return randomPool.Spawn();
	}

	[ContextMenu("All Flag On")]
	private void TurnOnFlagAll()
	{
		for (int i = 0; i < m_Origins.Count; ++i)
		{
			var temp = m_Origins[i];
			temp.useFlag = true;
			m_Origins[i] = temp;
		}
	}
	[ContextMenu("All Flag Off")]
	private void TurnOffFlagAll()
	{
		for (int i = 0; i < m_Origins.Count; ++i)
		{
			var temp = m_Origins[i];
			temp.useFlag = false;
			m_Origins[i] = temp;
		}
	}
}