using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Enum;
using AYellowpaper.SerializedCollections;

[RequireComponent(typeof(PathFindingMap))]
public class Room : ObjectPoolItemBase
{
	#region Enum
	// 타일맵 레이어
	public enum E_RoomTilemapLayer
	{
		// 뒷 배경
		BackGround,
		// 벽
		TileMap,
		// 점프 가능한 벽
		OneWayMap,
		// 앞 배경 (장식 등등)
		Environment,

		Max
	}
	#endregion

	#region 변수
	// 방 크기
	[SerializeField]
	private Vector2 m_RoomSize;
	// 방 위치 오프셋
	[SerializeField]
	private Vector2 m_Offset;

	// 타일맵
	private SerializedDictionary<E_RoomTilemapLayer, Tilemap> m_TilemapMap;
	// 길 찾기 정보
	private PathFindingMap m_PathFindingMap;

	#region 적 생성 관련
	// 이 방 클리어 여부
	private bool m_IsClear;

	// 적 생성 정보
	[SerializeField]
	private List<EnemyWave> m_EnemyWave;
	[SerializeField, ReadOnly]
	private int m_EnemyWaveIndex;
	#endregion

	#region 워프 관련
	// 주변 방
	private Dictionary<E_Direction, Room> m_NearRoomMap;

	// 워프포인트
	private Dictionary<E_Direction, Dictionary<int, WarpPoint>> m_WarpPointMap; // 방향, 인덱스, 워프포인트
	private Dictionary<E_Direction, int> m_WarpPointCountMap; // 방향, 갯수 
	#endregion
	#endregion

	#region 프로퍼티
	public Vector2 roomSize
	{
		get { return m_RoomSize; }
		set { m_RoomSize = value; }
	}
	public Vector2 offset
	{
		get { return m_Offset; }
		set { m_Offset = value; }
	}

	public PathFindingMap pathFindingMap => m_PathFindingMap;

	public bool isClear => m_IsClear;
	#endregion

	public override void Initialize()
	{
		base.Initialize();

		#region SAFE_INIT
		// 타일맵 딕셔너리 초기화
		if (m_TilemapMap != null)
			m_TilemapMap.Clear();
		else
			m_TilemapMap = new SerializedDictionary<E_RoomTilemapLayer, Tilemap>();

		// 길 찾기 정보 초기화
		this.Safe_GetComponent<PathFindingMap>(ref m_PathFindingMap);

		// 방 클리어 여부 초기화
		m_IsClear = false;
		// 적 생성 정보 초기화
		if (m_EnemyWave.Count > 0)
		{
			m_EnemyWaveIndex = Random.Range(0, m_EnemyWave.Count);
			m_EnemyWave[m_EnemyWaveIndex].Initialize(this);
		}

		// 주변 방 딕셔너리 초기화
		if (m_NearRoomMap != null)
			m_NearRoomMap.Clear();
		else
			m_NearRoomMap = new Dictionary<E_Direction, Room>();

		// 워프포인트 딕셔너리 초기화
		if (m_WarpPointMap != null)
		{
			foreach (var item in m_WarpPointMap)
			{
				item.Value.Clear();
			}
			m_WarpPointMap.Clear();
		}
		else
			m_WarpPointMap = new Dictionary<E_Direction, Dictionary<int, WarpPoint>>();

		// 워프포인트 갯수 딕셔너리 초기화
		if (m_WarpPointCountMap != null)
			m_WarpPointCountMap.Clear();
		else
			m_WarpPointCountMap = new Dictionary<E_Direction, int>();
		#endregion

		Transform tilemapLayer = transform.Find("TileMapLayer");
		for (E_RoomTilemapLayer layer = 0; layer < E_RoomTilemapLayer.Max; ++layer)
		{
			Tilemap tileMap = tilemapLayer.Find(layer.ToString()).GetComponent<Tilemap>();
			m_TilemapMap.Add(layer, tileMap);

			if (layer == E_RoomTilemapLayer.OneWayMap ||
				layer == E_RoomTilemapLayer.TileMap)
			{
				TilemapCollider2D tilemapCollider2D = tileMap.GetComponent<TilemapCollider2D>();
				tilemapCollider2D?.ProcessTilemapChanges();
			}
		}

		m_PathFindingMap.Initialize();

		WarpPoint[] warpPointArray = GetComponentsInChildren<WarpPoint>();
		foreach (WarpPoint warpPoint in warpPointArray)
		{
			warpPoint.Initialize(this);
			warpPoint.onWarp += OnWarp;

			E_Direction direction = warpPoint.direction;

			if (m_WarpPointMap.ContainsKey(direction) == false)
				m_WarpPointMap.Add(direction, new Dictionary<int, WarpPoint>());

			if (m_WarpPointCountMap.ContainsKey(direction) == false)
				m_WarpPointCountMap.Add(direction, 0);

			m_WarpPointMap[direction].Add(warpPoint.index, warpPoint);
			++m_WarpPointCountMap[direction];
		}
	}
	public override void Finallize()
	{
		base.Finallize();

		foreach (var item in m_WarpPointMap)
		{
			foreach (var item2 in item.Value)
			{
				item2.Value.Finallize();
			}
		}
	}

	private void OnWarp(WarpPoint.WarpArg arg)
	{
		if (m_IsClear == true)
			return;
		if (arg.warpObject.gameObject.layer != LayerMask.NameToLayer("Player"))
			return;

		arg.currRoom.OnEnterRoom();
		arg.prevRoom.OnExitRoom();
	}
	private void OnEnterRoom()
	{
		if (m_IsClear == true)
			return;
		//if (m_EnemyWave.Count <= 0)
		//	return;
		//if (m_EnemyWaveIndex < 0 || m_EnemyWaveIndex >= m_EnemyWave.Count)
		//	throw new System.ArgumentOutOfRangeException();

		m_EnemyWave[m_EnemyWaveIndex].CreateEnemy();
	}
	private void OnExitRoom()
	{
		if (m_IsClear == true)
			return;
		//if (m_EnemyWave.Count <= 0)
		//	return;
		//if (m_EnemyWaveIndex < 0 || m_EnemyWaveIndex >= m_EnemyWave.Count)
		//	throw new System.ArgumentOutOfRangeException();

		m_EnemyWave[m_EnemyWaveIndex].Reset();

		StopAllCoroutines();
	}

	public void ClearRoom()
	{
		m_IsClear = true;

		Debug.Log("방 클리어");
	}

	public Room GetNearRoom(E_Direction direction)
	{
		if (m_NearRoomMap.ContainsKey(direction) == false)
			return null;

		return m_NearRoomMap[direction];
	}
	public void SetNearRoom(E_Direction direction, Room nearRoom)
	{
		if (nearRoom == null)
			return;

		if (m_NearRoomMap.TryGetValue(direction, out Room room) == true)
			m_NearRoomMap[direction] = nearRoom;
		else
			m_NearRoomMap.Add(direction, nearRoom);

		E_Direction otherDir = DirEnumUtil.GetOtherDir(direction);

		if (nearRoom.m_NearRoomMap.TryGetValue(otherDir, out room) == true)
			nearRoom.m_NearRoomMap[otherDir] = this;
		else
			nearRoom.m_NearRoomMap.Add(otherDir, this);
	}

	public Tilemap GetTilemap(E_RoomTilemapLayer layer)
	{
		if (m_TilemapMap == null)
			return null;

		return m_TilemapMap[layer];
	}
	public List<WarpPoint> GetAllWarpPoint()
	{
		List<WarpPoint> result = new List<WarpPoint>();

		foreach (var item in m_WarpPointMap)
		{
			Dictionary<int, WarpPoint> index_WarpPointMap = item.Value;

			result.AddRange(index_WarpPointMap.Values);
		}

		return result;
	}
	public Dictionary<int, WarpPoint> GetIndexWarpPointMap(E_Direction direction)
	{
		if (m_WarpPointMap.ContainsKey(direction) == false)
			return null;

		return m_WarpPointMap[direction];
	}
	public int GetWarpPointCount(E_Direction direction)
	{
		if (m_WarpPointCountMap.ContainsKey(direction) == false)
			return 0;

		return m_WarpPointCountMap[direction];
	}

	#region Draw Gizmo
	private void OnDrawGizmosSelected()
	{
		Color color = Gizmos.color;

		Gizmos.color = new Color(0, 1, 0, 0.1f);
		Gizmos.DrawCube((Vector2)transform.position + m_Offset, m_RoomSize);

		Gizmos.color = color;
	}
	#endregion
}