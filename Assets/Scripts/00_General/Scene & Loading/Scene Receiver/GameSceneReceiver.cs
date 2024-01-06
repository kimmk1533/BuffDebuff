using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneReceiver : SceneReceiver
{
	protected override void Initialize()
	{
		//Scene scene = SceneManager.GetSceneByName("Init Scene");
		//GameObject[] objs = scene.GetRootGameObjects();

		//SceneLoader sceneLoader = null;
		//for (int i = 0; i < objs.Length; ++i)
		//{
		//	sceneLoader = objs[i].GetComponent<SceneLoader>();
		//	if (sceneLoader != null)
		//		break;
		//}

		SceneLoader[] objs = GameObject.FindObjectsOfType<SceneLoader>();
		for (int i = 0; i < objs.Length; ++i)
		{
			objs[i].GetSceneEvent("Game Scene")?.Invoke();
		}

		SceneManager.UnloadSceneAsync("Loading Scene");
	}
	protected override void Finallize()
	{

	}
}