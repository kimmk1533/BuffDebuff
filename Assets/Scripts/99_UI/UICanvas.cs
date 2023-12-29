using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class UICanvas : MonoBehaviour
{
	#region 변수
	private Canvas m_Canvas;
	#endregion

	#region 프로퍼티
	#endregion

	private void Awake()
	{
		Initialize();
	}

	public void Initialize()
	{
		this.Safe_GetComponent<Canvas>(ref m_Canvas);
		m_Canvas.renderMode = RenderMode.ScreenSpaceCamera;
		m_Canvas.pixelPerfect = true;
		m_Canvas.worldCamera = UICamera.Instance.uiCamera;
	}
	public void Finallize()
	{

	}
}