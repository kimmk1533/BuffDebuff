using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Room : MonoBehaviour
{
	[SerializeField]
	Vector2 m_ClampOffset;
	[SerializeField]
	Vector2 m_ClampAreaSize;

	[SerializeField]
	Dictionary<E_RoomTilemapLayer, Tilemap> m_TilemapMap;
	[SerializeField]
	Dictionary<E_RoomDir, Transform> m_WarpPointMap;

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
		m_TilemapMap = new Dictionary<E_RoomTilemapLayer, Tilemap>();
		m_WarpPointMap = new Dictionary<E_RoomDir, Transform>();

		Transform tilemapLayer = transform.Find("TileMapLayer");
		for (E_RoomTilemapLayer layer = E_RoomTilemapLayer.BackGround; layer != E_RoomTilemapLayer.Max; ++layer)
		{
			Tilemap tileMap = tilemapLayer.Find(layer.ToString()).GetComponent<Tilemap>();
			m_TilemapMap.Add(layer, tileMap);
		}

		Transform portal = transform.Find("Portal");
		for (E_RoomDir dir = E_RoomDir.Left; dir != E_RoomDir.Max; ++dir)
		{
			Transform warpPoint = portal.Find(dir.ToString()).Find("WarpPoint");

			m_WarpPointMap.Add(dir, warpPoint);
		}
	}
	public Tilemap GetTilemap(E_RoomTilemapLayer layer)
	{
		if (m_TilemapMap == null)
			return null;

		return m_TilemapMap[layer];
	}
	public Transform GetWarpPoint(E_RoomDir dir)
	{
		return m_WarpPointMap[dir];
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = new Color(0, 1, 0, 0.1f);
		Gizmos.DrawCube((Vector2)transform.position + m_ClampOffset, m_ClampAreaSize);
	}

	public enum E_RoomTilemapLayer
	{
		// 뒷 배경
		BackGround,
		// 벽
		TileMap,
		// 점프 가능한 벽
		ThroughMap,
		// 앞 배경 (장식 등등)
		Environment,

		Max
	}
	public enum E_RoomDir
	{
		Left = 0,
		Right = 1,
		Top = 2,
		Bottom = 3,

		Max
	}
}