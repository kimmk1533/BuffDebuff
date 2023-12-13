using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ObjectPool<Item> : System.IDisposable where Item : ObjectPoolItemBase
{
	#region 변수
	// 풀에 담을 원본
	private Item m_Origin;
	// 초기 풀 사이즈
	private int m_PoolSize;
	// 오브젝트들을 담을 실제 풀
	private Queue<Item> m_Queue;
	// 생성한 오브젝트를 기억하고 있다가 디스폰 시 확인할 리스트
	private List<Item> m_DespawnCheckList;
	// 하이어라키 창에서 관리하기 쉽도록 parent 지정
	private Transform m_Parent = null;

	private ItemBuilder m_ItemBuilder;
	#endregion

	#region 프로퍼티
	public bool autoExpandPool { get; set; }
	#endregion

	#region 이벤트
	// 오브젝트가 복제될 때 실행될 이벤트
	private UnityEvent<Item> m_OnInstantiated;
	public event UnityAction<Item> onInstantiated
	{
		add
		{
			m_OnInstantiated.AddListener(value);
		}
		remove
		{
			m_OnInstantiated.RemoveListener(value);
		}
	}
	#endregion

	#region 생성자
	// 부모 지정 안하고 생성하는 경우
	public ObjectPool(Item origin, int poolSize)
	{
		m_Origin = origin;
		m_PoolSize = poolSize;
		m_Queue = new Queue<Item>(poolSize);
		m_DespawnCheckList = new List<Item>(poolSize);
		m_Parent = null;
		m_OnInstantiated = new UnityEvent<Item>();

		m_ItemBuilder = new ItemBuilder(this);

		autoExpandPool = true;
	}
	// 부모 지정하여 생성하는 경우
	public ObjectPool(Item origin, int poolSize, Transform parent)
	{
		m_Origin = origin;
		m_PoolSize = poolSize;
		m_Queue = new Queue<Item>(poolSize);
		m_DespawnCheckList = new List<Item>(poolSize);
		m_Parent = parent;
		m_OnInstantiated = new UnityEvent<Item>();

		m_ItemBuilder = new ItemBuilder(this);

		autoExpandPool = true;
	}
	#endregion

	/// <summary>
	/// 초기 풀 세팅
	/// </summary>
	public void Initialize()
	{
		ExpandPool(m_PoolSize);
	}

	// 오브젝트 풀이 빌 경우 선택적으로 call
	// 절반만큼 증가
	private void ExpandPool()
	{
		int newSize = m_PoolSize + Mathf.RoundToInt(m_PoolSize * 1.5f);

		ExpandPool(newSize);
	}
	private void ExpandPool(int size)
	{
		for (int i = 0; i < size; ++i)
		{
			Item newItem = GameObject.Instantiate<Item>(m_Origin);
			newItem.name = m_Origin.name;

			m_OnInstantiated?.Invoke(newItem);

			newItem.gameObject.SetActive(false);
			if (m_Parent != null)
				newItem.transform.SetParent(m_Parent);

			m_Queue.Enqueue(newItem);
		}

		m_PoolSize += size;
	}

	public ItemBuilder GetBuilder()
	{
		return m_ItemBuilder;
	}
	// 모든 오브젝트 사용시 추가로 생성할 경우 
	// expand 를 true 로 설정
	private Item Spawn()
	{
		if (autoExpandPool && m_Queue.Count <= 0)
			ExpandPool();

		Item item = m_Queue.Dequeue();

		m_DespawnCheckList.Add(item);

		return item;
	}
	// 회수 작업
	public bool Despawn(Item item, bool autoFinal = true)
	{
		if (item == null)
			throw new System.NullReferenceException();

		if (m_DespawnCheckList.Contains(item) == false)
			return false;

		if (m_Queue.Contains(item) == true)
			return false;

		item.gameObject.SetActive(false);
		if (m_Parent != null)
			item.transform.SetParent(m_Parent);
		item.transform.localPosition = Vector3.zero;

		if (autoFinal)
			item.Finallize();

		m_DespawnCheckList.Remove(item);

		m_Queue.Enqueue(item);

		return true;
	}

	// foreach 문을 위한 반복자
	public IEnumerator<Item> GetEnumerator()
	{
		foreach (Item item in m_Queue)
			yield return item;
	}
	// 메모리 해제
	public void Dispose()
	{
		foreach (Item item in m_Queue)
		{
			GameObject.DestroyImmediate(item);
		}
		m_Queue.Clear();
		m_Queue = null;
		m_DespawnCheckList.Clear();
		m_DespawnCheckList = null;

		m_OnInstantiated.RemoveAllListeners();
		m_OnInstantiated = null;
	}

	public class ItemBuilder
	{
		private ObjectPool<Item> m_Pool;

		private ItemProperty<string> m_Name;
		private ItemProperty<bool> m_Active;
		private ItemProperty<bool> m_AutoInit;
		private ItemProperty<Transform> m_Parent;
		private ItemProperty<Vector3> m_Position;
		private ItemProperty<Quaternion> m_Rotation;
		private ItemProperty<Vector3> m_Scale;

		public ItemBuilder(ObjectPool<Item> pool)
		{
			m_Pool = pool;

			m_Name = new ItemProperty<string>();
			m_Active = new ItemProperty<bool>();
			m_AutoInit = new ItemProperty<bool>();
			m_Parent = new ItemProperty<Transform>();
			m_Position = new ItemProperty<Vector3>();
			m_Rotation = new ItemProperty<Quaternion>();
			m_Scale = new ItemProperty<Vector3>();

			Clear();
		}

		public ItemBuilder SetName(string name)
		{
			m_Name.isUse = true;
			m_Name.value = name;

			return this;
		}
		public ItemBuilder SetActive(bool active)
		{
			m_Active.isUse = true;
			m_Active.value = active;

			return this;
		}
		public ItemBuilder SetAutoInit(bool autoInit)
		{
			m_AutoInit.isUse = true;
			m_AutoInit.value = autoInit;

			return this;
		}
		public ItemBuilder SetParent(Transform parent)
		{
			m_Parent.isUse = true;
			m_Parent.value = parent;

			return this;
		}
		public ItemBuilder SetPosition(Vector3 position)
		{
			m_Position.isUse = true;
			m_Position.value = position;

			return this;
		}
		public ItemBuilder SetRotation(Quaternion rotation)
		{
			m_Rotation.isUse = true;
			m_Rotation.value = rotation;

			return this;
		}
		public ItemBuilder SetScale(Vector3 scale)
		{
			m_Scale.isUse = true;
			m_Scale.value = scale;

			return this;
		}

		public Item Spawn()
		{
			Item item = m_Pool.Spawn();

			if (m_Name.isUse)
				item.name = m_Name.value;

			if (m_Parent.isUse)
				item.transform.SetParent(m_Parent.value);

			if (m_Position.isUse)
				item.transform.position = m_Position.value;
			if (m_Rotation.isUse)
				item.transform.rotation = m_Rotation.value;
			if (m_Scale.isUse)
				item.transform.localScale = m_Scale.value;

			if (m_Active.isUse)
				item.gameObject.SetActive(m_Active.value);

			if (m_AutoInit.isUse &&
				m_AutoInit.value)
				item.Initialize();

			Clear();

			return item;
		}

		public void Clear()
		{
			m_Name.isUse = false;
			m_Active.isUse = false;
			m_AutoInit.isUse = false;
			m_Parent.isUse = false;
			m_Position.isUse = false;
			m_Rotation.isUse = false;
			m_Scale.isUse = false;

			m_Name.value = string.Empty;
			m_Active.value = false;
			m_AutoInit.value = false;
			m_Parent.value = null;
			m_Position.value = Vector3.zero;
			m_Rotation.value = Quaternion.identity;
			m_Scale.value = Vector3.one;
		}

		public class ItemProperty<T>
		{
			public bool isUse { get; set; }
			public T value { get; set; }
		}
	}
}