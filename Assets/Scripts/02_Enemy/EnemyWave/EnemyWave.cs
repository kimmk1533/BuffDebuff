using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AYellowpaper.SerializedCollections;

[System.Serializable]
public struct EnemyWave
{
	// 몬스터 생성 조건
	public enum E_SpawnCondition : byte
	{
		// 방에 입장 시
		EnterRoom,

		// 방에 존재하던 모든 적을 처치했을 시
		ClearRoom,

		// 일정 시간이 흐를 시
		Timer,

		// 랜덤 확률로
		Random,

		Max
	}

	#region 변수
	[Space(10)]
	[SerializeField]
	[SerializedDictionary("조건", "적 정보")]
	private SerializedDictionary<E_SpawnCondition, List<EnemyWaveInfo>> m_EnemyWaveInfoMap;
	private List<Enemy> m_SpawnedEnemyList;

	private bool m_IsSpawned;

	[Space(10)]
	[SerializeField]
	private UtilClass.Timer m_SpawnEnemyTimer;
	#endregion

	#region 프로퍼티
	//public E_SpawnCondition spawnCondition => m_SpawnCondition;
	#endregion

	#region 매니저
	#endregion

	public List<Enemy> CreateEnemy(E_SpawnCondition condition, Room currentRoom, int index = 0)
	{
		if (m_EnemyWaveInfoMap.ContainsKey(condition) == false)
			return null;

		if (m_IsSpawned == true)
			return m_SpawnedEnemyList;

		if (m_SpawnedEnemyList == null)
			m_SpawnedEnemyList = new List<Enemy>();
		else
			m_SpawnedEnemyList.Clear();

		List<EnemyWaveInfo> waveInfoList = m_EnemyWaveInfoMap[condition];
		EnemyWaveInfo info = waveInfoList[index];

		//foreach (var info in waveInfoList)
		//{
		float delay = info.delayTime;

		if (condition == E_SpawnCondition.Timer)
			delay += info.waitTimerTime;

		currentRoom.StartCoroutine(SpawnEnemyCoroutine(info, currentRoom, delay));
		//}

		m_IsSpawned = true;

		return m_SpawnedEnemyList;
	}
	public void Reset()
	{
		m_IsSpawned = false;

		m_SpawnEnemyTimer.Clear();
	}
	public void TimerUpdate()
	{
		m_SpawnEnemyTimer.Update();
	}

	private IEnumerator SpawnEnemyCoroutine(EnemyWaveInfo info, Room currentRoom, float delay)
	{
		yield return new WaitForSeconds(delay);

		Enemy enemy = info.Spawn(currentRoom);

		enemy.onDeath += currentRoom.OnCreatedEnemyDeath;

		m_SpawnedEnemyList.Add(enemy);
	}
}