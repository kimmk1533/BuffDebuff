using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuffDebuff
{
	public class ProjectileData : SOData
	{
		#region 변수
		// 이동 속도
		[SerializeField, ReadOnly]
		private float m_MoveSpeed;
		// 생존 시간
		[SerializeField, ReadOnly]
		private float m_LifeTime;
		#endregion

		#region 프로퍼티
		// 이동 속도
		public float moveSpeed => m_MoveSpeed;
		// 생존 시간
		public float lifeTime => m_LifeTime;
		#endregion

		public void Initialize(string _assetPath, int _code, string _title, float _moveSpeed, float _lifeTime)
		{
			// 에셋 경로
			m_AssetPath = _assetPath;
			// 코드
			m_Code = _code;
			// 명칭
			m_Title = _title;

			// 이동 속도
			m_MoveSpeed = _moveSpeed;
			// 생존 시간
			m_LifeTime = _lifeTime;
		}
	}
}