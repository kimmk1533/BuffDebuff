using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class DoubleKeyDictionary<TPrimaryKey, TSubKey, TValue> : IEnumerable<KeyValuePair<TPrimaryKey, TValue>>
{
	#region 변수
	private Dictionary<TSubKey, TPrimaryKey> m_SubDictionary = new Dictionary<TSubKey, TPrimaryKey>();
	private Dictionary<TPrimaryKey, TValue> m_PrimaryDictionary = new Dictionary<TPrimaryKey, TValue>();
	#endregion

	#region 프로퍼티
	public Dictionary<TSubKey, TPrimaryKey>.KeyCollection SubKeys => m_SubDictionary.Keys;
	public Dictionary<TPrimaryKey, TValue>.KeyCollection PrimaryKeys => m_PrimaryDictionary.Keys;
	public Dictionary<TPrimaryKey, TValue>.ValueCollection Values => m_PrimaryDictionary.Values;
	public int Count => m_PrimaryDictionary.Count;

	#region 인덱서
	public TValue this[TPrimaryKey primaryKey]
	{
		get
		{
			if (m_PrimaryDictionary.TryGetValue(primaryKey, out TValue value) == false)
				return default(TValue);

			return value;
		}
		set
		{
			m_PrimaryDictionary[primaryKey] = value;
		}
	}
	public TValue this[TSubKey subKey]
	{
		get
		{
			if (m_SubDictionary.TryGetValue(subKey, out TPrimaryKey primaryKey) == false)
				return default(TValue);

			return this[primaryKey];
		}
		set
		{
			if (m_SubDictionary.TryGetValue(subKey, out TPrimaryKey primaryKey) == false)
				return;

			this[primaryKey] = value;
		}
	}
	#endregion
	#endregion

	public void Add((TPrimaryKey primary, TSubKey sub) key, TValue value)
	{
		m_SubDictionary.Add(key.sub, key.primary);
		m_PrimaryDictionary.Add(key.primary, value);
	}
	public void Add(TPrimaryKey primaryKey, TSubKey subKey, TValue value)
	{
		m_SubDictionary.Add(subKey, primaryKey);
		m_PrimaryDictionary.Add(primaryKey, value);
	}

	public bool TryAdd((TPrimaryKey primary, TSubKey sub) key, TValue value)
	{
		if (m_SubDictionary.TryAdd(key.sub, key.primary) == false)
			return false;
		if (m_PrimaryDictionary.TryAdd(key.primary, value) == false)
			return false;

		return true;
	}
	public bool TryAdd(TPrimaryKey primaryKey, TSubKey subKey, TValue value)
	{
		if (m_SubDictionary.TryAdd(subKey, primaryKey) == false)
			return false;
		if (m_PrimaryDictionary.TryAdd(primaryKey, value) == false)
			return false;

		return true;
	}
	public bool TryGetValue(TPrimaryKey primaryKey, out TValue value)
	{
		return m_PrimaryDictionary.TryGetValue(primaryKey, out value);
	}
	public bool TryGetValue(TSubKey subKey, out TValue value)
	{
		if (m_SubDictionary.TryGetValue(subKey, out TPrimaryKey primaryKey) == false)
		{
			value = default(TValue);
			return false;
		}

		return TryGetValue(primaryKey, out value);
	}

	public void Clear()
	{
		m_SubDictionary.Clear();
		m_PrimaryDictionary.Clear();
	}

	public bool ContainsPrimaryKey(TPrimaryKey primaryKey)
	{
		return m_PrimaryDictionary.ContainsKey(primaryKey);
	}
	public bool ContainsSubKey(TSubKey subKey)
	{
		return m_SubDictionary.ContainsKey(subKey);
	}
	public bool ContainsValue(TValue value)
	{
		return m_PrimaryDictionary.ContainsValue(value);
	}

	public void Remove(TSubKey subKey)
	{
		m_SubDictionary.Remove(subKey, out TPrimaryKey primaryKey);
		m_PrimaryDictionary.Remove(primaryKey);
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
				catch (IndexOutOfRangeException)
				{
					throw new InvalidOperationException();
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
				catch (IndexOutOfRangeException)
				{
					throw new InvalidOperationException();
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

	public IEnumerator<KeyValuePair<TPrimaryKey, TValue>> GetEnumerator()
	{
		return new Enumerator<TPrimaryKey, TValue>(m_PrimaryDictionary);
	}
	IEnumerator IEnumerable.GetEnumerator()
	{
		return new Enumerator<TPrimaryKey, TValue>(m_PrimaryDictionary);
	}
	#endregion
}