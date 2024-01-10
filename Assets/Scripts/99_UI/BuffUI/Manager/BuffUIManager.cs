using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Enum;

[RequireComponent(typeof(BuffInventory))]
public class BuffUIManager : ObjectManager<BuffUIManager, BuffUI>
{
	#region 변수
	// Canvas
	[Space(10)]
	[SerializeField]
	private UICanvas m_Canvas = null;

	// Buff Rewards
	private int m_RewardsCount = 3;

	// Buff Inventory
	private BuffInventory m_BuffInventory = null;
	private SortedList<int, BuffUI> m_BuffUIMap = null;

	// Buff Combine
	[Space(10)]
	[SerializeField]
	private BuffCombineUIPanel m_BuffCombinePanel = null;

	// Panels
	private BuffPanel m_CurrentBuffPanel = null;

	[Space(10)]
	[SerializeField]
	private List<BuffPanel> m_BuffPanelList = null;
	private Dictionary<string, BuffPanel> m_BuffPanelMap = null;
	#endregion

	#region 프로퍼티
	public bool isUIOpened
	{
		get
		{
			for (int i = 0; i < m_BuffPanelList.Count; ++i)
			{
				if (m_BuffPanelList[i].active)
					return true;
			}

			return false;
		}
	}
	private BuffPanel rewardsPanel => m_BuffPanelMap["Buff Rewards"];
	private BuffPanel inventoryPanel => m_BuffPanelMap["Buff Inventory"];
	private BuffPanel combinePanel => m_BuffPanelMap["Buff Combine"];
	#endregion

	#region 매니저
	private static BuffManager M_Buff => BuffManager.Instance;
	private static RoomManager M_Room => RoomManager.Instance;
	#endregion

	public override void Initialize()
	{
		base.Initialize();

		if (m_BuffPanelMap == null)
			m_BuffPanelMap = new Dictionary<string, BuffPanel>();

		this.Safe_GetComponent<BuffInventory>(ref m_BuffInventory);
		m_BuffInventory.Initialize();

		if (m_BuffUIMap == null)
			m_BuffUIMap = new SortedList<int, BuffUI>();

		m_BuffCombinePanel.Initialize();
	}
	public override void Finallize()
	{
		base.Finallize();

		if (m_BuffPanelMap != null)
			m_BuffPanelMap.Clear();

		m_BuffInventory.Finallize();

		if (m_BuffUIMap != null)
		{
			foreach (var item in m_BuffUIMap)
			{
				Despawn(item.Value);
			}
			m_BuffUIMap.Clear();
		}

		m_BuffCombinePanel.Finallize();
	}

	public override void InitializeGame()
	{
		Initialize();

		base.InitializeGame();

		foreach (var item in m_BuffPanelList)
		{
			m_BuffPanelMap.Add(item.name, item);
		}

		m_BuffCombinePanel.InitializeGame();

		rewardsPanel.onEnabled += RerollBuffRewards;

		combinePanel.onEnabled += OnCombinePanelEnabled;
		combinePanel.onDisabled += OnCombinePanelDisabled;
	}
	public override void FinallizeGame()
	{
		base.FinallizeGame();

		m_BuffCombinePanel.FinallizeGame();

		rewardsPanel.onEnabled -= RerollBuffRewards;

		combinePanel.onEnabled -= OnCombinePanelEnabled;
		combinePanel.onDisabled -= OnCombinePanelDisabled;
	}

	public void InitializeRoomClearEvent()
	{
		M_Room.onRoomClear += OnRoomCleared;
	}
	public void FinallizeRoomClearEvent()
	{
		M_Room.onRoomClear -= OnRoomCleared;
	}

	private void Update()
	{
		BuffPanel buffPanel;
		for (int i = 0; i < m_BuffPanelList.Count; ++i)
		{
			buffPanel = m_BuffPanelList[i];

			if (Input.GetKeyDown(buffPanel.keyCode))
			{
				m_CurrentBuffPanel = buffPanel;
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
				for (int i = 0; i < m_BuffPanelList.Count; ++i)
				{
					buffPanel = m_BuffPanelList[i];

					if (m_CurrentBuffPanel == buffPanel)
						buffPanel.active = true;
					else
						buffPanel.active = false;
				}
			}

			m_CurrentBuffPanel = null;
		}
	}

	private void RerollBuffRewards()
	{
		int childCount = rewardsPanel.content.transform.childCount;
		int offset = 0;

		for (int i = 0; i < childCount; ++i)
		{
			BuffUI buffUI = rewardsPanel.content.transform.GetChild<BuffUI>(offset);

			if (buffUI == null ||
				Despawn(buffUI) == false)
				++offset;
		}

		List<BuffData> buffDataList = new List<BuffData>();

		int count = m_RewardsCount;
		for (int i = 0; i < count; ++i)
		{
			InfiniteLoopDetector.Run();

			BuffData buffData = M_Buff.GetRandomBuffData(E_BuffType.Buff);

			if (buffData == null)
				throw new System.NullReferenceException("BuffData is null.");

			if (buffDataList.Contains(buffData) == true)
			{
				--i;
				continue;
			}

			buffDataList.Add(buffData);
		}

		for (int i = 0; i < buffDataList.Count; ++i)
		{
			BuffData buffData = buffDataList[i];

			BuffUI buffUI = GetBuilder("Buff Rewards")
				.SetName(buffData.title)
				.SetActive(true)
				.SetAutoInit(true)
				.SetParent(rewardsPanel.content.transform)
				.Spawn();

			buffUI.ChangeBuff(new Buff(buffData));
			buffUI.transform.localScale = Vector3.one;
			buffUI.transform.localPosition = Vector3.zero;

			buffUI.onClick += () =>
			{
				AddBuff(buffData);
				rewardsPanel.active = false;
			};
		}
	}

	public void AddBuff(BuffData buffData)
	{
		BuffUI buffUI = null;

		if (m_BuffInventory.HasBuff(buffData) == true)
		{
			buffUI = m_BuffUIMap[buffData.code];

			++buffUI.buffCount;

			buffUI.UpdateBuffUIData();

			return;
		}

		buffUI = GetBuilder("Buff Inventory")
			.SetName(buffData.title)
			.SetActive(true)
			.SetAutoInit(true)
			.SetParent(inventoryPanel.content.transform)
			.Spawn();

		buffUI.ChangeBuff(new Buff(buffData));
		buffUI.UpdateBuffUIData();

		buffUI.transform.localPosition = Vector3.zero;
		buffUI.transform.localScale = Vector3.one * 0.75f;

		buffUI.onClick += () =>
		{
			Debug.Log("설명 추가");
		};

		m_BuffInventory.AddBuff(buffData);
		m_BuffUIMap.Add(buffData.code, buffUI);

		SortBuffList(m_BuffUIMap, inventoryPanel);
	}
	public void RemoveBuff(BuffData buffData)
	{
		if (m_BuffUIMap.TryGetValue(buffData.code, out BuffUI buffUI) == false)
			return;

		--buffUI.buffCount;
		buffUI.UpdateBuffUIData();

		if (buffUI.buffCount == 0)
		{
			Despawn(buffUI);

			m_BuffUIMap.Remove(buffData.code);
		}
	}

	private void SortBuffList(SortedList<int, BuffUI> buffUIMap, BuffPanel panel)
	{
		panel.content.transform.DetachChildren();

		BuffUI buffUI;
		int count = buffUIMap.Count;
		for (int i = 0; i < count; ++i)
		{
			buffUI = buffUIMap.Values[i];
			buffUI.transform.SetParent(panel.content.transform);
			buffUI.transform.localPosition = Vector3.zero;
		}
	}

	private void OnRoomCleared(Room room)
	{
		rewardsPanel.active = true;
	}
	private void OnCombinePanelEnabled()
	{
		RectTransform inventory = combinePanel.scrollRect.GetComponent<RectTransform>();

		inventory.SetParent(combinePanel.panel.transform);
		inventory.localPosition += new Vector3(0f, -165f, 0f);
		inventory.sizeDelta = new Vector2(1420f, 600f);
	}
	private void OnCombinePanelDisabled()
	{
		RectTransform inventory = combinePanel.scrollRect.GetComponent<RectTransform>();

		inventory.SetParent(inventoryPanel.panel.transform);
		inventory.localPosition += new Vector3(0f, 165f, 0f);
		inventory.sizeDelta = new Vector2(1620f, 800f);
	}

#if UNITY_EDITOR
	[ContextMenu("Load Origin")]
	protected override void LoadOrigin()
	{
		base.LoadOrigin_Inner();
	}
#endif

	[System.Serializable]
	private class BuffPanel
	{
		public string name = null;
		public KeyCode keyCode;
		public GameObject panel = null;
		public ScrollRect scrollRect = null;
		public LayoutGroup content = null;

		public event System.Action onEnabled = null;
		public event System.Action onDisabled = null;

		public bool active
		{
			get => panel.activeSelf;
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