using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class SceneLoader : MonoBehaviour
{
	#region 이벤트
	[Space(10)]
	[SerializeField]
	protected UnityEvent m_OnSceneLoadCompleted;
	public UnityEvent onSceneLoadCompleted => m_OnSceneLoadCompleted;
	#endregion

	protected virtual void Awake()
	{
		Initialize();
	}
	protected virtual void OnDestroy()
	{
		Finallize();
	}
	protected abstract void Initialize();
	protected abstract void Finallize();
}