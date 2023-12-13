using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPoolItem
{
	public void Initialize();
	public void Finallize();
}

public class ObjectPoolItemBase : MonoBehaviour, IPoolItem
{
	#region 변수
	[SerializeField]
	protected string m_ItemName;
	protected bool m_IsSpawning;
	#endregion

	#region 프로퍼티
	public string itemName => m_ItemName;
	public bool isSpawning => m_IsSpawning;
	#endregion

	#region 이벤트
	public event System.Action onSpawn;
	public event System.Action onDespawn;
	#endregion

	#region 매니저
	#endregion

	public virtual void Initialize()
	{
		m_IsSpawning = true;

		onSpawn?.Invoke();
	}
	public virtual void Finallize()
	{
		if (m_IsSpawning == true)
			m_IsSpawning = false;

		onDespawn?.Invoke();
	}
}