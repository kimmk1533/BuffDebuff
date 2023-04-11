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
	private void AddNearNode(in TileBase[] tiles, Vector2Int size, ref CustomNode node)
	{
		AddNearNode(in tiles, size.x, size.y, ref node);
	}
	private bool AddNearNode(in TileBase[] tiles, int width, int height, ref CustomNode node)
	{
		for (int y = -1; y <= 1; ++y)
		{
			for (int x = -1; x <= 1; ++x)
			{
				int realX = node.position.x + x;
				int realY = node.position.y + y;

				#region 예외처리
				// 배열 범위 밖 체크
				if (realX < 0 || realX >= width ||
					realY < 0 || realY >= height)
					continue;
				// 노드 체크
				if (y == 0 && x == 0)
					continue;

				if (!m_AllowDiagonal &&
					x != 0 && y != 0)
					continue;
				#endregion

				int index = realX + realY * width;

				if (tiles[index] != null)
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

				foreach (var item in m_OpenList)
				{
					if (item.Equals(near))
					{
						item.G = Mathf.Min(item.G, near.G);

						break;
					}
				}

				m_OpenList.Enqueue(near);
			}
		}

		return false;
	}

	public List<Vector2Int> PathFinding(TileBase[] tiles, Vector2Int start, Vector2Int end, Vector2Int size)
	{
		return PathFinding(tiles, start.x, start.y, end.x, end.y, size.x, size.y);
	}
	public List<Vector2Int> PathFinding(TileBase[] tiles, int sx, int sy, int ex, int ey, int width, int height)
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

			if (AddNearNode(in tiles, width, height, ref node))
				break;
		}

		if (node.position.x != ex || node.position.y != ey)
			return null;

		List<Vector2Int> result = new List<Vector2Int>();
		while (node != null)
		{
			Vector2Int pos = node.position;
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
		public int H; // 휴리스틱 함수

		public Node parent;

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
		public int Compare(Node lhs, Node rhs)
		{
			if (lhs.F > rhs.F)
				return 1;
			else if (lhs.F < rhs.F)
				return -1;

			return 0;
		}
	}
	public class CustomNode : Node, IEquatable<CustomNode>
	{
		public Vector2Int position;
		public Vector2Int start;
		public Vector2Int end;

		public bool Equals(CustomNode other)
		{
			return this.position == other.position;
		}
	}
}