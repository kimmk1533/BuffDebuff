using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D), typeof(PlayerController2D))]
public sealed class Player : MonoBehaviour
{
	private PlayerController2D m_Controller;

	[SerializeField, ReadOnly]
	private Vector2 m_Velocity;
	private float m_VelocityXSmoothing;
	private float m_AccelerationTimeAirborne = 0.2f;
	private float m_AccelerationTimeGrounded = 0.1f;
	[SerializeField]
	private bool m_IsSimulating = true;

	private Vector2 m_DirectionalInput;

	private PlayerCharacter m_Character;

	[SerializeField, ChildComponent("Renderer")]
	private PlayerAnimator m_Animator;

	[SerializeField]
	private List<Transform> m_AttackSpotList;

	public int maxLevel => m_Character.maxStat.Level;
	public int currentLevel => m_Character.currentStat.Level;

	private ProjectileManager M_Projectile => ProjectileManager.Instance;
	private StageManager M_Stage => StageManager.Instance;

	private void Update()
	{
		// 테스트
		if (Input.GetKeyDown(KeyCode.F))
		{
			m_Character.AddBuff("체력 증가");
		}
		//

		Debug.Log(m_Character.CanAttack());
		if (Input.GetMouseButtonDown(0) && m_Character.CanAttack() == true)
		{
			m_Animator.Anim_Attack();
		}

		if (m_IsSimulating == false)
			return;

		Move();

		if (Input.GetKeyDown(KeyCode.Space))
		{
			JumpInputDown();
		}
		if (Input.GetKeyUp(KeyCode.Space))
		{
			JumpInputUp();
		}

		if (Input.GetMouseButtonDown(1))
		{
			Dash();
		}
	}

	public void Initialize()
	{
		m_Controller = GetComponent<PlayerController2D>();
		m_Controller.Initialize();

		m_Character = GetComponent<PlayerCharacter>();
		m_Character.Initialize();

		m_Animator.Initialize();
	}

	private void SetDirectionalInput(Vector2 input)
	{
		m_DirectionalInput = input;
		m_Animator.Anim_SetDirectionalInput(input);
	}
	private void CalculateVelocity()
	{
		float targetVelocityX = m_DirectionalInput.x * m_Character.currentStat.MoveSpeed;

		m_Velocity.x = Mathf.SmoothDamp(m_Velocity.x, targetVelocityX, ref m_VelocityXSmoothing, (m_Controller.collisions.grounded) ? m_AccelerationTimeGrounded : m_AccelerationTimeAirborne);

		m_Velocity.y += m_Controller.gravity * Time.deltaTime;
	}
	private void Move()
	{
		Vector2 directionalInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
		SetDirectionalInput(directionalInput);

		CalculateVelocity();

		m_Controller.Move(m_Velocity * Time.deltaTime, m_DirectionalInput);

		RaycastHit2D hit = Physics2D.BoxCast(m_Controller.collider.bounds.center, m_Controller.collider.bounds.size, 0.0f, m_Velocity, 0.1f, LayerMask.GetMask("Portal"));

		if (hit)
		{
			Transform spawnPoint = M_Stage.GetSpawnPoint(hit.collider);

			if (spawnPoint != null)
				transform.position = spawnPoint.position;
		}

		if (m_Controller.collisions.above || m_Controller.collisions.below)
		{
			m_Velocity.y = 0;
		}

		m_Animator.Anim_SetVelocity(m_Velocity);
		m_Animator.Anim_SetIsGround(m_Controller.collisions.grounded);
	}

	public void AttackStart()
	{
		m_Character.AttackStart();

		m_IsSimulating = false;
	}
	public void Attack(int attackIndex)
	{
		m_Character.Attack();

		CreateProjectile(attackIndex);
	}
	public void AttackEnd()
	{
		m_Character.AttackEnd();

		m_IsSimulating = true;
	}
	public void AirAttackStart()
	{
		m_IsSimulating = false;

		m_Velocity = Vector2.zero;
	}
	public void AirAttackEnd()
	{
		m_IsSimulating = true;
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

	private void GiveDamage(Projectile projectile, Enemy enemy)
	{
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

	private void JumpInputDown()
	{
		if ((m_Controller.collisions.below && m_DirectionalInput.y != -1) == false)
			return;

		m_Animator.Anim_Jump();
		m_Character.Jump();

		m_Velocity.y = m_Controller.maxJumpVelocity;
	}
	private void JumpInputUp()
	{
		if (m_Velocity.y > m_Controller.minJumpVelocity)
			m_Velocity.y = m_Controller.minJumpVelocity;
	}
	private void Dash()
	{
		if (m_Character.CanDash() == false)
			return;

		m_Character.Dash();

		Vector2 dir = UtilClass.GetMouseWorldPosition() - transform.position;

		// 좌우 대쉬
		m_Velocity.x = Mathf.Sign(dir.x) * m_Character.currentStat.DashSpeed;

		// 마우스 대쉬
		//m_Velocity = dir.normalized * m_Character.currentStat.DashSpeed;
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