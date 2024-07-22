using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuffDebuff
{
	[RequireComponent(typeof(PlayerController2D), typeof(BoxCollider2D))]
	public sealed class Player : Character<PlayerStat, PlayerController2D, PlayerAnimator>, IDamageGiver, IDamageTaker
	{
		#region 변수
		[Header("===== 플레이어 전용 변수 ====="), Space(10)]
		private Vector2 m_DirectionalInput;

		[SerializeField, ReadOnly]
		private bool m_IsAttacking;
		[SerializeField, ReadOnly]
		private bool m_CanComboAttack;
		[SerializeField]
		private Transform m_AttackSpot;

		[SerializeField, ReadOnly]
		private UtilClass.Timer m_DashTimer;
		#endregion

		#region 프로퍼티
		public float attackSpeed
		{
			get
			{
				return m_Stat.AttackSpeed;
			}
			set
			{
				m_Stat.AttackSpeed = value;
				m_Animator.Anim_SetAttackSpeed(value);
			}
		}
		public int maxLevel
		{
			get
			{
				return m_Stat.Level.max;
			}
		}
		public int currentLevel
		{
			get
			{
				return m_Stat.Level.current;
			}
		}
		#endregion

		#region 매니저
		private static ProjectileManager M_Projectile => ProjectileManager.Instance;
		private static BuffInventory M_BuffInventory => BuffInventory.Instance;
		private static InputManager M_Input => InputManager.Instance;
		#endregion

		public override void Initialize()
		{
			base.Initialize();

			m_IsAttacking = false;
			m_CanComboAttack = false;

			// 타이머 초기화
			if (m_DashTimer == null)
				m_DashTimer = new UtilClass.Timer();
			m_DashTimer.interval = m_Stat.DashRechargeTime;
		}
		public override void Finallize()
		{
			base.Finallize();

			if (m_DashTimer != null)
				m_DashTimer.Clear();
		}

		protected override void Update()
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
			SetDirectionalInput(directionalInput);

			if (M_Input.GetKeyDown(E_InputType.PlayerAttack))
			{
				Attack();
			}
			if (M_Input.GetKeyDown(E_InputType.PlayerDash))
			{
				Dash();
			}

			if (M_Input.GetKeyDown(E_InputType.PlayerJump))
			{
				JumpInputDown();
			}
			if (M_Input.GetKeyUp(E_InputType.PlayerJump))
			{
				JumpInputUp();
			}

			base.Update();

			DashTimer();
		}

		public void AddXp(float xp)
		{
			float Xp = m_Stat.Xp.current + xp * m_Stat.XpScale;

			while (Xp >= m_Stat.Xp.max)
			{
				InfiniteLoopDetector.Run("AddXp", "Player.cs");

				Xp -= m_Stat.Xp.max;

				LevelUp();
			}

			// 추후 최대 경험치도 현재 레벨에 맞게 변환해야함.
			m_Stat.Xp = new StatValue<float>(Xp, m_Stat.Xp.max);
		}
		private void LevelUp()
		{
			StatValue<int> level = m_Stat.Level;
			++level.current;
			m_Stat.Level = level;
		}

		public void SetDirectionalInput(Vector2 input)
		{
			m_DirectionalInput = input;
			m_Animator.Anim_SetDirectionalInput(input);
		}
		protected override void CalculateVelocity()
		{
			float targetVelocityX = m_DirectionalInput.x * m_Stat.MoveSpeed;

			m_Velocity.x = Mathf.SmoothDamp(m_Velocity.x, targetVelocityX, ref m_VelocityXSmoothing, (m_Controller.collisions.grounded) ? m_AccelerationTimeGrounded : m_AccelerationTimeAirborne);
			m_Velocity.y += m_Controller.gravity * Time.deltaTime;

			Vector3 scale = transform.localScale;
			if (m_Velocity.x > 0)
				scale.x = Mathf.Abs(scale.x);
			else if (m_Velocity.x < 0)
				scale.x = -Mathf.Abs(scale.x);
			transform.localScale = scale;
		}
		protected override void Move()
		{
			CalculateVelocity();

			m_Controller.Move(m_Velocity * Time.deltaTime);
			if (m_Controller.collisions.above || m_Controller.collisions.below)
			{
				m_Velocity.y = 0;
			}

			m_Animator.Anim_SetVelocity(m_Velocity);
			m_Animator.Anim_SetIsGround(m_Controller.collisions.grounded);
		}

		// Jump Func
		public void JumpInputDown()
		{
			if (m_IsSimulating == false)
				return;

			if ((m_Controller.collisions.below && m_DirectionalInput.y != -1f) == false)
				return;

			m_Animator.Anim_Jump();
			OnBuffJump();

			m_Velocity.y = m_Controller.maxJumpVelocity;
		}
		public void JumpInputUp()
		{
			if (m_IsSimulating == false)
				return;

			if (m_Velocity.y > m_Controller.minJumpVelocity)
				m_Velocity.y = m_Controller.minJumpVelocity;
		}

		// Dash Func
		public bool CanDash()
		{
			return m_Stat.DashCount.current > 0;
		}
		public void Dash()
		{
			if (CanDash() == false)
				return;

			StatValue<int> newDashCount = m_Stat.DashCount;
			--newDashCount.current;
			m_Stat.DashCount = newDashCount;

			OnBuffDash();

			Vector2 dir = UtilClass.GetMouseWorldPosition() - transform.position;

			if (M_BuffInventory.HasBuff("전방향 대쉬") == true)
			{
				// 마우스 대쉬
				m_Velocity = dir.normalized * m_Stat.DashSpeed;
			}
			else
			{
				// 좌우 대쉬
				m_Velocity.x = Mathf.Sign(dir.x) * m_Stat.DashSpeed;
			}
		}

		// Attack Func
		protected override bool CanAttack()
		{
			return base.CanAttack() && (m_IsAttacking ? m_CanComboAttack : true);
		}
		public override bool Attack()
		{
			if (CanAttack() == false)
				return false;

			m_IsAttacking = true;
			m_Animator.Anim_Attack(m_AttackIndex++);

			return true;
		}
		private void CreateProjectile()
		{
			Vector3 position = m_AttackSpot.position;

			Vector2 mousePos = UtilClass.GetMouseWorldPosition();
			float angle = position.GetAngle(mousePos);
			Quaternion quaternion = Quaternion.AngleAxis(angle - 90, Vector3.forward);

			Projectile projectile = M_Projectile.GetBuilder("Projectile")
				.SetActive(true)
				.SetAutoInit(true)
				.SetParent(null)
				.SetPosition(position)
				.SetRotation(quaternion)
				.SetMoveSpeed(m_Stat.ShotSpeed)
				.SetLifeTime(m_Stat.AttackRange)
				.SetMoveType(new StraightMove())
				.Spawn();

			//projectile.Initialize(5.0f, m_CurrentStat.AttackRange);

			projectile.SetMovingStrategy(new StraightMove());
			projectile["Enemy"].onEnter2D += (Collider2D collider) =>
			{
				Enemy enemy = collider.GetComponent<Enemy>();

				DamageArg<IDamageGiver, IDamageTaker> damageArg = new DamageArg<IDamageGiver, IDamageTaker>(
					m_Stat.AttackPower,
					this,
					enemy,
					projectile);

				GiveDamage(damageArg);

				M_Projectile.Despawn(projectile);
			};
			projectile["Obstacle"].onEnter2D += (Collider2D collider) =>
			{
				M_Projectile.Despawn(projectile);
			};
		}
		public void GiveDamage(DamageArg<IDamageGiver, IDamageTaker> arg)
		{
			Enemy enemy = arg.damageTaker as Enemy;
			Projectile projectile = arg.projectile;

			if (projectile == null || enemy == null)
				return;

			if (projectile.gameObject.activeSelf == false ||
				enemy.gameObject.activeSelf == false)
				return;

			enemy.TakeDamage(arg.damage);
		}
		public void TakeDamage(float damage)
		{
			StatValue<float> newHp = m_Stat.Hp;
			newHp.current -= damage;

			if (newHp.current <= 0f)
				Death();

			m_Stat.Hp = newHp;
		}
		private void Death()
		{
			Debug.LogError("Game Over!");
		}


		// Timer Func
		private void DashTimer()
		{
			if (m_Stat.DashCount.current >= m_Stat.DashCount.max)
			{
				m_DashTimer.Clear();
				return;
			}

			m_DashTimer.Update();

			if (m_DashTimer.TimeCheck(true))
			{
				StatValue<int> newDashCount = m_Stat.DashCount;
				++newDashCount.current;
				m_Stat.DashCount = newDashCount;
			}
		}

		// Buff Event
		public void OnBuffJump()
		{
			//foreach (var item in m_BuffList.Values)
			//{
			//	(item as IOnBuffJump)?.OnBuffJump(this);
			//}
		}
		private void OnBuffDash()
		{
			//foreach (var item in m_BuffList.Values)
			//{
			//	(item as IOnBuffDash)?.OnBuffDash(this);
			//}
		}

		// Anim Event
		public override void AnimEvent_AttackStart()
		{
			base.AnimEvent_AttackStart();

			m_IsSimulating = false;
			m_Velocity.x = System.MathF.Sign(m_Velocity.x) * float.Epsilon;
		}
		public override void AnimEvent_Attacking()
		{
			base.AnimEvent_Attacking();

			CreateProjectile();
		}
		public override void AnimEvent_AttackEnd()
		{
			base.AnimEvent_AttackEnd();

			m_IsSimulating = true;
			m_IsAttacking = false;
			m_AttackIndex = 0;
		}
		public void AnimEvent_AirAttackStart()
		{
			m_IsSimulating = false;

			m_Velocity = Vector2.zero;
		}
		public void AnimEvent_AirAttackEnd()
		{
			m_IsSimulating = true;
			m_IsAttacking = false;
		}
		public void AnimEvent_StartCombo()
		{
			m_CanComboAttack = true;
		}
		public void AnimEvent_EndCombo()
		{
			m_CanComboAttack = false;
		}
	}
}