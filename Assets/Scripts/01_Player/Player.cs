using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuffDebuff
{
	[RequireComponent(typeof(BoxCollider2D))]
	public sealed class Player : MonoBehaviour
	{
		#region 변수
		private PlayerCharacter m_Character = null;

		[SerializeField]
		private List<Transform> m_AttackSpotList = null;
		#endregion

		#region 프로퍼티
		public PlayerCharacter character => m_Character;
		public int maxLevel
		{
			get
			{
#if UNITY_EDITOR

				if (Application.isPlaying == false)
					return GetComponent<PlayerCharacter>().maxStat.Level;

#endif

				return m_Character.maxStat.Level;
			}
		}
		public int currentLevel
		{
			get
			{
#if UNITY_EDITOR

				if (Application.isPlaying == false)
					return GetComponent<PlayerCharacter>().currentStat.Level;

#endif

				return m_Character.currentStat.Level;
			}
		}
		#endregion

		#region 매니저
		private static InputManager M_Input => InputManager.Instance;
		private static ProjectileManager M_Projectile => ProjectileManager.Instance;
		#endregion

		public void Initialize()
		{
			this.Safe_GetComponent<PlayerCharacter>(ref m_Character);

			m_Character.Initialize();
		}
		public void Finallize()
		{
			m_Character.Finallize();
		}

		private void Update()
		{
			// 테스트
			if (Input.GetKeyDown(KeyCode.F))
			{
				BuffInventory.Instance.AddBuff("전방향 대쉬");
			}
			if (Input.GetKeyDown(KeyCode.E))
			{
				BuffInventory.Instance.RemoveBuff("전방향 대쉬");
			}

			Vector2 directionalInput = new Vector2(M_Input.GetAxisRaw("PlayerMoveHorizontal"), M_Input.GetAxisRaw("PlayerMoveVertical"));
			m_Character.SetDirectionalInput(directionalInput);

			if (M_Input.GetKeyDown(E_InputType.PlayerAttack))
			{
				m_Character.Attack();
			}
			if (M_Input.GetKeyDown(E_InputType.PlayerDash))
			{
				m_Character.Dash();
			}

			if (M_Input.GetKeyDown(E_InputType.PlayerJump))
			{
				m_Character.JumpInputDown();
			}
			if (M_Input.GetKeyUp(E_InputType.PlayerJump))
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

			projectile.SetMovingStrategy(new StraightMove());
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

		public void AddXp(float xp)
		{
			float Xp = m_Character.currentStat.Xp + xp * m_Character.currentStat.XpScale;

			while (Xp >= m_Character.maxStat.Xp)
			{
				InfiniteLoopDetector.Run();

				Xp -= m_Character.maxStat.Xp;
				++m_Character.currentStat.Level;
			}

			m_Character.currentStat.Xp = Xp;
		}
		private void LevelUp()
		{

		}
	}
}