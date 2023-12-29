using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class UICamera : Singleton<UICamera>
{
	#region 변수
	private Camera m_Camera;
	#endregion

	#region 프로퍼티
	public Camera uiCamera => m_Camera;
	#endregion

	public void Initialize()
	{
		this.Safe_GetComponent<Camera>(ref m_Camera);
	}
	public void Finallize()
	{

	}
}