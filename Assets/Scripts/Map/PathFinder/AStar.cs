using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class AStar : MonoBehaviour
{
	PriorityQueue<CustomNode> m_OpenList = new PriorityQueue<CustomNode>();
	List<CustomNode> m_CloseList = new List<CustomNode>();

	public bool m_IsManhatten = false;

	private int Heuristic(int sx, int sy, int ex, int ey)
	{
		// 맨해튼(Manhattan)
		if (m_IsManhatten)
		{
			int m = Mathf.Abs(sx - ex) + Mathf.Abs(sy - ey);
			return m * m;
		}

		// 유클리드(Euclidean)
		int h = (sx - ex) * (sx - ex) + (sy - ey) * (sy - ey);
		return h;
	}
	private void AddNearNode(in TileBase[] tiles, int width, int height, CustomNode node)
	{
		List<CustomNode> nodeList = new List<CustomNode>();

		for (int y = -1; y <= 1; ++y)
		{
			for (int x = -1; x <= 1; ++x)
			{
				#region 예외처리
				// 배열 범위 밖 체크
				if (node.x + x < 0 || node.x + x >= width ||
					node.y + y < 0 || node.y + y >= height)
					continue;
				// 노드 체크
				if (y == 0 && x == 0)
					continue;
				#endregion

				int realX = node.x + x;
				int realY = node.y + y;

				int index = realX + realY * width;

				if (tiles[index] != null)
					continue;

				CustomNode near = new CustomNode();
				near.x = realX; near.y = realY;

				if (m_CloseList.Contains(near))
					continue;

				near.sx = node.sx; near.sy = node.sy;
				near.ex = node.ex; near.ey = node.ey;

				near.G = node.G;
				near.H = Heuristic(realX, realY, node.ex, node.ey);
				near.parent = node;

				// 직선 이동
				if (x == 0 || y == 0)
					near.G += 10 * 10;
				// 대각선 이동
				else
					near.G += 14 * 14;

				nodeList.Add(near);
			}
		}

		foreach (var item in m_OpenList)
		{
			foreach (var near in nodeList)
			{
				if (item.Equals(near))
				{
					item.G = Mathf.Min(item.G, near.G);
					nodeList.Remove(near);

					break;
				}
			}
		}

		if (nodeList.Count > 0)
			m_OpenList.EnqueueRange(nodeList);
	}
	public List<Vector2Int> PathFinding(TileBase[] tiles, int sx, int sy, int ex, int ey, int width, int height)
	{
		m_OpenList.Clear();
		m_CloseList.Clear();

		CustomNode start = new CustomNode();
		start.x = start.sx = sx;
		start.y = start.sy = sy;
		start.ex = ex;
		start.ey = ey;
		start.G = 0;
		start.H = Heuristic(sx, sy, ex, ey);

		m_OpenList.Enqueue(start);
		CustomNode node = null;

		while (m_OpenList.Count != 0)
		{
			node = m_OpenList.Dequeue();

			if (node.x == ex &&
				node.y == ey)
				break;

			m_CloseList.Add(node);

			AddNearNode(in tiles, width, height, node);
		}

		List<Vector2Int> result = new List<Vector2Int>();
		while (node != null)
		{
			Vector2Int pos = new Vector2Int(node.x, node.y);
			result.Add(pos);
			node = (CustomNode)node.parent;
		}
		return result;
	}

	public class Node : IComparer<Node>
	{
		public int F => G + H;

		// 시작점으로부터 현재 위치까지 이동하기 위한 비용
		public int G;
		// 현재 위치부터 도착 위치까지 예상 비용
		public int H; // 휴리스틱

		public Node parent;

		public int Compare(Node lhs, Node rhs)
		{
			if (lhs.F < rhs.F)
				return -1;
			else if (lhs.F > rhs.F)
				return 1;

			return 0;
		}

		public Node()
		{
			G = 0;
			H = int.MaxValue;
			parent = null;
		}
		public Node(int g, int h, Node node)
		{
			G = g;
			H = h;
			parent = node;
		}

		public override string ToString()
		{
			return "F: " + F + " | G: " + G + ", H: " + H;
		}
	}
	public class CustomNode : Node, IEquatable<CustomNode>
	{
		public int x, y;
		public int sx, sy;
		public int ex, ey;

		public bool Equals(CustomNode other)
		{
			return this.x == other.x && this.y == other.y;
		}
	}
}