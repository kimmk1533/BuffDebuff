using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BuffDebuff.Enum;
using RoomPool = ObjectPool<BuffDebuff.Room>;

namespace BuffDebuff
{
	public class RoomManager : ObjectManager<RoomManager, Room>
	{
		#region 변수
		private List<RoomPool> m_AllRoomPool = null;
		#endregion

		#region 이벤트
		public System.Action<Room> onRoomClear;
		#endregion

		public override void Initialize()
		{
			base.Initialize();

			if (m_AllRoomPool == null)
				m_AllRoomPool = new List<RoomPool>();
		}
		public override void Finallize()
		{
			base.Finallize();

			if (m_AllRoomPool != null)
			{
				foreach (var item in m_AllRoomPool)
				{
					item.Dispose();
				}
				m_AllRoomPool.Clear();
			}
		}

		public override void InitializeGame()
		{
			base.InitializeGame();

			foreach (var item in m_ObjectPoolMap)
			{
				m_AllRoomPool.Add(item.Value);
			}
			m_AllRoomPool = m_AllRoomPool.Distinct().ToList();
		}
		public override void FinallizeGame()
		{
			base.FinallizeGame();

			m_AllRoomPool.Clear();
		}

		public RoomPool.ItemBuilder GetRandomBuilder(E_DirectionFlag dirCheck)
		{
			List<RoomPool> roomPoolList = new List<RoomPool>(m_AllRoomPool);

			for (int roomPoolIndex = 0; roomPoolIndex < roomPoolList.Count; ++roomPoolIndex)
			{
				RoomPool pool = roomPoolList[roomPoolIndex];

				Room room = pool.GetBuilder()
					.SetAutoInit(true)
					.Spawn();

				for (E_DirectionFlag direction = E_DirectionFlag.None + 1; direction < E_DirectionFlag.Max; direction = (E_DirectionFlag)((int)direction << 1))
				{
					InfiniteLoopDetector.Run();

					// Log를 이용하여 E_DirectionFlag에서 E_Direction으로 변환
					int log = (int)(Mathf.Log((int)direction, 2) + 1);
					E_Direction dir = (E_Direction)(log - 1);

					// 해당 방향의 워프포인트 갯수 확인
					int count = room.GetWarpPointCount(dir);
					bool flag = false;
					// 조건 확인(해당 방향에 워프포인트가 존재하는 지 확인)
					if (dirCheck.HasFlag(direction) == true)
						flag = count <= 0;

					// 조건이 맞지 않으면 해당 방 제거
					if (flag)
					{
						roomPoolList.RemoveAt(roomPoolIndex);
						--roomPoolIndex;
						break;
					}
				}

				pool.Despawn(room, true);
			}

			if (roomPoolList.Count <= 0)
				throw new System.Exception("Error: 조건에 맞는 방이 없음");

			int index = Random.Range(0, roomPoolList.Count);

			return roomPoolList[index].GetBuilder();
		}

		#region UNITY_EDITOR
#if UNITY_EDITOR
		[ContextMenu("All Flag On")]
		private void TurnOnFlagAll()
		{
			for (int i = 0; i < m_Origins.Count; ++i)
			{
				var temp = m_Origins[i];
				temp.useFlag = true;
				m_Origins[i] = temp;
			}
		}
		[ContextMenu("All Flag Off")]
		private void TurnOffFlagAll()
		{
			for (int i = 0; i < m_Origins.Count; ++i)
			{
				var temp = m_Origins[i];
				temp.useFlag = false;
				m_Origins[i] = temp;
			}
		}

		[ContextMenu("Load Origin")]
		protected override void LoadOrigin()
		{
			base.LoadOrigin_Inner();
		}
#endif
		#endregion
	}
}