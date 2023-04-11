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
		int m = (width + height) * straight;
		return m;

		// 유클리드
		//int h = width * width + height * height;
		//return h;
	}
	//private void AddNearNode(Tilemap tilemap, Tilemap throughMap, Vector2Int size, CustomNode node, float maxJump)
	//{
	//	AddNearNode(tilemap, throughMap, size.x, size.y, node, maxJump);
	//}
	//private void AddNearNode(Tilemap tilemap, Tilemap throughMap, Vector3Int size, CustomNode node, float maxJump)
	//{
	//	AddNearNode(tilemap, throughMap, size.x, size.y, node, maxJump);
	//}
	private bool AddNearNode(Tilemap tilemap, Tilemap throughMap, ref CustomNode node, int maxJump)
	{
		BoundsInt tileBounds = tilemap.cellBounds;
		TileBase[] allTile = tilemap.GetTilesBlock(tileBounds);
		TileBase[] allThrough = throughMap.GetTilesBlock(tileBounds);

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
				// 노드 체크
				if (x == 0 && y == 0)
					continue;

				// 대각선 이동 옵션 체크
				if (!m_AllowDiagonal &&
					x != 0 && y != 0)
					continue;

				realX = node.position.x + x;
				realY = node.position.y + y;

				// 배열 범위 밖 체크
				if (realX < 0 || realX >= width ||
					realY < 0 || realY >= height)
					continue;

				index = realX + realY * width;
				index_down =
					(realY - 1) >= 0 ?
					realX + (realY - 1) * width :
					0;

				if (allTile[index] != null)
					continue;

				if (allThrough[index] != null)
					continue;

				CustomNode near = new CustomNode();
				near.position.Set(realX, realY);
				{
					//	near.currentJump = node.currentJump;
					//	near.isFalling = node.isFalling;

					//	if (node.currentJump > maxJump)
					//		near.isFalling = true;

					//	if (near.isFalling == true)
					//	{
					//		if (y >= 0)
					//			continue;
					//		else
					//			--near.currentJump;
					//	}
					//	else
					//	{
					//		if (y > 0)
					//			++near.currentJump;
					//	}

					//	if (allTile[index_down] != null ||
					//		throughMap.GetTile(new Vector3Int(realX, realY - 1)) != null)
					//	{
					//		if (y < 0)
					//		{
					//			near.currentJump = 0;
					//			near.isFalling = false;
					//		}
					//	}

					//	if (near.currentJump > maxJump)
					//		continue;

					//	if (allTile[index_down - x] == null &&
					//		throughMap.GetTile(new Vector3Int(realX - x, realY - 1)) == null)
					//	{
					//		if (y == 0)
					//		{
					//			continue;
					//		}
					//	}
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

			m_CloseList.Add(node);

			if (AddNearNode(tilemap, throughMap, ref node, maxJump))
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

			if (lhs.G > rhs.G)
				return 1;
			else if (lhs.G < rhs.G)
				return -1;

			return 0;
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
			return this.position == other.position /*&& this.currentJump == other.currentJump*/;
		}
	}

	private void OnDrawGizmos()
	{
		Vector3 size = Vector3.one * 0.5f;
		Vector3 offset = size;

		for (int i = 1; i < m_CloseList.Count; ++i)
		{
			Vector3 center = new Vector3(m_CloseList[i].position.x, m_CloseList[i].position.y) + offset;

			Gizmos.DrawCube(center, size);

			print(i + ": " + (center - offset));
		}

		//foreach (var item in m_OpenList)
		//{
		//	Vector3 center = new Vector3(item.position.x, item.position.y) + offset;

		//	Gizmos.DrawCube(center, size);
		//}
	}
}