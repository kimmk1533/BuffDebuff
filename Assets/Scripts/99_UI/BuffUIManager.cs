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
	// Buff Rewards
	private int m_RewardsCount = 3;

	// Buff Combine
	[Space(10)]
	[SerializeField]
	private CombineBuffUIPanel m_FirstCombineBuffUIPanel;
	[SerializeField]
	private CombineBuffUIPanel m_SecondCombineBuffUIPanel;
	[SerializeField]
	private Button m_CombineButton;

	// Panels
	private BuffPanel m_CurrentBuffPanel;
	[SerializeField]
	private List<BuffPanel> m_BuffPanelList;
	private Dictionary<string, BuffPanel> m_BuffPanelMap;

	// Buff Inventory
	private List<BuffUI> m_BuffInventoryList;
	private Dictionary<int, BuffUI> m_BuffInventoryMap;
	private List<BuffUI> m_BuffCombineInventoryList;
	private Dictionary<int, BuffUI> m_BuffCombineInventoryMap;

	// property
	private BuffPanel rewardsPanel => m_BuffPanelMap["Buff Rewards"];
	private BuffPanel inventoryPanel => m_BuffPanelMap["Buff Inventory"];
	private BuffPanel combineInventoryPanel => m_BuffPanelMap["Buff Combine"];

	// Manager
	private BuffManager M_Buff => BuffManager.Instance;
	private PlayerManager M_Player => PlayerManager.Instance;

	public override void Initialize()
	{
		base.Initialize();

		foreach (var originInfo in m_Origins)
		{
			var pool = GetPool(originInfo.key);
			switch (originInfo.key)
			{
				case "Buff Rewards":
					pool.onInstantiated += (BuffUI buffUI) =>
					{
						buffUI.onClick += () =>
						{
							M_Buff.AddBuff(buffUI.buffData);
							rewardsPanel.active = false;
						};
					};
					break;
				case "Buff Inventory":
					//	pool.onInstantiated += (BuffUI buffUI) =>
					//	{
					//		buffUI.onClick += () =>
					//		{
					//			Debug.Log("설명 추가");
					//		};
					//	};
					break;
				case "Buff Combine Inventory":
					pool.onInstantiated += (BuffUI buffUI) =>
					{
						buffUI.onClick += () =>
						{
							if (AddBuff_Combine(buffUI.buffData))
							{
								RemoveBuff_CombineInventory(buffUI.buffData);
							}
						};
					};
					break;
				case "Buff Combine":
					pool.onInstantiated += (BuffUI buffUI) =>
					{
						buffUI.onClick += () =>
						{
							AddBuff_CombineInventory(buffUI.buffData);
							RemoveBuff_Combine(buffUI.buffData);
						};
					};
					break;
				default:
					Debug.LogError("Object Manager`s Origin Info key is not exist. key = " + originInfo.key);
					break;
			}

			pool.Initialize();
		}

		m_FirstCombineBuffUIPanel.Initialize();
		m_SecondCombineBuffUIPanel.Initialize();
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

		if (m_BuffPanelMap == null)
		{
			m_BuffPanelMap = new Dictionary<string, BuffPanel>();
			foreach (var item in m_BuffPanelList)
			{
				m_BuffPanelMap.Add(item.name, item);
			}
		}

		if (m_BuffInventoryList == null)
			m_BuffInventoryList = new List<BuffUI>();
		else
			m_BuffInventoryList.Clear();
		if (m_BuffInventoryMap == null)
			m_BuffInventoryMap = new Dictionary<int, BuffUI>();
		else
			m_BuffInventoryMap.Clear();

		if (m_BuffCombineInventoryList == null)
			m_BuffCombineInventoryList = new List<BuffUI>();
		else
			m_BuffCombineInventoryList.Clear();
		if (m_BuffCombineInventoryMap == null)
			m_BuffCombineInventoryMap = new Dictionary<int, BuffUI>();
		else
			m_BuffCombineInventoryMap.Clear();
	}
	public void InitializeBuffEvent()
	{
		M_Buff.onBuffAdded += AddBuff_Inventory;
		M_Buff.onBuffAdded += AddBuff_CombineInventory;
		M_Buff.onBuffRemoved += RemoveBuff_Inventory;
		M_Buff.onBuffRemoved += RemoveBuff_CombineInventory;
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

	public void RerollBuffRewards()
	{
		int childCount = rewardsPanel.content.transform.childCount;
		int offset = 0;
		for (int i = 0; i < childCount; ++i)
		{
			BuffUI buffUI = rewardsPanel.content.transform.GetChild<BuffUI>(offset);

			if (buffUI == null ||
				Despawn("Buff Rewards", buffUI) == false)
				++offset;
		}

		int count = m_RewardsCount;

		List<BuffData> buffDataList = new List<BuffData>();
		for (int i = 0; i < count; ++i)
		{
			E_BuffGrade grade = M_Buff.GetRandomGrade(M_Player.currentLevel);
			BuffData buffData = M_Buff.GetRandomBuffData(E_BuffType.Buff, grade);

			if (buffData == null)
				continue;

			buffDataList.Add(buffData);
		}

		foreach (var buffData in buffDataList)
		{
			BuffUI buffUI = Spawn("Buff Rewards");
			buffUI.Initialize(buffData);
			buffUI.transform.SetParent(rewardsPanel.content.transform);
			buffUI.name = buffData.title;
			buffUI.transform.localScale = Vector3.one;

			buffUI.gameObject.SetActive(true);
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

		// 버프가 인벤에 안들어감. 디버깅 해야함.
		buffUI = Spawn("Buff Inventory");
		buffUI.Initialize(buffData);
		buffUI.name = buffData.title;
		buffUI.transform.SetParent(inventoryPanel.content.transform);
		buffUI.transform.localPosition = Vector3.zero;
		buffUI.transform.localScale = Vector3.one * 0.75f;
		buffUI.gameObject.SetActive(true);

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

		buffUI = Spawn("Buff Combine Inventory");
		buffUI.Initialize(buffData);
		buffUI.name = buffData.title;
		buffUI.transform.SetParent(combineInventoryPanel.content.transform);
		buffUI.transform.localPosition = Vector3.zero;
		buffUI.transform.localScale = Vector3.one * 0.75f;
		buffUI.gameObject.SetActive(true);

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
			Despawn("Buff Inventory", buffUI);

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
			Despawn("Buff Combine Inventory", buffUI);

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