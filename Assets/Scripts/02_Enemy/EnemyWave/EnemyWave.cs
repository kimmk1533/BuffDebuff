using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AYellowpaper.SerializedCollections;

[System.Serializable]
public class EnemyWave
{
	#region Enum
	// 몬스터 생성 조건
	public enum E_SpawnCondition : byte
	{
		// 방에 입장 시
		EnterRoom,

		// 방에 존재하던 모든 적을 처치했을 시
		ClearRoom,

		Max
	}
	#endregion

	#region 변수
	private EnemyWaveSpawner m_Spawner = null;

	private bool m_IsInProgress;

	private int m_SpawnIndex;
	private int m_MaxSpawnIndex;

	[Space(10)]
	[SerializeField]
	[SerializedDictionary("조건", "적 정보")]
	private SerializedDictionary<E_SpawnCondition, List<EnemyWaveInfo>> m_EnemyWaveInfoMap;

	private List<Enemy> m_SpawnedEnemyList;
	#endregion

	#region 매니저
	private static EnemyManager M_Enemy => EnemyManager.Instance;
	#endregion

	public void Initialize(EnemyWaveSpawner spawner)
	{
		m_Spawner = spawner;

		m_IsInProgress = false;

		m_SpawnIndex = 0;
		m_MaxSpawnIndex = 0;
		if (m_EnemyWaveInfoMap.TryGetValue(E_SpawnCondition.ClearRoom, out List<EnemyWaveInfo> waveInfoList) == true)
		{
			for (int i = 0; i < waveInfoList.Count; ++i)
			{
				EnemyWaveInfo item = waveInfoList[i];

				m_MaxSpawnIndex = Mathf.Max(m_MaxSpawnIndex, item.index);
			}
		}

		if (m_SpawnedEnemyList == null)
			m_SpawnedEnemyList = new List<Enemy>();
	}
	public void Finallize()
	{
		Reset();

		if (m_SpawnedEnemyList != null)
			m_SpawnedEnemyList.Clear();
	}

	public void SpawnEnemy()
	{
		m_IsInProgress = true;

		CreateEnemy(E_SpawnCondition.EnterRoom);
	}
	private void CreateEnemy(E_SpawnCondition condition)
	{
		if (m_Spawner == null)
			return;
		if (m_EnemyWaveInfoMap.ContainsKey(condition) == false)
			return;

		List<EnemyWaveInfo> waveInfoList = m_EnemyWaveInfoMap[condition];

		bool anyEnemySpawned = false;

		for (int i = 0; i < waveInfoList.Count; ++i)
		{
			EnemyWaveInfo info = waveInfoList[i];

			if (condition == E_SpawnCondition.ClearRoom &&
				info.index != m_SpawnIndex)
				continue;

			if (info.CheckRandom() == false)
				continue;

			anyEnemySpawned = true;

			float delay = info.delayTime;

			if (delay > 0f)
				m_Spawner.StartCoroutine(Spawn_Delay(info, delay));
			else
				Spawn(info);
		}

		if (anyEnemySpawned == false)
			OnClearRoom();
	}
	private void OnClearRoom()
	{
		if (m_IsInProgress == false)
			return;
		if (m_SpawnIndex <= m_MaxSpawnIndex)
		{
			++m_SpawnIndex;

			CreateEnemy(E_SpawnCondition.ClearRoom);
			return;
		}

		m_Spawner.OnClearRoom();
	}
	public void Reset()
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

	private void Spawn(EnemyWaveInfo info)
	{
		Enemy enemy = info.Spawn(m_Spawner.transform.position);

		enemy.onDespawn -= OnEnemyDespawn;
		enemy.onDespawn += OnEnemyDespawn;

		m_SpawnedEnemyList.Add(enemy);
	}
	private IEnumerator Spawn_Delay(EnemyWaveInfo info, float delay)
	{
		yield return new WaitForSeconds(delay);

		Spawn(info);
	}
}