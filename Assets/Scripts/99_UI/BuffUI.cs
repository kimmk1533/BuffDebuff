using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BuffUI : MonoBehaviour
{
	[SerializeField]
	private int m_BuffCount;
	[SerializeField]
	private BuffUIData m_Data;

	[Space(10)]
	[SerializeField]
	private TextMeshProUGUI m_BuffCountText;
	[SerializeField]
	private TextMeshProUGUI m_TitleText;
	[SerializeField]
	private Image m_SpriteImage;
	[SerializeField]
	private TextMeshProUGUI m_BuffGradeText;
	[SerializeField]
	private TextMeshProUGUI m_DescriptionText;

	public int buffCount
	{
		get { return m_BuffCount; }
		set
		{
			if (value < 0)
			{
				return;
			}

			m_BuffCount = Mathf.Clamp(value, 0, m_Data.maxStack);

			if (value <= 1)
				m_BuffCountText.gameObject.SetActive(false);
			else
			{
				m_BuffCountText.gameObject.SetActive(true);

				m_BuffCountText.text = "x" + m_BuffCount.ToString();
			}
		}
	}

	public void Initialize(BuffUIData buffUIData)
	{
		#region Null 체크
		if (m_BuffCountText == null)
		{
			m_BuffCountText = transform.Find("BuffCount").GetComponent<TextMeshProUGUI>();
		}
		if (m_TitleText == null)
		{
			m_TitleText = transform.Find("Title").GetComponent<TextMeshProUGUI>();
		}
		if (m_SpriteImage == null)
		{
			m_SpriteImage = transform.Find("Sprite").GetComponent<Image>();
		}
		if (m_BuffGradeText == null)
		{
			m_BuffGradeText = transform.Find("Grade").GetComponent<TextMeshProUGUI>();
		}
		if (m_DescriptionText == null)
		{
			m_DescriptionText = transform.Find("Description").GetComponent<TextMeshProUGUI>();
		}
		#endregion

		m_Data = buffUIData;

		m_BuffCount = 1;
		m_BuffCountText.text = "x" + m_BuffCount.ToString();
		m_TitleText.text = m_Data.title;
		m_SpriteImage.sprite = m_Data.sprite;
		m_BuffGradeText.text = m_Data.buffGrade.ToString();
		m_DescriptionText.text = m_Data.description;
	}
}