using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using E_SpawnCondition = System.ValueTuple<BuffDebuff.EnemySpawner.E_MainSpawnCondition, BuffDebuff.EnemySpawner.E_SubSpawnCondition>;

namespace BuffDebuff
{
	public class EnemySpawner : SerializedMonoBehaviour
	{
		#region Enum
		// 몬스터 생성 조건
		public enum E_MainSpawnCondition : byte
		{
			None,

			// 방에 입장 시
			EnterRoom,
			ClearRoom,
		}
		public enum E_SubSpawnCondition : byte
		{
			// 방에 입장 시
			None,
			// 방에 존재하던 모든 적을 처치했을 시
			NextIndex,
			// 방에 입장 후 일정 시간이 지나면
			Delay,
		}
		#endregion

		#region 변수
		private Room m_Room = null;

		// 이 방 클리어 여부
		[OdinSerialize, Sirenix.OdinInspector.ReadOnly]
		private bool m_IsClear = false;

		[OdinSerialize, Sirenix.OdinInspector.ReadOnly]
		private int m_EnemyWaveIndex = -1;

		//// 적 생성 정보
		//[OdinSerialize]
		//private List<EnemyWave> m_EnemyWave = null;

		[OdinSerialize]
		[DictionaryDrawerSettings(KeyLabel = "적 생성 조건", ValueLabel = "적 생성 정보")]
		private Dictionary<E_MainSpawnCondition, List<EnemySpawnInfo>> m_EnemyWaveInfoMap = null;
		[OdinSerialize]
		[DictionaryDrawerSettings(KeyLabel = "적 생성 조건", ValueLabel = "적 생성 정보")]
		private Dictionary<E_SpawnCondition, List<EnemySpawnInfo>> m_Test = null;

		private bool m_IsInProgress;

		private int m_SpawnIndex;
		private int m_MaxSpawnIndex;

		//public EnemyWaveInfo m_Test;

		[SerializeField, Sirenix.OdinInspector.ReadOnly]
		private List<Enemy> m_SpawnedEnemyList;
		#endregion

		#region 프로퍼티
		public bool isClear => m_IsClear;
		#endregion

		#region 이벤트
		#endregion

		#region 매니저
		private static EnemyManager M_Enemy => EnemyManager.Instance;
		#endregion

		public void Initialize(Room room)
		{
			m_Room = room;

			// 방 클리어 여부 초기화
			m_IsClear = false;

			m_IsInProgress = false;

			m_SpawnIndex = 0;
			m_MaxSpawnIndex = -1;
			if (m_EnemyWaveInfoMap.TryGetValue(E_MainSpawnCondition.ClearRoom, out List<EnemySpawnInfo> waveInfoList) == true)
			{
				for (int i = 0; i < waveInfoList.Count; ++i)
				{
					EnemySpawnInfo item = waveInfoList[i];

					m_MaxSpawnIndex = Mathf.Max(m_MaxSpawnIndex, item.index);
				}
			}

			if (m_SpawnedEnemyList == null)
				m_SpawnedEnemyList = new List<Enemy>();
		}
		public void Finallize()
		{
			m_IsClear = false;

			Reset();
		}

		public void SpawnEnemyWave()
		{
			if (m_IsClear == true)
				return;

			m_IsInProgress = true;

			CreateEnemy(E_MainSpawnCondition.EnterRoom);
		}
		private void CreateEnemy(E_MainSpawnCondition condition)
		{
			if (m_EnemyWaveInfoMap.ContainsKey(condition) == false)
				return;

			List<EnemySpawnInfo> waveInfoList = m_EnemyWaveInfoMap[condition];

			bool anyEnemySpawned = false;

			for (int i = 0; i < waveInfoList.Count; ++i)
			{
				EnemySpawnInfo info = waveInfoList[i];

				if (condition == E_MainSpawnCondition.ClearRoom &&
					info.index != m_SpawnIndex)
					continue;

				anyEnemySpawned = true;

				float delay = info.delayTime;

				if (delay > 0f)
					StartCoroutine(Spawn_Delay(info, delay));
				else
					Spawn(info);
			}

			if (anyEnemySpawned == false)
				OnClearRoom();
		}
		private void Spawn(EnemySpawnInfo info)
		{
			Enemy enemy = info.Spawn(transform.position);

			enemy.onDespawn -= OnEnemyDespawn;
			enemy.onDespawn += OnEnemyDespawn;

			m_SpawnedEnemyList.Add(enemy);
		}
		private IEnumerator Spawn_Delay(EnemySpawnInfo info, float delay)
		{
			yield return new WaitForSeconds(delay);

			Spawn(info);
		}

		public void DespawnEnemyWave()
		{
			if (m_IsClear == true)
				return;

			Reset();

			StopAllCoroutines();
		}

		private void OnEnemyDespawn(ObjectPoolItemBase arg)
		{
			Enemy enemy = arg as Enemy;
			if (enemy == null)
				throw new System.NullReferenceException();

			m_SpawnedEnemyList.Remove(enemy);

			if (m_SpawnedEnemyList.Count == 0)
			{
				OnClearRoom();
			}
		}
		public void OnClearRoom()
		{
			if (m_IsInProgress == false)
				return;
			if (m_SpawnIndex <= m_MaxSpawnIndex)
			{
				++m_SpawnIndex;

				CreateEnemy(E_MainSpawnCondition.ClearRoom);
				return;
			}

			m_IsClear = true;

			m_Room.ClearRoom();
		}
		private void Reset()
		{
			m_IsInProgress = false;

			m_SpawnIndex = 0;

			int count = m_SpawnedEnemyList.Count;
			for (int i = 0; i < count; ++i)
			{
				M_Enemy.Despawn(m_SpawnedEnemyList[0]);
			}
			m_SpawnedEnemyList.Clear();
		}
	}
}