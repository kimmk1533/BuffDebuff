using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using BuffDebuff.Enum;

namespace BuffDebuff
{
	public class BuffUIManager : ObjectManager<BuffUIManager, BuffUI>
	{
		#region 기본 템플릿
		#region 변수
		// Canvas
		private UICanvas m_GameCanvas = null;

		// Buff Rewards
		private int m_RewardsCount = 3;

		// Buff Inventory
		private SortedList<int, BuffUI> m_BuffUIMap = new SortedList<int, BuffUI>();

		// Buff Combine
		private BuffCombineUIPanel m_BuffCombinePanel = null;

		// Panels
		private BuffPanel m_CurrentBuffPanel = null;
		private Dictionary<string, BuffPanel> m_BuffPanelMap = new Dictionary<string, BuffPanel>();
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

		public UICanvas gameCanvas
		{
			get => m_GameCanvas;
			set => m_GameCanvas = value;
		}
		public BuffCombineUIPanel buffCombineUIPanel
		{
			get => m_BuffCombinePanel;
			set => m_BuffCombinePanel = value;
		}

		public BuffPanel rewardsPanel => m_BuffPanelMap["Buff Rewards"];
		public BuffPanel inventoryPanel => m_BuffPanelMap["Buff Inventory"];
		public BuffPanel combinePanel => m_BuffPanelMap["Buff Combine"];

		protected BuffInventory buffInventory => M_Buff.inventory;
		#endregion

		#region 이벤트

		#region 이벤트 함수
		private void OnRoomCleared(Room room)
		{
			rewardsPanel.active = true;
		}
		private void OnCombinePanelEnabled()
		{
			RectTransform combineInventory = combinePanel.scrollRect.transform as RectTransform;

			combineInventory.SetParent(combinePanel.panel.transform);
			combineInventory.localPosition += new Vector3(0f, -165f, 0f);
			combineInventory.sizeDelta = new Vector2(1520f, 600f);

			foreach (var item in m_BuffUIMap)
			{
				item.Value.SetState(BuffUI.E_BuffUIState.BuffCombineInventory);
			}
		}
		private void OnCombinePanelDisabled()
		{
			RectTransform combineInventory = combinePanel.scrollRect.transform as RectTransform;

			combineInventory.SetParent(inventoryPanel.panel.transform);
			combineInventory.localPosition = Vector3.zero;
			combineInventory.sizeDelta = new Vector2(1520f, 800f);

			foreach (var item in m_BuffUIMap)
			{
				item.Value.SetState(BuffUI.E_BuffUIState.BuffInventory);
			}
		}
		#endregion
		#endregion

		#region 매니저
		private static BuffManager M_Buff => BuffManager.Instance;
		private static RoomManager M_Room => RoomManager.Instance;
		#endregion

		#region 유니티 콜백 함수
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
		#endregion

		#region 초기화 & 마무리화 함수
		/// <summary>
		/// 기본 초기화 함수 (Init Scene 진입 시, 즉 게임 실행 시 호출)
		/// </summary>
		public override void Initialize()
		{
			base.Initialize();

			M_Room.onRoomClear += OnRoomCleared;
		}
		/// <summary>
		/// 기본 마무리화 함수 (게임 종료 시 호출)
		/// </summary>
		public override void Finallize()
		{
			base.Finallize();

			M_Room.onRoomClear -= OnRoomCleared;
		}

		/// <summary>
		/// 메인 초기화 함수 (본인 Main Scene 진입 시 호출)
		/// </summary>
		public override void InitializeMain()
		{
			base.InitializeMain();

			int index = 0;
			foreach (var item in m_Origins)
			{
				Transform panel = m_GameCanvas.transform.Find(item.key + " Panel");

				m_BuffPanelMap.Add(item.key, new BuffPanel()
				{
					panel = panel.gameObject,
					keyCode = KeyCode.Alpha1 + (index++),
					scrollRect = panel.GetComponentInChildren<ScrollRect>()
				});
			}
			combinePanel.scrollRect = inventoryPanel.scrollRect;

			m_BuffCombinePanel.Initialize();

			rewardsPanel.onEnabled += RerollBuffRewards;

			combinePanel.onEnabled += OnCombinePanelEnabled;
			combinePanel.onDisabled += OnCombinePanelDisabled;
		}
		/// <summary>
		/// 메인 마무리화 함수 (본인 Main Scene 나갈 시 호출)
		/// </summary>
		public override void FinallizeMain()
		{
			base.FinallizeMain();

			rewardsPanel.onEnabled -= RerollBuffRewards;

			combinePanel.onEnabled -= OnCombinePanelEnabled;
			combinePanel.onDisabled -= OnCombinePanelDisabled;

			if (m_BuffPanelMap != null)
			{
				m_BuffPanelMap.Clear();
			}

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
		#endregion
		#endregion

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
				buffUI.SetState(BuffUI.E_BuffUIState.BuffRewards);
			}
		}

		public void AddBuff(BuffData buffData)
		{
			BuffUI buffUI;
			if (M_Buff.inventory.HasBuff(buffData) == true)
			{
				buffUI = m_BuffUIMap[buffData.code];

				if (buffUI.buffCount >= buffData.maxStack)
					return;

				M_Buff.inventory.AddBuff(buffData);
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
				buffUI.SetState(BuffUI.E_BuffUIState.BuffCombineInventory);
			else
				buffUI.SetState(BuffUI.E_BuffUIState.BuffInventory);

			M_Buff.inventory.AddBuff(buffData);
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

				M_Buff.inventory.RemoveBuff(buffData);
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

			foreach (var item in m_BuffUIMap)
			{
				BuffUI buffUI = item.Value;

				buffUI.transform.SetParent(inventoryPanel.content);
				buffUI.transform.localPosition = Vector3.zero;
			}
		}

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