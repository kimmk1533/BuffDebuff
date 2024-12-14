using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class DoubleKeyDictionary<TKey1, TKey2, TValue> : IEnumerable<KeyValuePair<TKey1, TValue>>
{
	#region 변수	
	[SerializeField]
	[DictionaryDrawerSettings(KeyLabel = "key1", ValueLabel = "value")]
	private Dictionary<TKey1, TValue> m_Key1Dictionary = null;

	[SerializeField]
	[DictionaryDrawerSettings(KeyLabel = "key2", ValueLabel = "key1")]
	private Dictionary<TKey2, TKey1> m_ForwardKey2Dictionary = null;
	private Dictionary<TKey1, TKey2> m_ReverseKey2Dictionary = null;
	#endregion

	#region 프로퍼티
	public Dictionary<TKey2, TKey1>.KeyCollection Keys2 => m_ForwardKey2Dictionary.Keys;
	public Dictionary<TKey1, TValue>.KeyCollection Keys1 => m_Key1Dictionary.Keys;
	public Dictionary<TKey1, TValue>.ValueCollection Values => m_Key1Dictionary.Values;

	#region 인덱서
	public TValue this[TKey1 key1]
	{
		get
		{
			if (m_Key1Dictionary.TryGetValue(key1, out TValue value) == false)
				return default(TValue);

			return value;
		}
		set
		{
			m_Key1Dictionary[key1] = value;
		}
	}
	public TValue this[TKey2 key2]
	{
		get
		{
			if (m_ForwardKey2Dictionary.TryGetValue(key2, out TKey1 primaryKey) == false)
				return default(TValue);

			return this[primaryKey];
		}
		set
		{
			if (m_ForwardKey2Dictionary.TryGetValue(key2, out TKey1 primaryKey) == false)
				return;

			this[primaryKey] = value;
		}
	}
	#endregion
	#endregion

	public DoubleKeyDictionary()
	{
		m_Key1Dictionary = new Dictionary<TKey1, TValue>();

		m_ForwardKey2Dictionary = new Dictionary<TKey2, TKey1>();
		m_ReverseKey2Dictionary = new Dictionary<TKey1, TKey2>();
	}

	public void Add(TKey1 key1, TKey2 key2, TValue value)
	{
		m_Key1Dictionary.Add(key1, value);

		m_ForwardKey2Dictionary.Add(key2, key1);
		m_ReverseKey2Dictionary.Add(key1, key2);
	}

	public bool TryAdd(TKey1 key1, TKey2 key2, TValue value)
	{
		if (m_ForwardKey2Dictionary.TryAdd(key2, key1) == false)
			return false;
		if (m_ReverseKey2Dictionary.TryAdd(key1, key2) == false)
			return false;

		if (m_Key1Dictionary.TryAdd(key1, value) == false)
			return false;

		return true;
	}
	public bool TryGetValue(TKey1 key1, out TValue value)
	{
		return m_Key1Dictionary.TryGetValue(key1, out value);
	}
	public bool TryGetValue(TKey2 key2, out TValue value)
	{
		if (m_ForwardKey2Dictionary.TryGetValue(key2, out TKey1 primaryKey) == false)
		{
			value = default(TValue);
			return false;
		}

		return TryGetValue(primaryKey, out value);
	}

	public void Clear()
	{
		m_Key1Dictionary.Clear();

		m_ForwardKey2Dictionary.Clear();
		m_ReverseKey2Dictionary.Clear();
	}

	public bool ContainsKey1(TKey1 key1)
	{
		return m_Key1Dictionary.ContainsKey(key1);
	}
	public bool ContainsKey2(TKey2 key2)
	{
		return m_ForwardKey2Dictionary.ContainsKey(key2);
	}
	public bool ContainsValue(TValue value)
	{
		return m_Key1Dictionary.ContainsValue(value);
	}

	public void Remove(TKey1 key1)
	{
		m_ReverseKey2Dictionary.Remove(key1, out TKey2 key2);
		m_ForwardKey2Dictionary.Remove(key2);

		m_Key1Dictionary.Remove(key1);
	}
	public void Remove(TKey2 key2)
	{
		m_ForwardKey2Dictionary.Remove(key2, out TKey1 key1);
		m_ReverseKey2Dictionary.Remove(key1);

		m_Key1Dictionary.Remove(key1);
	}

	#region Enumerator
	public class Enumerator<TEKey, TEValue> : IEnumerator<KeyValuePair<TEKey, TEValue>>, IEnumerator
	{
		List<KeyValuePair<TEKey, TEValue>> elementList;
		int index = -1;

		KeyValuePair<TEKey, TEValue> IEnumerator<KeyValuePair<TEKey, TEValue>>.Current
		{
			get
			{
				try
				{
					return elementList[index];
				}
				catch (System.IndexOutOfRangeException)
				{
					throw new System.InvalidOperationException();
				}
			}
		}
		object IEnumerator.Current
		{
			get
			{
				try
				{
					return elementList[index];
				}
				catch (System.IndexOutOfRangeException)
				{
					throw new System.InvalidOperationException();
				}
			}
		}

		public Enumerator(Dictionary<TEKey, TEValue> elements)
		{
			elementList = elements.ToList();
		}
		public bool MoveNext()
		{
			if (index == elementList.Count - 1)
			{
				Reset();
				return false;
			}

			return ++index < elementList.Count;
		}
		public void Reset()
		{
			index = -1;
		}

		public void Dispose()
		{
			elementList.Clear();
		}
	}

	public IEnumerator<KeyValuePair<TKey1, TValue>> GetEnumerator()
	{
		return new Enumerator<TKey1, TValue>(m_Key1Dictionary);
	}
	IEnumerator IEnumerable.GetEnumerator()
	{
		return new Enumerator<TKey1, TValue>(m_Key1Dictionary);
	}
	#endregion
}