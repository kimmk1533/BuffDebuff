using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class AStar : MonoBehaviour
{
	const int straight = 10;
	const int diagonal = 14;

	PriorityQueue<CustomNode> m_OpenList = new PriorityQueue<CustomNode>();
	List<CustomNode> m_CloseList = new List<CustomNode>();

	[SerializeField]
	private bool m_AllowDiagonal = true;

	private int Heuristic(Vector2Int start, Vector2Int end)
	{
		return Heuristic(start.x, start.y, end.x, end.y);
	}
	private int Heuristic(int sx, int sy, int ex, int ey)
	{
		int width = Mathf.Abs(sx - ex);
		int height = Mathf.Abs(sy - ey);

		// 맨해튼(Manhattan)
		int m = (width + height) * straight;
		return m;

		// 유클리드
		//int h = width * width + height * height;
		//return h;
	}
	private bool AddNearNode(Tilemap tilemap, ref CustomNode node)
	{
		BoundsInt tileBounds = tilemap.cellBounds;
		TileBase[] allTile = tilemap.GetTilesBlock(tileBounds);

		int width = tileBounds.size.x;
		int height = tileBounds.size.y;

		for (int y = -1; y <= 1; ++y)
		{
			for (int x = -1; x <= 1; ++x)
			{
				// 노드 체크
				if (x == 0 && y == 0)
					continue;

				if (!m_AllowDiagonal &&
					x != 0 && y != 0)
					continue;

				int realX = node.position.x + x;
				int realY = node.position.y + y;

				// 배열 범위 밖 체크
				if (realX < 0 || realX >= width ||
					realY < 0 || realY >= height)
					continue;

				int index = realX + realY * width;

				if (allTile[index] != null)
					continue;

				CustomNode near = new CustomNode();
				near.position.Set(realX, realY);

				if (m_CloseList.Contains(near))
					continue;

				near.start = node.start;
				near.end = node.end;

				near.G = node.G;
				near.H = Heuristic(near.position, near.end);
				near.parent = node;

				// 직선 이동
				if (x == 0 || y == 0)
					near.G += straight;
				// 대각선 이동
				else
					near.G += diagonal;

				if (near.position == near.end)
				{
					node = near;

					return true;
				}

				bool flag = false;

				foreach (var item in m_OpenList)
				{
					if (item.Equals(near))
					{
						flag = true;

						if (item.G > near.G)
						{
							item.G = near.G;
							item.parent = near.parent;
						}

						break;
					}
				}

				if (!flag)
					m_OpenList.Enqueue(near);
			}
		}

		return false;
	}

	public List<CustomNode> PathFinding(Tilemap tilemap, Vector2Int start, Vector2Int end)
	{
		return PathFinding(tilemap, start.x, start.y, end.x, end.y);
	}
	public List<CustomNode> PathFinding(Tilemap tilemap, int sx, int sy, int ex, int ey)
	{
		m_OpenList.Clear();
		m_CloseList.Clear();

		CustomNode start = new CustomNode();
		start.position.x = start.start.x = sx;
		start.position.y = start.start.y = sy;
		start.end.x = ex;
		start.end.y = ey;
		start.G = 0;
		start.H = Heuristic(sx, sy, ex, ey);

		m_OpenList.Enqueue(start);
		CustomNode node = null;

		while (m_OpenList.Count != 0)
		{
			node = m_OpenList.Dequeue();

			m_CloseList.Add(node);

			if (AddNearNode(tilemap, ref node))
				break;
		}

		if (node.position.x != ex || node.position.y != ey)
			return null;

		List<CustomNode> result = new List<CustomNode>();
		while (node != null)
		{
			result.Add(node);
			node = (CustomNode)node.parent;
		}
		return result;
	}

	private void OnDrawGizmos()
	{
		if (GridManager.Instance.m_Jump)
			return;

		Vector3 size = Vector3.one * 0.5f;
		Vector3 offset = size;

		Color color = Gizmos.color;
		Gizmos.color = Color.green * new Color(1f, 1f, 1f, 0.5f);
		foreach (var item in m_OpenList)
		{
			if (item.position == item.start ||
				item.position == item.end)
				continue;

			Vector3 center = new Vector3(item.position.x, item.position.y) + offset;

			Gizmos.DrawCube(center, size);
		}
		Gizmos.color = Color.cyan * new Color(1f, 1f, 1f, 0.5f);
		for (int i = 1; i < m_CloseList.Count; ++i)
		{
			CustomNode item = m_CloseList[i];

			if (item.position == item.start ||
				item.position == item.end)
				continue;

			Vector3 center = new Vector3(item.position.x, item.position.y) + offset;

			Gizmos.DrawCube(center, size);
		}
		Gizmos.color = color;
	}
}