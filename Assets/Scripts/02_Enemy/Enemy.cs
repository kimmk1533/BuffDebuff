using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
	protected EnemyCharacter m_Character;

	[SerializeField, ChildComponent("Renderer")]
	protected EnemyAnimator m_Animator;

	[SerializeField]
	private List<Transform> m_AttackSpotList;

	private PlayerManager M_Player => PlayerManager.Instance;
	private EnemyManager M_Enemy => EnemyManager.Instance;
	private ProjectileManager M_Projectile => ProjectileManager.Instance;

	// 초기화
	public virtual void Initialize()
	{
		m_Character = GetComponent<EnemyCharacter>();
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

		Projectile projectile = M_Projectile.Spawn("Projectile", position, quaternion);

		projectile.Initialize(5.0f, m_Character.currentStat.AttackRange);

		projectile.SetMovingStrategy(new Projectile.StraightMove());
		projectile["Player"].OnEnter2D += (Collider2D collider) =>
		{
			Player player = collider.GetComponent<Player>();

			GiveDamage(projectile, player);

			M_Projectile.Despawn("Projectile", projectile);
		};
		projectile["Obstacle"].OnEnter2D += (Collider2D collider) =>
		{
			M_Projectile.Despawn("Projectile", projectile);
		};

		projectile.gameObject.SetActive(true);
	}

	protected virtual void GiveDamage(Projectile projectile, Player player)
	{
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