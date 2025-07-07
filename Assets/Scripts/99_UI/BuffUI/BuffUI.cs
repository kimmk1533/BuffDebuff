using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace BuffDebuff
{
	public partial class BuffUI : ObjectPoolItem<BuffUI>
	{
		#region 기본 템플릿
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

		#region 매니저
		#endregion

		#region 이벤트
		#endregion

		#region 초기화 & 마무리화 함수
		/// <summary>
		/// 초기화 함수
		/// </summary>
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
		/// <summary>
		/// 마무리화 함수
		/// </summary>
		public override void FinallizePoolItem()
		{
			base.FinallizePoolItem();

			m_Buff.count = 0;
			m_Buff = null;

			m_Button.onClick.RemoveAllListeners();
		}

		#region 유니티 콜백 함수

		#endregion
		#endregion

		#region 유니티 콜백 함수
		#endregion
		#endregion

		#region 이벤트 함수
		private void OnClicked()
		{
			m_BuffUIState?.OnClicked(this);
		}
		#endregion

		public void SetState(E_BuffUIState state)
		{
			switch (state)
			{
				case E_BuffUIState.BuffRewards:
					m_BuffUIState = new RewardState();
					break;
				case E_BuffUIState.BuffInventory:
					m_BuffUIState = new InventoryState();
					break;
				case E_BuffUIState.BuffCombine:
					m_BuffUIState = new CombineState();
					break;
				case E_BuffUIState.BuffCombineInventory:
					m_BuffUIState = new CombineInventoryState();
					break;
			}
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
	}
}