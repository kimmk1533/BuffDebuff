using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class CustomNode : Node, IEquatable<CustomNode>
{
	public Vector2Int? jumpStartPos;

	public int currentJump;
	public bool isFalling;

	public CustomNode() : base()
	{

	}
	public CustomNode(Node node)
	{
		G = node.G;
		H = node.H;
		parent = node.parent;

		position = node.position;
		start = node.start;
		end = node.end;
	}
	public override string ToString()
	{
		return "(" + position.x + ", " + position.y + ") | " + base.ToString();
	}
	public bool Equals(CustomNode other)
	{
		return base.Equals(other) && jumpStartPos == other.jumpStartPos;
	}
}

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
	private bool AddNearNode(Tilemap tilemap, Tilemap throughMap, ref CustomNode node, int maxJump, int speed)
	{
		BoundsInt tileBounds = tilemap.cellBounds;
		TileBase[] allTile = tilemap.GetTilesBlock(tileBounds);
		//TileBase[] allThrough = throughMap.GetTilesBlock(tileBounds);

		int width = tileBounds.size.x;
		int height = tileBounds.size.y;

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

				int realX = node.position.x + x;
				int realY = node.position.y + y;

				// 배열 범위 밖 체크
				if (realX < 0 || realX >= width ||
					realY < 0 || realY >= height)
					continue;

				int index = realX + realY * width;
				int index_down =
					(realY - 1) >= 0 ?
					realX + (realY - 1) * width :
					0;

				if (allTile[index] != null)
					continue;

				//if (
				//	throughMap.GetTile(new Vector3Int(realX, realY)) != null)
				//	continue;

				CustomNode near = new CustomNode();
				near.position.Set(realX, realY);

				// 닫힌 리스트에 포함되어 있는 경우
				if (m_CloseList.Contains(near))
					continue;

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
						else
							--near.currentJump;
					}
					else
					{
						if (y < 0)
							continue;

						if (y > 0)
						{
							++near.currentJump;

							if (node.jumpStartPos == null)
								near.jumpStartPos = node.position;
						}
					}

					// 착지
					// 이동할 곳이 비어 있어야 하고
					if (allTile[index] == null &&
						throughMap.GetTile(new Vector3Int(realX, realY)) == null)
					{
						// 이동할 곳 밑에 블럭이 있어야 함
						if (allTile[index_down] != null ||
							throughMap.GetTile(new Vector3Int(realX, realY - 1)) != null)
						{
							near.currentJump = 0;
							near.isFalling = false;
							near.jumpStartPos = null;
						}
					}

					// 최대 점프 높이 이상 점프 금지
					if (near.currentJump >= maxJump)
						continue;

					// 아직 떨어지는 중이면 상승 금지
					if (near.isFalling == true &&
						y > 0)
						continue;

					// 좌우이동 시
					if (y == 0)
					{
						// 이동할 곳 밑에 발판이 없는 경우
						if (allTile[index_down] == null &&
							throughMap.GetTile(new Vector3Int(realX, realY - 1)) == null)
							continue;

						// 현재 공중에 떠 있는 경우
						if (allTile[index_down - x] == null &&
							throughMap.GetTile(new Vector3Int(realX - x, realY - 1)) == null)
							continue;

						if (allTile[index] != null ||
							throughMap.GetTile(new Vector3Int(realX, realY)) != null)
							continue;
					}
				}

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

							item.jumpStartPos = near.jumpStartPos;
							item.currentJump = near.currentJump;
							item.isFalling = near.isFalling;
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

	public List<CustomNode> PathFinding(Tilemap tilemap, Tilemap throughMap, Vector2Int start, Vector2Int end, int maxJump, int speed)
	{
		return PathFinding(tilemap, throughMap, start.x, start.y, end.x, end.y, maxJump, speed);
	}
	public List<CustomNode> PathFinding(Tilemap tilemap, Tilemap throughMap, int sx, int sy, int ex, int ey, int maxJump, int speed)
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

			if (AddNearNode(tilemap, throughMap, ref node, maxJump, speed))
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

				Vector3 center = new Vector3(item.position.x, item.position.y) + offset;

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

				Vector3 center = new Vector3(item.position.x, item.position.y) + offset;

				Gizmos.DrawCube(center, size);
			}
		}

		Gizmos.color = color;
	}
}