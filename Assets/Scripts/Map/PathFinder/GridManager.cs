using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(JumpAStar))]
public class GridManager : Singleton<GridManager>
{
	public Tilemap m_Tilemap;
	public Tilemap m_ThroughMap;

	private JumpAStar m_AStar;

	public List<Vector2Int> m_Road;
	public Vector2Int m_Start;
	public Vector2Int m_End;

	private void OnValidate()
	{
		if (m_Tilemap != null)
		{
			m_Start.Clamp((Vector2Int)m_Tilemap.cellBounds.min, (Vector2Int)m_Tilemap.cellBounds.max - Vector2Int.one);
			m_End.Clamp((Vector2Int)m_Tilemap.cellBounds.min, (Vector2Int)m_Tilemap.cellBounds.max - Vector2Int.one);
		}
	}
	private void Awake()
	{
		//PriorityQueue<int, int> test;

		m_AStar = GetComponent<JumpAStar>();

		Test();
	}

	[ContextMenu("Test")]
	public void Test()
	{
		if (m_AStar == null)
			m_AStar = GetComponent<JumpAStar>();

		float s_time = Time.realtimeSinceStartup;
		m_Road = m_AStar.PathFinding(m_Tilemap, m_ThroughMap, m_Start.x, m_Start.y, m_End.x, m_End.y, 6);
		float e_time = Time.realtimeSinceStartup;

		print(e_time - s_time);
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

		for (int i = 0; i < m_Road.Count - 1; ++i)
		{
			//Vector3 center = new Vector3(m_Road[i].x, m_Road[i].y) + offset;

			//Gizmos.DrawCube(center, size);

			Vector3 from = new Vector3(m_Road[i].x, m_Road[i].y) + offset;
			Vector3 to = new Vector3(m_Road[i + 1].x, m_Road[i + 1].y) + offset;
			Gizmos.DrawLine(from, to);
		}
	}
}