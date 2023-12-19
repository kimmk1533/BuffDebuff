using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : ObjectPoolItemBase
{
	#region 변수
	protected EnemyCharacter m_Character;

	[SerializeField, ChildComponent("Renderer")]
	protected EnemyAnimator m_Animator;

	[SerializeField]
	private List<Transform> m_AttackSpotList;
	#endregion

	#region 프로퍼티
	#endregion

	#region 이벤트
	#endregion

	#region 매니저
	private static PlayerManager M_Player => PlayerManager.Instance;
	private static EnemyManager M_Enemy => EnemyManager.Instance;
	private static ProjectileManager M_Projectile => ProjectileManager.Instance;
	#endregion

	// 초기화
	public override void Initialize()
	{
		base.Initialize();

		this.Safe_GetComponent<EnemyCharacter>(ref m_Character);
		m_Character.Initialize();

		m_Animator.Initialize();
	}

	protected void Attack()
	{
		m_Character.AnimEvent_Attacking();

		CreateProjectile(0);
	}
	protected void CreateProjectile(int attackIndex)
	{
		if (attackIndex < 0 || attackIndex >= m_AttackSpotList.Count)
			return;

		Vector3 position = m_AttackSpotList[attackIndex].position;

		Vector3 targetPos = m_Character.targetCollider.bounds.center;
		float angle = position.GetAngle(targetPos);
		Quaternion quaternion = Quaternion.AngleAxis(angle - 90, Vector3.forward);

		Projectile projectile = M_Projectile.GetBuilder("Projectile")
			.SetActive(true)
			.SetAutoInit(false)
			.SetParent(null)
			.SetPosition(position)
			.SetRotation(quaternion)
			.Spawn();

		projectile.Initialize(5.0f, m_Character.currentStat.AttackRange);

		projectile.SetMovingStrategy(new Projectile.StraightMove());
		projectile["Player"].onEnter2D += (Collider2D collider) =>
		{
			Player player = collider.GetComponent<Player>();

			GiveDamage(new DamageArg<Enemy, Player>()
			{
				damageGiver = this,
				damageTaker = player,
				projectile = projectile,
			});

			M_Projectile.Despawn(projectile);
		};
		projectile["Obstacle"].onEnter2D += (Collider2D collider) =>
		{
			M_Projectile.Despawn(projectile);
		};
	}

	protected virtual void GiveDamage(DamageArg<Enemy, Player> damageArg)
	{
		Player player = damageArg.damageTaker;
		Projectile projectile = damageArg.projectile;

		if (projectile == null || player == null)
			return;

		if (projectile.gameObject.activeSelf == false ||
			player.gameObject.activeSelf == false)
			return;

		player.TakeDamage(m_Character.currentStat.Attack);
	}
	public void TakeDamage(float damage)
	{
		m_Character.currentStat.Hp -= damage;

		if (m_Character.currentStat.Hp <= 0)
			Death();
	}
	protected void Death()
	{
		float xp = m_Character.currentStat.Xp * m_Character.currentStat.XpScale;
		M_Player.AddXp(xp);

		M_Enemy.Despawn(this);
	}
}

public struct DamageArg<DamageGiver, DamageTaker>
{
	public DamageGiver damageGiver;
	public DamageTaker damageTaker;
	public Projectile projectile;
}