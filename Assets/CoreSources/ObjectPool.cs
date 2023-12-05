using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public interface IPoolItem<T> where T : MonoBehaviour
{
	public string itemName { get; set; }
}

public class ObjectPool<T> : System.IDisposable where T : MonoBehaviour, IPoolItem<T>
{
	// 풀에 담을 원본
	private T m_Origin;
	// 초기 풀 사이즈
	private int m_PoolSize;
	// 오브젝트들을 담을 실제 풀
	private Queue<T> m_Queue;
	// 생성한 오브젝트를 기억하고 있다가 디스폰 시 확인할 리스트
	private List<T> m_DespawnCheckList;
	// 하이어라키 창에서 관리하기 쉽도록 parent 지정
	private Transform m_Parent = null;

	// 오브젝트가 복제될 때 실행될 이벤트
	private UnityEvent<T> m_OnInstantiated;
	public event UnityAction<T> onInstantiated
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

	// 부모 지정 안하고 생성하는 경우
	public ObjectPool(T origin, int poolSize)
	{
		m_Origin = origin;
		m_PoolSize = poolSize;
		m_Queue = new Queue<T>(poolSize);
		m_DespawnCheckList = new List<T>(poolSize);
		m_Parent = null;
		m_OnInstantiated = new UnityEvent<T>();
	}
	// 부모 지정하여 생성하는 경우
	public ObjectPool(T origin, int poolSize, Transform parent)
	{
		m_Origin = origin;
		m_PoolSize = poolSize;
		m_Queue = new Queue<T>(poolSize);
		m_DespawnCheckList = new List<T>(poolSize);
		m_Parent = parent;
		m_OnInstantiated = new UnityEvent<T>();
	}

	/// <summary>
	/// 초기 풀 세팅
	/// </summary>
	public void Initialize()
	{
		ExpandPool(m_PoolSize);
	}
	// 오브젝트 풀이 빌 경우 선택적으로 call
	// 절반만큼 증가
	void ExpandPool()
	{
		int newSize = m_PoolSize + Mathf.RoundToInt(m_PoolSize * 1.5f);

		ExpandPool(newSize);
	}
	void ExpandPool(int size)
	{
		for (int i = 0; i < size; ++i)
		{
			T newItem = Object.Instantiate<T>(m_Origin);
			newItem.name = m_Origin.name;

			m_OnInstantiated?.Invoke(newItem);

			newItem.gameObject.SetActive(false);
			if (m_Parent != null)
				newItem.transform.SetParent(m_Parent);

			m_Queue.Enqueue(newItem);
			m_DespawnCheckList.Add(newItem);
		}

		m_PoolSize += size;
	}

	// 모든 오브젝트 사용시 추가로 생성할 경우 
	// expand 를 true 로 설정
	public T Spawn(bool expand = true)
	{
		if (expand && m_Queue.Count <= 0)
		{
			ExpandPool();
		}

		if (m_Queue.Count > 0)
		{
			T item = m_Queue.Dequeue();
			return item;
		}

		Debug.LogError("Pool Size Over");
		return null;
	}
	// 회수 작업
	public bool Despawn(T obj)
	{
		if (obj == null)
		{
			return false;
			throw new System.NullReferenceException();
		}

		if (m_DespawnCheckList.Contains(obj) == false)
		{
			return false;
			throw new System.Exception("obj is not ObjectPool_Mono Object");
		}

		if (m_Queue.Contains(obj) == true)
		{
			return false;
		}

		obj.gameObject.SetActive(false);
		if (m_Parent != null)
			obj.transform.SetParent(m_Parent);
		obj.transform.localPosition = Vector3.zero;

		m_Queue.Enqueue(obj);

		return true;
	}

	// foreach 문을 위한 반복자
	public IEnumerator<T> GetEnumerator()
	{
		foreach (T item in m_Queue)
			yield return item;
	}
	// 메모리 해제
	public void Dispose()
	{
		foreach (T item in m_Queue)
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
}