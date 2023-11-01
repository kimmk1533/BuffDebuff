using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Room : MonoBehaviour
{
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

	[SerializeField]
	private Vector2 m_ClampOffset;
	[SerializeField]
	private Vector2 m_ClampAreaSize;

	private Dictionary<E_RoomTilemapLayer, Tilemap> m_TilemapMap;
	private List<WarpPoint> m_WarpPointMap;
	private Dictionary<WarpPoint.E_WarpPointPos, List<WarpPoint>> m_DirWarpPointMap;

	public Vector2 clampOffset
	{
		get { return m_ClampOffset; }
		set { m_ClampOffset = value; }
	}
	public Vector2 clampAreaSize
	{
		get { return m_ClampAreaSize; }
		set { m_ClampAreaSize = value; }
	}

	public void Initialize()
	{
		// Init Tilemap Map
		m_TilemapMap = new Dictionary<E_RoomTilemapLayer, Tilemap>();
		Transform tilemapLayer = transform.Find("TileMapLayer");
		for (E_RoomTilemapLayer layer = E_RoomTilemapLayer.BackGround; layer != E_RoomTilemapLayer.Max; ++layer)
		{
			Tilemap tileMap = tilemapLayer.Find(layer.ToString()).GetComponent<Tilemap>();
			m_TilemapMap.Add(layer, tileMap);
		}

		// Init WarpPoint Map
		m_WarpPointMap = new List<WarpPoint>();
		GetComponentsInChildren<WarpPoint>(m_WarpPointMap);

		// Init DirWarpPoint Map
		m_DirWarpPointMap = new Dictionary<WarpPoint.E_WarpPointPos, List<WarpPoint>>();
		for (WarpPoint.E_WarpPointPos dir = 0; dir < WarpPoint.E_WarpPointPos.Max; ++dir)
		{
			m_DirWarpPointMap.Add(dir, new List<WarpPoint>());
		}
		foreach (var item in m_WarpPointMap)
		{
			m_DirWarpPointMap[item.warpPointPos].Add(item);
		}
	}
	public Tilemap GetTilemap(E_RoomTilemapLayer layer)
	{
		if (m_TilemapMap == null)
			return null;

		return m_TilemapMap[layer];
	}
	//public WarpPoint GetWarpPoint(WarpPoint.E_WarpPointPos warpPointPos)
	//{

	//}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = new Color(0, 1, 0, 0.1f);
		Gizmos.DrawCube((Vector2)transform.position + m_ClampOffset, m_ClampAreaSize);
	}
}