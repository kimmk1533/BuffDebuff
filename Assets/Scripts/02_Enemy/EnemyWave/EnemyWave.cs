using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct EnemyWave
{
	// 몬스터 생성 조건
	public enum E_InitCondition
	{
		// 방에 입장 시
		EnterRoom,

		// 방에 존재하던 모든 몬스터를 처치했을 시
		ClearRoom,

		// 일정 시간이 흐를 시
		Time,

		// 랜덤 확률로
		Random
	}

	#region 변수
	[SerializeField]
	private E_InitCondition m_InitCondition;

	private bool m_IsCreated;

	[Space(10)]
	[SerializeField]
	private DebugDictionary<Transform, string> m_EnemyList;

	[SerializeField]
	private UtilClass.Timer m_CreateEnemyTimer;
	#endregion

	#region 프로퍼티
	public E_InitCondition initCondition => m_InitCondition;
	public int Count => m_EnemyList.Count;
	#endregion

	#region 매니저
	private EnemyManager M_Enemy => EnemyManager.Instance;
	#endregion

	public List<Enemy> CreateEnemy(Room currentRoom)
	{
		List<Enemy> createdEnemyList = new List<Enemy>();

		if (m_IsCreated == true)
			return createdEnemyList;

		Vector3 positionOffset = currentRoom.transform.position;

		foreach (KeyValuePair<Transform, string> item in m_EnemyList)
		{
			string itemName = item.Value;

			Enemy enemy = M_Enemy.Spawn(itemName);

			enemy.gameObject.SetActive(true);

			enemy.Initialize();

			enemy.transform.position = item.Key.localPosition + positionOffset;

			createdEnemyList.Add(enemy);
		}

		m_IsCreated = true;

		return createdEnemyList;
	}
	public void Reset()
	{
		m_IsCreated = false;

		m_CreateEnemyTimer.Clear();
	}
	public void TimerUpdate()
	{
		m_CreateEnemyTimer.Update();
	}
}