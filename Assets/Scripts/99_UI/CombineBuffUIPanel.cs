using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CombineBuffUIPanel : MonoBehaviour
{
	private BuffUI m_BuffUI;

	private BuffUIManager M_BuffUI => BuffUIManager.Instance;

	public void Initialize()
	{
		m_BuffUI = M_BuffUI.Spawn("Buff Combine");
		m_BuffUI.transform.SetParent(transform);
		m_BuffUI.transform.localPosition = Vector3.zero;
		m_BuffUI.transform.localScale = Vector3.one;
	}
	public bool SetBuffUIData(BuffUIData buffUIData)
	{
		if (m_BuffUI == null)
			return false;

		if (m_BuffUI.gameObject.activeSelf == true)
			return false;

		m_BuffUI.Initialize(buffUIData);
		m_BuffUI.name = buffUIData.title;
		m_BuffUI.gameObject.SetActive(true);

		return true;
	}
	public bool RemoveBuffUIData(BuffUIData buffUIData)
	{
		if (m_BuffUI == null)
			return false;

		if (m_BuffUI.gameObject.activeSelf == false)
			return false;

		if (m_BuffUI.buffUIData != buffUIData)
			return false;

		m_BuffUI.gameObject.SetActive(false);
		return true;
	}
}