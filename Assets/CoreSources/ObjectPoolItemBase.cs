using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPoolItem
{
	public void InitializePoolItem();
	public void FinallizePoolItem();
}

public abstract class ObjectPoolItemBase : MonoBehaviour, IPoolItem
{
	#region 변수
	protected string m_PoolKey;
	protected bool m_IsSpawning;
	#endregion

	#region 프로퍼티
	public string poolKey
	{
		get => m_PoolKey;
		set => m_PoolKey = value;
	}
	public bool isSpawning
	{
		get => m_IsSpawning;
	}
	#endregion

	#region 이벤트
	public event System.Action<ObjectPoolItemBase> onSpawn;
	public event System.Action<ObjectPoolItemBase> onDespawn;
	#endregion

	#region 매니저
	#endregion

	public virtual void InitializePoolItem()
	{
		m_IsSpawning = true;

		onSpawn?.Invoke(this);
	}
	public virtual void FinallizePoolItem()
	{
		if (m_IsSpawning == true)
			m_IsSpawning = false;

		onDespawn?.Invoke(this);

		onSpawn = null;
		onDespawn = null;
	}
}