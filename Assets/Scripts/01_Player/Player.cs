using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public sealed class Player : MonoBehaviour
{
	#region 변수
	private PlayerCharacter m_Character;

	[SerializeField]
	private List<Transform> m_AttackSpotList;
	#endregion

	#region 프로퍼티
	public PlayerCharacter character => m_Character;
	public int maxLevel
	{
		get
		{
			if (Application.isPlaying == false)
				return GetComponent<PlayerCharacter>().maxStat.Level;

			return m_Character.maxStat.Level;
		}
	}
	public int currentLevel
	{
		get
		{
			if (Application.isPlaying == false)
				return GetComponent<PlayerCharacter>().currentStat.Level;

			return m_Character.currentStat.Level;
		}
	}
	#endregion

	#region 매니저
	private static ProjectileManager M_Projectile => ProjectileManager.Instance;
	#endregion

	private void Awake()
	{
		Initialize();
	}

	public void Initialize()
	{
		this.Safe_GetComponent<PlayerCharacter>(ref m_Character);

		m_Character.Initialize();
	}

	private void Update()
	{
		// 테스트
		if (Input.GetKeyDown(KeyCode.F))
		{
			m_Character.AddBuff("전방향 대쉬");
		}
		//

		if (Input.GetMouseButtonDown(0))
		{
			m_Character.Attack();
		}
		if (Input.GetMouseButtonDown(1))
		{
			m_Character.Dash();
		}

		if (Input.GetKeyDown(KeyCode.Space))
		{
			m_Character.JumpInputDown();
		}
		if (Input.GetKeyUp(KeyCode.Space))
		{
			m_Character.JumpInputUp();
		}
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

		Projectile projectile = M_Projectile.GetBuilder("Projectile")
			.SetActive(true)
			.SetAutoInit(false)
			.SetParent(null)
			.SetPosition(position)
			.SetRotation(quaternion)
			.Spawn();

		projectile.Initialize(5.0f, m_Character.currentStat.AttackRange);

		projectile.SetMovingStrategy(new Projectile.StraightMove());
		projectile["Enemy"].onEnter2D += (Collider2D collider) =>
		{
			Enemy enemy = collider.GetComponent<Enemy>();

			GiveDamage(new DamageArg<Player, Enemy>()
			{
				damageGiver = this,
				damageTaker = enemy,
				projectile = projectile,
			});

			M_Projectile.Despawn(projectile);
		};
		projectile["Obstacle"].onEnter2D += (Collider2D collider) =>
		{
			M_Projectile.Despawn(projectile);
		};
	}

	private void GiveDamage(DamageArg<Player, Enemy> arg)
	{
		Enemy enemy = arg.damageTaker;
		Projectile projectile = arg.projectile;

		if (projectile == null || enemy == null)
			return;

		if (projectile.gameObject.activeSelf == false ||
			enemy.gameObject.activeSelf == false)
			return;

		enemy.TakeDamage(m_Character.currentStat.Attack);
	}
	public void TakeDamage(float damage)
	{
		m_Character.currentStat.Hp -= damage;

		if (m_Character.currentStat.Hp <= 0f)
			Death();
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

			InfiniteLoopDetector.Run();
		}

		m_Character.currentStat.Xp = Xp;
	}
	private void LevelUp()
	{

	}
}