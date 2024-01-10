using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Enum;
using System;

public class BuffCombineUIPanel : MonoBehaviour
{
	#region 변수
	[SerializeField]
	private RectTransform m_First = null;
	[SerializeField]
	private RectTransform m_Second = null;

	private BuffUI m_FirstBuffUI = null;
	private BuffUI m_SecondBuffUI = null;

	[SerializeField]
	private Button m_CombineButton = null;
	#endregion

	#region 프로퍼티
	public BuffData buffData
	{
		get
		{
			if (m_SecondBuffUI == null)
				return null;

			if (m_SecondBuffUI.gameObject.activeSelf == false)
				return null;

			return m_SecondBuffUI.buffData;
		}
	}
	#endregion

	#region 매니저
	private static BuffManager M_Buff => BuffManager.Instance;
	private static BuffUIManager M_BuffUI => BuffUIManager.Instance;
	#endregion

	public void Initialize()
	{
		m_CombineButton.onClick.AddListener(CombineBuff);
	}
	public void Finallize()
	{
		m_CombineButton.onClick.RemoveAllListeners();
	}

	public void InitializeGame()
	{
		m_FirstBuffUI = M_BuffUI.GetBuilder("Buff Combine")
			.SetActive(false)
			.SetAutoInit(true)
			.SetParent(m_First)
			.Spawn();
		m_FirstBuffUI.transform.localPosition = Vector3.zero;
		m_FirstBuffUI.transform.localScale = Vector3.one;

		m_SecondBuffUI = M_BuffUI.GetBuilder("Buff Combine")
			.SetActive(false)
			.SetAutoInit(true)
			.SetParent(m_Second)
			.Spawn();
		m_SecondBuffUI.transform.localPosition = Vector3.zero;
		m_SecondBuffUI.transform.localScale = Vector3.one;
	}
	public void FinallizeGame()
	{
		M_BuffUI.Despawn(m_SecondBuffUI);

		m_SecondBuffUI = null;
	}

	private void CombineBuff()
	{
		BuffData first = m_FirstBuffUI.buffData;
		BuffData second = m_SecondBuffUI.buffData;

		if (first == null)
			return;

		if (second == null)
			return;

		#region 버프 타입 확인
		E_BuffType firstBuffType = first.buffType;
		E_BuffType secondBuffType = second.buffType;

		if (firstBuffType == E_BuffType.Bothbuff)
			firstBuffType = secondBuffType;
		else if (secondBuffType == E_BuffType.Bothbuff)
			secondBuffType = firstBuffType;

		if (firstBuffType != secondBuffType)
		{
			Debug.Log("Combine Buff`s buffType should be same. first = " + first.buffType.ToString() + ", second = " + second.buffType.ToString());
			return;
		}
		#endregion

		#region 버프 등급 확인
		E_BuffGrade firstBuffGrade = first.buffGrade;
		E_BuffGrade secondBuffGrade = second.buffGrade;

		if (firstBuffGrade != secondBuffGrade)
		{
			Debug.Log("Combine Buff`s buffGrade should be same. first = " + first.buffGrade.ToString() + ", second = " + second.buffGrade.ToString());
			return;
		}

		E_BuffGrade grade = firstBuffGrade + 1;

		if (grade == E_BuffGrade.Max)
			grade = E_BuffGrade.Max - 1;
		#endregion

		BuffData buffData = null;
		switch (firstBuffType)
		{
			case E_BuffType.Buff:
				buffData = M_Buff.GetRandomBuffData(E_BuffType.Debuff, grade);
				break;
			case E_BuffType.Debuff:
				buffData = M_Buff.GetRandomBuffData(E_BuffType.Buff, grade);
				break;
			case E_BuffType.Bothbuff:
				buffData = M_Buff.GetRandomBuffData();
				break;
		}

		if (buffData == null)
			return;

		M_BuffUI.AddBuff(buffData);

		m_FirstBuffUI.gameObject.SetActive(false);
		m_SecondBuffUI.gameObject.SetActive(false);
	}

	public void AddCombineBuffData(BuffData buffData)
	{

	}
	private void RemoveBuffData(BuffUI buffUI)
	{
		if (buffUI.gameObject.activeSelf == false)
			return;

		buffUI.gameObject.SetActive(false);
	}
}