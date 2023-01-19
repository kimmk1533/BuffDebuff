using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Controller2D))]
public class Player : MonoBehaviour
{
	Controller2D m_Controller;

	private void Start()
	{
		m_Controller = GetComponent<Controller2D>();
	}
}