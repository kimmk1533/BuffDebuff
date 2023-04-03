using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(AStar))]
public class GridManager : Singleton<GridManager>
{
	public Tilemap m_Tilemap;
	AStar m_AStar;

	public List<Vector2Int> m_Road;
	public Vector2Int m_Start;
	public Vector2Int m_End;

	private void Awake()
	{
		//PriorityQueue<int, int> test;

		m_AStar = GetComponent<AStar>();

		Test();
	}

	[ContextMenu("Test")]
	public void Test()
	{
		BoundsInt bounds = m_Tilemap.cellBounds;
		TileBase[] allTiles = m_Tilemap.GetTilesBlock(bounds);

		if (m_AStar == null)
			m_AStar = GetComponent<AStar>();

		m_Road = m_AStar.PathFinding(allTiles, m_Start.x, m_Start.y, m_End.x, m_End.y, bounds.size.x, bounds.size.y);
	}


	private void OnDrawGizmos()
	{
		BoundsInt bounds = m_Tilemap.cellBounds;
		Vector3 size = Vector3.one * 0.5f;
		Vector3 offset = size + bounds.min;

		Vector3 start = new Vector3(m_Start.x, m_Start.y);
		Vector3 end = new Vector3(m_End.x, m_End.y);

		for (int i = 0; i < m_Road.Count; ++i)
		{
			Vector3 center = new Vector3(m_Road[i].x, m_Road[i].y) + offset;

			Gizmos.DrawCube(center, size);
		}

		Color color = Gizmos.color;

		Gizmos.color = Color.red;
		Gizmos.DrawCube(start + offset, Vector3.one * 0.5f);
		Gizmos.color = Color.blue;
		Gizmos.DrawCube(end + offset, Vector3.one * 0.5f);

		Gizmos.color = color;
	}
}