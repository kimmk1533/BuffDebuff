using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enum;
using E_Condition = RoomManager.E_Condition;
using RoomCondition = System.ValueTuple<RoomManager.E_Condition, Enum.E_Direction, int>;

// 아이작 방식 맵 생성
// 참고: https://minpro-tech.tistory.com/106
public class StageGenerator : MonoBehaviour
{
	#region 변수
	[SerializeField, Min(1.0f)]
	private float m_StageMagnification = 2.6f;

	[SerializeField]
	private bool m_AutoClear = true;

	private int m_MaxRoomCount;
	private int m_StageRoomCount;
	private int m_GeneratedRoomCount;

	private Transform m_StageParent;
	private int m_CurrentStageLevel;
	private Vector2Int m_StageSize;

	private bool[,] m_StageRoomCheck;
	private Queue<Vector2Int> m_StageRoomQueue;
	private Dictionary<Vector2Int, Room> m_GeneratedRoomMap;

	private CameraFollow m_CameraFollow;
	#endregion

	#region 프로퍼티
	private Transform stageParent => m_StageParent;
	private int currStageLevel => m_CurrentStageLevel;
	private Vector2Int stageSize => m_StageSize;
	#endregion

	#region 매니저
	private static RoomManager M_Room => RoomManager.Instance;
	private static StageManager M_Stage => StageManager.Instance;
	#endregion

	public void Initialize()
	{
		m_StageRoomCount = 0;
		m_GeneratedRoomCount = 0;

		if (m_StageRoomQueue == null)
			m_StageRoomQueue = new Queue<Vector2Int>();
		else
			m_StageRoomQueue.Clear();

		if (m_GeneratedRoomMap == null)
			m_GeneratedRoomMap = new Dictionary<Vector2Int, Room>();
		else
			m_GeneratedRoomMap.Clear();
	}
	public void InitializeGame()
	{
		Camera.main.Safe_GetComponent(ref m_CameraFollow);
	}

	public Stage GenerateStage(StageGeneratorArg arg)
	{
		SetStageGeneratorArg(arg);

		m_StageRoomCheck = new bool[m_StageSize.y, m_StageSize.x];

		if (m_AutoClear)
			ClearStage();

		Stage stage = new GameObject(currStageLevel.ToString("Stage 00")).AddComponent<Stage>();
		stage.gameObject.isStatic = true;
		stage.transform.SetParent(stageParent);

		Vector2Int checkPos;
		Vector2Int current;

		ResetRoomCheck(currStageLevel, ref stage);

		while (m_StageRoomCount < m_MaxRoomCount)
		{
			if (m_StageRoomQueue.Count == 0)
			{
				ResetRoomCheck(currStageLevel, ref stage);
				//Debug.Log("맵 재생성");
			}

			current = m_StageRoomQueue.Dequeue();

			for (E_Direction direction = 0; direction < E_Direction.Max; ++direction)
			{
				checkPos = current + DirEnumUtil.ConvertToVector2Int(direction);

				UpdateRoomCheck(checkPos);
			}

			InfiniteLoopDetector.Run();
		}

		#region Debug
		//string debugingLine = "";

		//for (int i = 0; i < m_StageRoomCheck.GetLength(1) + 2; ++i)
		//	debugingLine += "＝";
		//debugingLine += "\n";

		//for (int y = m_StageRoomCheck.GetLength(0) - 1; y >= 0; --y)
		//{
		//	debugingLine += "＝";
		//	for (int x = 0; x < m_StageRoomCheck.GetLength(1); ++x)
		//	{
		//		debugingLine += (m_StageRoomCheck[y, x] == true) ? "ㅁ" : "　";
		//	}
		//	debugingLine += "＝\n";
		//}

		//for (int i = 0; i < m_StageRoomCheck.GetLength(1) + 2; ++i)
		//	debugingLine += "＝";
		//Debug.Log(debugingLine);
		#endregion

		for (int y = 0; y < m_StageRoomCheck.GetLength(0); ++y)
		{
			for (int x = 0; x < m_StageRoomCheck.GetLength(1); ++x)
			{
				if (m_StageRoomCheck[y, x] == false)
					continue;

				current = new Vector2Int(x, y);

				GenerateRoom(current, ref stage);
			}
		}

		#region 카메라 설정
		Vector2Int center = m_StageSize / 2;

		Room startRoom = m_GeneratedRoomMap[center];

		m_CameraFollow.UpdateClamp(startRoom.offset, startRoom.roomSize);
		#endregion

		stage.Initialize(stageSize, m_GeneratedRoomMap);

		return stage;
	}
	public void ClearStage()
	{
		int count = stageParent.childCount;
		for (int i = 0; i < count; ++i)
		{
			GameObject.Destroy(stageParent.GetChild(0).gameObject);
		}
	}

	public bool CheckRoomGenerated(int x, int y)
	{
		return m_StageRoomCheck[y, x];
	}
	public bool CheckRoomGenerated(Vector2Int check)
	{
		return m_StageRoomCheck[check.y, check.x];
	}

	private bool CheckRoomCondition(Vector2Int roomPos)
	{
		// 이미 충분한 방이 채워졌으면 포기
		if (m_StageRoomCount >= m_MaxRoomCount)
			return false;

		// 인덱스 체크
		if (roomPos.x < 0 || roomPos.x >= stageSize.x ||
			roomPos.y < 0 || roomPos.y >= stageSize.y)
			return false;

		// 이미 채워져 있으면 포기
		if (m_StageRoomCheck[roomPos.y, roomPos.x])
			return false;

		// 인접 칸이 둘 이상 채워져 있으면 포기
		int count = 0;
		Vector2Int check = roomPos + Vector2Int.left;
		if (!(
			check.x < 0 || check.x >= stageSize.x ||
			check.y < 0 || check.y >= stageSize.y
			) &&
			m_StageRoomCheck[check.y, check.x])
		{
			if (++count > 2)
				return false;
		}
		check = roomPos + Vector2Int.right;
		if (!(
			check.x < 0 || check.x >= stageSize.x ||
			check.y < 0 || check.y >= stageSize.y
			) &&
			m_StageRoomCheck[check.y, check.x])
		{
			if (++count > 2)
				return false;
		}
		check = roomPos + Vector2Int.up;
		if (!(
			check.x < 0 || check.x >= stageSize.x ||
			check.y < 0 || check.y >= stageSize.y
			) &&
			m_StageRoomCheck[check.y, check.x])
		{
			if (++count > 2)
				return false;
		}
		check = roomPos + Vector2Int.down;
		if (!(
			check.x < 0 || check.x >= stageSize.x ||
			check.y < 0 || check.y >= stageSize.y
			) &&
			m_StageRoomCheck[check.y, check.x])
		{
			if (++count > 2)
				return false;
		}

		// 50% 확률로 포기
		if (Random.Range(0, 2) == 0)
			return false;

		return true;
	}
	private void UpdateRoomCheck(Vector2Int roomPos)
	{
		if (CheckRoomCondition(roomPos) == false)
			return;

		int x = roomPos.x;
		int y = roomPos.y;

		++m_StageRoomCount;

		m_StageRoomCheck[y, x] = true;
		m_StageRoomQueue.Enqueue(roomPos);
		m_GeneratedRoomMap.Add(roomPos, null);
	}
	private void GenerateRoom(Vector2Int roomPos, ref Stage stage)
	{
		// 이미 생성한 곳
		if (m_GeneratedRoomMap.TryGetValue(roomPos, out Room currRoom) == true &&
			currRoom != null)
			return;

		Room room;
		Vector2Int center = stageSize / 2;

		if (roomPos == center)
		{
			room = M_Room.SpawnStartRoom("StartRoom");
			room.gameObject.SetActive(false);
			room.transform.SetParent(stage.transform);
			room.transform.position = Vector3.zero;
			room.Initialize();

			room.name = m_GeneratedRoomCount.ToString("00_StartRoom");
		}
		else
		{
			List<RoomCondition> conditionList = new List<RoomCondition>();

			#region 조건 확인
			for (E_Direction direction = 0; direction < E_Direction.Max; ++direction)
			{
				E_Condition condition;
				int count;

				Vector2Int nearRoomPos = roomPos + DirEnumUtil.ConvertToVector2Int(direction);

				if (m_GeneratedRoomMap.TryGetValue(nearRoomPos, out Room nearRoom) == false)
				{
					condition = E_Condition.Equal;

					count = 0;
				}
				else if (nearRoom != null)
				{
					condition = E_Condition.Equal;

					E_Direction nearRoomWarpDir = DirEnumUtil.GetOtherDir(direction);
					count = nearRoom.GetWarpPointCount(nearRoomWarpDir);
				}
				else
				{
					condition = E_Condition.More;

					count = 1;
				}

				conditionList.Add((condition, direction, count));
			}
			#endregion

			//for (int i = 0; i < conditionList.Count; ++i)
			//{
			//	Debug.Log((m_GeneratedRoomCount + 1).ToString("00_") + i.ToString("00: ") + conditionList[i].ToString());
			//}

			int x = roomPos.x;
			int y = roomPos.y;

			Vector3 pos = new Vector3((x - center.x) * 100, (y - center.y) * 100);

			room = M_Room.GetBuilder(conditionList.ToArray())
				.SetActive(false)
				.SetAutoInit(true)
				.SetParent(stage.transform)
				.SetPosition(pos)
				.Spawn();

			room.name = m_GeneratedRoomCount.ToString("00_") + room.name;
		}

		++m_GeneratedRoomCount;

		m_GeneratedRoomMap[roomPos] = room;

		for (E_Direction direction = 0; direction < E_Direction.Max; ++direction)
		{
			Vector2Int nearRoomPos = roomPos + DirEnumUtil.ConvertToVector2Int(direction);

			if (m_GeneratedRoomMap.ContainsKey(nearRoomPos) == false)
				continue;

			room.SetNearRoom(direction, m_GeneratedRoomMap[nearRoomPos]);
		}
	}
	private void ResetRoomCheck(int stageLevel, ref Stage stage)
	{
		m_MaxRoomCount = Random.Range(0, 2) + 5 + (int)(stageLevel * m_StageMagnification);

		Vector2Int center = m_StageSize / 2;

		m_StageRoomQueue.Clear();
		m_StageRoomQueue.Enqueue(center);

		for (int y = 0; y < m_StageSize.y; ++y)
		{
			for (int x = 0; x < m_StageSize.x; ++x)
			{
				m_StageRoomCheck[y, x] = false;
			}
		}
		m_StageRoomCheck[center.y, center.x] = true;
		m_StageRoomCount = 1;
		m_GeneratedRoomCount = 0;

		var rooms = stage.GetComponentsInChildren<Room>();
		for (int i = 0; i < rooms.Length; ++i)
		{
			M_Room.Despawn(rooms[i]);

			string[] splitedName = rooms[i].name.Split('_');
			rooms[i].name = splitedName[splitedName.Length - 1];
		}

		m_GeneratedRoomMap.Clear();
		m_GeneratedRoomMap.Add(center, null);
	}

	private void SetStageGeneratorArg(StageGeneratorArg arg)
	{
		m_StageParent = arg.stageParent;
		m_CurrentStageLevel = arg.currentStageLevel;
		m_StageSize = arg.stageSize;
	}

	public struct StageGeneratorArg
	{
		public Transform stageParent;

		public int currentStageLevel;

		public Vector2Int stageSize;
	}

	#region Debuging Info
	[Space(10)]
	[SerializeField, ReadOnly]
	private Info m_Info;

	public void OnValidate()
	{
		m_Info.MinExpectedRoomCount = 0 + 5 + (int)(M_Stage.currentStageLevel * m_StageMagnification);
		m_Info.MaxExpectedRoomCount = 1 + 5 + (int)(M_Stage.currentStageLevel * m_StageMagnification);
		m_Info.MaxRoomCount = m_MaxRoomCount;
	}

	[System.Serializable]
	struct Info
	{
		public int MinExpectedRoomCount;
		public int MaxExpectedRoomCount;
		public int MaxRoomCount;
	}
	#endregion
}