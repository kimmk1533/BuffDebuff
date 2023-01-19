using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Controller2D))]
public class Player : MonoBehaviour
{
	float m_MoveSpeed = 6;
	float m_Gravity = -20;
	Vector3 m_Velocity;

	Controller2D m_Controller;

	private void Start()
	{
		m_Controller = GetComponent<Controller2D>();
	}

	private void Update()
	{
		Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

		m_Velocity.x = input.x * m_MoveSpeed;
		m_Velocity.y += m_Gravity * Time.deltaTime;
		m_Controller.Move(m_Velocity * Time.deltaTime);
	}
}