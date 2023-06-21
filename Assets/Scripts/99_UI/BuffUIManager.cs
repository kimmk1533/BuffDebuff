using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BuffUIManager : ObjectManager<BuffUIManager, BuffUI>
{
	// Buff Combine
	[Space(10)]
	private BuffUI m_FirstCombineBuff;
	[SerializeField]
	private CombineBuffUIPanel m_FirstCombineBuffUIPanel;
	private BuffUI m_SecondCombineBuff;
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

	public override void Initialize()
	{
		base.Initialize();

		foreach (var originInfo in m_Origins)
		{
			AddPool(originInfo, transform, false);

			var pool = GetPool(originInfo.key);
			switch (originInfo.key)
			{
				case "Buff Rewards":
					pool.onInstantiated += (BuffUI buffUI) =>
					{
						buffUI.onClick += () =>
						{
							// 나중에 최대 갯수인 버프는 안나오게 BuffManager 수정해야함.
							rewardsPanel.active = false;
							AddBuff_Inventory(buffUI.buffUIData);
							AddBuff_CombineInventory(buffUI.buffUIData);
							M_Player.AddBuff(buffUI.buffUIData.code);
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
							if (AddBuff_Combine(buffUI.buffUIData))
							{
								RemoveBuff_CombineInventory(buffUI.buffUIData);
							}
						};
					};
					break;
				case "Buff Combine":
					pool.onInstantiated += (BuffUI buffUI) =>
					{
						buffUI.onClick += () =>
						{
							AddBuff_CombineInventory(buffUI.buffUIData);
							RemoveBuff_Combine(buffUI.buffUIData);
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

		foreach (var item in m_BuffPanelList)
		{
			item.panel.SetActive(true);
			item.panel.SetActive(false);
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

		int count = M_Buff.rewardsCount;
		List<BuffUIData> buffUIDataList = M_Buff.GetRandomBuffUIData(E_BuffType.Buff, count);
		foreach (var buffUIData in buffUIDataList)
		{
			BuffUI buffUI = Spawn("Buff Rewards");
			buffUI.Initialize(buffUIData);
			buffUI.transform.SetParent(rewardsPanel.content.transform);
			buffUI.name = buffUIData.title;
			buffUI.transform.localScale = Vector3.one;

			buffUI.gameObject.SetActive(true);
		}
	}
	public void AddBuff_Inventory(BuffUIData buffUIData)
	{
		if (m_BuffInventoryMap.TryGetValue(buffUIData.code, out BuffUI buffUI) == true)
		{
			if (buffUI.buffCount < buffUIData.maxStack)
				++buffUI.buffCount;

			return;
		}

		buffUI = Spawn("Buff Inventory");
		buffUI.Initialize(buffUIData);
		buffUI.name = buffUIData.title;
		buffUI.transform.SetParent(inventoryPanel.content.transform);
		buffUI.transform.localPosition = Vector3.zero;
		buffUI.transform.localScale = Vector3.one * 0.75f;
		buffUI.gameObject.SetActive(true);

		m_BuffInventoryList.Add(buffUI);
		m_BuffInventoryMap.Add(buffUIData.code, buffUI);

		m_BuffInventoryList = m_BuffInventoryList
			.OrderBy((BuffUI buff) =>
			{
				return buff.buffUIData.code % 1000;
			})
			.ToList();

		inventoryPanel.content.transform.DetachChildren();
		int count = m_BuffInventoryList.Count;
		for (int i = 0; i < count; ++i)
		{
			buffUI = m_BuffInventoryList[i];
			buffUI.transform.SetParent(inventoryPanel.content.transform);
			buffUI.transform.localPosition = Vector3.zero;
		}
	}
	public void AddBuff_CombineInventory(BuffUIData buffUIData)
	{
		if (m_BuffCombineInventoryMap.TryGetValue(buffUIData.code, out BuffUI buffUI) == true)
		{
			if (buffUI.buffCount < buffUIData.maxStack)
				++buffUI.buffCount;

			return;
		}

		buffUI = Spawn("Buff Combine Inventory");
		buffUI.Initialize(buffUIData);
		buffUI.name = buffUIData.title;
		buffUI.transform.SetParent(combineInventoryPanel.content.transform);
		buffUI.transform.localPosition = Vector3.zero;
		buffUI.transform.localScale = Vector3.one * 0.75f;
		buffUI.gameObject.SetActive(true);

		m_BuffCombineInventoryList.Add(buffUI);
		m_BuffCombineInventoryMap.Add(buffUIData.code, buffUI);

		m_BuffCombineInventoryList = m_BuffCombineInventoryList
			.OrderBy((BuffUI buff) =>
			{
				return buff.buffUIData.code % 1000;
			})
			.ToList();

		combineInventoryPanel.content.transform.DetachChildren();
		int count = m_BuffCombineInventoryList.Count;
		for (int i = 0; i < count; ++i)
		{
			buffUI = m_BuffCombineInventoryList[i];
			buffUI.transform.SetParent(combineInventoryPanel.content.transform);
			buffUI.transform.localPosition = Vector3.zero;
		}
	}
	public bool AddBuff_Combine(BuffUIData buffUIData)
	{
		if (m_FirstCombineBuffUIPanel.SetBuffUIData(buffUIData) == true)
		{
			//m_FirstCombineBuff = buffUI;
			return true;
		}

		if (m_SecondCombineBuffUIPanel.SetBuffUIData(buffUIData) == true)
		{
			//m_SecondCombineBuff = buffUI;
			return true;
		}

		return false;
	}
	public void RemoveBuff_Inventory(BuffUIData buffUIData)
	{
		if (m_BuffInventoryMap.TryGetValue(buffUIData.code, out BuffUI buffUI) == false)
			return;

		if (--buffUI.buffCount == 0)
		{
			Despawn("Buff Inventory", buffUI);

			m_BuffInventoryList.Remove(buffUI);
			m_BuffInventoryMap.Remove(buffUIData.code);
		}
	}
	public void RemoveBuff_CombineInventory(BuffUIData buffUIData)
	{
		if (m_BuffCombineInventoryMap.TryGetValue(buffUIData.code, out BuffUI buffUI) == false)
			return;

		if (--buffUI.buffCount == 0)
		{
			Despawn("Buff Combine Inventory", buffUI);

			m_BuffCombineInventoryList.Remove(buffUI);
			m_BuffCombineInventoryMap.Remove(buffUIData.code);
		}
	}
	public void RemoveBuff_Combine(BuffUIData buffUIData)
	{
		if (m_FirstCombineBuffUIPanel.RemoveBuffUIData(buffUIData) == false)
			m_SecondCombineBuffUIPanel.RemoveBuffUIData(buffUIData);

		//if (m_FirstCombineBuff == buffUI)
		//{
		//	m_FirstCombineBuffUIPanel.RemoveBuffUIData(buffUI.buffUIData);
		//	m_FirstCombineBuff = null;
		//	return;
		//}

		//if (m_SecondCombineBuff == buffUI)
		//{
		//	m_SecondCombineBuffUIPanel.RemoveBuffUIData(buffUI.buffUIData);
		//	m_SecondCombineBuff = null;
		//	return;
		//}
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