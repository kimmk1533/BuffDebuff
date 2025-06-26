using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<TItem> where TItem : ObjectPoolItem<TItem>
{
	#region 변수
	// 이 오브젝트 풀의 Key
	private string m_PoolKey = string.Empty;
	// 오브젝트 풀 원본
	private TItem m_Origin = null;
	// 초기 풀 사이즈
	private int m_PoolSize = 0;
	// 오브젝트들을 담을 실제 풀
	private Queue<TItem> m_PoolItemQueue = null;
	// 생성한 오브젝트를 기억하고 있다가 디스폰 시 확인할 리스트
	private List<TItem> m_SpawnedItemList = null;
	// 하이어라키 창에서 관리하기 쉽도록 parent 지정
	private Transform m_Parent = null;

	private ItemBuilder m_ItemBuilder = null;
	#endregion

	#region 프로퍼티
	public int Count => m_PoolItemQueue.Count;
	public int SpawnedItemCount => m_SpawnedItemList.Count;

	public TItem origin => m_Origin;
	public bool autoExpandPool { get; set; }
	public ItemBuilder builder => m_ItemBuilder;
	#endregion

	#region 이벤트
	// 오브젝트가 복제될 때 실행될 이벤트
	public event System.Action<TItem> onItemInstantiated = null;
	// 오브젝트 스폰할 때 실행될 이벤트
	public event System.Action<TItem> onItemSpawned = null;
	// 오브젝트 디스폰할 때 실행될 이벤트
	public event System.Action<TItem> onItemDespawned = null;
	#endregion

	#region 생성자
	// 부모 지정 안하고 생성하는 경우
	public ObjectPool(string key, TItem origin, int poolSize) : this(key, origin, poolSize, null)
	{

	}
	// 부모 지정하여 생성하는 경우
	public ObjectPool(string key, TItem origin, int poolSize, Transform parent)
	{
		m_PoolKey = key;
		m_Origin = origin;
		m_PoolSize = poolSize;
		m_PoolItemQueue = new Queue<TItem>(poolSize * 2);
		m_SpawnedItemList = new List<TItem>(poolSize * 2);
		m_Parent = parent;

		autoExpandPool = true;
	}
	#endregion

	#region 초기화 & 마무리화 함수
	/// <summary>
	/// 초기 풀 세팅
	/// </summary>
	public void Initialize(ItemBuilder itemBuilder)
	{
		m_ItemBuilder = itemBuilder;

		ExpandPool(m_PoolSize);
	}
	public void Finallize()
	{
		// 이미 Dispose된 경우 중도 return
		if (m_SpawnedItemList == null)
			return;

		int count = m_SpawnedItemList.Count;
		for (int i = 0; i < count; ++i)
		{
			Despawn(m_SpawnedItemList[0], true);
		}
		m_SpawnedItemList.Clear();

		onItemInstantiated = null;
		onItemSpawned = null;
		onItemDespawned = null;
	}
	#endregion

	// 오브젝트 풀이 빌 경우 선택적으로 call
	// 절반만큼 증가
	private void ExpandPool()
	{
		int newSize = m_PoolSize == 0 ? 10 : m_PoolSize + Mathf.RoundToInt(m_PoolSize * 1.5f);

		ExpandPool(newSize);
	}
	private void ExpandPool(int newSize)
	{
		if (Count + SpawnedItemCount >= newSize)
			return;

		int size = newSize - (Count + SpawnedItemCount);

		for (int i = 0; i < size; ++i)
		{
			TItem newItem = GameObject.Instantiate<TItem>(m_Origin);
			newItem.name = m_Origin.name;
			newItem.poolKey = m_PoolKey;

			onItemInstantiated?.Invoke(newItem);

			newItem.gameObject.SetActive(false);
			if (m_Parent != null)
				newItem.transform.SetParent(m_Parent);

			m_PoolItemQueue.Enqueue(newItem);
		}

		m_PoolSize = newSize;
	}

	// 모든 오브젝트 사용시 추가로 생성할 경우 
	// expand 를 true 로 설정
	private TItem Spawn()
	{
		if (autoExpandPool && m_PoolItemQueue.Count <= 0)
			ExpandPool();

		if (m_PoolItemQueue.Count <= 0)
			return null;

		TItem item = m_PoolItemQueue.Dequeue();

		m_SpawnedItemList.Add(item);

		//item.name = item.name + m_Count.ToString("_00");

		return item;
	}
	// 회수 작업
	public bool Despawn(TItem item, bool autoFinal)
	{
		if (item == null)
			throw new System.NullReferenceException();

		// 디스폰할 아이템이 스폰한 아이템이 아닌 경우
		if (m_SpawnedItemList.Contains(item) == false)
			return false;

		// 디스폰할 아이템이 이미 아이템 큐에 들어가 있는 경우
		if (m_PoolItemQueue.Contains(item) == true)
			return false;

		item.gameObject.SetActive(false);

		if (m_Parent != null)
			item.transform.SetParent(m_Parent);

		item.transform.localPosition = Vector3.zero;

		if (autoFinal)
			item.FinallizePoolItem();

		m_SpawnedItemList.Remove(item);

		m_PoolItemQueue.Enqueue(item);

		onItemDespawned?.Invoke(item);

		return true;
	}

	// foreach 문을 위한 반복자
	public IEnumerator<TItem> GetEnumerator()
	{
		foreach (TItem item in m_PoolItemQueue)
			yield return item;
	}
	// 메모리 해제
	public void Dispose()
	{
		int count = SpawnedItemCount;
		for (int i = 0; i < count; ++i)
		{
			Despawn(m_SpawnedItemList[i], true);
		}
		m_SpawnedItemList.Clear();
		m_SpawnedItemList = null;

		count = Count;
		for (int i = 0; i < count; ++i)
		{
			TItem item = m_PoolItemQueue.Dequeue();

			if (item == null)
				continue;

			GameObject.DestroyImmediate(item.gameObject);
		}
		m_PoolItemQueue.Clear();
		m_PoolItemQueue = null;

		onItemInstantiated = null;
		onItemSpawned = null;
		onItemDespawned = null;
	}

	public interface IItemBuilder
	{
		public IItemBuilder SetName(string name);
		public IItemBuilder SetActive(bool active);
		public IItemBuilder SetAutoInit(bool autoInit);
		public IItemBuilder SetParent(Transform parent);
		public IItemBuilder SetPosition(Vector3 position);
		public IItemBuilder SetLocalPosition(Vector3 localPosition);
		public IItemBuilder SetRotation(Quaternion rotation);
		public IItemBuilder SetLocalRotation(Quaternion localRotation);
		public IItemBuilder SetScale(Vector3 scale);

		public TItem Spawn(bool autoReset = true);
		public T Spawn<T>(bool autoReset = true) where T : TItem;

		public void Reset();
	}
	public class ItemBuilder : IItemBuilder
	{
		#region 변수
		protected ObjectPool<TItem> m_Pool = null;

		protected ItemProperty<string> m_Name = null;
		protected ItemProperty<bool> m_Active = null;
		protected ItemProperty<bool> m_AutoInit = null;
		protected ItemProperty<Transform> m_Parent = null;
		protected ItemProperty<Vector3> m_Position = null;
		protected ItemProperty<Vector3> m_LocalPosition = null;
		protected ItemProperty<Quaternion> m_Rotation = null;
		protected ItemProperty<Quaternion> m_LocalRotation = null;
		protected ItemProperty<Vector3> m_Scale;
		#endregion

		#region 생성자
		public ItemBuilder(ObjectPool<TItem> pool)
		{
			m_Pool = pool;

			m_Name = new ItemProperty<string>();
			m_Active = new ItemProperty<bool>();
			m_AutoInit = new ItemProperty<bool>();
			m_Parent = new ItemProperty<Transform>();
			m_Position = new ItemProperty<Vector3>();
			m_LocalPosition = new ItemProperty<Vector3>();
			m_Rotation = new ItemProperty<Quaternion>();
			m_LocalRotation = new ItemProperty<Quaternion>();
			m_Scale = new ItemProperty<Vector3>();
		}
		#endregion

		public IItemBuilder SetName(string name)
		{
			m_Name.isUse = true;
			m_Name.value = name;

			return this;
		}
		public IItemBuilder SetActive(bool active)
		{
			m_Active.isUse = true;
			m_Active.value = active;

			return this;
		}
		public IItemBuilder SetAutoInit(bool autoInit)
		{
			m_AutoInit.isUse = true;
			m_AutoInit.value = autoInit;

			return this;
		}
		public IItemBuilder SetParent(Transform parent)
		{
			m_Parent.isUse = true;
			m_Parent.value = parent;

			return this;
		}
		public IItemBuilder SetPosition(Vector3 position)
		{
			m_Position.isUse = true;
			m_Position.value = position;

			return this;
		}
		public IItemBuilder SetLocalPosition(Vector3 localPosition)
		{
			m_LocalPosition.isUse = true;
			m_LocalPosition.value = localPosition;

			return this;
		}
		public IItemBuilder SetRotation(Quaternion rotation)
		{
			m_Rotation.isUse = true;
			m_Rotation.value = rotation;

			return this;
		}
		public IItemBuilder SetLocalRotation(Quaternion localRotation)
		{
			m_LocalRotation.isUse = true;
			m_LocalRotation.value = localRotation;

			return this;
		}
		public IItemBuilder SetScale(Vector3 scale)
		{
			m_Scale.isUse = true;
			m_Scale.value = scale;

			return this;
		}

		public virtual TItem Spawn(bool autoReset = true)
		{
			TItem item = m_Pool.Spawn();

			if (m_Name.isUse)
				item.name = m_Name.value;

			if (m_Active.isUse)
				item.gameObject.SetActive(m_Active.value);

			if (m_Parent.isUse)
				item.transform.SetParent(m_Parent.value);

			if (m_Position.isUse)
				item.transform.position = m_Position.value;
			if (m_LocalPosition.isUse)
				item.transform.localPosition = m_LocalPosition.value;

			if (m_Rotation.isUse)
				item.transform.rotation = m_Rotation.value;
			if (m_LocalRotation.isUse)
				item.transform.localRotation = m_Rotation.value;

			if (m_Scale.isUse)
				item.transform.localScale = m_Scale.value;

			if (m_AutoInit.isUse &&
				m_AutoInit.value)
				item.InitializePoolItem();

			m_Pool.onItemSpawned?.Invoke(item);

			if (autoReset)
				Reset();

			return item;
		}
		public virtual T Spawn<T>(bool autoReset = true) where T : TItem
		{
			return Spawn(autoReset) as T;
		}

		public virtual void Reset()
		{
			m_Name.isUse = false;
			m_Name.value = string.Empty;

			m_Active.isUse = false;
			m_Active.value = false;

			m_AutoInit.isUse = false;
			m_AutoInit.value = false;

			m_Parent.isUse = false;
			m_Parent.value = null;

			m_Position.isUse = false;
			m_Position.value = Vector3.zero;

			m_Rotation.isUse = false;
			m_Rotation.value = Quaternion.identity;

			m_Scale.isUse = false;
			m_Scale.value = Vector3.one;
		}

		public class ItemProperty<T>
		{
			public bool isUse { get; set; }
			public T value { get; set; }
		}
	}
}