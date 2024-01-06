using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using AYellowpaper.SerializedCollections;

public abstract class SceneLoader : MonoBehaviour
{
	#region 이벤트
	//[SerializeField]
	[SerializedDictionary("Scene Name", "Scene Load Completed Event")]
	protected SerializedDictionary<string, UnityEvent> m_OnLoadCompletedMap = null;
	#endregion

	protected virtual void Awake()
	{
		Initialize();
	}
	protected virtual void OnDestroy()
	{
		Finallize();
	}

	protected virtual void Initialize()
	{
		if (m_OnLoadCompletedMap == null)
			m_OnLoadCompletedMap = new SerializedDictionary<string, UnityEvent>();
	}
	protected virtual void Finallize()
	{
		if (m_OnLoadCompletedMap != null)
			m_OnLoadCompletedMap.Clear();
	}

	public UnityEvent GetSceneEvent(string sceneName)
	{
		if (m_OnLoadCompletedMap.TryGetValue(sceneName, out UnityEvent unityEvent) == false)
			return null;

		return unityEvent;
	}
}