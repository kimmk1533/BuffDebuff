using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuffDebuff
{
	public class EnemyWaveSpawner : MonoBehaviour
	{
		#region 변수
		private Room m_Room = null;

		// 이 방 클리어 여부
		private bool m_IsClear = false;

		// 적 생성 정보
		[SerializeField]
		private List<EnemyWave> m_EnemyWave = null;
		[SerializeField, ReadOnly]
		private int m_EnemyWaveIndex = -1;
		#endregion

		#region 프로퍼티
		public bool isClear => m_IsClear;
		#endregion

		#region 이벤트
		#endregion

		#region 매니저
		#endregion

		public void Initialize(Room room)
		{
			m_Room = room;

			// 방 클리어 여부 초기화
			m_IsClear = false;

			// 적 생성 정보 초기화
			if (m_EnemyWave.Count > 0)
			{
				m_EnemyWaveIndex = Random.Range(0, m_EnemyWave.Count);
				m_EnemyWave[m_EnemyWaveIndex].Initialize(this);
			}
		}
		public void Finallize()
		{
			m_IsClear = false;

			if (m_EnemyWave.Count > 0)
			{
				m_EnemyWave[m_EnemyWaveIndex].Finallize();
				m_EnemyWaveIndex = -1;
			}
		}

		public void SpawnEnemyWave()
		{
			if (m_IsClear == true)
				return;
			if (m_EnemyWave.Count <= 0)
				return;
			if (m_EnemyWaveIndex < 0 || m_EnemyWaveIndex >= m_EnemyWave.Count)
				throw new System.ArgumentOutOfRangeException();

			m_EnemyWave[m_EnemyWaveIndex].SpawnEnemy();
		}
		public void DespawnEnemyWave()
		{
			if (m_IsClear == true)
				return;
			if (m_EnemyWave.Count <= 0)
				return;
			if (m_EnemyWaveIndex < 0 || m_EnemyWaveIndex >= m_EnemyWave.Count)
				throw new System.ArgumentOutOfRangeException();

			m_EnemyWave[m_EnemyWaveIndex].Reset();

			StopAllCoroutines();
		}

		public void OnClearRoom()
		{
			m_IsClear = true;

			m_Room.ClearRoom();
		}
	}
}