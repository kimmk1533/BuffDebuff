using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;

public class BuffUI : ObjectPoolItemBase
{
	#region 변수
	protected Buff m_Buff;

	[Space(10)]
	[SerializeField]
	protected TextMeshProUGUI m_BuffCountText;
	[SerializeField]
	protected TextMeshProUGUI m_BuffCode;
	[SerializeField]
	protected TextMeshProUGUI m_TitleText;
	[SerializeField]
	protected Image m_SpriteImage;
	[SerializeField]
	protected TextMeshProUGUI m_BuffGradeText;
	[SerializeField]
	protected TextMeshProUGUI m_DescriptionText;

	[SerializeField]
	protected Button m_Button;
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

			if (value <= 1)
				m_BuffCountText.gameObject.SetActive(false);
			else
				m_BuffCountText.gameObject.SetActive(true);

			m_BuffCountText.text = "x" + m_Buff.count.ToString();
		}
	}
	#endregion

	#region 이벤트
	public event UnityAction onClick
	{
		add
		{
			m_Button.onClick.AddListener(value);
		}
		remove
		{
			m_Button.onClick.RemoveListener(value);
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
	}
	public override void FinallizePoolItem()
	{
		base.FinallizePoolItem();

		m_Button.onClick.RemoveAllListeners();
	}

	public void UpdateBuffUIData()
	{
		m_BuffCountText.text = "x" + m_Buff.count.ToString();
		m_BuffCode.text = m_Buff.buffData.code.ToString();
		m_TitleText.text = m_Buff.buffData.title;
		m_SpriteImage.sprite = m_Buff.buffData.sprite;
		m_BuffGradeText.text = m_Buff.buffData.buffGrade.ToString();
		m_DescriptionText.text = m_Buff.buffData.description;
	}

	public void ChangeBuff(Buff buff)
	{
		m_Buff = buff;

		UpdateBuffUIData();
	}
}