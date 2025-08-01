using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BuffDebuff
{
	[RequireComponent(typeof(PlayerController2D), typeof(BoxCollider2D))]
	public class Player : Character<Player, PlayerStat, PlayerController2D, PlayerAnimator>, IDamageGiver, IDamageTaker
	{
		#region 기본 템플릿
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
		[FoldoutGroup("타이머"), LabelText("대쉬 타이머")]
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
				m_TimerController.GetTimer("Attack", 0).interval = 1f / value;
				m_Animator.Anim_SetAttackSpeed(1f / value);
			}
		}
		#endregion

		#region 매니저
		private static PlayerManager M_Player => PlayerManager.Instance;
		private static ProjectileManager M_Projectile => ProjectileManager.Instance;
		private static BuffManager M_Buff => BuffManager.Instance;
		private static InputManager M_Input => InputManager.Instance;
		#endregion

		#region 이벤트
		#endregion

		#region 초기화 & 마무리화 함수
		/// <summary>
		/// 초기화 함수
		/// </summary>
		public override void InitializePoolItem()
		{
			base.InitializePoolItem();

			m_IsAttacking = false;
			m_CanComboAttack = false;

			if (m_AttackSpot == null)
				m_AttackSpot = transform.FindInChildren("AttackSpot");

			// 타이머 초기화
			if (m_DashTimer == null)
			{
				m_DashTimer = new UtilClass.Timer()
				{
					autoClear = true,
				};
			}
			m_DashTimer.interval = m_Stat.DashRechargeTime;
			m_DashTimer.Clear();
		}
		/// <summary>
		/// 마무리화 함수
		/// </summary>
		public override void FinallizePoolItem()
		{
			base.FinallizePoolItem();

			if (m_DashTimer != null)
				m_DashTimer.Clear();
		}
		#endregion

		#region 유니티 콜백 함수
		protected override void Update()
		{
			// 테스트
			if (Input.GetKeyDown(KeyCode.F))
			{
				M_Buff.inventory.AddBuff("전방향 대쉬");
			}
			if (Input.GetKeyDown(KeyCode.E))
			{
				M_Buff.inventory.RemoveBuff("전방향 대쉬");
			}

			Vector2 directionalInput = new Vector2(M_Input.GetAxisRaw("PlayerMoveHorizontal"), M_Input.GetAxisRaw("PlayerMoveVertical"));
			SetDirectionalInput(directionalInput);

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
		#endregion
		#endregion

		#region 이벤트 함수
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
			m_AttackPatternIndex = 0;
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
		#endregion

		public void AddXp(float xp)
		{
			if (m_Stat.Xp.max <= 0)
			{
				Debug.LogError("플레이어 최대 경험치가 0보다 작거나 같음");
				return;
			}
			if (m_Stat.Level.current >= m_Stat.Level.max)
			{
				Debug.LogError("플레이어 레벨이 최대임");
				return;
			}

			float currentXp = m_Stat.Xp.current + xp * m_Stat.XpScale;
			float maxXp = m_Stat.Xp.max;

			while (currentXp >= maxXp)
			{
				InfiniteLoopDetector.Run("AddXp(float xp)", "Player.cs");

				if (m_Stat.Level.current >= m_Stat.Level.max)
				{
					currentXp = maxXp = 0;
					break;
				}

				LevelUp();

				currentXp -= maxXp;
				maxXp = M_Player.GetRequiredXp(m_Stat.Level.current);
			}

			m_Stat.Xp = new StatValue<float>(currentXp, maxXp);
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
			float targetVelocityX = m_DirectionalInput.x * m_Stat.MovementSpeed;

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

			Vector2 dir = UtilClass.GetMouseWorldPosition3D() - transform.position;

			if (M_Buff.inventory.HasBuff("전방향 대쉬") == true)
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
		protected override void CreateProjectile()
		{
			Vector3 position = m_AttackSpot.position;

			string projectileKey = "Fire Bite";
			Vector2 mousePos = UtilClass.GetMouseWorldPosition2D();
			float angle = position.GetAngle(mousePos);
			Quaternion quaternion = Quaternion.AngleAxis(angle, Vector3.forward);
			float movementSpeed = m_Stat.ShotSpeed * M_Projectile.GetMovementSpeed(projectileKey);
			float lifeTime = m_Stat.AttackRange * M_Projectile.GetLifeTime(projectileKey);
			ProjectileMover projectileMover = new StraightMover();

			Projectile projectile = M_Projectile.GetBuilder(projectileKey)
				.SetActive(true)
				.SetAutoInit(true)
				.SetParent(null)
				.SetPosition(position)
				.SetRotation(quaternion)
				.SetMovementSpeed(movementSpeed)
				.SetLifeTime(lifeTime)
				.SetMover(projectileMover)
				.Spawn();

			projectile["Enemy"].onEnter2D += (Collider2D collider) =>
			{
				Enemy enemy = collider.GetComponent<Enemy>();

				DamageArg<IDamageGiver, IDamageTaker> damageArg = new DamageArg<IDamageGiver, IDamageTaker>(
					m_Stat.AttackPower,
					this,
					enemy,
					projectile);

				GiveDamage(damageArg);

				projectile.Death();
			};
			projectile["Obstacle"].onEnter2D += (Collider2D collider) =>
			{
				projectile.Death();
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

			if (m_DashTimer.TimeCheck())
			{
				StatValue<int> newDashCount = m_Stat.DashCount;
				++newDashCount.current;
				m_Stat.DashCount = newDashCount;
			}
		}
	}
}