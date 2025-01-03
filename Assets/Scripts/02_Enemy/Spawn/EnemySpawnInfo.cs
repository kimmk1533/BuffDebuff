using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BuffDebuff
{
	[System.Serializable]
	public struct EnemySpawnInfo
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

		// ClearRoom 조건에 필요한 변수
		// 생성 인덱스
		[SerializeField, Min(0)]
		private int m_Index;
		#endregion

		#region 프로퍼티
		public string key => m_Key;
		public float delayTime => m_DelayTime;
		public int index => m_Index;
		#endregion

		#region 매니저
		private static EnemyManager M_Enemy => EnemyManager.Instance;
		#endregion

		public Enemy Spawn(Vector3 offset)
		{
			Enemy enemy = M_Enemy.GetBuilder(m_Key)
				.SetActive(true)
				.SetAutoInit(true)
				.SetParent(null)
				.SetPosition(m_SpawnPoint.localPosition + offset)
				.Spawn();

			return enemy;
		}
	}
}