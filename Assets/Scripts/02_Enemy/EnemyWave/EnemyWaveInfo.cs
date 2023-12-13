using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct EnemyWaveInfo
{
	#region 변수
	// 생성할 적 key
	[SerializeField]
	private string m_Key;
	// 생성될 위치
	[SerializeField]
	private Transform m_SpawnPoint;
	// 지연 시간
	[SerializeField]
	private float m_DelayTime;

	// Clear Room 조건에 필요한 변수
	[SerializeField, Min(0)]
	private int m_Index;

	// Timer 조건에 필요한 변수
	[SerializeField]
	private float m_WaitTimerTime;

	// Random 조건에 필요한 변수
	[SerializeField, Range(0, 100)]
	private int m_RandomPercent;
	#endregion

	#region 프로퍼티
	public string key => m_Key;
	public float delayTime => m_DelayTime;
	public int index => m_Index;
	public float waitTimerTime => m_WaitTimerTime;
	#endregion

	#region 매니저
	private EnemyManager M_Enemy => EnemyManager.Instance;
	#endregion

	public Enemy Spawn(Room currentRoom)
	{
		Enemy enemy = M_Enemy.GetBuilder(m_Key)
			.SetActive(true)
			.SetAutoInit(true)
			.SetParent(null)
			.SetPosition(currentRoom.transform.position + m_SpawnPoint.localPosition)
			.Spawn();

		return enemy;
	}

	public bool CheckRandom() => Random.Range(0, 100) < m_RandomPercent;
}