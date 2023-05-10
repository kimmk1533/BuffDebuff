using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-99)]
public class LoadingManager : Singleton<LoadingManager>
{
	private Dictionary<int, Scene> m_SceneDictionary = new Dictionary<int, Scene>();
#if UNITY_EDITOR
	[SerializeField]
	private DebugDictionary<int, Scene> Debug_SceneDictionary = new DebugDictionary<int, Scene>();
#endif

	private void Awake()
	{
		DontDestroyOnLoad(gameObject);
	}
	

}