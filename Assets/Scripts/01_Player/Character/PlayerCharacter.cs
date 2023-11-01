using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerController2D))]
public sealed class PlayerCharacter : Character<PlayerCharacterStat, PlayerController2D, PlayerAnimator>
{
	private Vector2 m_DirectionalInput;

	[SerializeField]
	private UtilClass.Timer m_DashTimer;

	private BuffManager M_Buff => BuffManager.Instance;
	private StageManager M_Stage => StageManager.Instance;

	protected override void Update()
	{
		base.Update();

		DashTimer();

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

	public override void Initialize()
	{
		base.Initialize();

		// Stat Init
		m_CurrentStat.Xp = 0.0f;
		m_CurrentStat.Level = 0;

		// Timer Init
		m_HealTimer = new UtilClass.Timer(m_CurrentStat.HpRegenTime);
		m_AttackTimer = new UtilClass.Timer(1f / m_CurrentStat.AttackSpeed, true);
		m_DashTimer = new UtilClass.Timer(m_CurrentStat.DashRechargeTime);
	}

	private void SetDirectionalInput(Vector2 input)
	{
		m_DirectionalInput = input;
		m_Animator.Anim_SetDirectionalInput(input);
	}
	protected override void CalculateVelocity()
	{
		float targetVelocityX = m_DirectionalInput.x * m_CurrentStat.MoveSpeed;

		m_Velocity.x = Mathf.SmoothDamp(m_Velocity.x, targetVelocityX, ref m_VelocityXSmoothing, (m_Controller.collisions.grounded) ? m_AccelerationTimeGrounded : m_AccelerationTimeAirborne);

		m_Velocity.y += m_Controller.gravity * Time.deltaTime;
	}
	protected override void Move()
	{
		Vector2 directionalInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
		SetDirectionalInput(directionalInput);

		CalculateVelocity();

		m_Controller.Move(m_Velocity * Time.deltaTime, m_DirectionalInput);
		if (m_Controller.collisions.above || m_Controller.collisions.below)
		{
			m_Velocity.y = 0;
		}

		//CheckPortal();

		m_Animator.Anim_SetVelocity(m_Velocity);
		m_Animator.Anim_SetIsGround(m_Controller.collisions.grounded);
	}
	//private void CheckPortal()
	//{
	//	RaycastHit2D hit = Physics2D.BoxCast(m_Controller.collider.bounds.center, m_Controller.collider.bounds.size, 0.0f, m_Velocity, 0.1f, LayerMask.GetMask("Portal"));

	//	if (hit)
	//	{
	//		Transform spawnPoint = M_Stage.GetSpawnPoint(hit.collider);

	//		if (spawnPoint != null)
	//			transform.position = spawnPoint.position;
	//	}
	//}

	// Timer Func
	private void DashTimer()
	{
		if (m_CurrentStat.DashCount >= m_MaxStat.DashCount)
		{
			m_DashTimer.Clear();
			return;
		}

		m_DashTimer.Update();

		if (m_DashTimer.TimeCheck(true))
		{
			++m_CurrentStat.DashCount;
		}
	}

	// Buff Func
	public bool AddBuff(string name)
	{
		if (name == null || name == string.Empty)
			return false;

		BuffData buffData = M_Buff.GetBuffData(name);

		return this.AddBuff(buffData);
	}
	public bool AddBuff(BuffData buffData)
	{
		if (buffData == null)
			return false;

		if (m_BuffList.TryGetValue(buffData.code, out AbstractBuff buff) &&
			buff != null)
		{
			if (buff.count < buffData.maxStack)
			{
				++buff.count;
				(buff as IOnBuffAdded)?.OnBuffAdded(this);
			}
			else
			{
				Debug.Log("Buff Count is Max. title =" + buffData.title + ", maxStack = " + buffData.maxStack.ToString());

				return false;
			}

			return true;
		}

		buff = M_Buff.CreateBuff(buffData);

		m_BuffList.Add(buffData.code, buff);

		(buff as IOnBuffAdded)?.OnBuffAdded(this);

		return true;
	}
	public bool RemoveBuff(string name)
	{
		if (name == null || name == string.Empty)
			return false;

		BuffData buff = M_Buff.GetBuffData(name);

		return this.RemoveBuff(buff);
	}
	public bool RemoveBuff(BuffData buffData)
	{
		if (buffData == null)
			return false;

		if (m_BuffList.TryGetValue(buffData.code, out AbstractBuff buff) &&
			buff != null)
		{
			if (buff.count > 0)
			{
				--buff.count;
			}
			else
			{
				m_BuffList.Remove(buffData.code);
			}

			(buff as IOnBuffRemoved)?.OnBuffRemoved(this);

			return true;
		}

		Debug.Log("버프 없는데 제거");

		return false;
	}

	public override void AnimEvent_AttackStart()
	{
		base.AnimEvent_AttackStart();

		m_IsSimulating = false;
	}
	public override void AnimEvent_Attacking()
	{
		base.AnimEvent_Attacking();
	}
	public override void AnimEvent_AttackEnd()
	{
		base.AnimEvent_AttackEnd();

		m_IsSimulating = true;
	}
	public void AnimEvent_AirAttackStart()
	{
		m_IsSimulating = false;

		m_Velocity = Vector2.zero;
	}
	public void AnimEvent_AirAttackEnd()
	{
		m_IsSimulating = true;
	}

	private void JumpInputDown()
	{
		if ((m_Controller.collisions.below && m_DirectionalInput.y != -1) == false)
			return;

		m_Animator.Anim_Jump();
		Jump();

		m_Velocity.y = m_Controller.maxJumpVelocity;
	}
	private void JumpInputUp()
	{
		if (m_Velocity.y > m_Controller.minJumpVelocity)
			m_Velocity.y = m_Controller.minJumpVelocity;
	}
	public void Jump()
	{
		foreach (var item in m_BuffList.Values)
		{
			(item as IOnBuffJump)?.OnBuffJump(this);
		}
	}
	public bool CanDash()
	{
		return m_CurrentStat.DashCount > 0;
	}
	public void Dash()
	{
		if (CanDash() == false)
			return;

		--m_CurrentStat.DashCount;

		foreach (var item in m_BuffList.Values)
		{
			(item as IOnBuffDash)?.OnBuffDash(this);
		}

		Vector2 dir = UtilClass.GetMouseWorldPosition() - transform.position;

		// 좌우 대쉬
		m_Velocity.x = Mathf.Sign(dir.x) * m_CurrentStat.DashSpeed;

		// 마우스 대쉬
		//m_Velocity = dir.normalized * m_Character.currentStat.DashSpeed;
	}

	private void OnValidate()
	{
		M_Buff.OnValidate();
	}
}