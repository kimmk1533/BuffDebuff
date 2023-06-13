using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-97)]
public abstract class ObjectManager<Pool, Origin> : Singleton<Pool> where Pool : MonoBehaviour where Origin : MonoBehaviour
{
	[SerializeField, ReadOnly(true)]
	protected int m_PoolSize = 100;

	[SerializeField]
	protected List<OriginInfo> m_Origins = new List<OriginInfo>();
	protected Dictionary<string, ObjectPool<Origin>> m_Pools = null;

	protected virtual void Awake()
	{
		__Initialize();
	}

	public Origin Spawn(string key)
	{
		ObjectPool<Origin> pool = GetPool(key);
		if (pool == null)
		{
			throw new System.NullReferenceException("pool is null. key is " + key);
		}

		Origin item = pool.Spawn();

		return item;
	}
	public Origin Spawn(string key, Transform parent)
	{
		Origin item = Spawn(key);

		item.transform.SetParent(parent);

		return item;
	}
	public Origin Spawn(string key, Vector3 position, Quaternion rotation)
	{
		Origin item = Spawn(key);

		item.transform.position = position;
		item.transform.rotation = rotation;

		return item;
	}
	public Origin Spawn(string key, Vector3 position, Quaternion rotation, Transform parent)
	{
		Origin item = Spawn(key);

		item.transform.SetParent(parent);
		item.transform.position = position;
		item.transform.rotation = rotation;

		return item;
	}
	public bool Despawn(Origin item)
	{
		foreach (var pool in m_Pools.Values)
		{
			if (pool.DeSpawn(item))
				return true;
		}

		return false;
	}
	public bool DeSpawn(string key, Origin item)
	{
		ObjectPool<Origin> pool = GetPool(key);
		if (pool == null)
		{
			return false;
			throw new System.NullReferenceException("pool is null. key is " + key);
		}

		return pool.DeSpawn(item);
	}

	public virtual void __Initialize()
	{
		m_Pools = new Dictionary<string, ObjectPool<Origin>>();

		foreach (var item in m_Origins)
		{
			AddPool(item.key, item.item, transform);
		}
	}
	protected virtual ObjectPool<Origin> GetPool(string key)
	{
		if (key == null)
			return null;

		if (m_Pools.ContainsKey(key))
			return m_Pools[key];

		return null;
	}

	private void AddPool(string key, Origin origin, Transform parent)
	{
		origin.name = key;
		origin.transform.SetParent(transform);
		origin.gameObject.SetActive(false);

		GameObject Parent = new GameObject();
		Parent.name = key + "_Pool";
		Parent.transform.SetParent(parent);

		m_Pools.Add(key, new ObjectPool<Origin>(origin, m_PoolSize, Parent.transform));
	}

	[System.Serializable]
	public struct OriginInfo : IEqualityComparer<OriginInfo>
	{
		public string key;
		public Origin item;

		public OriginInfo(string key, Origin item)
		{
			this.key = key;
			this.item = item;
		}

		public bool Equals(OriginInfo x, OriginInfo y)
		{
			return string.Equals(x.key, y.key);
		}
		public int GetHashCode(OriginInfo obj)
		{
			return obj.key.GetHashCode();
		}
	}
}