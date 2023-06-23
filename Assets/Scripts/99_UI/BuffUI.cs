using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;

public class BuffUI : MonoBehaviour
{
	[SerializeField]
	private int m_BuffCount;
	private BuffData m_BuffData;

	[Space(10)]
	[SerializeField]
	private TextMeshProUGUI m_BuffCountText;
	[SerializeField]
	private TextMeshProUGUI m_BuffCode;
	[SerializeField]
	private TextMeshProUGUI m_TitleText;
	[SerializeField]
	private Image m_SpriteImage;
	[SerializeField]
	private TextMeshProUGUI m_BuffGradeText;
	[SerializeField]
	private TextMeshProUGUI m_DescriptionText;

	[SerializeField]
	private Button m_Button;

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

	public void Initialize(BuffData buffData)
	{
		m_BuffCount = 1;
		UpdateBuffUIData(buffData);
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