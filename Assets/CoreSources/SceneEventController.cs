using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

[DefaultExecutionOrder(-99)]
public abstract class SceneEventController : SerializedMonoBehaviour
{
	#region 기본 템플릿
	#region 변수
	[SerializeField, PropertySpace(10, 0)]
	[DictionaryDrawerSettings(KeyLabel = "전환될 씬 이름", ValueLabel = "씬 로딩 전 호출할 이벤트")]
	protected Dictionary<string, UnityEvent> m_OnSceneSwitchingBeforeEventMap = new Dictionary<string, UnityEvent>();
	[SerializeField, PropertySpace(0, 10)]
	[DictionaryDrawerSettings(KeyLabel = "전환된 씬 이름", ValueLabel = "씬 로딩 후 호출할 이벤트")]
	protected Dictionary<string, UnityEvent> m_OnSceneSwitchingAfterEventMap = new Dictionary<string, UnityEvent>();
	#endregion

	#region 프로퍼티

	#endregion

	#region 이벤트

	#region 이벤트 함수
	public void OnBeforeSceneSwitching(string sceneName)
	{
		UnityEvent unityEvent = GetSceneSwitchingBeforeEvent(sceneName);

		unityEvent?.Invoke();
	}
	public void OnAfterSceneSwitching(string sceneName)
	{
		UnityEvent unityEvent = GetSceneSwitchingAfterEvent(sceneName);

		unityEvent?.Invoke();
	}
	#endregion
	#endregion

	#region 매니저

	#endregion

	#region 유니티 콜백 함수
	protected virtual void Awake()
	{
		Initialize();
	}
	protected virtual void OnDestroy()
	{
		Finallize();
	}
	protected abstract void OnApplicationQuit();
	#endregion

	#region 초기화 & 마무리화 함수
	/// <summary>
	/// 초기화 함수
	/// </summary>
	protected virtual void Initialize()
	{
		if (m_OnSceneSwitchingBeforeEventMap == null)
			m_OnSceneSwitchingBeforeEventMap = new Dictionary<string, UnityEvent>();

		if (m_OnSceneSwitchingAfterEventMap == null)
			m_OnSceneSwitchingAfterEventMap = new Dictionary<string, UnityEvent>();
	}
	/// <summary>
	/// 마무리화 함수
	/// </summary>
	protected virtual void Finallize()
	{
		if (m_OnSceneSwitchingBeforeEventMap != null)
		{
			foreach (var item in m_OnSceneSwitchingBeforeEventMap)
			{
				item.Value.RemoveAllListeners();
			}
			m_OnSceneSwitchingBeforeEventMap.Clear();
		}

		if (m_OnSceneSwitchingAfterEventMap != null)
		{
			foreach (var item in m_OnSceneSwitchingAfterEventMap)
			{
				item.Value.RemoveAllListeners();
			}
			m_OnSceneSwitchingAfterEventMap.Clear();
		}
	}
	#endregion
	#endregion

	private UnityEvent GetSceneSwitchingBeforeEvent(string sceneName)
	{
		if (m_OnSceneSwitchingBeforeEventMap.TryGetValue(sceneName, out UnityEvent unityEvent) == false)
			return null;

		return unityEvent;
	}
	private UnityEvent GetSceneSwitchingAfterEvent(string sceneName)
	{
		if (m_OnSceneSwitchingAfterEventMap.TryGetValue(sceneName, out UnityEvent unityEvent) == false)
			return null;

		return unityEvent;
	}

	public void AddBeforeEvent(string sceneName, UnityAction action)
	{
		if (m_OnSceneSwitchingBeforeEventMap.TryGetValue(sceneName, out UnityEvent unityEvent) == false)
		{
			unityEvent = new UnityEvent();

			m_OnSceneSwitchingBeforeEventMap.Add(sceneName, unityEvent);
		}

		unityEvent.AddListener(action);
	}
	public void AddAfterEvent(string sceneName, UnityAction action)
	{
		if (m_OnSceneSwitchingAfterEventMap.TryGetValue(sceneName, out UnityEvent unityEvent) == false)
		{
			unityEvent = new UnityEvent();

			m_OnSceneSwitchingAfterEventMap.Add(sceneName, unityEvent);
		}

		unityEvent.AddListener(action);
	}
}