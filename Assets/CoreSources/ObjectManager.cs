using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-97)]
public abstract class ObjectManager<Pool, Origin> : Singleton<Pool> where Pool : MonoBehaviour where Origin : PoolItemBase
{
	[SerializeField]
	protected List<OriginInfo> m_Origins = new List<OriginInfo>();
	protected Dictionary<string, ObjectPool<Origin>> m_Pools = null;

	public virtual void Initialize()
	{
		Initialize(true);
	}
	public virtual void Initialize(bool autoInit = true)
	{
		m_Pools = new Dictionary<string, ObjectPool<Origin>>();

		foreach (var originInfo in m_Origins)
		{
			if (originInfo.useFlag == false)
				continue;

			AddPool(originInfo, transform, autoInit);
		}
	}

	protected void AddPool(OriginInfo info, Transform parent, bool autoInit = true)
	{
		AddPool(info.key, info.poolSize, info.origin, parent, autoInit);
	}
	protected void AddPool(string key, int poolSize, Origin origin, Transform parent, bool autoInit = true)
	{
		if (origin == null)
			throw new System.ArgumentNullException(transform.name + ": origin이 null 입니다. key는 \"" + key + "\" 였습니다.");

		if (origin.gameObject.scene.buildIndex == -1)
			throw new System.Exception(transform.name + ": Object Manager의 Origin은 씬에 존재하는 오브젝트여야 합니다.");

		origin.name = key;
		origin.transform.SetParent(transform);
		origin.gameObject.SetActive(false);

		GameObject poolParent = new GameObject();
		poolParent.name = key + "_Pool";
		poolParent.transform.SetParent(parent);
		poolParent.SetActive(false);

		ObjectPool<Origin> pool = new ObjectPool<Origin>(origin, poolSize, poolParent.transform);

		if (autoInit)
			pool.Initialize();

		m_Pools.Add(key, pool);
	}

	public ObjectPool<Origin>.ItemBuilder GetBuilder(string key)
	{
		ObjectPool<Origin> pool = GetPool(key);

		if (pool == null)
			throw new System.NullReferenceException(transform.name + ": Pool이 null 입니다. key는 \"" + key + "\" 였습니다.");

		return pool.GetBuilder();
	}
	//public Origin Spawn(string key, Transform parent = null, bool autoInit = true)
	//{
	//	ObjectPool<Origin> pool = GetPool(key);

	//	if (pool == null)
	//		throw new System.NullReferenceException(transform.name + ": Pool이 null 입니다. key는 \"" + key + "\" 였습니다.");

	//	Origin item = pool.GetBuilder()
	//		.SetActive(true)
	//		.SetAutoInit(autoInit)
	//		.SetParent(parent)
	//		.Spawn();

	//	return item;
	//}
	//public Origin Spawn(string key, Vector3 position, Transform parent = null, bool autoInit = true)
	//{
	//	ObjectPool<Origin> pool = GetPool(key);

	//	if (pool == null)
	//		throw new System.NullReferenceException(transform.name + ": Pool이 null 입니다. key는 \"" + key + "\" 였습니다.");

	//	Origin item = pool.GetBuilder()
	//		.SetActive(true)
	//		.SetAutoInit(autoInit)
	//		.SetParent(parent)
	//		.SetPosition(position)
	//		.Spawn();

	//	item.transform.position = position;

	//	return item;
	//}
	//public Origin Spawn(string key, Vector3 position, Quaternion rotation, Transform parent = null, bool autoInit = true)
	//{
	//	ObjectPool<Origin> pool = GetPool(key);

	//	if (pool == null)
	//		throw new System.NullReferenceException(transform.name + ": Pool이 null 입니다. key는 \"" + key + "\" 였습니다.");

	//	Origin item = pool.GetBuilder()
	//		.SetActive(true)
	//		.SetAutoInit(autoInit)
	//		.SetParent(parent)
	//		.SetPosition(position)
	//		.SetRotation(rotation)
	//		.Spawn();

	//	item.transform.position = position;

	//	return item;
	//}
	public bool Despawn(Origin item, bool autoFinal = true)
	{
		foreach (var pool in m_Pools.Values)
		{
			if (pool.Despawn(item, autoFinal))
				return true;
		}

		return false;
	}
	public bool Despawn(string key, Origin item, bool autoFinal = true)
	{
		ObjectPool<Origin> pool = GetPool(key);

		if (pool == null)
			throw new System.NullReferenceException(transform.name + ": Pool이 null 입니다. key는 \"" + key + "\" 였습니다.");

		return pool.Despawn(item, autoFinal);
	}

	protected virtual ObjectPool<Origin> GetPool(string key)
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

	[System.Serializable]
	public struct OriginInfo : IEqualityComparer<OriginInfo>
	{
		[field: SerializeField, ReadOnly(true)]
		public string key { get; set; }

		[field: Space]
		[field: SerializeField, ReadOnly(true)]
		public bool useFlag { get; set; }
		[field: SerializeField, ReadOnly(true)]
		public int poolSize { get; set; }
		[field: SerializeField, ReadOnly(true)]
		public Origin origin { get; set; }

		public OriginInfo(string key, Origin item)
		{
			this.useFlag = true;
			this.key = key;
			this.poolSize = 100;
			this.origin = item;
		}
		public OriginInfo(string key, int poolSize, Origin item)
		{
			this.useFlag = true;
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