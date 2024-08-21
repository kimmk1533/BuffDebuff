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

			if (m_ProjectileDataMap == null)
				m_ProjectileDataMap = new DataDictionary();

			LoadAllProjectileData();
		}
		public override void Finallize()
		{
			base.Finallize();
		}

		public override void InitializeGame()
		{
			for (int i = 0; i < m_Origins.Count; ++i)
			{
				OriginInfo originInfo = m_Origins[i];

				if (originInfo.useFlag == false)
					continue;

				ObjectPool<Projectile> projectilePool = GetPool(originInfo.key);
				ProjectileBuilder projectileBuilder = new ProjectileBuilder(projectilePool);
				projectilePool.Initialize(projectileBuilder);
			}
		}
		public override void FinallizeGame()
		{
			base.FinallizeGame();
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

		public new ProjectileBuilder GetBuilder(string key)
		{
			return (ProjectileBuilder)base.GetBuilder(key);
		}

#if UNITY_EDITOR
		[ContextMenu("Load Origin")]
		protected override void LoadOrigin()
		{
			base.LoadOrigin_Inner();
		}
#endif

		public class ProjectileBuilder : ObjectPool<Projectile>.ItemBuilder
		{
			private ItemProperty<float> m_MoveSpeed;
			private ItemProperty<float> m_LifeTime;
			private ItemProperty<ProjectileMove> m_MoveType;

			public ProjectileBuilder(ObjectPool<Projectile> pool) : base(pool)
			{
				m_MoveSpeed = new ItemProperty<float>();
				m_LifeTime = new ItemProperty<float>();
				m_MoveType = new ItemProperty<ProjectileMove>();
			}

			public new ProjectileBuilder SetName(string name)
			{
				return (ProjectileBuilder)base.SetName(name);
			}
			public new ProjectileBuilder SetActive(bool active)
			{
				return (ProjectileBuilder)base.SetActive(active);
			}
			public new ProjectileBuilder SetAutoInit(bool autoInit)
			{
				return (ProjectileBuilder)base.SetAutoInit(autoInit);
			}
			public new ProjectileBuilder SetParent(Transform parent)
			{
				return (ProjectileBuilder)base.SetParent(parent);
			}
			public new ProjectileBuilder SetPosition(Vector3 position)
			{
				return (ProjectileBuilder)base.SetPosition(position);
			}
			public new ProjectileBuilder SetLocalPosition(Vector3 localPosition)
			{
				return (ProjectileBuilder)base.SetLocalPosition(localPosition);
			}
			public new ProjectileBuilder SetRotation(Quaternion rotation)
			{
				return (ProjectileBuilder)base.SetRotation(rotation);
			}
			public new ProjectileBuilder SetLocalRotation(Quaternion localRotation)
			{
				return (ProjectileBuilder)base.SetLocalRotation(localRotation);
			}
			public new ProjectileBuilder SetScale(Vector3 scale)
			{
				return (ProjectileBuilder)base.SetScale(scale);
			}
			public ProjectileBuilder SetMoveSpeed(float moveSpeed)
			{
				m_MoveSpeed.isUse = true;
				m_MoveSpeed.value = moveSpeed;

				return this;
			}
			public ProjectileBuilder SetLifeTime(float lifeTime)
			{
				m_LifeTime.isUse = true;
				m_LifeTime.value = lifeTime;

				return this;
			}
			public ProjectileBuilder SetMoveType(ProjectileMove projectileMove)
			{
				m_MoveType.isUse = true;
				m_MoveType.value = projectileMove;

				return this;
			}

			public override Projectile Spawn(bool autoReset = true)
			{
				Projectile projectile = base.Spawn(false);

				if (m_MoveSpeed.isUse == true)
					projectile.SetMoveSpeed(m_MoveSpeed.value);
				if (m_LifeTime.isUse == true)
					projectile.SetLifeTime(m_LifeTime.value);
				if (m_MoveType.isUse == true)
					projectile.SetMovingStrategy(m_MoveType.value);

				if (autoReset)
					Reset();

				return projectile;
			}

			public override void Reset()
			{
				base.Reset();

				m_MoveType.isUse = false;
				m_MoveType.value = null;
			}
		}
	}
}