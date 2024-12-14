using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

public abstract class SceneLoader : SerializedMonoBehaviour
{
	#region 이벤트
	//[SerializeField]
	[DictionaryDrawerSettings(KeyLabel = "Scene Name", ValueLabel = "Scene Load Completed Event")]
	protected Dictionary<string, UnityEvent> m_OnLoadCompletedMap = null;
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
			m_OnLoadCompletedMap = new Dictionary<string, UnityEvent>();
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