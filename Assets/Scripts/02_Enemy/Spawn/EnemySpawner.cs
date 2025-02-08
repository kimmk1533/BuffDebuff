using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace BuffDebuff
{
	public class EnemySpawner : SerializedMonoBehaviour
	{
		#region 변수
		private Room m_Room = null;

		// 적 생성 정보 리스트 (인스펙터 작성용)
		[OdinSerialize]
		private List<EnemySpawnInfo> m_EnemySpawnInfoList = null;
		// 적 생성 정보를 담은 우선순위 큐
		[OdinSerialize, Sirenix.OdinInspector.ReadOnly]
		private PriorityQueue<float, EnemySpawnInfo> m_EnemySpawnInfoQueue = null;
		// 적 생성 타이머
		[SerializeField, Sirenix.OdinInspector.ReadOnly]
		[InlineProperty(LabelWidth = 100)]
		private UtilClass.Timer m_EnemySpawnTimer = null;

		// 적 생성 진행 여부
		private bool m_IsInProgress = false;
		// 이 방 클리어 여부
		[OdinSerialize, Sirenix.OdinInspector.ReadOnly]
		private bool m_IsClear = false;

		// 생성한 적 리스트
		[SerializeField, Sirenix.OdinInspector.ReadOnly]
		private List<Enemy> m_SpawnedEnemyList;
		#endregion

		#region 프로퍼티
		public bool isClear => m_IsClear;
		#endregion

		#region 이벤트
		/// <summary>
		/// 플레이어가 방에 입장할 때 호출되는 이벤트 함수
		/// </summary>
		public void OnPlayerEnterRoom()
		{
			if (m_IsClear == true)
				return;

			m_IsInProgress = true;

			UpdateEnemySpawnQueue();

			m_EnemySpawnTimer.Resume();
		}
		/// <summary>
		/// 플레이어가 방에서 퇴장할 때 호출되는 이벤트 함수
		/// </summary>
		public void OnPlayerExitRoom()
		{
			if (m_IsClear == true)
				return;

			m_IsInProgress = false;

			DespawnAllEnemy();

			m_EnemySpawnTimer.Pause();
			m_EnemySpawnTimer.Clear();
		}
		/// <summary>
		/// 플레이어가 방을 클리어했을 때 호출되는 이벤트 함수
		/// </summary>
		public void OnPlayerClearRoom()
		{
			if (m_IsInProgress == false)
				return;

			m_IsClear = true;
			m_Room.ClearRoom();
		}

		/// <summary>
		/// 적이 디스폰 될 때 호출되는 이벤트 함수
		/// </summary>
		/// <param name="arg"></param>
		/// <exception cref="System.NullReferenceException"></exception>
		private void OnEnemyDespawn(ObjectPoolItemBase arg)
		{
			Enemy enemy = arg as Enemy;
			if (enemy == null)
				throw new System.NullReferenceException();

			// 생성된 적 리스트에서 제거
			m_SpawnedEnemyList.Remove(enemy);

			if (m_IsInProgress == false)
				return;

			// 현재 생성한 모든 적을 잡은 경우
			if (m_SpawnedEnemyList.Count == 0)
			{
				// 더 생성할 적이 없으면
				if (m_EnemySpawnInfoQueue.Count == 0)
					// 방 클리어
					OnPlayerClearRoom();
				// 더 생성할 적이 있으면
				else
					// 다음 적 바로 생성
					m_EnemySpawnTimer.time = m_EnemySpawnInfoQueue.Peek().Priorty;
			}
		}
		#endregion

		#region 매니저
		private static EnemyManager M_Enemy => EnemyManager.Instance;
		#endregion

		#region 유니티 함수
		private void Update()
		{
			if (m_EnemySpawnTimer.isPaused == true)
				return;

			m_EnemySpawnTimer.Update();
			while (m_EnemySpawnInfoQueue.Count > 0 &&
				m_EnemySpawnTimer.time >= m_EnemySpawnInfoQueue.Peek().Priorty)
			{
				(float delayTime, EnemySpawnInfo enemySpawnInfo) = m_EnemySpawnInfoQueue.Dequeue();

				Spawn(enemySpawnInfo);
			}

			void Spawn(EnemySpawnInfo info)
			{
				Enemy enemy = info.Spawn(transform.position);

				enemy.onDespawn -= OnEnemyDespawn;
				enemy.onDespawn += OnEnemyDespawn;

				m_SpawnedEnemyList.Add(enemy);
			}
		}
		#endregion

		public void Initialize(Room room)
		{
			m_Room = room;

			if (m_EnemySpawnInfoQueue == null)
				m_EnemySpawnInfoQueue = new PriorityQueue<float, EnemySpawnInfo>();

			if (m_EnemySpawnTimer == null)
				m_EnemySpawnTimer = new UtilClass.Timer();
			m_EnemySpawnTimer.Clear();
			m_EnemySpawnTimer.Pause();
			m_EnemySpawnTimer.interval = float.PositiveInfinity;

			// 방 클리어 여부 초기화
			m_IsClear = false;

			m_IsInProgress = false;

			if (m_SpawnedEnemyList == null)
				m_SpawnedEnemyList = new List<Enemy>();
		}
		public void Finallize()
		{
			m_IsClear = false;

			m_IsInProgress = false;

			DespawnAllEnemy();
		}

		private void UpdateEnemySpawnQueue()
		{
			m_EnemySpawnInfoQueue.Clear();
			for (int i = 0; i < m_EnemySpawnInfoList.Count; ++i)
			{
				EnemySpawnInfo info = m_EnemySpawnInfoList[i];

				m_EnemySpawnInfoQueue.Enqueue(info.delayTime, info);
			}
		}
		private void DespawnAllEnemy()
		{
			int count = m_SpawnedEnemyList.Count;
			for (int i = 0; i < count; ++i)
			{
				M_Enemy.Despawn(m_SpawnedEnemyList[0]);
			}
			m_SpawnedEnemyList.Clear();
		}
	}
}