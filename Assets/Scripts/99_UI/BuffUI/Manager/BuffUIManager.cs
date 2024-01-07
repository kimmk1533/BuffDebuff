using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Enum;

public class BuffUIManager : ObjectManager<BuffUIManager, BuffUI>
{
	#region 변수
	// Panels
	[Space(10)]
	private BuffPanel m_CurrentBuffPanel = null;
	[SerializeField]
	private List<BuffPanel> m_BuffPanelList = null;
	private Dictionary<string, BuffPanel> m_BuffPanelMap = null;

	// Buff Rewards
	private int m_RewardsCount = 3;

	// Buff Combine
	[Space(10)]
	[SerializeField]
	private CombineBuffUIPanel m_FirstCombineBuffUIPanel = null;
	[SerializeField]
	private CombineBuffUIPanel m_SecondCombineBuffUIPanel = null;
	[SerializeField]
	private Button m_CombineButton = null;

	// Buff Inventory
	private List<BuffUI> m_BuffInventoryList = null;
	private Dictionary<int, BuffUI> m_BuffInventoryMap = null;
	private List<BuffUI> m_BuffCombineInventoryList = null;
	private Dictionary<int, BuffUI> m_BuffCombineInventoryMap = null;
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
	private BuffPanel combineInventoryPanel => m_BuffPanelMap["Buff Combine"];
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

		if (m_BuffInventoryList == null)
			m_BuffInventoryList = new List<BuffUI>();

		if (m_BuffInventoryMap == null)
			m_BuffInventoryMap = new Dictionary<int, BuffUI>();

		if (m_BuffCombineInventoryList == null)
			m_BuffCombineInventoryList = new List<BuffUI>();

		if (m_BuffCombineInventoryMap == null)
			m_BuffCombineInventoryMap = new Dictionary<int, BuffUI>();
	}
	public override void Finallize()
	{
		base.Finallize();

		if (m_BuffPanelMap != null)
			m_BuffPanelMap.Clear();

		if (m_BuffInventoryList != null)
			m_BuffInventoryList.Clear();

		if (m_BuffInventoryMap != null)
			m_BuffInventoryMap.Clear();

		if (m_BuffCombineInventoryList != null)
			m_BuffCombineInventoryList.Clear();

		if (m_BuffCombineInventoryMap != null)
			m_BuffCombineInventoryMap.Clear();
	}

	public override void InitializeGame()
	{
		Initialize();

		base.InitializeGame();

		foreach (var item in m_BuffPanelList)
		{
			m_BuffPanelMap.Add(item.name, item);
		}

		m_FirstCombineBuffUIPanel.Initialize();
		m_SecondCombineBuffUIPanel.Initialize();

		m_FirstCombineBuffUIPanel.onClick += () =>
		{
			AddBuff_CombineInventory(m_FirstCombineBuffUIPanel.buffData);
			RemoveBuff_Combine(m_FirstCombineBuffUIPanel.buffData);
		};
		m_SecondCombineBuffUIPanel.onClick += () =>
		{
			AddBuff_CombineInventory(m_SecondCombineBuffUIPanel.buffData);
			RemoveBuff_Combine(m_SecondCombineBuffUIPanel.buffData);
		};

		m_CombineButton.onClick.AddListener(
			() =>
			{
				BuffData first = m_FirstCombineBuffUIPanel.buffData;
				BuffData second = m_SecondCombineBuffUIPanel.buffData;

				if (M_Buff.CombineBuff(first, second) == true)
				{
					m_FirstCombineBuffUIPanel.RemoveBuffData(first);
					m_SecondCombineBuffUIPanel.RemoveBuffData(second);
				}
			});
	}
	public override void FinallizeGame()
	{
		base.FinallizeGame();
	}

	public void InitializeBuffEvent()
	{
		M_Buff.onBuffAdded += AddBuff_Inventory;
		M_Buff.onBuffRemoved += RemoveBuff_Inventory;

		M_Buff.onBuffAdded += AddBuff_CombineInventory;
		M_Buff.onBuffRemoved += RemoveBuff_CombineInventory;
	}
	public void FinallizeBuffEvent()
	{
		M_Buff.onBuffAdded -= AddBuff_Inventory;
		M_Buff.onBuffRemoved -= RemoveBuff_Inventory;

		M_Buff.onBuffAdded -= AddBuff_CombineInventory;
		M_Buff.onBuffRemoved -= RemoveBuff_CombineInventory;
	}

	public void InitializeRoomClearEvent()
	{
		M_Room.onRoomClear += TurnOnBuffRewardsPanel;
	}
	public void FinallizeRoomClearEvent()
	{
		M_Room.onRoomClear -= TurnOnBuffRewardsPanel;
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

	private void TurnOnBuffRewardsPanel(Room room)
	{
		RerollBuffRewards();

		rewardsPanel.active = true;
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
		BuffData buffData;
		for (int i = 0; i < count; ++i)
		{
			InfiniteLoopDetector.Run();

			buffData = M_Buff.GetRandomBuffData(E_BuffType.Buff);

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
			buffData = buffDataList[i];

			BuffUI buffUI = GetBuilder("Buff Rewards")
				.SetName(buffData.title)
				.SetActive(true)
				.SetAutoInit(true)
				.SetParent(rewardsPanel.content.transform)
				.Spawn();

			buffUI.UpdateBuffUIData(buffData);
			buffUI.transform.localScale = Vector3.one;
			buffUI.transform.localPosition = Vector3.zero;

			buffUI.onClick += () =>
			{
				M_Buff.AddBuff(buffUI.buffData);
				rewardsPanel.active = false;
			};
		}
	}
	private bool AddBuff_Inventory(BuffData buffData)
	{
		if (m_BuffInventoryMap.TryGetValue(buffData.code, out BuffUI buffUI) == true)
		{
			if (buffUI.buffCount < buffData.maxStack)
				++buffUI.buffCount;
			else
				return false;

			return true;
		}

		buffUI = GetBuilder("Buff Inventory")
			.SetName(buffData.title)
			.SetActive(true)
			.SetAutoInit(true)
			.SetParent(inventoryPanel.content.transform)
			.Spawn();

		buffUI.UpdateBuffUIData(buffData);
		buffUI.transform.localPosition = Vector3.zero;
		buffUI.transform.localScale = Vector3.one * 0.75f;

		buffUI.onClick += () =>
		{
			Debug.Log("설명 추가");
		};

		m_BuffInventoryList.Add(buffUI);
		m_BuffInventoryMap.Add(buffData.code, buffUI);

		SortBuffList(ref m_BuffInventoryList, inventoryPanel);

		return true;
	}
	private bool AddBuff_CombineInventory(BuffData buffData)
	{
		if (m_BuffCombineInventoryMap.TryGetValue(buffData.code, out BuffUI buffUI) == true)
		{
			if (buffUI.buffCount < buffData.maxStack)
				++buffUI.buffCount;
			else
				return false;

			return true;
		}

		buffUI = GetBuilder("Buff Combine Inventory")
			.SetName(buffData.title)
			.SetActive(true)
			.SetAutoInit(true)
			.SetParent(combineInventoryPanel.content.transform)
			.Spawn();

		buffUI.UpdateBuffUIData(buffData);
		buffUI.transform.localPosition = Vector3.zero;
		buffUI.transform.localScale = Vector3.one * 0.75f;

		buffUI.onClick += () =>
		{
			if (AddBuff_Combine(buffUI.buffData))
			{
				RemoveBuff_CombineInventory(buffUI.buffData);
			}
		};

		m_BuffCombineInventoryList.Add(buffUI);
		m_BuffCombineInventoryMap.Add(buffData.code, buffUI);

		SortBuffList(ref m_BuffCombineInventoryList, combineInventoryPanel);

		return true;
	}
	private bool AddBuff_Combine(BuffData buffData)
	{
		if (m_FirstCombineBuffUIPanel.SetBuffData(buffData) == true)
			return true;

		if (m_SecondCombineBuffUIPanel.SetBuffData(buffData) == true)
			return true;

		return false;
	}
	private bool RemoveBuff_Inventory(BuffData buffData)
	{
		if (m_BuffInventoryMap.TryGetValue(buffData.code, out BuffUI buffUI) == false)
			return false;

		if (--buffUI.buffCount == 0)
		{
			Despawn(buffUI);

			m_BuffInventoryList.Remove(buffUI);
			m_BuffInventoryMap.Remove(buffData.code);
		}

		return true;
	}
	private bool RemoveBuff_CombineInventory(BuffData buffData)
	{
		if (m_BuffCombineInventoryMap.TryGetValue(buffData.code, out BuffUI buffUI) == false)
			return false;

		if (--buffUI.buffCount == 0)
		{
			Despawn(buffUI);

			m_BuffCombineInventoryList.Remove(buffUI);
			m_BuffCombineInventoryMap.Remove(buffData.code);
		}

		return true;
	}
	private bool RemoveBuff_Combine(BuffData buffData)
	{
		if (m_FirstCombineBuffUIPanel.RemoveBuffData(buffData) == false)
			if (m_SecondCombineBuffUIPanel.RemoveBuffData(buffData) == false)
				return false;

		return true;
	}

	private void SortBuffList(ref List<BuffUI> buffUIList, BuffPanel panel)
	{
		buffUIList = buffUIList
			.OrderBy((BuffUI buff) =>
			{
				return buff.buffData.code % 1000;
			})
			.ToList();

		BuffUI buffUI;
		panel.content.transform.DetachChildren();
		int count = buffUIList.Count;
		for (int i = 0; i < count; ++i)
		{
			buffUI = buffUIList[i];
			buffUI.transform.SetParent(panel.content.transform);
			buffUI.transform.localPosition = Vector3.zero;
		}
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