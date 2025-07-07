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
		public IProjectileBuilder SetMovementSpeed(float movementSpeed);
		public IProjectileBuilder SetLifeTime(float lifeTime);
		public IProjectileBuilder SetMover(ProjectileMover projectileMover);

		public new Projectile Spawn(bool autoReset = true);
		public new T Spawn<T>(bool autoReset = true) where T : Projectile;
	}
	public class ProjectileBuilder : ObjectPool<Projectile>.ItemBuilder, IProjectileBuilder
	{
		private ItemProperty<float> m_MovementSpeed;
		private ItemProperty<float> m_LifeTime;
		private ItemProperty<ProjectileMover> m_Mover;

		public ProjectileBuilder(ObjectPool<Projectile> pool) : base(pool)
		{
			m_MovementSpeed = new ItemProperty<float>();
			m_LifeTime = new ItemProperty<float>();
			m_Mover = new ItemProperty<ProjectileMover>();
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

		public IProjectileBuilder SetMovementSpeed(float movementSpeed)
		{
			m_MovementSpeed.isUse = true;
			m_MovementSpeed.value = movementSpeed;

			return this;
		}
		public IProjectileBuilder SetLifeTime(float lifeTime)
		{
			m_LifeTime.isUse = true;
			m_LifeTime.value = lifeTime;

			return this;
		}
		public IProjectileBuilder SetMover(ProjectileMover projectileMover)
		{
			m_Mover.isUse = true;
			m_Mover.value = projectileMover;

			return this;
		}

		public override Projectile Spawn(bool autoReset = true)
		{
			Projectile projectile = base.Spawn(false);

			if (m_MovementSpeed.isUse == true)
				projectile.movementSpeed = m_MovementSpeed.value;
			if (m_LifeTime.isUse == true)
				projectile.lifeTime = m_LifeTime.value;
			if (m_Mover.isUse == true)
				projectile.mover = m_Mover.value;

			if (autoReset)
				Reset();

			return projectile;
		}

		public override void Reset()
		{
			base.Reset();

			m_MovementSpeed.Reset();
			m_LifeTime.Reset();
			m_Mover.Reset();
		}
	}
}