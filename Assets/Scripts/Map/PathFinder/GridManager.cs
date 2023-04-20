using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

//[RequireComponent(typeof(JumpAStar))]
public class GridManager : Singleton<GridManager>
{
	public Tilemap m_Tilemap;
	public Tilemap m_ThroughMap;

	public bool m_Jump;
	public bool m_ShowOpenList;
	public bool m_ShowCloseList;

	private AStar m_AStar;
	private JumpAStar m_JumpAStar;

	List<Node> m_Road;
	List<CustomNode> m_JumpRoad;

	public Vector2Int m_Start;
	public Vector2Int m_End;

	protected StageManager M_Stage => StageManager.Instance;

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

		m_AStar = GetComponent<AStar>();
		m_JumpAStar = GetComponent<JumpAStar>();

		m_Road = new List<Node>();
		m_JumpRoad = new List<CustomNode>();
	}
	private void Start()
	{
		m_Tilemap = M_Stage.currentRoom.Find("TileMapLayer").Find("TileMap").GetComponent<Tilemap>();
		m_ThroughMap = M_Stage.currentRoom.Find("TileMapLayer").Find("ThroughMap").GetComponent<Tilemap>();
	}

	public List<CustomNode> PathFinding(Vector3 start, Vector3 end, int maxJump)
	{
		Vector2Int tileStart = (Vector2Int)m_Tilemap.WorldToCell(start);
		Vector2Int tileEnd = (Vector2Int)m_Tilemap.WorldToCell(end);

		if (tileStart == tileEnd)
			return null;

		if (m_Start == tileStart &&
			m_End == tileEnd)
			return m_JumpRoad;

		m_Start = tileStart;
		m_End = tileEnd;

		//m_Road = m_AStar.PathFinding(m_Tilemap, tileStart, tileEnd);

		//m_JumpRoad.Clear();
		//foreach (Node node in m_Road)
		//{
		//	CustomNode customNode = new CustomNode(node);
		//	m_JumpRoad.Add(customNode);
		//}

		m_JumpRoad = m_JumpAStar.PathFinding(m_Tilemap, m_ThroughMap, tileStart, tileEnd, maxJump, 5);

		m_JumpRoad.Reverse();
		m_JumpRoad.RemoveAt(0);

		return m_JumpRoad;
	}


	private void OnDrawGizmos()
	{
		if (m_Tilemap == null)
			return;

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

		// 시작점
		Gizmos.color = Color.red;
		Gizmos.DrawCube(start + offset, Vector3.one * 0.5f);

		// 도착점
		Gizmos.color = Color.blue;
		Gizmos.DrawCube(end + offset, Vector3.one * 0.5f);

		if (!m_Jump && m_Road == null)
			return;
		if (m_Jump && m_JumpRoad == null)
			return;

		float width = 0.01f;

		Gizmos.color = Color.yellow;

		int Count = (m_Jump) ? m_JumpRoad.Count : m_Road.Count;

		for (int i = 0; i < Count - 1; ++i)
		{
			for (float y = -0.05f; y <= 0.05f; y += width)
			{
				for (float x = -0.05f; x <= 0.05f; x += width)
				{
					float realX = ((m_Jump) ? m_JumpRoad[i].position.x : m_Road[i].position.x) + x;
					float realY = ((m_Jump) ? m_JumpRoad[i].position.y : m_Road[i].position.y) + y;

					Vector3 from = new Vector3(realX, realY) + offset;

					realX = ((m_Jump) ? m_JumpRoad[i + 1].position.x : m_Road[i + 1].position.x) + x;
					realY = ((m_Jump) ? m_JumpRoad[i + 1].position.y : m_Road[i + 1].position.y) + y;
					Vector3 to = new Vector3(realX, realY) + offset;
					Gizmos.DrawLine(from, to);
				}
			}
		}

		Gizmos.color = color;
	}
}