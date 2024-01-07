using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Enum;
using RoomPool = ObjectPool<Room>;

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
	[SerializeField]
	protected List<OriginInfo> m_StartRoomOrigins = null;

	private List<RoomPool> m_AllRoomPool = null;
	#endregion

	#region 이벤트
	public System.Action<Room> onRoomClear;
	#endregion

	public override void Initialize()
	{
		base.Initialize();

		if (m_AllRoomPool == null)
			m_AllRoomPool = new List<RoomPool>();
	}
	public override void Finallize()
	{
		base.Finallize();

		if (m_AllRoomPool != null)
		{
			foreach (var item in m_AllRoomPool)
			{
				item.Dispose();
			}
			m_AllRoomPool.Clear();
		}
	}

	public override void InitializeGame()
	{
		base.InitializeGame();

		foreach (var item in m_ObjectPoolMap)
		{
			m_AllRoomPool.Add(item.Value);
		}
		m_AllRoomPool = m_AllRoomPool.Distinct().ToList();
	}
	public override void FinallizeGame()
	{
		base.FinallizeGame();
	}

	public RoomPool.ItemBuilder GetBuilder(params (E_Condition condition, E_Direction direction, int count)[] conditions)
	{
		List<RoomPool> poolList = new List<RoomPool>(m_AllRoomPool);

		for (int i = 0; i < conditions.Length; ++i)
		{
			(E_Condition condition, E_Direction direction, int count) condition = conditions[i];

			for (int poolListIndex = 0; poolListIndex < poolList.Count; ++poolListIndex)
			{
				RoomPool pool = poolList[poolListIndex];

				Room room = pool.GetBuilder()
					.SetAutoInit(true)
					.Spawn();

				int count = room.GetWarpPointCount(condition.direction);

				pool.Despawn(room, true);

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

		if (poolList.Count <= 0)
			throw new System.Exception("Error: 조건에 맞는 방이 없음");

		return poolList[Random.Range(0, poolList.Count)].GetBuilder();
	}

	public StartRoom SpawnStartRoom(string key)
	{
		StartRoom startRoom = null;

		foreach (var item in m_StartRoomOrigins)
		{
			if (item.key.Equals(key) == false)
				continue;

			startRoom = item.origin as StartRoom;
		}

		if (startRoom == null)
			return null;

		return GameObject.Instantiate(startRoom);
	}

#if UNITY_EDITOR
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

	[ContextMenu("Load Origin")]
	protected override void LoadOrigin()
	{
		base.LoadOrigin_Inner();

		for (int i = 0; i < m_StartRoomOrigins.Count; ++i)
		{
			OriginInfo info = m_StartRoomOrigins[i];
			info.origin = Resources.Load<StartRoom>(System.IO.Path.Combine(m_Path, info.path, info.key));
			m_StartRoomOrigins[i] = info;
		}
	}
#endif
}