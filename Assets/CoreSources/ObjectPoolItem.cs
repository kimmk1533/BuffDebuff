using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public interface IPoolItem
{
	public void InitializePoolItem();
	public void FinallizePoolItem();
}

public abstract class ObjectPoolItem<TItem> : SerializedMonoBehaviour, IPoolItem where TItem : ObjectPoolItem<TItem>
{
	#region 변수

	#endregion

	#region 프로퍼티
	public string poolKey { get; set; }
	public bool isSpawning { get; private set; }
	#endregion

	#region 이벤트
	public event System.Action<TItem> onSpawn = null;
	public event System.Action<TItem> onDespawn = null;
	#endregion

	#region 매니저
	#endregion

	// ObjectManager를 통해 스폰하면 자동으로 호출되므로 직접 호출 X
	public virtual void InitializePoolItem()
	{
		isSpawning = true;

		onSpawn?.Invoke(this as TItem);
	}
	// ObjectManager를 통해 스폰하면 자동으로 호출되므로 직접 호출 X
	public virtual void FinallizePoolItem()
	{
		if (isSpawning == true)
			isSpawning = false;

		onDespawn?.Invoke(this as TItem);

		onSpawn = null;
		onDespawn = null;
	}
}