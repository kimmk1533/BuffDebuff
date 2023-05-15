using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D), typeof(PlayerController2D))]
public sealed class Player : MonoBehaviour
{
	#region PlayerController Variables
	[SerializeField]
	float m_MoveSpeed = 10f;
	[SerializeField]
	float m_DashSpeed = 15f;

	[SerializeField, ReadOnly]
	Vector2 m_Velocity;
	float m_VelocityXSmoothing;
	float m_AccelerationTimeAirborne = 0.2f;
	float m_AccelerationTimeGrounded = 0.1f;

	Vector2 m_DirectionalInput;
	#endregion

	PlayerController2D m_Controller;
	PlayerRenderer m_Renderer;

	[SerializeField]
	Character m_Character;

	//int buff
	//{

	//}

	ProjectileManager M_Projectile => ProjectileManager.Instance;

	private void Awake()
	{
		Initialize();
	}
	//private void Start()
	//{
	//	m_Character.AddBuff(M_Buff.m_BuffDictionary["체력 증가"]);
	//	m_Character.AddBuff(M_Buff.m_BuffDictionary["재생"]);

	//	foreach (AbstractBuff item in m_Character.m_BuffList)
	//	{
	//		item.OnBuffInitialize.OnBuffInvoke(ref m_Character);
	//	}
	//}
	private void Update()
	{
		#region PlayerController Method
		CalculateVelocity();

		m_Controller.Move(m_Velocity * Time.deltaTime, m_DirectionalInput);

		if (m_Controller.collisions.above || m_Controller.collisions.below)
		{
			m_Velocity.y = 0;
		}

		m_Renderer.SetVelocity(m_Velocity);
		m_Renderer.SetIsGround(m_Controller.collisions.grounded);
		#endregion

		#region 임시 버프
		if (Input.GetKeyDown(KeyCode.F))
		{
			m_Character.AddBuff("체력 증가");
			//m_Character.AddBuff("재생");
		}
		//if (Input.GetMouseButtonDown(0))
		//{
		//	m_Character.AddBuff("빠른 재생");
		//}
		//if (Input.GetKeyDown(KeyCode.G))
		//{
		//	m_Character.RemoveBuff("체력 증가");
		//}
		#endregion

		m_Character.Update();
	}

	private void Initialize()
	{
		m_Controller = GetComponent<PlayerController2D>();
		m_Controller.Initialize();

		m_Renderer = GetComponentInChildren<PlayerRenderer>();

		m_Character = new Character();
	}

	#region PlayerController Func
	public void SetDirectionalInput(Vector2 input)
	{
		m_DirectionalInput = input;
		m_Renderer.SetDirectionalInput(input);
	}
	public void OnJumpInputDown()
	{
		if (!(m_Controller.collisions.below && m_DirectionalInput.y != -1))
			return;

		m_Renderer.Jump();
		m_Character.Jump();

		m_Velocity.y = m_Controller.maxJumpVelocity;
	}
	public void OnJumpInputUp()
	{
		if (m_Velocity.y > m_Controller.minJumpVelocity)
			m_Velocity.y = m_Controller.minJumpVelocity;
	}
	public void OnDashInputDown()
	{
		if (m_Character.Dash() == false)
			return;

		// 좌우 대쉬
		//m_Velocity.x = m_DirectionalInput.x * m_DashSpeed;

		// 마우스 대쉬
		Vector2 dir = UtilClass.GetMouseWorldPosition() - transform.position;
		m_Velocity = dir.normalized * m_DashSpeed;
	}

	void CalculateVelocity()
	{
		float targetVelocityX = m_DirectionalInput.x * m_MoveSpeed;

		m_Velocity.x = Mathf.SmoothDamp(m_Velocity.x, targetVelocityX, ref m_VelocityXSmoothing, (m_Controller.collisions.grounded) ? m_AccelerationTimeGrounded : m_AccelerationTimeAirborne);

		m_Velocity.y += m_Controller.gravity * Time.deltaTime;
	}
	#endregion

	public void DefaultAttack()
	{
		Projectile projectile = M_Projectile.Spawn("Projectile");
		projectile.transform.position = transform.position;
		projectile.m_MovingType = E_MovingType.Straight;

		Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		float angle = transform.position.GetAngle(mousePos);
		projectile.transform.rotation = Quaternion.AngleAxis(angle - 90, Vector3.forward);
		projectile.m_MoveSpeed = 5.0f;
	}
}