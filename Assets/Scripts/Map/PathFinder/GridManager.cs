using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

//[RequireComponent(typeof(JumpAStar))]
public class GridManager : Singleton<GridManager>
{
	public bool m_ShowGrid;
	public Color m_GridColor;

	[Space(10)]

	public bool m_ShowGridText;
	public Color m_GridTextColor;
	public Transform m_GridTextParent;

	[Space(10)]

	public bool m_AutoPathFinding;

	public Tilemap m_Tilemap;
	public Tilemap m_ThroughMap;

	[Space(10)]

	public bool m_Jump;
	public bool m_ShowOpenList;
	public bool m_ShowCloseList;

	private AStar m_AStar;
	private JumpAStar m_JumpAStar;

	Node m_Road;
	CustomNode m_JumpRoad;

	[Space(10)]

	public bool m_ShowPath;
	public bool m_ShowStartPoint;
	public bool m_ShowEndPoint;
	public Vector2Int m_Start;
	public Vector2Int m_End;

	protected StageManager M_Stage => StageManager.Instance;

	private void OnValidate()
	{
		if (m_GridTextParent != null)
			m_GridTextParent.gameObject.SetActive(m_ShowGridText);

		if (m_Tilemap != null)
		{
			m_Start.Clamp((Vector2Int)m_Tilemap.cellBounds.min, (Vector2Int)m_Tilemap.cellBounds.max - Vector2Int.one);
			m_End.Clamp((Vector2Int)m_Tilemap.cellBounds.min, (Vector2Int)m_Tilemap.cellBounds.max - Vector2Int.one);

			if (m_AutoPathFinding)
			{
				Test();
			}
		}
	}
	private void Awake()
	{
		//PriorityQueue<int, int> test;

		m_AStar = GetComponent<AStar>();
		m_JumpAStar = GetComponent<JumpAStar>();
	}
	private void Start()
	{
		m_Tilemap = M_Stage.currentRoom.Find("TileMapLayer").Find("TileMap").GetComponent<Tilemap>();
		m_ThroughMap = M_Stage.currentRoom.Find("TileMapLayer").Find("ThroughMap").GetComponent<Tilemap>();
	}

	[ContextMenu("Create Grid Text")]
	public void CreateGridText()
	{
		if (m_Tilemap == null)
			return;
		if (m_GridTextParent == null)
			return;

		BoundsInt boundsInt = m_Tilemap.cellBounds;

		Vector3Int min = boundsInt.min;
		Vector3Int max = boundsInt.max;

		DestroyGridText();

		for (int y = min.y; y < max.y; y++)
		{
			for (int x = min.x; x < max.x; x++)
			{
				Vector2 pos = new Vector2(x, y) + Vector2.one * 0.5f;
				string text = new Vector2Int(x, y).ToString();

				UtilClass.CreateWorldText(text, m_GridTextParent, pos, 0.1f, 25, m_GridTextColor, TextAnchor.MiddleCenter);
			}
		}
	}
	[ContextMenu("Destroy Grid Text")]
	public void DestroyGridText()
	{
		if (m_GridTextParent == null)
			return;

		int count = m_GridTextParent.childCount;
		for (int i = 0; i < count; ++i)
		{
			DestroyImmediate(m_GridTextParent.GetChild(0).gameObject);
		}
	}
	[ContextMenu("Test")]
	public void Test()
	{
		if (!m_Jump && m_AStar == null)
			m_AStar = GetComponent<AStar>();
		if (m_Jump && m_JumpAStar == null)
			m_JumpAStar = GetComponent<JumpAStar>();

		if (m_Start == m_End)
			return;

		if (m_Tilemap.GetTile((Vector3Int)m_Start) != null ||
			m_Tilemap.GetTile((Vector3Int)m_End) != null)
			return;

		float s = Time.realtimeSinceStartup;
		if (!m_Jump)
		{
			m_Road = m_AStar.PathFinding(m_Tilemap, m_Start, m_End);
		}
		else
		{
			Vector3 start = new Vector3(m_Start.x, m_Start.y);
			Vector3 end = new Vector3(m_End.x, m_End.y);

			PathFinding(start, end, 6);
			//m_JumpRoad = m_JumpAStar.PathFinding(m_Tilemap, m_ThroughMap, m_Start, m_End, 6);
		}
		float e = Time.realtimeSinceStartup;
		Debug.Log(e - s);
	}
	public CustomNode PathFinding(Vector3 start, Vector3 end, int maxJump)
	{
		Vector2Int tileStart = (Vector2Int)m_Tilemap.WorldToCell(start);
		Vector2Int tileEnd = (Vector2Int)m_Tilemap.WorldToCell(end);

		//tileStart.y = tileStart.y + 1;
		//tileEnd.y = tileEnd.y + 1;

		if (m_Tilemap.GetTile((Vector3Int)tileStart) != null ||
			m_Tilemap.GetTile((Vector3Int)tileEnd) != null)
			return null;

		if (tileStart == tileEnd)
			return null;

		if (Application.isEditor == false &&
			m_Start == tileStart &&
			m_End == tileEnd)
			return m_JumpRoad;

		m_Start = tileStart;
		m_End = tileEnd;

		m_JumpRoad = m_JumpAStar.PathFinding(m_Tilemap, m_ThroughMap, tileStart, tileEnd, maxJump);

		CustomNode.Reverse(ref m_JumpRoad);

		return m_JumpRoad;
	}

	private void OnDrawGizmos()
	{
		if (m_Tilemap == null)
			return;

		float width = m_Tilemap.cellSize.x;
		float height = m_Tilemap.cellSize.y;

		if (width <= 0 || height <= 0)
			return;

		Color color = Gizmos.color;

		BoundsInt bounds = m_Tilemap.cellBounds;
		Vector3Int min = bounds.min;
		Vector3Int max = bounds.max;

		if (m_ShowGrid)
		{
			Gizmos.color = m_GridColor;

			for (float y = min.y; y <= max.y; y += height)
			{
				Gizmos.DrawLine(new Vector3(min.x, Mathf.Floor(y / height) * height, 0f), new Vector3(max.x, Mathf.Floor(y / height) * height, 0f));
			}

			for (float x = min.x; x <= max.x; x += width)
			{
				Gizmos.DrawLine(new Vector3(Mathf.Floor(x / width) * width, min.y, 0f), new Vector3(Mathf.Floor(x / width) * width, max.y, 0f));
			}

			Gizmos.color = color;
		}

		Vector3 size = Vector3.one * 0.5f;
		Vector3 offset = size + min + transform.position;

		Vector3 start = new Vector3(m_Start.x, m_Start.y);
		Vector3 end = new Vector3(m_End.x, m_End.y);

		Vector3 leftBottom = min;
		Vector3 rightBottom = min + new Vector3(bounds.size.x, 0f);
		Vector3 leftTop = min + new Vector3(0f, bounds.size.y);
		Vector3 rightTop = max;

		Gizmos.DrawLine(leftBottom, rightBottom);
		Gizmos.DrawLine(leftBottom, leftTop);
		Gizmos.DrawLine(leftTop, rightTop);
		Gizmos.DrawLine(rightBottom, rightTop);

		color = Gizmos.color;

		// 시작점
		if (m_ShowStartPoint)
		{
			Gizmos.color = Color.red;
			Gizmos.DrawCube(start + offset + ((Application.isPlaying) ? Vector3.down : Vector3.zero), Vector3.one * 0.5f);
		}

		// 도착점
		if (m_ShowEndPoint)
		{
			Gizmos.color = Color.blue;
			Gizmos.DrawCube(end + offset + ((Application.isPlaying) ? Vector3.down : Vector3.zero), Vector3.one * 0.5f);
		}

		if (m_ShowPath == false)
			return;

		if (m_Jump == false && m_Road == null)
			return;
		if (m_Jump && m_JumpRoad == null)
			return;

		width = 0.005f;
		Node node = m_Jump ? m_JumpRoad : m_Road;

		Gizmos.color = Color.yellow;
		while (node.parent != null)
		{
			for (float y = -0.05f; y <= 0.05f; y += width)
			{
				for (float x = -0.05f; x <= 0.05f; x += width)
				{
					float realX = node.x + x;
					float realY = node.y + y;
					Vector3 from = new Vector3(realX, realY) + offset;

					realX = node.parent.x + x;
					realY = node.parent.y + y;
					Vector3 to = new Vector3(realX, realY) + offset;

					Gizmos.DrawLine(from, to);
				}
			}

			node = node.parent;
		}
		Gizmos.color = color;
	}
}