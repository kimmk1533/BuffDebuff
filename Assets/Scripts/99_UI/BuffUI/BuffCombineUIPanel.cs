using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Enum;

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

	}
	public void FinallizeGame()
	{
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

		M_BuffUI.Despawn(m_FirstBuffUI);
		M_BuffUI.Despawn(m_SecondBuffUI);
		m_FirstBuffUI = null;
		m_SecondBuffUI = null;
	}

	public bool AddBuff(BuffData buffData)
	{
		if (AddBuff(ref m_FirstBuffUI, m_First, buffData) == true)
			return true;
		else if (AddBuff(ref m_SecondBuffUI, m_Second, buffData) == true)
			return true;

		return false;
	}
	public bool RemoveBuff(BuffData buffData)
	{
		if (m_FirstBuffUI != null &&
			m_FirstBuffUI.buffData == buffData)
		{
			M_BuffUI.Despawn(m_FirstBuffUI);
			m_FirstBuffUI = null;

			if (m_SecondBuffUI != null)
			{
				m_SecondBuffUI.transform.SetParent(m_First);
				m_SecondBuffUI.transform.localPosition = Vector3.zero;
				m_FirstBuffUI = m_SecondBuffUI;
				m_SecondBuffUI = null;
			}

			return true;
		}
		else if (m_SecondBuffUI != null &&
			m_SecondBuffUI.buffData == buffData)
		{
			M_BuffUI.Despawn(m_SecondBuffUI);
			m_SecondBuffUI = null;
			return true;
		}

		return false;
	}

	private bool AddBuff(ref BuffUI buffUI, RectTransform parent, BuffData buffData)
	{
		if (buffUI != null)
			return false;

		buffUI = M_BuffUI.GetBuilder("Buff Combine")
			.SetActive(true)
			.SetAutoInit(true)
			.SetParent(parent)
			.SetLocalPosition(Vector3.zero)
			.SetScale(Vector3.one)
			.Spawn();

		buffUI.SetBuffData(buffData);
		buffUI.SetType(BuffUI.E_Type.BuffCombine);

		return true;
	}
}