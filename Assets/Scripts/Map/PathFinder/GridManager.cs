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
		TileBase[] allTile = m_Tilemap.GetTilesBlock(bounds);

		int width = bounds.size.x;
		int height = bounds.size.y;

		if (m_AStar == null)
			m_AStar = GetComponent<AStar>();

		m_Road = m_AStar.PathFinding(allTile, m_Start.x, m_Start.y, m_End.x, m_End.y, width, height);
	}


	private void OnDrawGizmos()
	{
		BoundsInt bounds = m_Tilemap.cellBounds;

		Vector3 size = Vector3.one * 0.5f;
		Vector3 offset = size + bounds.min + transform.position;

		Vector3 start = new Vector3(m_Start.x, m_Start.y);
		Vector3 end = new Vector3(m_End.x, m_End.y);

		Vector3 leftBottom = bounds.min;
		Vector3 rightBottom = bounds.min + new Vector3(bounds.size.x, 0f);
		Vector3 leftTop = bounds.min + new Vector3(0f, bounds.size.y);
		Vector3 rightTop = bounds.max;

		Gizmos.DrawLine(leftBottom, rightBottom);
		Gizmos.DrawLine(leftBottom, leftTop);
		Gizmos.DrawLine(leftTop, rightTop);
		Gizmos.DrawLine(rightBottom, rightTop);

		Color color = Gizmos.color;

		Gizmos.color = Color.red;
		Gizmos.DrawCube(start + offset, Vector3.one * 0.5f);
		Gizmos.color = Color.blue;
		Gizmos.DrawCube(end + offset, Vector3.one * 0.5f);

		Gizmos.color = color;

		if (m_Road == null)
			return;

		for (int i = 1; i < m_Road.Count - 1; ++i)
		{
			Vector3 center = new Vector3(m_Road[i].x, m_Road[i].y) + offset;

			Gizmos.DrawCube(center, size);
		}
	}
}