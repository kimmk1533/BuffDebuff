using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public sealed class Player : MonoBehaviour
{
	private PlayerCharacter m_Character;

	[SerializeField]
	private List<Transform> m_AttackSpotList;

	public int maxLevel => m_Character.maxStat.Level;
	public int currentLevel => m_Character.currentStat.Level;

	private ProjectileManager M_Projectile => ProjectileManager.Instance;

	private void Update()
	{
		// 테스트
		if (Input.GetKeyDown(KeyCode.F))
		{
			m_Character.AddBuff("체력 증가");
		}
		//

		if (Input.GetMouseButtonDown(0))
		{
			m_Character.Attack();
		}
	}

	public void Initialize()
	{
		m_Character = GetComponent<PlayerCharacter>();
		m_Character.Initialize();
	}

	public void Attack(int attackIndex)
	{
		m_Character.AnimEvent_Attacking();

		CreateProjectile(attackIndex);
	}
	private void CreateProjectile(int attackIndex)
	{
		if (attackIndex < 0 || attackIndex >= m_AttackSpotList.Count)
			return;

		Vector3 position = m_AttackSpotList[attackIndex].position;
		Vector2 mousePos = UtilClass.GetMouseWorldPosition();
		float angle = position.GetAngle(mousePos);
		Quaternion quaternion = Quaternion.AngleAxis(angle - 90, Vector3.forward);

		Projectile projectile = M_Projectile.Spawn("Projectile", position, quaternion);

		projectile.Initialize(5.0f, m_Character.currentStat.AttackRange);

		projectile.SetMovingStrategy(new Projectile.StraightMove());
		projectile["Enemy"].OnEnter2D += (Collider2D collider) =>
		{
			Enemy enemy = collider.GetComponent<Enemy>();

			GiveDamage(projectile, enemy);

			M_Projectile.Despawn("Projectile", projectile);
		};
		projectile["Obstacle"].OnEnter2D += (Collider2D collider) =>
		{
			M_Projectile.Despawn("Projectile", projectile);
		};

		projectile.gameObject.SetActive(true);
	}

	public void TakeDamage(float damage)
	{
		m_Character.currentStat.Hp -= damage;

		if (m_Character.currentStat.Hp <= 0f)
			Death();
	}
	private void GiveDamage(Projectile projectile, Enemy enemy)
	{
		if (projectile == null || enemy == null)
			return;

		if (projectile.gameObject.activeSelf == false ||
			enemy.gameObject.activeSelf == false)
			return;

		enemy.TakeDamage(m_Character.currentStat.Attack);
	}
	private void Death()
	{
		Debug.LogError("Game Over!");
	}

	public bool AddBuff(BuffData buffData)
	{
		return m_Character.AddBuff(buffData);
	}
	public bool RemoveBuff(BuffData buffData)
	{
		return m_Character.RemoveBuff(buffData);
	}

	public void AddXp(float xp)
	{
		float Xp = m_Character.currentStat.Xp + xp * m_Character.currentStat.XpScale;

		while (Xp >= m_Character.maxStat.Xp)
		{
			Xp -= m_Character.maxStat.Xp;
			++m_Character.currentStat.Level;
		}

		m_Character.currentStat.Xp = Xp;
	}
}