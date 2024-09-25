using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BuffDebuff.Enum;
using AYellowpaper.SerializedCollections;

namespace BuffDebuff
{
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
		private SortedList<int, BuffUI> m_BuffUIMap = null;

		// Buff Combine
		[Space(10)]
		[SerializeField]
		private BuffCombineUIPanel m_BuffCombinePanel = null;

		// Panels
		private BuffPanel m_CurrentBuffPanel = null;

		[Space(10)]
		[SerializeField]
		[SerializedDictionary("Key", "Buff Panel")]
		private SerializedDictionary<string, BuffPanel> m_BuffPanelMap = null;
		#endregion

		#region 프로퍼티
		public bool isUIOpened
		{
			get
			{
				foreach (var item in m_BuffPanelMap)
				{
					if (item.Value.active)
						return true;
				}

				return false;
			}
		}
		public BuffPanel rewardsPanel => m_BuffPanelMap["Buff Rewards"];
		public BuffPanel inventoryPanel => m_BuffPanelMap["Buff Inventory"];
		public BuffPanel combinePanel => m_BuffPanelMap["Buff Combine"];
		#endregion

		#region 매니저
		private static BuffManager M_Buff => BuffManager.Instance;
		private static BuffInventory M_BuffInventory => BuffInventory.Instance;
		private static RoomManager M_Room => RoomManager.Instance;
		#endregion

		public override void Initialize()
		{
			base.Initialize();

			if (m_BuffUIMap == null)
				m_BuffUIMap = new SortedList<int, BuffUI>();

			m_BuffCombinePanel.Initialize();
		}
		public override void Finallize()
		{
			base.Finallize();

			m_BuffCombinePanel.Finallize();
		}

		public override void InitializeGame()
		{
			Initialize();

			base.InitializeGame();

			m_BuffCombinePanel.InitializeGame();

			rewardsPanel.onEnabled += RerollBuffRewards;

			combinePanel.onEnabled += OnCombinePanelEnabled;
			combinePanel.onDisabled += OnCombinePanelDisabled;
		}
		public override void FinallizeGame()
		{
			base.FinallizeGame();

			if (m_BuffUIMap != null)
			{
				foreach (var item in m_BuffUIMap)
				{
					Despawn(item.Value);
				}
				m_BuffUIMap.Clear();
			}

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
			foreach (var item in m_BuffPanelMap)
			{
				buffPanel = item.Value;

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
					foreach (var item in m_BuffPanelMap)
					{
						buffPanel = item.Value;

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
			int childCount = rewardsPanel.content.childCount;
			int offset = 0;

			for (int i = 0; i < childCount; ++i)
			{
				BuffUI buffUI = rewardsPanel.content.GetChild<BuffUI>(offset);

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
					.SetParent(rewardsPanel.content)
					.SetLocalPosition(Vector3.zero)
					.SetScale(Vector3.one)
					.Spawn();

				buffUI.SetBuffData(buffData);
				buffUI.SetType(BuffUI.E_Type.BuffRewards);
			}
		}

		public void AddBuff(BuffData buffData)
		{
			BuffUI buffUI;
			if (M_BuffInventory.HasBuff(buffData) == true)
			{
				buffUI = m_BuffUIMap[buffData.code];

				++buffUI.buffCount;

				return;
			}

			buffUI = GetBuilder("Buff Inventory")
				.SetName(buffData.title)
				.SetActive(true)
				.SetAutoInit(true)
				.SetParent(inventoryPanel.content)
				.SetLocalPosition(Vector3.zero)
				.SetScale(Vector3.one * 0.75f)
				.Spawn();

			buffUI.SetBuffData(buffData);
			if (combinePanel.active == true)
				buffUI.SetType(BuffUI.E_Type.BuffCombineInventory);
			else
				buffUI.SetType(BuffUI.E_Type.BuffInventory);

			M_BuffInventory.AddBuff(buffData);
			m_BuffUIMap.Add(buffData.code, buffUI);

			SortBuffInventory();
		}
		public bool AddCombineBuff(BuffData buffData)
		{
			if (m_BuffCombinePanel.AddBuff(buffData) == false)
				return false;

			return true;
		}
		public void RemoveBuff(BuffData buffData)
		{
			if (m_BuffUIMap.TryGetValue(buffData.code, out BuffUI buffUI) == false)
				return;

			--buffUI.buffCount;

			if (buffUI.buffCount == 0)
			{
				Despawn(buffUI);

				M_BuffInventory.RemoveBuff(buffData);
				m_BuffUIMap.Remove(buffData.code);
			}
		}
		public bool RemoveCombineBuff(BuffData buffData)
		{
			if (m_BuffCombinePanel.RemoveBuff(buffData) == false)
				return false;

			SortBuffInventory();

			return true;
		}

		private void SortBuffInventory()
		{
			inventoryPanel.content.DetachChildren();

			BuffUI buffUI;
			int count = m_BuffUIMap.Count;
			for (int i = 0; i < count; ++i)
			{
				buffUI = m_BuffUIMap.Values[i];
				buffUI.transform.SetParent(inventoryPanel.content);
				buffUI.transform.localPosition = Vector3.zero;
			}
		}

		#region 이벤트 함수
		private void OnRoomCleared(Room room)
		{
			rewardsPanel.active = true;
		}
		private void OnCombinePanelEnabled()
		{
			RectTransform inventory = combinePanel.scrollRect.GetComponent<RectTransform>();

			inventory.SetParent(combinePanel.panel.transform);
			inventory.localPosition += new Vector3(0f, -165f, 0f);
			inventory.sizeDelta = new Vector2(1520f, 600f);

			foreach (var item in m_BuffUIMap)
			{
				item.Value.SetType(BuffUI.E_Type.BuffCombineInventory);
			}
		}
		private void OnCombinePanelDisabled()
		{
			RectTransform inventory = combinePanel.scrollRect.GetComponent<RectTransform>();

			inventory.SetParent(inventoryPanel.panel.transform);
			inventory.localPosition = Vector3.zero;
			inventory.sizeDelta = new Vector2(1520f, 800f);

			foreach (var item in m_BuffUIMap)
			{
				item.Value.SetType(BuffUI.E_Type.BuffInventory);
			}
		}
		#endregion

		#region UNITY_EDITOR
#if UNITY_EDITOR
		[ContextMenu("Load Origin")]
		protected override void LoadOrigin()
		{
			base.LoadOrigin_Inner();
		}
#endif
		#endregion

		[System.Serializable]
		public class BuffPanel
		{
			public GameObject panel = null;
			public KeyCode keyCode;
			public ScrollRect scrollRect = null;

			public RectTransform content => scrollRect.content;

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
}