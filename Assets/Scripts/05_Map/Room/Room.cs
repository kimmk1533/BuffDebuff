using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Enum;

public class Room : PoolItemBase
{
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

	#region 변수
	// 방 크기
	[SerializeField]
	private Vector2 m_RoomSize;

	// 방 위치 오프셋
	[SerializeField]
	private Vector2 m_Offset;

	// 이 방 클리어 여부
	private bool m_IsClear;

	// 적 생성 리스트
	[SerializeField]
	private List<EnemyWave> m_EnemyWaveList; // 인스펙터 용도
	private Dictionary<EnemyWave.E_InitCondition, List<EnemyWave>> m_EnemyWaveMap;
	// 생성된 적 리스트
	private List<Enemy> m_CreatedEnemyList;

	// 주변 방
	private Dictionary<E_Direction, Room> m_NearRoomMap;

	// 타일맵
	private Dictionary<E_RoomTilemapLayer, Tilemap> m_TilemapMap;

	// 워프포인트
	private Dictionary<E_Direction, Dictionary<int, WarpPoint>> m_WarpPointMap; // 방향, 인덱스, 워프포인트
	private Dictionary<E_Direction, int> m_WarpPointCountMap; // 방향, 갯수
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
	#endregion

	#region 매니저
	private EnemyManager M_Enemy => EnemyManager.Instance;
	private StageManager M_Stage => StageManager.Instance;
	#endregion

	public override void Initialize()
	{
		base.Initialize();

		// 적 생성 정보 딕셔너리 초기화
		m_EnemyWaveMap = new Dictionary<EnemyWave.E_InitCondition, List<EnemyWave>>();
		foreach (var item in m_EnemyWaveList)
		{
			if (m_EnemyWaveMap.ContainsKey(item.initCondition) == false)
				m_EnemyWaveMap.Add(item.initCondition, new List<EnemyWave>());

			m_EnemyWaveMap[item.initCondition].Add(item);
		}

		// 생성된 적 리스트 초기화
		m_CreatedEnemyList = new List<Enemy>();

		// 주변 방 딕셔너리 초기화
		m_NearRoomMap = new Dictionary<E_Direction, Room>();

		// 타일맵 딕셔너리 초기화
		m_TilemapMap = new Dictionary<E_RoomTilemapLayer, Tilemap>();
		Transform tilemapLayer = transform.Find("TileMapLayer");
		for (E_RoomTilemapLayer layer = 0; layer < E_RoomTilemapLayer.Max; ++layer)
		{
			Tilemap tileMap = tilemapLayer.Find(layer.ToString()).GetComponent<Tilemap>();
			m_TilemapMap.Add(layer, tileMap);
		}

		// 워프포인트, 워프포인트 갯수 딕셔너리 초기화
		m_WarpPointMap = new Dictionary<E_Direction, Dictionary<int, WarpPoint>>();
		m_WarpPointCountMap = new Dictionary<E_Direction, int>();

		WarpPoint[] warpPointArray = GetComponentsInChildren<WarpPoint>();

		foreach (WarpPoint warpPoint in warpPointArray)
		{
			warpPoint.Initialize(this);

			E_Direction direction = warpPoint.direction;

			if (m_WarpPointMap.ContainsKey(direction) == false)
				m_WarpPointMap.Add(direction, new Dictionary<int, WarpPoint>());

			if (m_WarpPointCountMap.ContainsKey(direction) == false)
				m_WarpPointCountMap.Add(direction, 0);

			m_WarpPointMap[direction].Add(warpPoint.index, warpPoint);
			++m_WarpPointCountMap[direction];
		}
	}
	public void InitializeEvent()
	{
		foreach (var item in m_WarpPointMap)
		{
			foreach (var item2 in item.Value)
			{
				item2.Value.onWarp += OnEnterRoom;
				item2.Value.onWarp += OnExitRoom;
			}
		}
	}

	// 추후 이벤트로 변경할 것.
	private void Update()
	{
		if (M_Stage.currentStage.currentRoom != this)
			return;

		for (int i = 0; i < m_CreatedEnemyList.Count; ++i)
		{
			Enemy enemy = m_CreatedEnemyList[i];

			if (enemy.gameObject.activeSelf == false)
			{
				m_CreatedEnemyList.Remove(enemy);
				--i;

				if (m_CreatedEnemyList.Count == 0)
				{
					OnClearRoom();
				}
			}
		}
	}

	public void OnCreatedEnemyDeath(Enemy.OnDeathArg arg)
	{
		Enemy enemy = arg.enemy;

		m_CreatedEnemyList.Remove(enemy);

		if (m_CreatedEnemyList.Count == 0)
		{
			OnClearRoom();
		}
	}

	private void OnEnterRoom(WarpPoint.WarpArg arg)
	{
		if (arg.currRoom != this)
			return;

		if (arg.warpObject.gameObject.layer != LayerMask.NameToLayer("Player"))
			return;

		EnemyWave.E_InitCondition condition = EnemyWave.E_InitCondition.EnterRoom;

		if (m_EnemyWaveMap.ContainsKey(condition) == false ||
			m_EnemyWaveMap[condition].Count <= 0)
			return;

		List<EnemyWave> waveList = m_EnemyWaveMap[condition];

		for (int i = 0; i < waveList.Count; ++i)
		{
			EnemyWave wave = waveList[i];

			m_CreatedEnemyList.AddRange(wave.CreateEnemy(this));

			waveList[i] = wave;
		}
	}
	private void OnExitRoom(WarpPoint.WarpArg arg)
	{
		if (arg.prevRoom != this)
			return;

		if (arg.warpObject.gameObject.layer != LayerMask.NameToLayer("Player"))
			return;

		if (m_IsClear == true)
			return;

		foreach (var item in m_EnemyWaveMap)
		{
			List<EnemyWave> waveList = item.Value;

			for (int i = 0; i < waveList.Count; ++i)
			{
				EnemyWave wave = waveList[i];

				wave.Reset();

				waveList[i] = wave;
			}
		}

		foreach (var item in m_CreatedEnemyList)
		{
			M_Enemy.Despawn(item.itemName, item);
		}
	}
	private void OnClearRoom()
	{
		EnemyWave.E_InitCondition condition = EnemyWave.E_InitCondition.ClearRoom;

		if (m_EnemyWaveMap.ContainsKey(condition) == false ||
			m_EnemyWaveMap[condition].Count <= 0)
			return;

		List<EnemyWave> waveList = m_EnemyWaveMap[condition];

		for (int i = 0; i < waveList.Count; ++i)
		{
			EnemyWave wave = waveList[i];

			m_CreatedEnemyList.AddRange(wave.CreateEnemy(this));

			waveList[i] = wave;
		}
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

		Room room = null;

		if (m_NearRoomMap.TryGetValue(direction, out room) == true)
			m_NearRoomMap[direction] = nearRoom;
		else
			m_NearRoomMap.Add(direction, nearRoom);

		E_Direction otherDir = DirEnumUtil.GetOtherDir(direction);

		if (nearRoom.m_NearRoomMap.TryGetValue(otherDir, out room) == true)
			nearRoom.m_NearRoomMap[otherDir] = this;
		else
			nearRoom.m_NearRoomMap.Add(otherDir, this);

		//if (m_NearRoomMap.TryGetValue(direction, out Room room) == true)
		//{
		//	if (room == null)
		//		m_NearRoomMap[direction] = nearRoom;

		//	return;
		//}

		//m_NearRoomMap.Add(direction, nearRoom);
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