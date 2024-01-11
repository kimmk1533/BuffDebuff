using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BuffUI : ObjectPoolItemBase
{
	public enum E_Type
	{
		None = -1,

		BuffRewards,
		BuffInventory,
		BuffCombine,
		BuffCombineInventory,
	}

	#region 변수
	private Buff m_Buff = null;

	private IBuffUIState m_BuffUIState = null;

	[Space(10)]
	[SerializeField]
	private TextMeshProUGUI m_BuffCountText = null;
	[SerializeField]
	private TextMeshProUGUI m_BuffCode = null;
	[SerializeField]
	private TextMeshProUGUI m_TitleText = null;
	[SerializeField]
	private Image m_SpriteImage = null;
	[SerializeField]
	private TextMeshProUGUI m_BuffGradeText = null;
	[SerializeField]
	private TextMeshProUGUI m_DescriptionText = null;

	[SerializeField]
	private Button m_Button = null;
	#endregion

	#region 프로퍼티
	public Buff buff => m_Buff;
	public BuffData buffData => m_Buff.buffData;

	public int buffCount
	{
		get { return m_Buff.count; }
		set
		{
			m_Buff.count = Mathf.Clamp(value, 0, m_Buff.maxStack);

			if (m_Buff.count <= 1)
				m_BuffCountText.gameObject.SetActive(false);
			else
				m_BuffCountText.gameObject.SetActive(true);

			m_BuffCountText.text = "x" + m_Buff.count.ToString();
		}
	}
	#endregion

	public override void InitializePoolItem()
	{
		base.InitializePoolItem();

		if (m_Button == null)
			m_Button = transform.Find<Button>("BackGround");

		if (m_BuffCountText == null)
			m_BuffCountText = transform.Find<TextMeshProUGUI>("BuffCount");

		if (m_BuffCode == null)
			m_BuffCode = transform.Find<TextMeshProUGUI>("BuffCode");

		if (m_TitleText == null)
			m_TitleText = transform.Find<TextMeshProUGUI>("Title");

		if (m_SpriteImage == null)
			m_SpriteImage = transform.Find<Image>("Sprite");

		if (m_BuffGradeText == null)
			m_BuffGradeText = transform.Find<TextMeshProUGUI>("Grade");

		if (m_DescriptionText == null)
			m_DescriptionText = transform.Find<TextMeshProUGUI>("Description");

		m_Button.onClick.AddListener(OnClicked);
	}
	public override void FinallizePoolItem()
	{
		base.FinallizePoolItem();

		m_Buff.count = 0;
		m_Buff = null;

		m_Button.onClick.RemoveAllListeners();
	}

	public void SetBuffData(BuffData buffData)
	{
		if (m_Buff == null ||
			m_Buff.buffData != buffData)
		{
			m_Buff = new Buff(buffData);
			buffCount = 1;

			UpdateBuffUIData();
		}
	}
	private void UpdateBuffUIData()
	{
		m_BuffCountText.text = "x" + buffCount.ToString();
		m_BuffCode.text = m_Buff.buffData.code.ToString();
		m_TitleText.text = m_Buff.buffData.title;
		m_SpriteImage.sprite = m_Buff.buffData.sprite;
		m_BuffGradeText.text = m_Buff.buffData.buffGrade.ToString();
		m_DescriptionText.text = m_Buff.buffData.description;
	}

	private void OnClicked()
	{
		m_BuffUIState?.OnClicked(this);
	}

	public void SetType(E_Type type)
	{
		switch (type)
		{
			case E_Type.BuffRewards:
				m_BuffUIState = RewardState.Instance;
				break;
			case E_Type.BuffInventory:
				m_BuffUIState = InventoryState.Instance;
				break;
			case E_Type.BuffCombine:
				m_BuffUIState = CombineState.Instance;
				break;
			case E_Type.BuffCombineInventory:
				m_BuffUIState = CombineInventoryState.Instance;
				break;
		}
	}

	private interface IBuffUIState
	{
		public abstract void OnClicked(BuffUI buffUI);
	}
	private class RewardState : SingletonBasic<RewardState>, IBuffUIState
	{
		private static BuffUIManager M_BuffUI => BuffUIManager.Instance;

		public void OnClicked(BuffUI buffUI)
		{
			M_BuffUI.AddBuff(buffUI.buffData);
			M_BuffUI.rewardsPanel.active = false;
		}
	}
	private class InventoryState : SingletonBasic<InventoryState>, IBuffUIState
	{
		private static BuffUIManager M_BuffUI => BuffUIManager.Instance;

		public void OnClicked(BuffUI buffUI)
		{
			Debug.Log(buffUI.buffData.title + ": " + buffUI.buffCount.ToString());
		}
	}
	private class CombineState : SingletonBasic<CombineState>, IBuffUIState
	{
		private static BuffUIManager M_BuffUI => BuffUIManager.Instance;

		public void OnClicked(BuffUI buffUI)
		{
			BuffData buffData = buffUI.buffData;

			if (M_BuffUI.RemoveCombineBuff(buffData) == false)
				return;

			M_BuffUI.AddBuff(buffData);
		}
	}
	private class CombineInventoryState : SingletonBasic<CombineInventoryState>, IBuffUIState
	{
		private static BuffUIManager M_BuffUI => BuffUIManager.Instance;

		public void OnClicked(BuffUI buffUI)
		{
			BuffData buffData = buffUI.buffData;

			if (M_BuffUI.AddCombineBuff(buffData) == false)
				return;

			M_BuffUI.RemoveBuff(buffData);
		}
	}
}