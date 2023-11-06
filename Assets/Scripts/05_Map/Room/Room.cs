using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using E_Direction = WarpPoint.E_Direction;

public class Room : MonoBehaviour
{
	// E_RoomTilemapLayer
	#region Enum
	public enum E_RoomTilemapLayer
	{
		// 뒷 배경
		BackGround,
		// 벽
		TileMap,
		// 점프 가능한 벽
		OneWayMap,
		// 앞 배경 (장식 등등)
		Environment,

		Max
	}
	#endregion

	#region Variables
	// Room Size
	[SerializeField]
	private Vector2 m_RoomSize;

	// Tilemap
	private Dictionary<E_RoomTilemapLayer, Tilemap> m_TilemapMap;

	// WarpPoint
	private Dictionary<WarpPoint.E_Direction, List<WarpPoint>> m_WarpPointMap;
	private Dictionary<WarpPoint.E_Direction, int> m_WarpPointCountMap;
	#endregion

	#region Property
	public Vector2 roomSize
	{
		get { return m_RoomSize; }
		set { m_RoomSize = value; }
	}
	#endregion

	public void Initialize()
	{
		// Init Tilemap Map
		m_TilemapMap = new Dictionary<E_RoomTilemapLayer, Tilemap>();
		Transform tilemapLayer = transform.Find("TileMapLayer");
		for (E_RoomTilemapLayer layer = E_RoomTilemapLayer.BackGround; layer < E_RoomTilemapLayer.Max; ++layer)
		{
			Tilemap tileMap = tilemapLayer.Find(layer.ToString()).GetComponent<Tilemap>();
			m_TilemapMap.Add(layer, tileMap);
		}

		// Init WarpPoint, WarpPointCount Map
		m_WarpPointMap = new Dictionary<E_Direction, List<WarpPoint>>();
		m_WarpPointCountMap = new Dictionary<E_Direction, int>();

		WarpPoint[] warpPointArray = GetComponentsInChildren<WarpPoint>();
		foreach (WarpPoint warpPoint in warpPointArray)
		{
			E_Direction direction = warpPoint.direction;

			if (m_WarpPointMap.ContainsKey(direction) == false)
				m_WarpPointMap.Add(direction, new List<WarpPoint>());

			if (m_WarpPointCountMap.ContainsKey(direction) == false)
				m_WarpPointCountMap.Add(direction, 0);

			m_WarpPointMap[direction].Add(warpPoint);
			m_WarpPointCountMap[direction]++;
		}
	}

	public Tilemap GetTilemap(E_RoomTilemapLayer layer)
	{
		if (m_TilemapMap == null)
			return null;

		return m_TilemapMap[layer];
	}
	public List<WarpPoint> GetWarpPointList()
	{
		List<WarpPoint> result = new List<WarpPoint>();

		foreach (var item in m_WarpPointMap)
		{
			result.AddRange(item.Value);
		}

		return result;
	}
	public List<WarpPoint> GetWarpPointList(E_Direction direction)
	{
		List<WarpPoint> result = new List<WarpPoint>();

		result.AddRange(m_WarpPointMap[direction]);

		return result;
	}
	public int GetWarpPointCount(E_Direction direction)
	{
		return m_WarpPointCountMap[direction];
	}

	#region Draw Gizmo
	[SerializeField]
	private Vector2 m_Offset;

	public Vector2 offset
	{
		get { return m_Offset; }
		set { m_Offset = value; }
	}

	private void OnDrawGizmosSelected()
	{
		Color color = Gizmos.color;

		Gizmos.color = new Color(0, 1, 0, 0.1f);
		Gizmos.DrawCube((Vector2)transform.position + m_Offset, m_RoomSize);

		Gizmos.color = color;
	}
	#endregion
}