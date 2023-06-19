using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
	// Buff UI
	[SerializeField]
	private BuffUI m_BuffUIOrigin;
	private ObjectPool<BuffUI> m_BuffUIPool;

	// Buff Combine
	[SerializeField]
	private RectTransform m_FirstCombineBuff;
	[SerializeField]
	private RectTransform m_SecondCombineBuff;
	[SerializeField]
	private Button m_CombineButton;

	// Panels
	private BuffPanel m_CurrentBuffPanel;
	[SerializeField]
	private List<BuffPanel> m_BuffPanelList;
	private Dictionary<string, BuffPanel> m_BuffPanelMap;

	// Buff Inventory
	private Dictionary<int, BuffUI> m_BuffInventoryMap;

	// property
	private BuffPanel rewardsPanel => m_BuffPanelMap["Buff Rewards"];
	private BuffPanel inventoryPanel => m_BuffPanelMap["Buff Inventory"];
	private BuffPanel combinePanel => m_BuffPanelMap["Buff Combine"];

	// Manager
	private BuffManager M_Buff => BuffManager.Instance;
	private PlayerManager M_Player => PlayerManager.Instance;

	private void Awake()
	{
		Initialize();
	}
	private void Start()
	{
		RerollBuffRewards();
	}
	private void Update()
	{
		foreach (var item in m_BuffPanelList)
		{
			if (Input.GetKeyDown(item.keyCode))
			{
				m_CurrentBuffPanel = item;
				break;
			}
		}
		if (m_CurrentBuffPanel != null)
		{
			if (m_CurrentBuffPanel.active)
			{
				m_CurrentBuffPanel.active = false;
			}
			else
			{
				foreach (var item in m_BuffPanelMap)
				{
					item.Value.active = false;
				}

				m_CurrentBuffPanel.active = true;
			}

			m_CurrentBuffPanel = null;
		}
	}

	public void Initialize()
	{
		if (m_BuffUIPool == null)
		{
			GameObject buffUIPoolParent = new GameObject("BuffUI_Pool");
			buffUIPoolParent.transform.SetParent(transform);

			m_BuffUIPool = new ObjectPool<BuffUI>(m_BuffUIOrigin, 100, buffUIPoolParent.transform);
		}

		if (m_BuffPanelMap == null)
		{
			m_BuffPanelMap = new Dictionary<string, BuffPanel>();
			foreach (var item in m_BuffPanelList)
			{
				m_BuffPanelMap.Add(item.name, item);
			}
		}

		if (m_BuffInventoryMap == null)
			m_BuffInventoryMap = new Dictionary<int, BuffUI>();
		else
			m_BuffInventoryMap.Clear();
	}

	public void AddBuff_Inventory(BuffUIData buffUIData)
	{
		if (m_BuffInventoryMap.TryGetValue(buffUIData.code, out BuffUI buffUI) == true)
		{
			if (buffUI.buffCount < buffUIData.maxStack)
				++buffUI.buffCount;

			return;
		}

		buffUI = m_BuffUIPool.Spawn();
		buffUI.Initialize(buffUIData);
		buffUI.name = "BuffUI (" + buffUIData.title + ")";
		buffUI.transform.SetParent(inventoryPanel.content.transform);
		buffUI.transform.localPosition = Vector3.zero;
		buffUI.transform.localScale = Vector3.one * 0.75f;

		buffUI.gameObject.SetActive(true);

		m_BuffInventoryMap.Add(buffUIData.code, buffUI);
	}
	public void RemoveBuff_Inventory(BuffUIData buffUIData)
	{
		if (m_BuffInventoryMap.TryGetValue(buffUIData.code, out BuffUI buffUI) == false)
			return;

		if (buffUI.buffCount > 0)
			--buffUI.buffCount;

		if (buffUI.buffCount == 0)
		{
			m_BuffUIPool.Despawn(buffUI);

			m_BuffInventoryMap.Remove(buffUIData.code);
		}
	}

	public void RerollBuffRewards()
	{
		int childCount = rewardsPanel.content.transform.childCount;
		int offset = 0;
		for (int i = 0; i < childCount; ++i)
		{
			BuffUI buffUI = rewardsPanel.content.transform.GetChild(offset).GetComponent<BuffUI>();

			buffUI.onClick.RemoveAllListeners();

			if (buffUI == null ||
				m_BuffUIPool.Despawn(buffUI) == false)
				++offset;
		}

		int count = M_Buff.rewardsCount;
		List<BuffUIData> buffUIDataList = M_Buff.GetRandomBuffUIData(E_BuffType.Buff, count);
		foreach (var buffUIData in buffUIDataList)
		{
			BuffUI buffUI = m_BuffUIPool.Spawn();
			buffUI.Initialize(buffUIData);
			buffUI.transform.SetParent(rewardsPanel.content.transform);
			buffUI.name = "BuffUI (" + buffUIData.title + ")";
			buffUI.transform.localScale = Vector3.one;
			buffUI.onClick.AddListener(() =>
			{
				m_BuffPanelMap["Buff Rewards"].active = false;
				AddBuff_Inventory(buffUIData);
				M_Player.AddBuff(buffUIData.code);
			});

			buffUI.gameObject.SetActive(true);
		}
	}
	public void OnBuffInventoryEnabled()
	{
		combinePanel.scrollRect.transform.SetParent(inventoryPanel.panel.transform);

		RectTransform rectTransform = inventoryPanel.scrollRect.GetComponent<RectTransform>();
		rectTransform.offsetMin = Vector2.zero;
		rectTransform.offsetMax = new Vector2(rectTransform.offsetMax.x, 0f);

		inventoryPanel.scrollRect.verticalScrollbar.value = 0f;
	}
	public void OnBuffCombineEnabled()
	{
		inventoryPanel.scrollRect.transform.SetParent(combinePanel.panel.transform);

		RectTransform rectTransform = combinePanel.scrollRect.GetComponent<RectTransform>();
		rectTransform.offsetMin = Vector2.zero;
		rectTransform.offsetMax = new Vector2(rectTransform.offsetMax.x, -450f);

		combinePanel.scrollRect.verticalScrollbar.value = 0f;

		int childCount = combinePanel.content.transform.childCount;
	}

	[System.Serializable]
	private class BuffPanel
	{
		public string name;
		public KeyCode keyCode;
		public GameObject panel;
		public ScrollRect scrollRect;
		public LayoutGroup content;

		[Space(10)]
		public UnityEvent onEnabled;
		public UnityEvent onDisabled;

		public bool active
		{
			get
			{
				return panel.activeSelf;
			}
			set
			{
				if (value == panel.activeSelf)
					return;

				if (value)
					onEnabled?.Invoke();
				else
					onDisabled?.Invoke();

				panel.SetActive(value);
			}
		}
	}
}