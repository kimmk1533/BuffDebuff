using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Algorithms;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace BuffDebuff
{
	public class PathFindingMap : SerializedMonoBehaviour
	{
		public const int c_tileSize = 16;
		public const int c_halfSize = c_tileSize / 2;

		#region Enum
		public enum E_TileType : byte
		{
			Empty = 0,
			Block,
			OneWay,

			Max
		}

		#endregion

		#region 변수
		private Room m_Room;

		private byte[,] m_Grid;
		private E_TileType[,] m_Tiles;

		[SerializeField, ReadOnly]
		private int m_Width;
		[SerializeField, ReadOnly]
		private int m_Height;

		[SerializeField]
		private PathFinderFast m_PathFinder;

		[SerializeField]
		private bool m_Debug;

		[SerializeField, ShowIf("@m_Debug == true")]
		private Vector2Int m_DebugStartPos;
		[SerializeField, ShowIf("@m_Debug == true")]
		private Vector2Int m_DebugEndPos;
		[SerializeField, ShowIf("@m_Debug == true"), Min(1)]
		private int m_DebugCharacterWidth = 1;
		[SerializeField, ShowIf("@m_Debug == true"), Min(1)]
		private int m_DebugCharacterHeight = 1;
		[SerializeField, ShowIf("@m_Debug == true")]
		private short m_DebugMaxJumpHeight = 6;
		[SerializeField, ShowIf("@m_Debug == true")]
		private float m_DebugTimeToJumpApex = 0.4f;
		[SerializeField, ShowIf("@m_Debug == true")]
		private float m_DebugMovementSpeed = 3f;

		[SerializeField, ShowIf("@m_Debug == true")]
		private bool m_Debug_ShowTile;
		[SerializeField, ShowIf("@m_Debug == true")]
		private Transform m_Debug_TileParent;
		private SpriteRenderer[,] m_Debug_TileRenderers;
		[SerializeField, ShowIf("@m_Debug == true")]
		private SpriteRenderer m_Debug_TilePrefab;
		#endregion

		public void Initialize()
		{
			// 추후에 MyTilemap완성 후 MyTilemap으로 수정
			// 임시
			this.NullCheckGetComponent<Room>(ref m_Room);

			Tilemap tileMap = m_Room.GetTilemap(Room.E_TilemapLayer.TileMap);
			Tilemap oneWayMap = m_Room.GetTilemap(Room.E_TilemapLayer.OneWayMap);

			m_Width = tileMap.cellBounds.size.x;
			m_Height = tileMap.cellBounds.size.y;
			//

			m_Grid = new byte[Mathf.NextPowerOfTwo(m_Height), Mathf.NextPowerOfTwo(m_Width)];
			m_Tiles = new E_TileType[m_Height, m_Width];

			InitPathFinder();

			if (m_Debug)
				m_Debug_TileRenderers = new SpriteRenderer[m_Height, m_Width];

			Vector3 offset = new Vector3(0.5f, 0.5f);
			for (int y = 0; y < m_Height; ++y)
			{
				for (int x = 0; x < m_Width; ++x)
				{
					Vector3 position = transform.position + new Vector3(x, y, 10.0f);
					Vector3Int index = new Vector3Int(x, y);

					TileBase tile = tileMap.GetTile(index);
					TileBase oneWay = oneWayMap.GetTile(index);

					if (m_Debug)
					{
						m_Debug_TileRenderers[y, x] = Instantiate<SpriteRenderer>(m_Debug_TilePrefab);
						m_Debug_TileRenderers[y, x].transform.SetParent(m_Debug_TileParent == null ? transform : m_Debug_TileParent);
						m_Debug_TileRenderers[y, x].transform.position = position + offset;
					}

					if (tile != null)
						SetTile(x, y, E_TileType.Block);
					else if (oneWay != null)
						SetTile(x, y, E_TileType.OneWay);
					else if (tile == null && oneWay == null)
						SetTile(x, y, E_TileType.Empty);
				}
			}
		}
		public void InitPathFinder()
		{
			m_PathFinder = new PathFinderFast(m_Grid, this);

			m_PathFinder.Formula = E_HeuristicFormula.Manhattan;
			// if false then diagonal movement will be prohibited
			m_PathFinder.Diagonals = false;
			// if true then diagonal movement will have higher cost
			m_PathFinder.HeavyDiagonals = false;
			// estimate of path length
			m_PathFinder.HeuristicEstimate = 6;
			m_PathFinder.PunishChangeDirection = false;
			m_PathFinder.TieBreaker = false;
			m_PathFinder.SearchLimit = 10000;
			m_PathFinder.UseFiltering = true;
			m_PathFinder.DebugProgress = false;
			m_PathFinder.DebugFoundPath = false;
			m_PathFinder.exceptionWeight = 2;
			m_PathFinder.maxExceptionWeight = 8;
			m_PathFinder.groundExceptionValue = 1;
			m_PathFinder.airExceptionValue = 7;
		}

		public bool CheckTile(int x, int y, E_TileType type)
		{
			if (x < 0 || x >= m_Width ||
				y < 0 || y >= m_Height)
				return false;

			return m_Tiles[y, x] == type;
		}
		public bool TryGetTile(int x, int y, out E_TileType type)
		{
			type = E_TileType.Empty;
			if (x < 0 || x >= m_Width ||
				y < 0 || y >= m_Height)
				return false;

			type = m_Tiles[y, x];

			return true;
		}
		public bool IsBlock(int x, int y)
		{
			if (x < 0 || x >= m_Width ||
				y < 0 || y >= m_Height)
				return true;

			return (m_Tiles[y, x] == E_TileType.Block);
		}
		public bool IsOneWayPlatform(int x, int y)
		{
			if (x < 0 || x >= m_Width ||
				y < 0 || y >= m_Height)
				return false;

			return (m_Tiles[y, x] == E_TileType.OneWay);
		}
		public bool IsGround(int x, int y)
		{
			return IsBlock(x, y) || IsOneWayPlatform(x, y);
		}
		public bool IsEmpty(int x, int y)
		{
			if (x < 0 || x >= m_Width ||
				y < 0 || y >= m_Height)
				return true;

			return (m_Tiles[y, x] == E_TileType.Empty);
		}

		public void GetMapTileAtPoint(Vector2 point, out int tileIndexX, out int tileIndexY)
		{
			tileIndexX = (int)((point.x - transform.position.x + c_halfSize) / (float)(c_tileSize));
			tileIndexY = (int)((point.y - transform.position.y + c_halfSize) / (float)(c_tileSize));
		}
		public Vector2Int GetMapTileAtPoint(Vector2 point)
		{
			GetMapTileAtPoint(point, out int indexX, out int indexY);

			return new Vector2Int(indexX, indexY);
		}
		public Vector2 GetMapTilePosition(int tileIndexX, int tileIndexY)
		{
			return new Vector2(
				transform.position.x + (tileIndexX * c_tileSize),
				transform.position.y + (tileIndexY * c_tileSize)
				);
		}
		public Vector2 GetMapTilePosition(Vector2Int tileCoords)
		{
			return GetMapTilePosition(tileCoords.x, tileCoords.y);
		}

		public bool AnySolidBlockInRectangle(Vector2 start, Vector2 end)
		{
			return AnySolidBlockInRectangle(GetMapTileAtPoint(start), GetMapTileAtPoint(end));
		}
		public bool AnySolidBlockInRectangle(Vector2Int start, Vector2Int end)
		{
			int startX, startY, endX, endY;

			startX = Mathf.Min(start.x, end.x);
			startY = Mathf.Min(start.y, end.y);

			endX = Mathf.Max(start.x, end.x);
			endY = Mathf.Max(start.y, end.y);

			for (int y = startY; y <= endY; ++y)
			{
				for (int x = startX; x <= endX; ++x)
				{
					if (CheckTile(x, y, E_TileType.Block))
						return true;
				}
			}

			return false;
		}
		public bool AnySolidBlockInStripe(int x, int y0, int y1)
		{
			int startY = Mathf.Min(y0, y1);
			int endY = Mathf.Max(y0, y1);

			for (int y = startY; y <= endY; ++y)
			{
				if (IsBlock(x, y) == true)
					return true;
			}

			return false;
		}

		private void SetTile(int x, int y, E_TileType type)
		{
			if (x < 0 || x >= m_Width ||
				y < 0 || y >= m_Height)
				return;

			switch (type)
			{
				case E_TileType.Block:
					m_Grid[y, x] = 0;
					// AutoTile(type, x, y, 1, 8, 4, 4, 4, 4);

					if (m_Debug)
						m_Debug_TileRenderers[y, x].gameObject.SetActive(true);
					break;
				case E_TileType.OneWay:
					m_Grid[y, x] = 1;

					if (m_Debug)
					{
						m_Debug_TileRenderers[y, x].gameObject.SetActive(true);
						m_Debug_TileRenderers[y, x].color = Color.red;

						m_Debug_TileRenderers[y, x].transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
						m_Debug_TileRenderers[y, x].transform.eulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
					}
					//m_TileRenderers[y, x].sprite = mDirtSprites[25];
					break;
				case E_TileType.Empty:
					m_Grid[y, x] = 1;

					if (m_Debug)
						m_Debug_TileRenderers[y, x].gameObject.SetActive(false);
					break;
				default:
				case E_TileType.Max:
					throw new System.Exception("");
			}

			m_Tiles[y, x] = type;

			// 주변 타일 Sprite 자동 변경
			//AutoTile(type, x - 1, y, 1, 8, 4, 4, 4, 4);
			//AutoTile(type, x + 1, y, 1, 8, 4, 4, 4, 4);
			//AutoTile(type, x, y - 1, 1, 8, 4, 4, 4, 4);
			//AutoTile(type, x, y + 1, 1, 8, 4, 4, 4, 4);
		}

		public List<Vector2Int> FindPath(Vector2Int start, Vector2Int end, int characterWidth, int characterHeight, short maxCharacterJumpHeight, float timeToJumpApex, float characterMovementSpeed)
		{
			// y 1칸 움직이는데 걸리는 시간
			float yMovementTime = timeToJumpApex / maxCharacterJumpHeight;
			//// x 1칸 움직이는데 걸리는 시간
			//float xMovementTime = characterMovementSpeed * yMovementTime;

			List<Vector2Int> result = m_PathFinder.FindPath(start, end, characterWidth, characterHeight, maxCharacterJumpHeight, characterMovementSpeed, yMovementTime);

			return result;
		}

		private void OnValidate()
		{
			if (m_Debug == false)
				return;
			if (m_Debug_TileParent == null)
				return;

			m_Debug_TileParent.gameObject.SetActive(m_Debug_ShowTile);
		}
		private void OnDrawGizmosSelected()
		{
			if (m_Debug == false)
				return;

			Vector2 offset = Vector2.one * 0.5f;
			Color color = Gizmos.color;

			List<Vector2Int> pathList = new List<Vector2Int>();
			List<Vector2Int> findingPathList = FindPath(m_DebugStartPos, m_DebugEndPos, m_DebugCharacterWidth, m_DebugCharacterHeight, m_DebugMaxJumpHeight, m_DebugTimeToJumpApex, m_DebugMovementSpeed);

			if (findingPathList == null)
				findingPathList = new List<Vector2Int>();

			pathList.Add(m_DebugStartPos);
			pathList.AddRange(findingPathList);
			pathList.Add(m_DebugEndPos);

			for (int i = 0; i < pathList.Count - 1; ++i)
			{
				Vector2Int path1 = pathList[i];
				Vector2Int path2 = pathList[i + 1];

				Gizmos.color = Color.white;
				Gizmos.DrawLine(path1 + offset, path2 + offset);

				if (i != pathList.Count - 2)
				{
					Gizmos.color = Color.green;
					Gizmos.DrawSphere(path2 + offset, 0.25f);
				}
			}
			Gizmos.color = Color.white;
			Gizmos.DrawLine(pathList[pathList.Count - 1] + offset, m_DebugEndPos + offset);

			Gizmos.color = Color.red;
			Gizmos.DrawSphere(m_DebugStartPos + offset, 0.5f);
			Gizmos.color = Color.blue;
			Gizmos.DrawSphere(m_DebugEndPos + offset, 0.5f);

			Gizmos.color = color;
		}
	}
}