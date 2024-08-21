using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuffDebuff
{
	public class LevelData : SOData
	{
		#region 변수
		// 현재 레벨
		[SerializeField, ReadOnly]
		private int m_CurrentLevel;
		// 다음 레벨
		[SerializeField, ReadOnly]
		private int m_NextLevel;
		// 필요 경험치
		[SerializeField, ReadOnly]
		private float m_RequiredXp;
		#endregion

		#region 프로퍼티
		public int currentLevel => m_CurrentLevel;
		public int nextLevel => m_NextLevel;
		public float requiredXp => m_RequiredXp;
		#endregion

		public void Initialize(int _currentLevel, int _nextLevel, float _requiredXp)
		{
			// 에셋 경로
			m_AssetPath = "";
			// 코드
			m_Code = _currentLevel;
			// 명칭
			m_Title = "Level " + _currentLevel.ToString();

			// 현재 레벨
			m_CurrentLevel = _currentLevel;
			// 다음 레벨
			m_NextLevel = _nextLevel;
			// 필요 경험치
			m_RequiredXp = _requiredXp;
		}
	}
}