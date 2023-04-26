using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class CustomNode : Node, IComparer<CustomNode>, IEquatable<CustomNode>
{
	public Vector2Int? jumpStartPos;

	public int currentJump;
	public bool isFalling;

	public CustomNode() : base()
	{
		jumpStartPos = null;
	}
	public CustomNode(Node node)
	{
		G = node.G;
		H = node.H;
		parent = node.parent;

		position = node.position;
		start = node.start;
		end = node.end;

		CustomNode customNode = node as CustomNode;

		if (customNode != null)
		{
			jumpStartPos = customNode.jumpStartPos;
			currentJump = customNode.currentJump;
			isFalling = customNode.isFalling;
		}
	}
	public override string ToString()
	{
		return "(" + x + ", " + y + ") | " + base.ToString();
	}
	public int Compare(CustomNode lhs, CustomNode rhs)
	{
		float lF = lhs.F + lhs.currentJump * JumpAStar.straight;
		float rF = rhs.F + rhs.currentJump * JumpAStar.straight;

		if (lF > rF)
			return 1;
		else if (lF < rF)
			return -1;

		if (lhs.G > rhs.G)
			return 1;
		else if (lhs.G < rhs.G)
			return -1;

		return 0;
	}
	public bool Equals(CustomNode other)
	{
		if (!base.Equals(other))
			return false;

		if (isFalling != other.isFalling)
			return false;

		if (jumpStartPos != other.jumpStartPos)
			return false;

		CustomNode nodeParent = this.parent as CustomNode;
		CustomNode otherParent = other.parent as CustomNode;

		while (nodeParent != null &&
			otherParent != null)
		{
			if (nodeParent.position != otherParent.position)
				return false;

			nodeParent = nodeParent.parent as CustomNode;
			otherParent = otherParent.parent as CustomNode;
		}

		return true;
	}
	public static CustomNode Reverse(ref CustomNode customNode)
	{
		Stack<CustomNode> nodeStack = new Stack<CustomNode>();
		CustomNode root;
		CustomNode node = customNode;

		while (node != null)
		{
			nodeStack.Push(node);
			node = node.parent as CustomNode;
		}

		customNode = root = node = nodeStack.Pop();

		while (nodeStack.Count > 0)
		{
			node.parent = nodeStack.Pop();
			node = node.parent as CustomNode;
		}
		node.parent = null;

		return root;
	}
}

public class JumpAStar : MonoBehaviour
{
	public const int straight = 10;
	public const int diagonal = 14;

	[SerializeField]
	private bool m_AllowDiagonal = true;

	PriorityQueue<CustomNode> m_OpenList = new PriorityQueue<CustomNode>();
	List<CustomNode> m_CloseList = new List<CustomNode>();

	float m_StartTime;
	float m_EndTime;

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
	private bool AddNearNode(Tilemap tilemap, Tilemap throughMap, ref CustomNode node, int maxJump)
	{
		if (node.position == node.end)
		{
			return true;
		}

		BoundsInt tileBounds = tilemap.cellBounds;
		TileBase[] allTile = tilemap.GetTilesBlock(tileBounds);
		TileBase[] allThrough = throughMap.GetTilesBlock(tileBounds);

		int width = tileBounds.size.x;
		int height = tileBounds.size.y;

		for (int y = -1; y <= 1; ++y)
		{
			for (int x = 1; x >= -1; --x)
			{
				// 노드 체크
				if (x == 0 && y == 0)
					continue;

				// 대각선 이동 옵션 체크
				if (m_AllowDiagonal == false &&
					x != 0 && y != 0)
					continue;

				int realX = node.x + x;
				int realY = node.y + y;

				// 배열 범위 밖 체크
				if (realX < 0 || realX >= width ||
					realY < 0 || realY >= height)
					continue;

				int index = realX + realY * width;

				if (allTile[index] != null)
					continue;

				//TileBase tile = tilemap.GetTile(new Vector3Int(realX, realY));
				//TileBase tileDown = tilemap.GetTile(new Vector3Int(realX, realY - 1));

				//TileBase through = throughMap.GetTile(new Vector3Int(realX, realY));
				//TileBase throughDown = throughMap.GetTile(new Vector3Int(realX, realY - 1));

				//if (tile != null)
				//	continue;

				CustomNode near = new CustomNode();
				near.position.Set(realX, realY);

				if (node.parent != null &&
					node.parent.position == near.position)
					continue;

				// Jump
				{
					near.currentJump = node.currentJump;
					near.isFalling = node.isFalling;
					near.jumpStartPos = node.jumpStartPos;

					if (y < 0)
						near.isFalling = true;

					// 점프
					if (near.isFalling == true)
					{
						if (y >= 0)
							continue;

						--near.currentJump;
					}
					else
					{
						if (y > 0)
						{
							++near.currentJump;

							// 최대 점프 높이 이상 점프 금지
							if (near.currentJump >= maxJump)
								continue;

							if (near.jumpStartPos == null)
							{
								near.jumpStartPos = node.position;
							}
						}
					}

					int index_down = realX + (realY - 1) * width;

					// 이동할 곳이 비어있고
					if (allTile[index] == null && allThrough[index] == null)
					{
						// 이동할 곳 밑에 블럭이 있으면
						if (allTile[index_down] != null || allThrough[index_down] != null)
						{
							// 착지
							near.currentJump = 0;
							near.isFalling = false;
							near.jumpStartPos = null;
						}
						// 공중 이동 금지
						else if (y == 0)
							continue;
					}
					// 공중 이동 금지
					else if (y == 0)
						continue;
				}

				// 닫힌 리스트에 포함되어 있는 경우
				if (m_CloseList.Contains(near))
					continue;

				near.G = node.G + ((x == 0 || y == 0) ? straight : diagonal);
				near.H = Heuristic(near.position, node.end);

				near.start = node.start;
				near.end = node.end;
				near.parent = node;

				bool flag = false;
				foreach (var item in m_OpenList)
				{
					if (item.Equals(near))
					{
						flag = true;

						if (item.G > near.G)
						{
							CustomNode itemParent = item.parent as CustomNode;
							node = near.parent as CustomNode;
							while (node.jumpStartPos != null)
							{
								if (itemParent.position != node.position)
									break;

								itemParent.G = node.G;
								itemParent.jumpStartPos = node.jumpStartPos;
								itemParent.currentJump = node.currentJump;
								itemParent.isFalling = node.isFalling;

								itemParent = item.parent as CustomNode;
								node = node.parent as CustomNode;
							}

							item.G = near.G;
							item.parent = near.parent;

							item.currentJump = near.currentJump;
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

	public CustomNode PathFinding(Tilemap tilemap, Tilemap throughMap, Vector2Int start, Vector2Int end, int maxJump)
	{
		return PathFinding(tilemap, throughMap, start.x, start.y, end.x, end.y, maxJump);
	}
	public CustomNode PathFinding(Tilemap tilemap, Tilemap throughMap, int sx, int sy, int ex, int ey, int maxJump)
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

		m_StartTime = Time.realtimeSinceStartup;

		while (m_OpenList.Count != 0)
		{
			node = m_OpenList.Dequeue();

			m_CloseList.Add(node);

			if (AddNearNode(tilemap, throughMap, ref node, maxJump))
				break;

			m_EndTime = Time.realtimeSinceStartup;
			// 타임오버 (임시로 0.3초로 설정)
			if (m_EndTime - m_StartTime > 0.3f)
			{
				m_CloseList.Sort(node);
				return m_CloseList[0];
			}
		}

		if (node.x != ex || node.y != ey)
			return null;

		//List<CustomNode> result = new List<CustomNode>();
		//while (node != null)
		//{
		//	result.Add(node);
		//	node = node.parent as CustomNode;
		//}

		return node;
	}

	private void OnValidate()
	{
		GridManager.Instance.Test();
	}
	private void OnDrawGizmos()
	{
		if (GridManager.Instance.m_Jump == false)
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

				Vector3 center = new Vector3(item.x, item.y) + offset;

				Gizmos.DrawCube(center, size);
			}
		}

		if (GridManager.Instance.m_ShowCloseList)
		{
			Gizmos.color = Color.cyan * new Color(1f, 1f, 1f, 0.5f);
			for (int i = 1; i < m_CloseList.Count; ++i)
			{
				CustomNode item = m_CloseList[i];

				if (item.position == item.start ||
					item.position == item.end)
					continue;

				Vector3 center = new Vector3(item.x, item.y) + offset;

				Gizmos.DrawCube(center, size);
			}
		}

		Gizmos.color = color;
	}
}