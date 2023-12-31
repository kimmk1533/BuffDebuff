using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitSceneLoader : SceneLoader
{
	#region 매니저
	private static GameManager M_Game => GameManager.Instance;
	#endregion

	protected override void Initialize()
	{
		m_OnSceneLoadCompleted.AddListener(Physics2D.SyncTransforms);

		LoadingSceneManager.LoadScene("Game Scene");
	}
	protected override void Finallize()
	{

	}
}