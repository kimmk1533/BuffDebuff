using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class JumpAStar : MonoBehaviour
{
	const int straight = 10;
	const int diagonal = 14;

	PriorityQueue<CustomNode> m_OpenList = new PriorityQueue<CustomNode>();
	List<CustomNode> m_CloseList = new List<CustomNode>();

	public bool m_AllowDiagonal = true;

	private int Heuristic(Vector2Int start, Vector2Int end)
	{
		return Heuristic(start.x, start.y, end.x, end.y);
	}
	private int Heuristic(int sx, int sy, int ex, int ey)
	{
		int width = Mathf.Abs(sx - ex);
		int height = Mathf.Abs(sy - ey);

		// 맨해튼(Manhattan)
		int m = width * straight + height * straight;
		return m;
	}
	//private void AddNearNode(Tilemap tilemap, Tilemap throughMap, Vector2Int size, CustomNode node, float maxJump)
	//{
	//	AddNearNode(tilemap, throughMap, size.x, size.y, node, maxJump);
	//}
	//private void AddNearNode(Tilemap tilemap, Tilemap throughMap, Vector3Int size, CustomNode node, float maxJump)
	//{
	//	AddNearNode(tilemap, throughMap, size.x, size.y, node, maxJump);
	//}
	private void AddNearNode(Tilemap tilemap, Tilemap throughMap, CustomNode node, int maxJump)
	{
		List<CustomNode> nodeList = new List<CustomNode>();

		BoundsInt tileBounds = tilemap.cellBounds;
		TileBase[] allTile = tilemap.GetTilesBlock(tileBounds);

		int width = tileBounds.size.x;
		int height = tileBounds.size.y;

		int realX = node.position.x;
		int realY = node.position.y;
		int index = realX + realY * width;
		int index_down = realX + (realY - 1) * width;

		for (int y = -1; y <= 1; ++y)
		{
			for (int x = -1; x <= 1; ++x)
			{
				realX = node.position.x + x;
				realY = node.position.y + y;

				#region 예외처리
				// 배열 범위 밖 체크
				if (realX < 0 || realX >= width ||
					realY < 0 || realY >= height)
					continue;

				// 노드 체크
				if (y == 0 && x == 0)
					continue;

				// 대각선 이동 옵션 체크
				if (!m_AllowDiagonal &&
					x != 0 && y != 0)
					continue;
				#endregion

				index = realX + realY * width;
				index_down =
					(realY - 1) >= 0 ?
					realX + (realY - 1) * width :
					0;

				if (allTile[index] != null)
					continue;

				CustomNode near = new CustomNode();
				near.position.Set(realX, realY);
				near.currentJump = node.currentJump;
				near.isFalling = node.isFalling;

				if (node.currentJump > maxJump)
					near.isFalling = true;

				if (near.isFalling == true)
				{
					if (y >= 0)
						continue;
					else
						--near.currentJump;
				}
				else
				{
					if (y > 0)
						++near.currentJump;
				}

				if (allTile[index_down] != null ||
					throughMap.GetTile(new Vector3Int(realX, realY - 1)) != null)
				{
					if (y < 0)
					{
						near.currentJump = 0;
						near.isFalling = false;
					}
				}

				if (near.currentJump > maxJump)
					continue;

				if (allTile[index_down - x] == null &&
					throughMap.GetTile(new Vector3Int(realX - x, realY - 1)) == null)
				{
					if (y == 0)
					{
						continue;
					}
				}

				// 닫힌 리스트에 포함되어 있는 경우
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

	public List<Vector2Int> PathFinding(Tilemap tilemap, Tilemap throughMap, Vector2Int start, Vector2Int end, int maxJump)
	{
		return PathFinding(tilemap, throughMap, start.x, start.y, end.x, end.y, maxJump);
	}
	public List<Vector2Int> PathFinding(Tilemap tilemap, Tilemap throughMap, int sx, int sy, int ex, int ey, int maxJump)
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

			if (node.position.x == ex && node.position.y == ey)
				break;

			m_CloseList.Add(node);

			AddNearNode(tilemap, throughMap, node, maxJump);
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
		public Vector2Int position;
		public Vector2Int start;
		public Vector2Int end;

		public int currentJump;
		public bool isFalling;

		public bool Equals(CustomNode other)
		{
			return this.position == other.position &&
				this.currentJump == other.currentJump;
		}
	}
}