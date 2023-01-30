using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-97)]
public abstract class ObjectManager<Pool, Origin> : Singleton<Pool> where Pool : MonoBehaviour where Origin : MonoBehaviour
{
	[SerializeField, ReadOnly(true)]
	private int m_PoolSize = 100;
	[SerializeField, ReadOnly(true)]
	protected Origin m_Origin = null;

	private Dictionary<string, Origin> m_Origins = null;
	private Dictionary<string, MemoryPool<Origin>> m_Pools = null;

	public Origin Spawn(string key)
	{
		Origin item = m_Pools[key].Spawn();
		item.gameObject.SetActive(true);
		return item;
	}
	public Origin Spawn(string key, Transform parent)
	{
		Origin item = m_Pools[key].Spawn();
		item.transform.SetParent(parent);
		item.gameObject.SetActive(true);
		return item;
	}
	public Origin Spawn(string key, Vector3 position, Quaternion rotation)
	{
		Origin item = m_Pools[key].Spawn();
		item.transform.position = position;
		item.transform.rotation = rotation;
		item.gameObject.SetActive(true);
		return item;
	}
	public Origin Spawn(string key, Vector3 position, Quaternion rotation, Transform parent)
	{
		Origin item = m_Pools[key].Spawn();
		item.transform.SetParent(parent);
		item.transform.position = position;
		item.transform.rotation = rotation;
		item.gameObject.SetActive(true);
		return item;
	}
	protected bool AddPool(string key, Transform parent)
	{
		if (m_Origins.ContainsKey(key))
			return false;

		m_Origins.Add(key, m_Origin);

		GameObject Parent = new GameObject();
		Parent.name = m_Origin.name + "_Pool";
		Parent.transform.SetParent(parent);
		m_Origin.transform.SetParent(Parent.transform);

		m_Pools.Add(key, new MemoryPool<Origin>(m_Origin, m_PoolSize, Parent.transform));

		m_Origin.name += "_Origin";
		m_Origin.gameObject.SetActive(false);

		return true;
	}
	public virtual void __Initialize()
	{
		m_Origins = new Dictionary<string, Origin>();
		m_Pools = new Dictionary<string, MemoryPool<Origin>>();

		//for (int i = 0; i < m_Origins.Count; ++i)
		//{
		//    GameObject parent = new GameObject();
		//    parent.transform.SetParent(this.transform);
		//    parent.name = m_Origins[i].name + "_parent";

		//    m_Pools.Add(m_Origins[i].name, new MemoryPool(m_Origins[i], m_PoolSize, parent.transform));
		//}
	}
	public virtual void __Finalize()
	{
		if (m_Pools == null)
			return;

		foreach (var item in m_Pools)
		{
			item.Value?.Dispose();
		}

		m_Pools.Clear();
		m_Pools = null;
	}
	public virtual MemoryPool<Origin> GetPool(string key)
	{
		if (key == null)
			return null;

		if (m_Pools.ContainsKey(key))
			return m_Pools[key];

		return null;
	}
	protected virtual void Awake()
	{
		__Initialize();
	}
	protected virtual void OnApplicationQuit()
	{
		__Finalize();
	}
}

[DefaultExecutionOrder(-97)]
public abstract class ObjectManager<Pool> : Singleton<Pool> where Pool : MonoBehaviour
{
	[SerializeField, ReadOnly(true)]
	private int m_PoolSize = 100;
	[SerializeField, ReadOnly(true)]
	protected GameObject m_Origin = null;

	private Dictionary<string, GameObject> m_Origins = null;
	private Dictionary<string, MemoryPool> m_Pools = null;

	protected GameObject Spawn(string key)
	{
		GameObject item = m_Pools[key].Spawn();
		item.SetActive(true);
		return item;
	}
	protected GameObject Spawn(string key, Transform parent)
	{
		GameObject item = m_Pools[key].Spawn();
		item.transform.SetParent(parent);
		item.SetActive(true);
		return item;
	}
	protected GameObject Spawn(string key, Vector3 position, Quaternion rotation)
	{
		GameObject item = m_Pools[key].Spawn();
		item.transform.position = position;
		item.transform.rotation = rotation;
		item.SetActive(true);
		return item;
	}
	protected GameObject Spawn(string key, Vector3 position, Quaternion rotation, Transform parent)
	{
		GameObject item = m_Pools[key].Spawn();
		item.transform.SetParent(parent);
		item.transform.position = position;
		item.transform.rotation = rotation;
		item.SetActive(true);
		return item;
	}
	protected bool AddPool(string key, Transform parent)
	{
		if (m_Origins.ContainsKey(key))
			return false;

		m_Origins.Add(key, m_Origin);

		GameObject Parent = new GameObject();
		Parent.name = m_Origin.name + "_Pool";
		Parent.transform.SetParent(parent);
		m_Origin.transform.SetParent(Parent.transform);

		m_Pools.Add(key, new MemoryPool(m_Origin, m_PoolSize, Parent.transform));

		m_Origin.name += "_Origin";
		m_Origin.SetActive(false);

		return true;
	}
	public virtual void __Initialize()
	{
		m_Origins = new Dictionary<string, GameObject>();
		m_Pools = new Dictionary<string, MemoryPool>();

		//for (int i = 0; i < m_Origins.Count; ++i)
		//{
		//    GameObject parent = new GameObject();
		//    parent.transform.SetParent(this.transform);
		//    parent.name = m_Origins[i].name + "_parent";

		//    m_Pools.Add(m_Origins[i].name, new MemoryPool(m_Origins[i], m_PoolSize, parent.transform));
		//}
	}
	public virtual void __Finalize()
	{
		if (m_Pools == null)
			return;

		foreach (var item in m_Pools)
		{
			item.Value?.Dispose();
		}

		m_Pools.Clear();
		m_Pools = null;
	}
	public virtual MemoryPool GetPool(string key)
	{
		if (key == null)
			return null;

		if (m_Pools.ContainsKey(key))
			return m_Pools[key];

		return null;
	}
	protected virtual void Awake()
	{
		__Initialize();
	}
	protected virtual void OnApplicationQuit()
	{
		__Finalize();
	}
}
