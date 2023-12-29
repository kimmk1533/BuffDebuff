using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManager : Singleton<SceneManager>
{
	private void Awake()
	{
		DontDestroyOnLoad(gameObject);

		LoadingSceneManager.LoadScene("TestScene");
		LoadingSceneManager.onLoadSceneCompleted += GameManager.Instance.Initialize;
		LoadingSceneManager.onLoadSceneCompleted += GameManager.Instance.InitializeGame;
		LoadingSceneManager.onLoadSceneCompleted += GameManager.Instance.InitializeEvent;
	}
}