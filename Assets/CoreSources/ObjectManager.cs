using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-97)]
public abstract class ObjectManager<TSelf, TItem> : Singleton<TSelf> where TSelf : Singleton<TSelf> where TItem : ObjectPoolItemBase
{
	[SerializeField]
	protected string m_Path;
	[SerializeField]
	protected List<OriginInfo> m_Origins = new List<OriginInfo>();
	protected Dictionary<string, ObjectPool<TItem>> m_Pools = null;

	public virtual void Initialize()
	{
		if (m_Pools != null)
		{
			foreach (var item in m_Pools)
			{
				item.Value.Dispose();
			}
			m_Pools.Clear();
		}
		else
			m_Pools = new Dictionary<string, ObjectPool<TItem>>();
	}
	public virtual void InitializeGame()
	{
		foreach (var originInfo in m_Origins)
		{
			if (originInfo.useFlag == false)
				continue;

			AddPool(originInfo, transform);
		}
	}

	protected void AddPool(OriginInfo info, Transform parent)
	{
		AddPool(info.key, info.poolSize, info.origin, parent);
	}
	protected void AddPool(string key, int poolSize, TItem origin, Transform parent)
	{
		if (origin == null)
			throw new System.ArgumentNullException(transform.name + ": origin이 null 입니다. key는 \"" + key + "\" 였습니다.");

		if (origin.gameObject.scene.buildIndex != -1)
		{
			origin.name = key;
			origin.transform.SetParent(transform);
			origin.gameObject.SetActive(false);
		}

		GameObject poolParent = new GameObject();
		poolParent.name = key + "_Pool";
		poolParent.transform.SetParent(parent);
		poolParent.SetActive(true);

		ObjectPool<TItem> pool = new ObjectPool<TItem>(key, origin, poolSize, poolParent.transform);
		pool.Initialize();

		m_Pools.Add(key, pool);
	}

	public ObjectPool<TItem>.ItemBuilder GetBuilder(string key)
	{
		ObjectPool<TItem> pool = GetPool(key);

		if (pool == null)
			throw new System.NullReferenceException(transform.name + ": Pool이 null 입니다. key는 \"" + key + "\" 였습니다.");

		return pool.GetBuilder();
	}

	public TItem Spawn(string key, Transform parent = null, bool autoInit = true)
	{
		ObjectPool<TItem> pool = GetPool(key);

		if (pool == null)
			throw new System.NullReferenceException(transform.name + ": Pool이 null 입니다. key는 \"" + key + "\" 였습니다.");

		TItem item = pool.GetBuilder()
			.SetActive(true)
			.SetAutoInit(autoInit)
			.SetParent(parent)
			.Spawn();

		return item;
	}
	public TItem Spawn(string key, Vector3 position, Transform parent = null, bool autoInit = true)
	{
		ObjectPool<TItem> pool = GetPool(key);

		if (pool == null)
			throw new System.NullReferenceException(transform.name + ": Pool이 null 입니다. key는 \"" + key + "\" 였습니다.");

		TItem item = pool.GetBuilder()
			.SetActive(true)
			.SetAutoInit(autoInit)
			.SetParent(parent)
			.SetPosition(position)
			.Spawn();

		item.transform.position = position;

		return item;
	}
	public TItem Spawn(string key, Vector3 position, Quaternion rotation, Transform parent = null, bool autoInit = true)
	{
		ObjectPool<TItem> pool = GetPool(key);

		if (pool == null)
			throw new System.NullReferenceException(transform.name + ": Pool이 null 입니다. key는 \"" + key + "\" 였습니다.");

		TItem item = pool.GetBuilder()
			.SetActive(true)
			.SetAutoInit(autoInit)
			.SetParent(parent)
			.SetPosition(position)
			.SetRotation(rotation)
			.Spawn();

		item.transform.position = position;

		return item;
	}
	public bool Despawn(TItem item, bool autoFinal = true)
	{
		if (item.poolKey == string.Empty)
			throw new System.Exception(item.ToString() + "에는 값이 있어야 합니다.");

		ObjectPool<TItem> pool = GetPool(item.poolKey);

		if (pool == null)
			throw new System.NullReferenceException(name + ": Pool이 null 입니다. itemName은 \"" + item.poolKey + "\" 였습니다.");

		return pool.Despawn(item, autoFinal);
	}

	protected virtual ObjectPool<TItem> GetPool(string key)
	{
		// 예외 처리: 초기화 안함
		if (m_Pools == null)
			return null;

		// 예외 처리: key가 null임
		if (key == null)
			return null;

		// 예외 처리: Pool에 올바른 key가 없음
		if (m_Pools.ContainsKey(key) == false)
			return null;

		return m_Pools[key];
	}

#if UNITY_EDITOR
	protected abstract void LoadOrigin();
	protected void LoadOrigin_Inner()
	{
		for (int i = 0; i < m_Origins.Count; ++i)
		{
			OriginInfo info = m_Origins[i];
			info.origin = Resources.Load<TItem>(System.IO.Path.Combine(m_Path, info.path, info.key));
			m_Origins[i] = info;
		}
	}
#endif

	[System.Serializable]
	public struct OriginInfo : IEqualityComparer<OriginInfo>
	{
		[field: SerializeField, ReadOnly(true)]
		public string key { get; set; }
		[field: SerializeField, ReadOnly(true)]
		public string path { get; set; }

		[field: Space]
		[field: SerializeField, ReadOnly(true)]
		public bool useFlag { get; set; }
		[field: SerializeField, ReadOnly(true), Min(1)]
		public int poolSize { get; set; }
		[field: SerializeField, ReadOnly(true)]
		public TItem origin { get; set; }

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