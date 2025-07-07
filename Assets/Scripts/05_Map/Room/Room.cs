using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using BuffDebuff.Enum;
using TilemapMap = System.Collections.Generic.Dictionary<BuffDebuff.Room.E_TilemapLayer, UnityEngine.Tilemaps.Tilemap>;

namespace BuffDebuff
{
	[RequireComponent(typeof(PathFindingMap), typeof(EnemySpawner))]
	public class Room : ObjectPoolItem<Room>
	{
		#region Enum
		// 타일맵 레이어
		public enum E_TilemapLayer
		{
			// 뒷 배경
			BackGround,
			// 점프 가능한 벽
			OneWayMap,
			// 벽
			TileMap,
			// 앞 배경 (장식 등등)
			Environment,

			Max
		}
		public enum E_DirectionLayer
		{
			Origin,
			Left,
			Right,
			Top,
			Bottom,

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
		private bool m_IsStartingRoom;

		// 타일맵
		private Dictionary<E_DirectionLayer, TilemapMap> m_TilemapMap_Direction = null;

		// 길 찾기 정보
		private PathFindingMap m_PathFindingMap = null;

		#region 워프 관련
		// 워프 시뮬 여부
		private bool m_IsSimulating;

		// 주변 방
		private Dictionary<E_Direction, Room> m_NearRoomMap;

		// 워프포인트
		private List<WarpPoint> m_WarpPointList;
		private Dictionary<E_Direction, Dictionary<int, WarpPoint>> m_WarpPointMap; // 방향, 인덱스, 워프포인트
		private Dictionary<E_Direction, int> m_WarpPointCountMap; // 방향, 갯수 
		#endregion

		// 적 생성 시스템
		private EnemySpawner m_EnemySpawner = null;
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
		public bool isStartingRoom
		{
			get { return m_IsStartingRoom; }
			set { m_IsStartingRoom = value; }
		}

		public PathFindingMap pathFindingMap => m_PathFindingMap;

		public bool isSimulating
		{
			get => m_IsSimulating;
			set
			{
				for (int i = 0; i < m_WarpPointList.Count; ++i)
				{
					m_WarpPointList[i].isSimulating = value;
				}
			}
		}
		#endregion

		#region 이벤트
		//public event System.Action<Room> onRoomClear;
		#endregion

		#region 매니저
		private static RoomManager M_Room => RoomManager.Instance;
		#endregion

		public override void InitializePoolItem()
		{
			base.InitializePoolItem();

			#region NULL Check New
			// 타일맵 딕셔너리 초기화
			if (m_TilemapMap_Direction == null)
				m_TilemapMap_Direction = new Dictionary<E_DirectionLayer, TilemapMap>();

			// 주변 방 딕셔너리 초기화
			if (m_NearRoomMap == null)
				m_NearRoomMap = new Dictionary<E_Direction, Room>();

			// 워프포인트 리스트 초기화
			if (m_WarpPointList == null)
				m_WarpPointList = new List<WarpPoint>();

			// 워프포인트 딕셔너리 초기화
			if (m_WarpPointMap == null)
				m_WarpPointMap = new Dictionary<E_Direction, Dictionary<int, WarpPoint>>();

			// 워프포인트 갯수 딕셔너리 초기화
			if (m_WarpPointCountMap == null)
				m_WarpPointCountMap = new Dictionary<E_Direction, int>();
			#endregion

			m_IsStartingRoom = false;

			Transform tilemapLayer = transform.Find("TileMapLayer");
			for (E_DirectionLayer dirLayer = 0; dirLayer < E_DirectionLayer.Max; ++dirLayer)
			{
				Transform dir = tilemapLayer.Find(dirLayer.ToString());

				if (dir == null)
					continue;

				for (E_TilemapLayer layer = 0; layer < E_TilemapLayer.Max; ++layer)
				{
					Tilemap tileMap = dir.Find<Tilemap>(layer.ToString());

					if (tileMap == null)
						continue;

					if (m_TilemapMap_Direction.ContainsKey(dirLayer) == false)
						m_TilemapMap_Direction.Add(dirLayer, new TilemapMap());

					m_TilemapMap_Direction[dirLayer].Add(layer, tileMap);
				}
			}

			// 길 찾기 정보 초기화
			this.NullCheckGetComponent<PathFindingMap>(ref m_PathFindingMap);
			m_PathFindingMap.Initialize();

			gameObject.GetComponentsInChildren<WarpPoint>(m_WarpPointList);
			for (int i = 0; i < m_WarpPointList.Count; ++i)
			{
				WarpPoint warpPoint = m_WarpPointList[i];
				warpPoint.Initialize(this);
				warpPoint.onWarp += OnPlayerWarp;

				E_Direction direction = warpPoint.direction;

				if (m_WarpPointMap.ContainsKey(direction) == false)
					m_WarpPointMap.Add(direction, new Dictionary<int, WarpPoint>());

				if (m_WarpPointCountMap.ContainsKey(direction) == false)
					m_WarpPointCountMap.Add(direction, 0);

				m_WarpPointMap[direction].Add(warpPoint.index, warpPoint);
				++m_WarpPointCountMap[direction];
			}

			this.NullCheckGetComponent<EnemySpawner>(ref m_EnemySpawner);
			m_EnemySpawner.Initialize(this);
		}
		public override void FinallizePoolItem()
		{
			base.FinallizePoolItem();

			SetDoorActive(true, E_DirectionLayer.Left);
			SetDoorActive(true, E_DirectionLayer.Right);
			SetDoorActive(true, E_DirectionLayer.Top);
			SetDoorActive(true, E_DirectionLayer.Bottom);

			// 타일맵 딕셔너리 마무리화
			if (m_TilemapMap_Direction != null)
			{
				foreach (var item in m_TilemapMap_Direction)
				{
					item.Value.Clear();
				}
				m_TilemapMap_Direction.Clear();
			}

			// 주변 방 딕셔너리 마무리화
			if (m_NearRoomMap != null)
				m_NearRoomMap.Clear();

			// 워프포인트 리스트 마무리화
			if (m_WarpPointList != null)
				m_WarpPointList.Clear();

			// 워프포인트 딕셔너리 마무리화
			if (m_WarpPointMap != null)
			{
				foreach (var item in m_WarpPointMap)
				{
					foreach (var item2 in item.Value)
					{
						item2.Value.Finallize();
					}
					item.Value.Clear();
				}
			}

			// 워프포인트 갯수 딕셔너리 마무리화
			if (m_WarpPointCountMap != null)
			{
				for (E_Direction direction = E_Direction.None + 1; direction < E_Direction.Max; ++direction)
				{
					if (m_WarpPointCountMap.ContainsKey(direction) == false)
						continue;

					m_WarpPointCountMap[direction] = 0;
				}
			}

			if (m_EnemySpawner != null)
				m_EnemySpawner.Finallize();
		}

		private void OnPlayerWarp(WarpPoint.WarpArg arg)
		{
			arg.currRoom.OnPlayerEnterRoom();
			arg.prevRoom.OnPlayerExitRoom();
		}
		private void OnPlayerEnterRoom()
		{
			if (m_IsStartingRoom == true)
				return;

			m_EnemySpawner.OnPlayerEnterRoom();
		}
		private void OnPlayerExitRoom()
		{
			if (m_IsStartingRoom == true)
				return;

			m_EnemySpawner.OnPlayerExitRoom();
		}

		public void ClearRoom()
		{
			M_Room.onRoomClear?.Invoke(this);

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

			if (m_NearRoomMap.ContainsKey(direction) == true)
				m_NearRoomMap[direction] = nearRoom;
			else
				m_NearRoomMap.Add(direction, nearRoom);

			E_Direction otherDir = DirEnumUtil.GetOtherDir(direction);

			if (nearRoom.m_NearRoomMap.ContainsKey(otherDir) == true)
				nearRoom.m_NearRoomMap[otherDir] = this;
			else
				nearRoom.m_NearRoomMap.Add(otherDir, this);

			MakeDoor(direction);
			nearRoom.MakeDoor(otherDir);
		}
		private void MakeDoor(E_Direction direction)
		{
			E_DirectionLayer dirLayer = E_DirectionLayer.Origin;

			switch (direction)
			{
				case E_Direction.Up:
					dirLayer = E_DirectionLayer.Top;
					break;
				case E_Direction.Down:
					dirLayer = E_DirectionLayer.Bottom;
					break;
				case E_Direction.Left:
					dirLayer = E_DirectionLayer.Left;
					break;
				case E_Direction.Right:
					dirLayer = E_DirectionLayer.Right;
					break;
			}

			SetDoorActive(false, dirLayer);
		}
		private void SetDoorActive(bool active, E_DirectionLayer dirLayer)
		{
			if (m_TilemapMap_Direction.TryGetValue(dirLayer, out TilemapMap tilemapMap) == false)
				return;

			for (E_TilemapLayer layer = E_TilemapLayer.OneWayMap; layer <= E_TilemapLayer.Environment; ++layer)
			{
				if (tilemapMap.TryGetValue(layer, out Tilemap tilemap) == false)
					continue;

				tilemap.gameObject.SetActive(active);
			}
		}

		public Tilemap GetTilemap(E_TilemapLayer layer)
		{
			if (m_TilemapMap_Direction == null)
				return null;

			return m_TilemapMap_Direction[E_DirectionLayer.Origin][layer];
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
		public Dictionary<int, WarpPoint> GetWarpPointMap(E_Direction direction)
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

			Gizmos.color = new Color(0f, 1f, 0f, 0.1f);
			Gizmos.DrawCube((Vector2)transform.position + m_Offset, m_RoomSize);

			Gizmos.color = color;
		}
		#endregion
	}
}