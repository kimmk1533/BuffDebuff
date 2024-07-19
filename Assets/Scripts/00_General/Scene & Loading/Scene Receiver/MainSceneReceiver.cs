using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BuffDebuff
{
	public class MainSceneReceiver : SceneReceiver
	{
		#region 변수
		[SerializeField]
		private MainSceneLoader m_MainSceneLoader = null;

		[SerializeField]
		private List<PlayerSelectButton> m_PlayerSelectButtonList = null;
		#endregion

		#region 매니저
		private static PlayerManager M_Player => PlayerManager.Instance;
		#endregion

		protected override void Initialize()
		{
			base.Initialize();

			LinkPlayerButton();
		}
		protected override void Finallize()
		{

		}

		private void LinkPlayerButton()
		{
			foreach (PlayerSelectButton item in m_PlayerSelectButtonList)
			{
				Button button = item.selectButton;
				button.onClick.RemoveAllListeners();
				button.onClick.AddListener(() =>
				{
					M_Player.SelectPlayerData(item.title);
					m_MainSceneLoader.OnStartGameButtonClick();
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