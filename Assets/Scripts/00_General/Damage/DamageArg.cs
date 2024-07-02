using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuffDebuff
{
	public interface IDamageGiver
	{
		public void GiveDamage(DamageArg<IDamageGiver, IDamageTaker> arg);
	}
	public interface IDamageTaker
	{
		public void TakeDamage(float damage);
	}
	public struct DamageArg<DamageGiver, DamageTaker> where DamageGiver : IDamageGiver where DamageTaker : IDamageTaker
	{
		private float m_Damage;
		private DamageGiver m_DamageGiver;
		private DamageTaker m_DamageTaker;
		private Projectile m_Projectile;

		public float damage => m_Damage;
		public DamageGiver damageGiver => m_DamageGiver;
		public DamageTaker damageTaker => m_DamageTaker;
		public Projectile projectile => m_Projectile;

		public DamageArg(float damage, DamageGiver giver, DamageTaker taker, Projectile projectile)
		{
			m_Damage = damage;
			m_DamageGiver = giver;
			m_DamageTaker = taker;
			m_Projectile = projectile;
		}

		public void GiveDamage()
		{

		}
		public void TakeDamage()
		{

		}
	}
}