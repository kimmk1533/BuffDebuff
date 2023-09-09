using System.Collections;
using System.Collections.Generic;
using Algorithms;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Map : MonoBehaviour
{
	#region Enum
	public enum E_TileType : byte
	{
		Empty = 0,
		Block,
		OneWay,

		Max
	}

	public static byte empty
	{
		get { return (byte)E_TileType.Empty; }
	}
	public static byte block
	{
		get { return (byte)E_TileType.Block; }
	}
	public static byte oneway
	{
		get { return (byte)E_TileType.OneWay; }
	}
	#endregion

	public const int c_tileSize = 16;
	public const int halfSize = c_tileSize / 2;

	private byte[,] m_Grid;
	private E_TileType[,] m_Tiles;
	private SpriteRenderer[,] m_TileRenderers;

	[SerializeField, ReadOnly(true)]
	private Transform m_TileParent;
	[SerializeField, ReadOnly(true)]
	private SpriteRenderer m_TilePrefab;

	private PathFinderFast m_PathFinder;

	private int m_Width;
	private int m_Height;

	private GridManager M_Grid => GridManager.Instance;
	private StageManager M_Stage => StageManager.Instance;

	private void Awake()
	{
		Initialize();
	}

	// 테스트
	public int testValue = 100;
	public float pathShowTime;
	private void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			float startT, endT;

			startT = Time.realtimeSinceStartup;
			for (int i = 0; i < testValue; ++i)
				Test(i == 0);
			endT = Time.realtimeSinceStartup;

			float time = endT - startT;

			Debug.Log("길을 " + testValue + "번 찾는 동안 걸린 시간: " + time);
		}
	}
	private void Test(bool showPath)
	{
		List<Vector2Int> path = m_PathFinder.FindPath(M_Grid.m_Start, M_Grid.m_End, 1, 1, 6);

		if (showPath == false)
			return;

		Vector3 offset = Vector3.one * 0.5f;

		if (path != null)
		{
			for (int i = 0; i < path.Count - 1; ++i)
			{
				Vector3 start = new Vector3(path[i].x, path[i].y);
				Vector3 end = new Vector3(path[i + 1].x, path[i + 1].y);

				Debug.DrawLine(start + offset, end + offset, Color.white, pathShowTime);

				DrawRect(start + offset, offset, Color.white, pathShowTime);
				var text = UtilClass.CreateWorldText(path.Count - i, transform, start + offset, 0.1f, 40, Color.white, TextAnchor.MiddleCenter);
				GameObject.Destroy(text.gameObject, pathShowTime);

				if (i == path.Count - 2)
				{
					DrawRect(end + offset, offset, Color.white, pathShowTime);

					text = UtilClass.CreateWorldText(1, transform, end + offset, 0.1f, 40, Color.white, TextAnchor.MiddleCenter);
					GameObject.Destroy(text.gameObject, pathShowTime);
				}
			}
		}
	}

	// MyDebug 클래스 만들어서 옮겨두자
	private void DrawRect(Vector3 center, Vector3 size)
	{
		bool depthTest = true;
		float duration = 0f;
		Color color = Color.white;
		DrawRect(center, size, color, duration, depthTest);
	}
	private void DrawRect(Vector3 center, Vector3 size, Color color)
	{
		bool depthTest = true;
		float duration = 0f;
		DrawRect(center, size, color, duration, depthTest);
	}
	private void DrawRect(Vector3 center, Vector3 size, Color color, float duration)
	{
		bool depthTest = true;
		DrawRect(center, size, color, duration, depthTest);
	}
	private void DrawRect(Vector3 center, Vector3 size, Color color, float duration, bool depthTest)
	{
		#region 변수 선언
		Vector3 left = Vector3.left * size.x * 0.5f;
		Vector3 right = Vector3.right * size.x * 0.5f;
		Vector3 top = Vector3.up * size.y * 0.5f;
		Vector3 bottom = Vector3.down * size.y * 0.5f;
		Vector3 front = Vector3.forward * size.z * 0.5f;
		Vector3 back = Vector3.back * size.z * 0.5f;

		Vector3 frontLeftTop, frontRightTop, frontLeftBottom, frontRightBottom;
		Vector3 backLeftTop, backRightTop, backLeftBottom, backRightBottom;

		frontLeftTop = center + front + left + top;
		frontRightTop = center + front + right + top;
		frontLeftBottom = center + front + left + bottom;
		frontRightBottom = center + front + right + bottom;

		backLeftTop = center + back + left + top;
		backRightTop = center + back + right + top;
		backLeftBottom = center + back + left + bottom;
		backRightBottom = center + back + right + bottom;
		#endregion

		Debug.DrawLine(frontLeftTop, frontRightTop, color, duration, depthTest);
		Debug.DrawLine(frontRightTop, frontRightBottom, color, duration, depthTest);
		Debug.DrawLine(frontRightBottom, frontLeftBottom, color, duration, depthTest);
		Debug.DrawLine(frontLeftBottom, frontLeftTop, color, duration, depthTest);

		Debug.DrawLine(backLeftTop, backRightTop, color, duration, depthTest);
		Debug.DrawLine(backRightTop, backRightBottom, color, duration, depthTest);
		Debug.DrawLine(backRightBottom, backLeftBottom, color, duration, depthTest);
		Debug.DrawLine(backLeftBottom, backLeftTop, color, duration, depthTest);

		Debug.DrawLine(frontLeftTop, backLeftTop, color, duration, depthTest);
		Debug.DrawLine(frontRightTop, backRightTop, color, duration, depthTest);
		Debug.DrawLine(frontRightBottom, backRightBottom, color, duration, depthTest);
		Debug.DrawLine(frontLeftBottom, backLeftBottom, color, duration, depthTest);
	}
	//

	public void Initialize()
	{
		Tilemap tileMap = M_Stage.currentRoom.GetTilemap(Room.E_RoomTilemapLayer.TileMap);
		Tilemap throughMap = M_Stage.currentRoom.GetTilemap(Room.E_RoomTilemapLayer.ThroughMap);

		// 임시
		m_Width = (int)(tileMap.cellBounds.size.x);
		m_Height = (int)(tileMap.cellBounds.size.y);
		//

		m_Grid = new byte[Mathf.NextPowerOfTwo(m_Height), Mathf.NextPowerOfTwo(m_Width)];
		InitPathFinder();

		m_Tiles = new E_TileType[m_Height, m_Width];
		m_TileRenderers = new SpriteRenderer[m_Height, m_Width];

		Vector3 offset = new Vector3(0.5f, 0.5f);
		for (int y = 0; y < m_Height; ++y)
		{
			for (int x = 0; x < m_Width; ++x)
			{
				Vector3 position = transform.position + new Vector3(x, y, 10.0f);

				m_TileRenderers[y, x] = Instantiate<SpriteRenderer>(m_TilePrefab);
				m_TileRenderers[y, x].transform.SetParent(m_TileParent == null ? transform : m_TileParent);
				m_TileRenderers[y, x].transform.position = position + offset;

				Vector3Int index = new Vector3Int(x, y);

				TileBase tile = tileMap.GetTile(index);
				SetTile(x, y, tile == null ? E_TileType.Empty : E_TileType.Block);
				//SetTile(x, y, mapRoom.tileData[y * m_Width + x]);

				tile = throughMap.GetTile(index);
				SetTile(x, y,
					CheckTile(x, y, E_TileType.Block) == true
					? E_TileType.Block
					: tile == null ? E_TileType.Empty : E_TileType.OneWay);
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
		m_PathFinder.DebugProgress = false;
		m_PathFinder.DebugFoundPath = false;
	}
	private void SetTile(int x, int y, E_TileType type)
	{
		if (x <= 1 || x >= m_Width - 2 ||
			y <= 1 || y >= m_Height - 2)
			return;

		switch (type)
		{
			default:
			case E_TileType.Empty:
				m_Grid[y, x] = empty;
				m_TileRenderers[y, x].enabled = false;
				break;
			case E_TileType.Block:
				m_Grid[y, x] = block;
				// AutoTile(type, x, y, 1, 8, 4, 4, 4, 4);
				m_TileRenderers[y, x].enabled = true;
				break;
			case E_TileType.OneWay:
				m_Grid[y, x] = oneway;
				m_TileRenderers[y, x].enabled = true;
				m_TileRenderers[y, x].color = Color.red;

				m_TileRenderers[y, x].transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
				m_TileRenderers[y, x].transform.eulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
				//m_TileRenderers[y, x].sprite = mDirtSprites[25];
				break;
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
	public bool IsNotEmpty(int x, int y)
	{
		if (x < 0 || x >= m_Width ||
			y < 0 || y >= m_Height)
			return true;

		return (m_Tiles[y, x] != E_TileType.Empty);
	}

	public void GetMapTileAtPoint(Vector2 point, out int tileIndexX, out int tileIndexY)
	{
		tileIndexX = (int)((point.x - transform.position.x + halfSize) / (float)(c_tileSize));
		tileIndexY = (int)((point.y - transform.position.y + halfSize) / (float)(c_tileSize));
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
}