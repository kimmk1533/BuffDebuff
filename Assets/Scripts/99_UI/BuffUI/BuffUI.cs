using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;

public class BuffUI : ObjectPoolItemBase
{
	#region 변수
	[SerializeField]
	protected int m_BuffCount;
	protected BuffData m_BuffData;

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
	public int buffCount
	{
		get { return m_BuffCount; }
		set
		{
			if (value < 0)
				return;
			else if (value > m_BuffData.maxStack)
				Debug.LogError("BuffUI MaxStack");

			m_BuffCount = Mathf.Clamp(value, 0, m_BuffData.maxStack);

			if (value <= 1)
				m_BuffCountText.gameObject.SetActive(false);
			else
				m_BuffCountText.gameObject.SetActive(true);

			m_BuffCountText.text = "x" + m_BuffCount.ToString();
		}
	}
	public BuffData buffData => m_BuffData;
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

	private void Awake()
	{
		#region Null Check
		if (m_Button == null)
		{
			m_Button = transform.Find<Button>("BackGround");
		}
		if (m_BuffCountText == null)
		{
			m_BuffCountText = transform.Find<TextMeshProUGUI>("BuffCount");
		}
		if (m_BuffCode == null)
		{
			m_BuffCode = transform.Find<TextMeshProUGUI>("BuffCode");
		}
		if (m_TitleText == null)
		{
			m_TitleText = transform.Find<TextMeshProUGUI>("Title");
		}
		if (m_SpriteImage == null)
		{
			m_SpriteImage = transform.Find<Image>("Sprite");
		}
		if (m_BuffGradeText == null)
		{
			m_BuffGradeText = transform.Find<TextMeshProUGUI>("Grade");
		}
		if (m_DescriptionText == null)
		{
			m_DescriptionText = transform.Find<TextMeshProUGUI>("Description");
		}
		#endregion
	}
	public virtual void Initialize(BuffData buffData)
	{
		base.Initialize();

		m_BuffCount = 1;
		UpdateBuffUIData(buffData);
	}
	public override void Finallize()
	{
		base.Finallize();

		m_Button.onClick.RemoveAllListeners();
	}

	public void UpdateBuffUIData()
	{
		m_BuffCountText.text = "x" + m_BuffCount.ToString();
		m_BuffCode.text = m_BuffData.code.ToString();
		m_TitleText.text = m_BuffData.title;
		m_SpriteImage.sprite = m_BuffData.sprite;
		m_BuffGradeText.text = m_BuffData.buffGrade.ToString();
		m_DescriptionText.text = m_BuffData.description;
	}
	public void UpdateBuffUIData(BuffData buffData)
	{
		m_BuffData = buffData;

		UpdateBuffUIData();
	}
}