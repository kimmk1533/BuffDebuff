using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
	// Buff UI
	[SerializeField]
	private BuffUI m_BuffUIOrigin;
	private GameObject m_BuffUIPoolParent;
	private ObjectPool<BuffUI> m_BuffUIPool;

	// Buff Rewards
	[SerializeField]
	private GameObject m_BuffRewardsPanel;
	[SerializeField]
	private HorizontalLayoutGroup m_BuffRewardsContent;

	// Buff Inventory
	[SerializeField]
	private GameObject m_BuffUIInventoryPanel;
	[SerializeField]
	private GridLayoutGroup m_BuffUIInventoryContent;
	private Dictionary<int, BuffUI> m_BuffUIInventoryMap;

	// Panels
	private Dictionary<string, Panel> m_PanelMap;

	// property
	private BuffManager M_Buff => BuffManager.Instance;
	private PlayerManager M_Player => PlayerManager.Instance;

	private void Awake()
	{
		Initialize();
	}
	private void Start()
	{
		UpdateBuffRewards();
	}
	private void Update()
	{
		Panel panel = null;

		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			panel = m_PanelMap["Buff Rewards"];

			if (panel.active)
			{
				panel.TurnOffPanel();
			}
			else
			{
				foreach (var item in m_PanelMap)
				{
					item.Value.TurnOffPanel();
				}
				UpdateBuffRewards();
				panel.TurnOnPanel();
			}
		}
		if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			panel = m_PanelMap["Buff Inventory"];

			if (panel.active)
			{
				panel.TurnOffPanel();
			}
			else
			{
				foreach (var item in m_PanelMap)
				{
					item.Value.TurnOffPanel();
				}

				panel.TurnOnPanel();
			}
		}
	}

	public void Initialize()
	{
		if (m_BuffUIPoolParent == null)
		{
			m_BuffUIPoolParent = new GameObject("BuffUI_Pool");
			m_BuffUIPoolParent.transform.SetParent(transform);
		}

		if (m_BuffUIPool == null)
		{
			m_BuffUIPool = new ObjectPool<BuffUI>(m_BuffUIOrigin, 100, m_BuffUIPoolParent.transform);
		}

		if (m_PanelMap == null)
		{
			m_PanelMap = new Dictionary<string, Panel>
			{
				{ "Buff Rewards", new Panel(m_BuffRewardsPanel) },
				{ "Buff Inventory", new Panel(m_BuffUIInventoryPanel) }
			};
		}

		if (m_BuffUIInventoryMap == null)
			m_BuffUIInventoryMap = new Dictionary<int, BuffUI>();
		else
			m_BuffUIInventoryMap.Clear();
	}

	public void UpdateBuffRewards()
	{
		int childCount = m_BuffRewardsContent.transform.childCount;
		int offset = 0;
		for (int i = 0; i < childCount; ++i)
		{
			BuffUI buffUI = m_BuffRewardsContent.transform.GetChild(offset).GetComponent<BuffUI>();

			buffUI.onClick.RemoveAllListeners();

			if (buffUI == null ||
				m_BuffUIPool.Despawn(buffUI) == false)
				++offset;
		}

		int count = M_Buff.rewardsCount;
		List<BuffUIData> buffUIDataList = M_Buff.GetRandomBuffUIData(E_BuffType.Buff, count);
		count = Mathf.Clamp(count, 0, buffUIDataList.Count);
		for (int i = 0; i < count; ++i)
		{
			BuffUIData buffUIData = buffUIDataList[i];

			BuffUI buffUI = m_BuffUIPool.Spawn();
			buffUI.Initialize(buffUIData);
			buffUI.transform.SetParent(m_BuffRewardsContent.transform);
			buffUI.name = "BuffUI (" + buffUIData.title + ")";
			buffUI.transform.localScale = Vector3.one;
			buffUI.onClick.AddListener(() =>
			{
				m_PanelMap["Buff Rewards"].TurnOffPanel();
				AddBuff_Inventory(buffUIData);
				M_Player.AddBuff(buffUIData.code);
			});

			buffUI.gameObject.SetActive(true);
		}
	}
	public void AddBuff_Inventory(BuffUIData buffUIData)
	{
		if (m_BuffUIInventoryMap.TryGetValue(buffUIData.code, out BuffUI buffUI) == true)
		{
			if (buffUI.buffCount < buffUIData.maxStack)
				++buffUI.buffCount;

			return;
		}

		buffUI = m_BuffUIPool.Spawn();
		buffUI.Initialize(buffUIData);
		buffUI.transform.SetParent(m_BuffUIInventoryContent.transform);
		buffUI.name = "BuffUI (" + buffUIData.title + ")";
		buffUI.transform.localScale = Vector3.one * 0.75f;

		buffUI.gameObject.SetActive(true);

		m_BuffUIInventoryMap.Add(buffUIData.code, buffUI);
	}
	public void RemoveBuff_Inventory(BuffUIData buffUIData)
	{
		if (m_BuffUIInventoryMap.TryGetValue(buffUIData.code, out BuffUI buffUI) == false)
			return;

		if (buffUI.buffCount > 0)
			--buffUI.buffCount;

		if (buffUI.buffCount == 0)
		{
			m_BuffUIPool.Despawn(buffUI);

			m_BuffUIInventoryMap.Remove(buffUIData.code);
		}
	}

	[System.Serializable]
	private class Panel
	{
		private GameObject panel;

		public Panel(GameObject panel)
		{
			this.panel = panel;
		}

		public bool active
		{
			get
			{
				return panel.activeSelf;
			}
			set
			{
				if (value == true)
					TurnOnPanel();
				else
					TurnOffPanel();
			}
		}

		public void TurnOnPanel()
		{
			panel.SetActive(true);
		}
		public void TurnOffPanel()
		{
			panel.SetActive(false);
		}
	}
}