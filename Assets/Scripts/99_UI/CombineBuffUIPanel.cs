using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CombineBuffUIPanel : MonoBehaviour
{
	#region 변수
	private BuffUI m_BuffUI;
	#endregion

	#region 프로퍼티
	public BuffData buffData
	{
		get
		{
			if (m_BuffUI == null)
				return null;

			if (m_BuffUI.gameObject.activeSelf == false)
				return null;

			return m_BuffUI.buffData;
		}
	}
	#endregion

	#region 매니저
	private static BuffUIManager M_BuffUI => BuffUIManager.Instance;
	#endregion

	public void Initialize()
	{
		m_BuffUI = M_BuffUI.GetBuilder("Buff Combine")
			.SetActive(true)
			.SetAutoInit(true)
			.SetParent(transform)
			.Spawn();

		m_BuffUI.transform.localPosition = Vector3.zero;
		m_BuffUI.transform.localScale = Vector3.one;
	}
	public bool SetBuffData(BuffData buffData)
	{
		if (m_BuffUI == null)
			return false;

		if (m_BuffUI.gameObject.activeSelf == true)
			return false;

		m_BuffUI.Initialize(buffData);
		m_BuffUI.name = buffData.title;
		m_BuffUI.gameObject.SetActive(true);

		return true;
	}
	public bool RemoveBuffData(BuffData buffData)
	{
		if (m_BuffUI == null)
			return false;

		if (m_BuffUI.gameObject.activeSelf == false)
			return false;

		if (m_BuffUI.buffData.code != buffData.code)
			return false;

		m_BuffUI.gameObject.SetActive(false);
		return true;
	}
}