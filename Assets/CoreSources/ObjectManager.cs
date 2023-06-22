using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-97)]
public abstract class ObjectManager<Pool, Origin> : Singleton<Pool> where Pool : MonoBehaviour where Origin : MonoBehaviour
{
	[SerializeField]
	protected List<OriginInfo> m_Origins = new List<OriginInfo>();
	protected Dictionary<string, ObjectPool<Origin>> m_Pools = null;

	public virtual void Initialize()
	{
		m_Pools = new Dictionary<string, ObjectPool<Origin>>();
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
			if (pool.Despawn(item))
				return true;
		}

		return false;
	}
	public bool Despawn(string key, Origin item)
	{
		ObjectPool<Origin> pool = GetPool(key);
		if (pool == null)
		{
			return false;
			throw new System.NullReferenceException("pool is null. key is " + key);
		}

		return pool.Despawn(item);
	}

	protected virtual ObjectPool<Origin> GetPool(string key)
	{
		if (key == null)
			return null;

		if (m_Pools.ContainsKey(key))
			return m_Pools[key];

		return null;
	}

	protected void AddPool(OriginInfo info, Transform parent, bool autoInit = true)
	{
		AddPool(info.key, info.poolSize, info.origin, parent, autoInit);
	}
	protected void AddPool(string key, int poolSize, Origin origin, Transform parent, bool autoInit = true)
	{
		if (origin.gameObject.scene.buildIndex == -1)
		{
			throw new System.Exception("Object Manager`s Origin should not be Asset`s Object.");
		}

		origin.name = key;
		origin.transform.SetParent(transform);
		origin.gameObject.SetActive(false);

		GameObject Parent = new GameObject();
		Parent.name = key + "_Pool";
		Parent.transform.SetParent(parent);

		ObjectPool<Origin> pool = new ObjectPool<Origin>(origin, poolSize, Parent.transform);

		if (autoInit)
			pool.Initialize();

		m_Pools.Add(key, pool);
	}

	[System.Serializable]
	public struct OriginInfo : IEqualityComparer<OriginInfo>
	{
		public string key;
		[SerializeField, ReadOnly(true)]
		public int poolSize;

		[Space]
		public Origin origin;

		public OriginInfo(string key, Origin item)
		{
			this.key = key;
			this.poolSize = 100;
			this.origin = item;
		}
		public OriginInfo(string key, int poolSize, Origin item)
		{
			this.key = key;
			this.poolSize = poolSize;
			this.origin = item;
		}

		public static bool operator ==(OriginInfo x, OriginInfo y)
		{
			return string.Equals(x.key, y.key);
		}
		public static bool operator !=(OriginInfo x, OriginInfo y)
		{
			return !(x == y);
		}

		public override bool Equals(object obj)
		{
			OriginInfo? objInfo = obj as OriginInfo?;

			if (objInfo == null)
				return false;

			if (Equals(this, objInfo.Value) == false)
				return false;

			return base.Equals(obj);
		}
		public bool Equals(OriginInfo x, OriginInfo y)
		{
			return x == y;
		}
		public override int GetHashCode()
		{
			return this.key.GetHashCode();
		}
		public int GetHashCode(OriginInfo obj)
		{
			return obj.GetHashCode();
		}
	}
}