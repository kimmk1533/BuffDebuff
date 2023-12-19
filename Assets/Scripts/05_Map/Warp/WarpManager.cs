using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enum;

public class WarpManager : Singleton<WarpManager>
{
	#region 변수
	private Dictionary<Room, Dictionary<E_Direction, List<WarpPoint>>> m_WarpPointMap;
	#endregion

	public void Initialize()
	{
		if (m_WarpPointMap != null)
			m_WarpPointMap.Clear();
		else
			m_WarpPointMap = new Dictionary<Room, Dictionary<E_Direction, List<WarpPoint>>>();
	}

	public void AddWarpPoint(Room room, WarpPoint warpPoint)
	{
		if (m_WarpPointMap == null)
			throw new System.NullReferenceException("m_WarpPointMap is null.");

		if (m_WarpPointMap.ContainsKey(room) == false)
			m_WarpPointMap.Add(room, new Dictionary<E_Direction, List<WarpPoint>>());

		E_Direction direction = warpPoint.direction;

		if (m_WarpPointMap[room].ContainsKey(direction) == false)
			m_WarpPointMap[room].Add(direction, new List<WarpPoint>());

		// 중복 검사
		if (m_WarpPointMap[room][direction].Contains(warpPoint) == true)
			return;

		m_WarpPointMap[room][direction].Add(warpPoint);
	}

	//public bool CheckLayerMask(GameObject checkObject)
	//{
	//	int layerMask = 1 << checkObject.layer;

	//	return (m_WarpLayerMask.value & layerMask) == layerMask;
	//}
}