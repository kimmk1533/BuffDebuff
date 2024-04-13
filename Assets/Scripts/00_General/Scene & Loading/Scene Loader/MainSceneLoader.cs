using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuffDebuff
{
	public class MainSceneLoader : SceneLoader
	{
		#region 매니저
		private static GameManager M_Game => GameManager.Instance;
		#endregion

		protected override void Initialize()
		{
			base.Initialize();

			string sceneName = "Game Scene";

			m_OnLoadCompletedMap.Add(sceneName, new UnityEngine.Events.UnityEvent());

			m_OnLoadCompletedMap[sceneName].AddListener(M_Game.InitializeEvent);
			m_OnLoadCompletedMap[sceneName].AddListener(M_Game.InitializeGame);
			m_OnLoadCompletedMap[sceneName].AddListener(Physics2D.SyncTransforms);
		}
		protected override void Finallize()
		{
			base.Finallize();
		}

		public void OnStartGameButtonClick()
		{
			LoadingSceneManager.LoadScene("Game Scene");
		}
		public void OnQuitButtonClick()
		{
#if UNITY_EDITOR
			UnityEditor.EditorApplication.ExitPlaymode();
#else
			Application.Quit();
#endif
		}
	}
}