using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-100)]
public abstract class SceneReceiver : MonoBehaviour
{
	private void Awake()
	{
		Initialize();
	}
	private void OnDestroy()
	{
		Finallize();
	}
	protected virtual void Initialize()
	{
		SceneManager.UnloadSceneAsync("Loading Scene");
	}
	protected virtual void Finallize()
	{

	}
}