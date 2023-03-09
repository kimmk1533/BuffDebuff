using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerInput : MonoBehaviour
{
	Player m_Player;

	private void Start()
	{
		m_Player = GetComponent<Player>();
	}
	private void Update()
	{
		Vector2 directionalInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
		m_Player.SetDirectionalInput(directionalInput);

		if (Input.GetKeyDown(KeyCode.Space))
		{
			m_Player.OnJumpInputDown();
		}
		if (Input.GetKeyUp(KeyCode.Space))
		{
			m_Player.OnJumpInputUp();
		}

		//if (Input.GetMouseButtonDown(0))
		//{
		//	m_Player.DefaultAttack();
		//}

		if (Input.GetMouseButtonDown(1))
		{
			m_Player.OnDashInputDown();
		}
	}
}