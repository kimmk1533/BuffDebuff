using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuffDebuff
{
	[RequireComponent(typeof(StageGenerator))]
	public class StageManager : Singleton<StageManager>
	{
		private static readonly Vector2Int[] c_StageSize =
		{
			new Vector2Int(5, 4),	// stage 1
			new Vector2Int(6, 5),	// stage 2
			new Vector2Int(7, 6),	// stage 3
			new Vector2Int(8, 7),	// stage 4
			new Vector2Int(9, 8),	// stage 5
			new Vector2Int(10, 9),	// stage 6
		};

		#region 변수
		private Transform m_StageParent = null;

		[SerializeField, Range(1, 6)]
		private int m_CurrentStageLevel = 1;
		[SerializeField]
		private Vector2Int m_StageSize = c_StageSize[0];
		private Stage m_CurrentStage = null;
		private StageGenerator m_StageGenerator = null;
		#endregion

		#region 프로퍼티
		public Stage currentStage => m_CurrentStage;
		public Transform stageParent => m_StageParent.transform;
		public int currentStageLevel => m_CurrentStageLevel;
		public Vector2Int stageSize => m_StageSize;
		public int stageWidth => m_StageSize.x;
		public int stageHeight => m_StageSize.y;
		#endregion

		#region 이벤트
		public event System.Action onStageGenerated;
		#endregion

		public void Initialize()
		{
			this.NullCheckGetComponent<StageGenerator>(ref m_StageGenerator);
			m_StageGenerator.Initialize();
		}
		public void Finallize()
		{
			m_StageGenerator.Finallize();
		}

		public void InitializeGame()
		{
			if (m_StageParent == null)
			{
				m_StageParent = new GameObject("Stage Grid").transform;
				m_StageParent.gameObject.isStatic = true;
				m_StageParent.AddComponent<Grid>();
			}

			//m_CurrentStageLevel = 1;
			m_StageSize = c_StageSize[m_CurrentStageLevel - 1];

			m_StageGenerator.InitializeGame();
			m_CurrentStage = m_StageGenerator.GenerateStage(new StageGenerator.StageGeneratorArg()
			{
				stageParent = m_StageParent,
				currentStageLevel = m_CurrentStageLevel,
				stageSize = m_StageSize
			});
			onStageGenerated?.Invoke();
		}
		public void FinallizeGame()
		{

		}

		public void NextStage()
		{
			++m_CurrentStageLevel;

			m_StageSize = c_StageSize[m_CurrentStageLevel - 1];

			m_CurrentStage = m_StageGenerator.GenerateStage(new StageGenerator.StageGeneratorArg()
			{
				stageParent = m_StageParent,
				currentStageLevel = m_CurrentStageLevel,
				stageSize = m_StageSize
			});
			onStageGenerated?.Invoke();
		}

		//public Transform GetSpawnPoint(Collider2D collider)
		//{
		//	Room room = null;
		//	Transform warpPoint = null;

		//	switch (collider.transform.parent.name)
		//	{
		//		case "Left":
		//			if (m_CurrentPos.x - 1 < 0)
		//				break;

		//			room = m_GeneratedRooms[m_CurrentPos.y, m_CurrentPos.x - 1];

		//			if (room != null)
		//			{
		//				warpPoint = room.GetWarpPoint(Room.E_RoomDir.Right);
		//				--m_CurrentPos.x;
		//				m_CameraFollow.clampOffset += Vector2.left * room.clampAreaSize.x;
		//			}
		//			break;
		//		case "Right":
		//			if (m_CurrentPos.x + 1 >= m_MapSize.x)
		//				break;

		//			room = m_GeneratedRooms[m_CurrentPos.y, m_CurrentPos.x + 1];

		//			if (room != null)
		//			{
		//				warpPoint = room.GetWarpPoint(Room.E_RoomDir.Left);
		//				++m_CurrentPos.x;
		//				m_CameraFollow.clampOffset += Vector2.right * room.clampAreaSize.x;
		//			}
		//			break;
		//		case "Top":
		//			if (m_CurrentPos.y + 1 >= m_MapSize.y)
		//				break;

		//			room = m_GeneratedRooms[m_CurrentPos.y + 1, m_CurrentPos.x];

		//			if (room != null)
		//			{
		//				warpPoint = room.GetWarpPoint(Room.E_RoomDir.Bottom);
		//				++m_CurrentPos.y;
		//				m_CameraFollow.clampOffset += Vector2.up * room.clampAreaSize.y;
		//			}
		//			break;
		//		case "Bottom":
		//			if (m_CurrentPos.y - 1 < 0)
		//				break;

		//			room = m_GeneratedRooms[m_CurrentPos.y - 1, m_CurrentPos.x];

		//			if (room != null)
		//			{
		//				warpPoint = room.GetWarpPoint(Room.E_RoomDir.Top);
		//				--m_CurrentPos.y;
		//				m_CameraFollow.clampOffset += Vector2.down * room.clampAreaSize.y;
		//			}
		//			break;
		//	}

		//	if (warpPoint != null)
		//	{
		//		m_CameraFollow.transform.position = warpPoint.position + Vector3.up * 3f;
		//	}

		//	return warpPoint;
		//}

		private void OnValidate()
		{
			m_StageSize = c_StageSize[m_CurrentStageLevel - 1];
			GetComponent<StageGenerator>().OnValidate();
		}
	}
}