using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class Node : IComparer<Node>, IEquatable<Node>
{
	public int F => G + H;

	// 시작점으로부터 현재 위치까지 이동하기 위한 비용
	public int G;
	// 현재 위치부터 도착 위치까지 예상 비용
	public int H; // 휴리스틱 함수

	public Node parent;

	public Vector2Int position;
	public Vector2Int start;
	public Vector2Int end;

	public Node()
	{
		G = 0;
		H = int.MaxValue;
		parent = null;

		position = new Vector2Int();
		start = new Vector2Int();
		end = new Vector2Int();
	}
	public Node(int g, int h, Node node)
	{
		G = g;
		H = h;
		parent = node;

		position = new Vector2Int();
		start = new Vector2Int();
		end = new Vector2Int();
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
	public bool Equals(Node other)
	{
		return this.position == other.position;
	}
}

public class AStar : MonoBehaviour
{
	const int straight = 10;
	const int diagonal = 14;

	PriorityQueue<Node> m_OpenList = new PriorityQueue<Node>();
	List<Node> m_CloseList = new List<Node>();

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
	private bool AddNearNode(Tilemap tilemap, ref Node node)
	{
		BoundsInt tileBounds = tilemap.cellBounds;
		TileBase[] allTile = tilemap.GetTilesBlock(tileBounds);

		int width = tileBounds.size.x;
		int height = tileBounds.size.y;
		Vector2Int position = new Vector2Int();

		for (int y = -1; y <= 1; ++y)
		{
			for (int x = -1; x <= 1; ++x)
			{
				// 노드 체크
				if (x == 0 && y == 0)
					continue;

				// 대각선 체크
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

				position.Set(realX, realY);
				Node near = new Node();
				near.G = node.G;
				near.H = Heuristic(position, node.end);

				near.position = position;
				near.start = node.start;
				near.end = node.end;

				near.parent = node;

				if (m_CloseList.Contains(near) == true)
					continue;

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

				// 오픈리스트에 없는 경우
				if (flag == false)
				{
					// 오픈리스트에 추가
					m_OpenList.Enqueue(near);
				}
			}
		}

		return false;
	}

	public List<Node> PathFinding(Tilemap tilemap, Vector2Int start, Vector2Int end)
	{
		return PathFinding(tilemap, start.x, start.y, end.x, end.y);
	}
	public List<Node> PathFinding(Tilemap tilemap, int sx, int sy, int ex, int ey)
	{
		m_OpenList.Clear();
		m_CloseList.Clear();

		Node root = new Node();
		root.G = 0;
		root.H = Heuristic(sx, sy, ex, ey);
		root.position.Set(sx, sy);
		root.start.Set(sx, sy);
		root.end.Set(ex, ey);

		m_OpenList.Enqueue(root);

		Node node = null;

		while (m_OpenList.Count != 0)
		{
			node = m_OpenList.Dequeue();

			m_CloseList.Add(node);

			if (AddNearNode(tilemap, ref node))
				break;
		}

		if (node.position.x != ex || node.position.y != ey)
			return null;

		List<Node> result = new List<Node>();
		while (node != null)
		{
			result.Add(node);
			node = node.parent;
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

		if (GridManager.Instance.m_ShowOpenList)
		{
			Gizmos.color = Color.green * new Color(1f, 1f, 1f, 0.5f);
			foreach (var item in m_OpenList)
			{
				if (item.position == item.start ||
					item.position == item.end)
					continue;

				Vector3 center = new Vector3(item.position.x, item.position.y) + offset;

				Gizmos.DrawCube(center, size);
			}
		}

		if (GridManager.Instance.m_ShowCloseList)
		{
			Gizmos.color = Color.cyan * new Color(1f, 1f, 1f, 0.5f);
			for (int i = 1; i < m_CloseList.Count; ++i)
			{
				Node item = m_CloseList[i];

				if (item.position == item.start ||
					item.position == item.end)
					continue;

				Vector3 center = new Vector3(item.position.x, item.position.y) + offset;

				Gizmos.DrawCube(center, size);
			}
		}

		Gizmos.color = color;
	}
}