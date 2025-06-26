using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BuffDebuff
{
	public class MainSceneEventController : SceneEventController
	{
		#region 변수
		[SerializeField]
		private UICanvas m_UICanvas = null;

		private Button m_QuitButton = null;
		#endregion

		#region 이벤트

		#region 이벤트 함수
		public void OnQuitButtonClick()
		{
#if UNITY_EDITOR
			UnityEditor.EditorApplication.ExitPlaymode();
#else
			Application.Quit();
#endif
		}
		#endregion
		#endregion

		#region 매니저
		private static GameManager M_Game => GameManager.Instance;

		private static PlayerManager M_Player => PlayerManager.Instance;
		#endregion

		#region 유니티 콜백 함수
		protected override void OnApplicationQuit()
		{
			M_Game.Finallize();
		}
		#endregion

		#region 초기화 & 마무리화 함수
		protected override void Initialize()
		{
			base.Initialize();

			LinkButtons();

			AddAfterEvent("Game Scene", M_Game.InitializeEvent);
			AddAfterEvent("Game Scene", M_Game.InitializeMain);
			AddAfterEvent("Game Scene", Physics2D.SyncTransforms);
		}
		protected override void Finallize()
		{
			base.Finallize();
		}
		#endregion

		private void LinkButtons()
		{
			// Quit Button
			m_QuitButton = m_UICanvas.transform.Find<Button>("Main Menu Buttons/Quit Button");
			m_QuitButton.onClick.AddListener(OnQuitButtonClick);

			// Player Select Button
			Transform characterSelectButtonTransform = m_UICanvas.transform.Find("Character Selection Panel");
			for (int i = 0; i < characterSelectButtonTransform.childCount - 1; ++i) // Back Button 제외
			{
				Transform item = characterSelectButtonTransform.GetChild(i);

				string title = item.name;
				Button selectButton = item.Find<Button>("Select Button");

				selectButton.onClick.RemoveAllListeners();
				selectButton.onClick.AddListener(() =>
				{
					M_Player.SelectPlayerData(title);

					SceneLoader.LoadScene("Game Scene");
				});
			}
		}

		[System.Serializable]
		public class PlayerSelectButton
		{
			public string title;
			public Button selectButton;
		}
	}
}