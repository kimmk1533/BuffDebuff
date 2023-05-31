using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
	private Dictionary<int, BuffUI> m_BuffMap;
	[SerializeField]
	private BuffUI m_BuffUIOrigin;
	[SerializeField]
	private GridLayoutGroup m_BuffUIContent;

	private void Awake()
	{
		Initialize();
	}
	#region Test
	public BuffUIData testData;

	private void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			AddBuff(testData);
		}
		if (Input.GetMouseButtonDown(1))
		{
			RemoveBuff(testData);
		}
	} 
	#endregion

	public void Initialize()
	{
		m_BuffMap = new Dictionary<int, BuffUI>();
	}
	public void AddBuff(BuffUIData buffUIData)
	{
		if (m_BuffMap.TryGetValue(buffUIData.code, out BuffUI buffUI))
		{
			if (buffUI.buffCount < buffUIData.maxStack)
				++buffUI.buffCount;

			return;
		}

		buffUI = GameObject.Instantiate<BuffUI>(m_BuffUIOrigin, m_BuffUIContent.transform);
		buffUI.Initialize(buffUIData);
		buffUI.name = "BuffUI (" + buffUIData.title + ")";
		buffUI.transform.localScale = Vector3.one * 0.75f;

		buffUI.gameObject.SetActive(true);

		m_BuffMap.Add(buffUIData.code, buffUI);
	}
	public void RemoveBuff(BuffUIData buffUIData)
	{
		if (m_BuffMap.TryGetValue(buffUIData.code, out BuffUI buffUI))
		{
			if (buffUI.buffCount > 0)
				--buffUI.buffCount;

			if (buffUI.buffCount == 0)
			{
				GameObject.Destroy(buffUI.gameObject);

				m_BuffMap.Remove(buffUIData.code);
			}
		}
	}
}