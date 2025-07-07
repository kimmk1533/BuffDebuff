using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace BuffDebuff
{
	public class BuffInventory : SerializedMonoBehaviour
	{
		#region 기본 템플릿
		#region 변수
		[OdinSerialize, InlineProperty, HideLabel, FoldoutGroup("Inventory"), ReadOnly]
		protected DoubleKeyDictionary<int, string, Buff> m_BuffInventory = null;

		protected Dictionary<int, System.Action<Buff>> m_BuffAddedEventMap = null;
		protected Dictionary<int, System.Action<Buff>> m_BuffRemovedEventMap = null;
		#endregion

		#region 프로퍼티
		#region 인덱서
		public Buff this[int code]
		{
			get => m_BuffInventory[code];
			set => m_BuffInventory[code] = value;
		}
		public Buff this[string title]
		{
			get => m_BuffInventory[title];
			set => m_BuffInventory[title] = value;
		}
		#endregion
		#endregion

		#region 이벤트
		public event System.Action<Buff> onBuffAdded = null;
		public event System.Action<Buff> onBuffRemoved = null;
		#endregion

		#region 매니저
		private static BuffManager M_Buff => BuffManager.Instance;
		#endregion

		#region 초기화 & 마무리화 함수
		/// <summary>
		/// 초기화 함수
		/// </summary>
		public void Initialize()
		{
			// Buff Inventory Init
			if (m_BuffInventory == null)
				m_BuffInventory = new DoubleKeyDictionary<int, string, Buff>();

			if (m_BuffAddedEventMap == null)
				m_BuffAddedEventMap = new Dictionary<int, System.Action<Buff>>();
			if (m_BuffRemovedEventMap == null)
				m_BuffRemovedEventMap = new Dictionary<int, System.Action<Buff>>();

			onBuffAdded += OnBuffAdded;
			onBuffRemoved += OnBuffRemoved;
		}
		/// <summary>
		/// 마무리화 함수
		/// </summary>
		public void Finallize()
		{
			if (m_BuffAddedEventMap != null)
				m_BuffAddedEventMap.Clear();
			if (m_BuffRemovedEventMap != null)
				m_BuffRemovedEventMap.Clear();

			if (m_BuffInventory != null)
				m_BuffInventory.Clear();

			onBuffAdded = null;
			onBuffRemoved = null;
		}
		#endregion

		#region 유니티 콜백 함수
		#endregion
		#endregion

		#region 이벤트 함수
		private void OnBuffAdded(Buff buff)
		{
			BuffData buffData = buff.buffData;

			if (m_BuffAddedEventMap.TryGetValue(buffData.code, out System.Action<Buff> action) &&
				action != null)
				action.Invoke(buff);
		}
		private void OnBuffRemoved(Buff buff)
		{
			BuffData buffData = buff.buffData;

			if (m_BuffRemovedEventMap.TryGetValue(buffData.code, out System.Action<Buff> action) &&
				action != null)
				action.Invoke(buff);
		}
		#endregion

		// Buff Func
		public bool AddBuff(string title)
		{
			if (string.IsNullOrEmpty(title) == true)
				return false;

			BuffData buffData = M_Buff.GetBuffData(title);

			return this.AddBuff(buffData);
		}
		public bool AddBuff(BuffData buffData)
		{
			if (buffData == null)
				return false;

			if (m_BuffInventory.TryGetValue(buffData.code, out Buff buff) &&
				buff != null)
			{
				if (buff.count < buffData.maxStack)
				{
					++buff.count;
					onBuffAdded?.Invoke(buff);
				}
				else
				{
					Debug.Log("Buff Count is Max. title =" + buffData.title + ", maxStack = " + buffData.maxStack.ToString());

					return false;
				}

				return true;
			}

			buff = new Buff(buffData);

			m_BuffInventory.Add(buffData.code, buffData.title, buff);

			onBuffAdded?.Invoke(buff);

			return true;
		}
		public bool RemoveBuff(string title)
		{
			if (title == null || title == string.Empty)
				return false;

			BuffData buffData = M_Buff.GetBuffData(title);

			return this.RemoveBuff(buffData);
		}
		public bool RemoveBuff(BuffData buffData)
		{
			if (buffData == null)
				return false;

			if (m_BuffInventory.TryGetValue(buffData.code, out Buff buff) &&
				buff != null)
			{
				if (buff.count > 0)
				{
					--buff.count;
				}
				else
				{
					m_BuffInventory.Remove(buffData.title);
				}

				onBuffRemoved?.Invoke(buff);

				return true;
			}

			Debug.Log("버프 없는데 제거");

			return false;
		}

		#region 버프 이벤트 추가, 제거 함수
		public bool AddOnBuffAddedEvent(string title, System.Action<Buff> newAction)
		{
			if (string.IsNullOrEmpty(title) == true)
				return false;
			if (newAction == null)
				return false;

			BuffData buffData = M_Buff.GetBuffData(title);

			return AddOnBuffAddedEvent(buffData, newAction);
		}
		public bool AddOnBuffAddedEvent(BuffData buffData, System.Action<Buff> newAction)
		{
			if (buffData == null)
				return false;

			if (m_BuffAddedEventMap.TryGetValue(buffData.code, out System.Action<Buff> action) == true)
			{
				action += newAction;

				return true;
			}

			m_BuffAddedEventMap.TryAdd(buffData.code, newAction);

			return true;
		}
		public bool AddOnBuffRemovedEvent(string title, System.Action<Buff> newAction)
		{
			if (string.IsNullOrEmpty(title) == true)
				return false;
			if (newAction == null)
				return false;

			BuffData buffData = M_Buff.GetBuffData(title);

			return AddOnBuffRemovedEvent(buffData, newAction);
		}
		public bool AddOnBuffRemovedEvent(BuffData buffData, System.Action<Buff> newAction)
		{
			if (buffData == null)
				return false;

			if (m_BuffRemovedEventMap.TryGetValue(buffData.code, out System.Action<Buff> action) == true)
			{
				action += newAction;

				return true;
			}

			m_BuffAddedEventMap.TryAdd(buffData.code, newAction);

			return true;
		}

		public bool RemoveOnBuffAddedEvent(string title, System.Action<Buff> newAction)
		{
			if (string.IsNullOrEmpty(title) == true)
				return false;
			if (newAction == null)
				return false;

			BuffData buffData = M_Buff.GetBuffData(title);

			return RemoveOnBuffAddedEvent(buffData, newAction);
		}
		public bool RemoveOnBuffAddedEvent(BuffData buffData, System.Action<Buff> newAction)
		{
			if (buffData == null)
				return false;

			if (m_BuffAddedEventMap.TryGetValue(buffData.code, out System.Action<Buff> action))
			{
				action -= newAction;

				return true;
			}

			return false;
		}
		public bool RemoveOnBuffRemovedEvent(string title, System.Action<Buff> newAction)
		{
			if (string.IsNullOrEmpty(title) == true)
				return false;
			if (newAction == null)
				return false;

			BuffData buffData = M_Buff.GetBuffData(title);

			return RemoveOnBuffRemovedEvent(buffData, newAction);
		}
		public bool RemoveOnBuffRemovedEvent(BuffData buffData, System.Action<Buff> newAction)
		{
			if (buffData == null)
				return false;

			if (m_BuffRemovedEventMap.TryGetValue(buffData.code, out System.Action<Buff> action) == true)
			{
				action -= newAction;

				return true;
			}

			return false;
		}
		#endregion

		public bool HasBuff(int code) => m_BuffInventory.ContainsKey1(code);
		public bool HasBuff(string buffName) => m_BuffInventory.ContainsKey2(buffName);
		public bool HasBuff(BuffData buffData) => m_BuffInventory.ContainsKey1(buffData.code);

		public bool TryGetBuff(int code, out Buff buff) => m_BuffInventory.TryGetValue(code, out buff);
		public bool TryGetBuff(string buffName, out Buff buff) => m_BuffInventory.TryGetValue(buffName, out buff);
	}
}