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

	[Space(10)]
	[SerializeField]
	private DebugDictionary<Transform, Enemy> m_EnemyList;
	#endregion

	#region 프로퍼티
	public E_InitCondition initCondition => m_InitCondition;
	public int Count => m_EnemyList.Count;
	#endregion
}