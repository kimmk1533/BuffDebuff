using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-97)]
public abstract class ObjectManager<TSelf, TItem> : Singleton<TSelf> where TSelf : Singleton<TSelf> where TItem : ObjectPoolItemBase
{
	[SerializeField]
	protected string m_Path = null;
	[SerializeField]
	protected List<OriginInfo> m_Origins = null;
	protected Dictionary<string, ObjectPool<TItem>> m_ObjectPoolMap = null;

	public virtual void Initialize()
	{
		if (m_Origins == null)
			m_Origins = new List<OriginInfo>();
		if (m_ObjectPoolMap == null)
			m_ObjectPoolMap = new Dictionary<string, ObjectPool<TItem>>();
	}
	public virtual void Finallize()
	{
	}

	public virtual void InitializeGame()
	{
		for (int i = 0; i < m_Origins.Count; ++i)
		{
			OriginInfo originInfo = m_Origins[i];

			if (originInfo.useFlag == false)
				continue;

			AddPool(originInfo, transform);
		}
	}
	public virtual void FinallizeGame()
	{
		
	}

	protected void AddPool(OriginInfo info, Transform parent)
	{
		AddPool(info.key, info.poolSize, info.origin, parent);
	}
	protected void AddPool(string key, int poolSize, TItem origin, Transform parent)
	{
		// 예외 처리
		if (ReferenceEquals(origin, null))
		{
			string errorMsg = name + ": origin이 null 입니다. key는 \"" + key + "\"였습니다.";
			throw new System.ArgumentNullException(errorMsg);
		}
		if (m_ObjectPoolMap.ContainsKey(key) == true)
		{
			string errorMsg = name + ": 이미 존재하는 key 입니다. key는 \"" + key + "\"였습니다.";
			throw new System.ArgumentException(errorMsg);
		}

		// origin이 현재 존재하는 씬 내의 오브젝트인 경우
		if (origin.gameObject.scene.buildIndex != -1)
		{
			origin.name = key;
			origin.transform.SetParent(transform);
			origin.gameObject.SetActive(false);
		}

		// Pool 부모 생성
		GameObject poolParent = new GameObject();
		poolParent.name = key + "_Pool";
		poolParent.transform.SetParent(parent);
		poolParent.SetActive(true);

		// Pool 생성
		ObjectPool<TItem> pool = new ObjectPool<TItem>(key, origin, poolSize, poolParent.transform);
		pool.Initialize();

		// Pool 추가
		m_ObjectPoolMap.Add(key, pool);
	}

	public ObjectPool<TItem>.ItemBuilder GetBuilder(string key)
	{
		ObjectPool<TItem> pool = GetPool(key);

		if (pool == null)
			throw new System.NullReferenceException(transform.name + ": Pool이 null 입니다. key는 \"" + key + "\" 였습니다.");

		return pool.GetBuilder();
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
		if (m_ObjectPoolMap == null)
			return null;

		// 예외 처리: key가 null임
		if (key == null)
			return null;

		// 예외 처리: Pool에 올바른 key가 없음
		if (m_ObjectPoolMap.ContainsKey(key) == false)
			return null;

		return m_ObjectPoolMap[key];
	}

#if UNITY_EDITOR
	/// <summary>
	/// Use [ContextMenu("Load Origin")] and base.LoadOrigin_Inner
	/// </summary>
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