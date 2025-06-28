using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BuffDebuff
{
	public interface IProjectileBuilder : ObjectPool<Projectile>.IItemBuilder
	{
		public new IProjectileBuilder SetName(string name);
		public new IProjectileBuilder SetActive(bool active);
		public new IProjectileBuilder SetAutoInit(bool autoInit);
		public new IProjectileBuilder SetParent(Transform parent);
		public new IProjectileBuilder SetPosition(Vector3 position);
		public new IProjectileBuilder SetLocalPosition(Vector3 localPosition);
		public new IProjectileBuilder SetRotation(Quaternion rotation);
		public new IProjectileBuilder SetLocalRotation(Quaternion localRotation);
		public new IProjectileBuilder SetScale(Vector3 scale);
		public IProjectileBuilder SetMoveSpeed(float moveSpeed);
		public IProjectileBuilder SetLifeTime(float lifeTime);
		public IProjectileBuilder SetMoveType(ProjectileMove projectileMove);

		public new Projectile Spawn(bool autoReset = true);
		public new T Spawn<T>(bool autoReset = true) where T : Projectile;
	}
	public class ProjectileBuilder : ObjectPool<Projectile>.ItemBuilder, IProjectileBuilder
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

		public new IProjectileBuilder SetName(string name) => base.SetName(name) as IProjectileBuilder;
		public new IProjectileBuilder SetActive(bool active) => base.SetActive(active) as IProjectileBuilder;
		public new IProjectileBuilder SetAutoInit(bool autoInit) => base.SetAutoInit(autoInit) as IProjectileBuilder;
		public new IProjectileBuilder SetParent(Transform parent) => base.SetParent(parent) as IProjectileBuilder;
		public new IProjectileBuilder SetPosition(Vector3 position) => base.SetPosition(position) as IProjectileBuilder;
		public new IProjectileBuilder SetLocalPosition(Vector3 localPosition) => base.SetLocalPosition(localPosition) as IProjectileBuilder;
		public new IProjectileBuilder SetRotation(Quaternion rotation) => base.SetRotation(rotation) as IProjectileBuilder;
		public new IProjectileBuilder SetLocalRotation(Quaternion localRotation) => base.SetLocalRotation(localRotation) as IProjectileBuilder;
		public new IProjectileBuilder SetScale(Vector3 scale) => base.SetScale(scale) as IProjectileBuilder;

		public IProjectileBuilder SetMoveSpeed(float moveSpeed)
		{
			m_MoveSpeed.isUse = true;
			m_MoveSpeed.value = moveSpeed;

			return this;
		}
		public IProjectileBuilder SetLifeTime(float lifeTime)
		{
			m_LifeTime.isUse = true;
			m_LifeTime.value = lifeTime;

			return this;
		}
		public IProjectileBuilder SetMoveType(ProjectileMove projectileMove)
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

			m_MoveSpeed.Reset();
			m_LifeTime.Reset();
			m_MoveType.Reset();
		}
	}
}