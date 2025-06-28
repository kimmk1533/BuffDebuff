using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[DefaultExecutionOrder(-97)]
public abstract class ObjectManager<TSelf, TItem> : SerializedSingleton<TSelf> where TSelf : SerializedSingleton<TSelf> where TItem : ObjectPoolItem<TItem>
{
	#region 변수
	// 공통 경로
	[SerializeField]
	protected string m_CommonPath = null;
	[SerializeField, PropertySpace(SpaceAfter = 10)]
	protected List<OriginInfo> m_Origins = null;
	protected Dictionary<string, ObjectPool<TItem>> m_ObjectPoolMap = null;
	#endregion

	#region 프로퍼티

	#endregion

	#region 초기화 & 마무리화 함수
	/// <summary>
	/// 초기화 함수 (Init Scene 진입 시, 즉 게임 실행 시 호출)
	/// </summary>
	public override void Initialize()
	{
		base.Initialize();

		if (m_Origins == null)
			m_Origins = new List<OriginInfo>();
		if (m_ObjectPoolMap == null)
			m_ObjectPoolMap = new Dictionary<string, ObjectPool<TItem>>();

		for (int i = 0; i < m_Origins.Count; ++i)
		{
			OriginInfo originInfo = m_Origins[i];

			if (originInfo.useFlag == false)
				continue;

			AddPool(originInfo, transform);
		}
	}
	/// <summary>
	/// 마무리화 함수 (게임 종료 시 호출)
	/// </summary>
	public override void Finallize()
	{
		base.Finallize();

		foreach (var item in m_ObjectPoolMap)
		{
			item.Value.Dispose();
		}
	}

	/// <summary>
	/// 메인 초기화 함수 (본인 Main Scene 진입 시 호출)
	/// </summary>
	public override void InitializeMain()
	{
		base.InitializeMain();

		for (int i = 0; i < m_Origins.Count; ++i)
		{
			OriginInfo originInfo = m_Origins[i];

			if (originInfo.useFlag == false)
				continue;

			ObjectPool<TItem> itemPool = GetPool(originInfo.key);
			itemPool.Initialize();
		}
	}
	/// <summary>
	/// 메인 마무리화 함수 (본인 Main Scene 나갈 시 호출)
	/// </summary>
	public override void FinallizeMain()
	{
		base.FinallizeMain();

		for (int i = 0; i < m_Origins.Count; ++i)
		{
			OriginInfo originInfo = m_Origins[i];

			if (originInfo.useFlag == false)
				continue;

			GetPool(originInfo.key).Finallize();
		}
	}
	#endregion

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

		// Pool 생성 (오브젝트 생성 X)
		ObjectPool<TItem> pool = new ObjectPool<TItem>(key, origin, poolSize, poolParent.transform);
		ObjectPool<TItem>.ItemBuilder itemBuilder = new ObjectPool<TItem>.ItemBuilder(pool);
		pool.builder = itemBuilder;

		// Pool 추가
		m_ObjectPoolMap.Add(key, pool);
	}

	public ObjectPool<TItem>.IItemBuilder GetBuilder(string key)
	{
		ObjectPool<TItem> pool = GetPool(key);

		if (pool == null)
			throw new System.NullReferenceException(transform.name + ": Pool이 null 입니다. key는 \"" + key + "\" 였습니다.");

		return pool.builder;
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
	[Button("모든 풀 사용")]
	private void TurnOnFlagAll()
	{
		for (int i = 0; i < m_Origins.Count; ++i)
		{
			var temp = m_Origins[i];
			temp.useFlag = true;
			m_Origins[i] = temp;
		}
	}
	[Button("모든 풀 사용 안함")]
	private void TurnOffFlagAll()
	{
		for (int i = 0; i < m_Origins.Count; ++i)
		{
			var temp = m_Origins[i];
			temp.useFlag = false;
			m_Origins[i] = temp;
		}
	}

	[Button("원본 불러오기")]
	protected void LoadOrigin()
	{
		for (int i = 0; i < m_Origins.Count; ++i)
		{
			OriginInfo info = m_Origins[i];
			string path = System.IO.Path.Combine(m_CommonPath, info.additionalPath, info.key);
			info.origin = Resources.Load<TItem>(path);
			if (info.origin == null)
				Debug.LogError("원본을 불러오는 데에 실패했습니다. 경로: " + path);
			m_Origins[i] = info;
		}
	}
#endif

	[System.Serializable]
	public struct OriginInfo : IEqualityComparer<OriginInfo>
	{
		[field: SerializeField, RuntimeReadOnly(true)]
		public string key { get; set; }
		[field: SerializeField, RuntimeReadOnly(true)]
		public string additionalPath { get; set; }

		[field: Space]
		[field: SerializeField, RuntimeReadOnly(true)]
		public bool useFlag { get; set; }
		[field: SerializeField, RuntimeReadOnly(true), Min(1)]
		public int poolSize { get; set; }
		[field: SerializeField, RuntimeReadOnly(true)]
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