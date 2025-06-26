using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-99)]
[RequireComponent(typeof(Camera))]
public class UICamera : SerializedSingleton<UICamera>
{
	#region 변수
	private Camera m_Camera;
	#endregion

	#region 프로퍼티
	public Camera uiCamera => m_Camera;
	#endregion

	private void Awake()
	{
		Initialize();
	}
	public void Initialize()
	{
		this.NullCheckGetComponent<Camera>(ref m_Camera);
	}
	public void Finallize()
	{

	}
}