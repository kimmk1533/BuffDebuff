using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataDictionary = DoubleKeyDictionary<int, string, (BuffDebuff.Projectile projectile, BuffDebuff.ProjectileData data)>;

namespace BuffDebuff
{
	public class ProjectileManager : ObjectManager<ProjectileManager, Projectile>
	{
		#region 변수
		private DataDictionary m_ProjectileDataMap = null;
		#endregion

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
		public override void Finallize()
		{
			base.Finallize();
		}

		public override void InitializeMain()
		{
			base.InitializeMain();

		}
		public override void FinallizeMain()
		{
			base.FinallizeMain();
		}

		private void LoadAllProjectileData()
		{
			string path = ProjectileSOManager.resourcesPath;
			ProjectileData[] ProjectileDatas = Resources.LoadAll<ProjectileData>(path);

			for (int i = 0; i < ProjectileDatas.Length; ++i)
			{
				ProjectileData ProjectileData = ProjectileDatas[i];

				if (m_ProjectileDataMap.ContainsKey1(ProjectileData.code) == true)
					continue;

				// 투사체 원본 로드
				Projectile origin = Resources.Load<Projectile>(ProjectileData.assetPath);

				// 딕셔너리에 추가
				m_ProjectileDataMap.Add(ProjectileData.code, ProjectileData.title, (origin, ProjectileData));
			}
		}

		public new IProjectileBuilder GetBuilder(string key)
		{
			return base.GetBuilder(key) as IProjectileBuilder;
		}
	}
}