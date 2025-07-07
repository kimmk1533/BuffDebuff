using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using DataDictionary = DoubleKeyDictionary<int, string, BuffDebuff.ProjectileData>;

namespace BuffDebuff
{
	public class ProjectileManager : ObjectManager<ProjectileManager, Projectile>
	{
		#region 기본 템플릿
		#region 변수
		private DataDictionary m_ProjectileDataMap = null;
		#endregion

		#region 프로퍼티
		#endregion

		#region 이벤트

		#region 이벤트 함수
		#endregion
		#endregion

		#region 매니저
		#endregion

		#region 초기화 & 마무리화 함수
		/// <summary>
		/// 초기화 함수 (Init Scene 진입 시, 즉 게임 실행 시 호출)
		/// </summary>
		public override void Initialize()
		{
			base.Initialize();

			for (int i = 0; i < m_Origins.Count; ++i)
			{
				OriginInfo originInfo = m_Origins[i];

				if (originInfo.useFlag == false)
					continue;

				ObjectPool<Projectile> projectilePool = GetPool(originInfo.key);
				ProjectileBuilder projectileBuilder = new ProjectileBuilder(projectilePool);
				projectilePool.builder = projectileBuilder;
			}

			if (m_ProjectileDataMap == null)
				m_ProjectileDataMap = new DataDictionary();

			LoadAllProjectileData();
		}
		/// <summary>
		/// 마무리화 함수 (게임 종료 시 호출)
		/// </summary>
		public override void Finallize()
		{
			base.Finallize();


		}

		/// <summary>
		/// 메인 초기화 함수 (본인 Main Scene 진입 시 호출)
		/// </summary>
		public override void InitializeMain()
		{
			base.InitializeMain();


		}
		/// <summary>
		/// 메인 마무리화 함수 (본인 Main Scene 나갈 시 호출)
		/// </summary>
		public override void FinallizeMain()
		{
			base.FinallizeMain();


		}
		#endregion

		#region 유니티 콜백 함수
		#endregion
		#endregion

		private void LoadAllProjectileData()
		{
			string path = ProjectileSOManager.resourcesPath;
			ProjectileData[] projectileDatas = Resources.LoadAll<ProjectileData>(path);

			for (int i = 0; i < projectileDatas.Length; ++i)
			{
				ProjectileData projectileData = projectileDatas[i];

				if (m_ProjectileDataMap.ContainsKey1(projectileData.code) == true)
					continue;

				// 투사체 원본 로드
				Projectile origin = Resources.Load<Projectile>(projectileData.assetPath);

				// 딕셔너리에 추가
				m_ProjectileDataMap.Add(projectileData.code, projectileData.title, projectileData);
			}
		}

		public new IProjectileBuilder GetBuilder(string key)
		{
			return base.GetBuilder(key) as IProjectileBuilder;
		}

		public float GetMovementSpeed(string key)
		{
			if (m_ProjectileDataMap.TryGetValue(key, out ProjectileData projectileData) == false)
				return 0f;

			return projectileData.movementSpeed;
		}
		public float GetLifeTime(string key)
		{
			if (m_ProjectileDataMap.TryGetValue(key, out ProjectileData projectileData) == false)
				return 0f;

			return projectileData.lifeTime;
		}
	}
}