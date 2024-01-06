using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainSceneReceiver : SceneReceiver
{
	protected override void Initialize()
	{
		SceneManager.UnloadSceneAsync("Loading Scene");
	}
	protected override void Finallize()
	{
		
	}
}