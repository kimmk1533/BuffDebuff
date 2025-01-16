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
		#endregion

		#region 프로퍼티
		public float delayTime => m_DelayTime;
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