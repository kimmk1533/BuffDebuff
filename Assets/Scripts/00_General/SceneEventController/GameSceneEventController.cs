using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuffDebuff
{
	public class GameSceneEventController : SceneEventController
	{
		#region 기본 템플릿
		#region 변수
		private Camera m_MainGameCamera = null;
		private Camera m_MiniMapCamera = null;

		[SerializeField]
		private UICanvas m_GameCanvas = null;
		[SerializeField]
		private BuffCombineUIPanel m_BuffCombineUIPanel = null;
		#endregion

		#region 프로퍼티

		#endregion

		#region 이벤트

		#region 이벤트 함수

		#endregion
		#endregion

		#region 매니저
		private static GameManager M_Game => GameManager.Instance;

		private static BuffUIManager M_BuffUI => BuffUIManager.Instance;
		#endregion

		#region 유니티 콜백 함수
		protected override void OnApplicationQuit()
		{
			M_Game.FinallizeOnGameScene();

			M_Game.Finallize();
		}
		#endregion

		#region 초기화 & 마무리화 함수
		/// <summary>
		/// 초기화 함수
		/// </summary>
		protected override void Initialize()
		{
			base.Initialize();

			M_BuffUI.gameCanvas = m_GameCanvas;
			M_BuffUI.buffCombineUIPanel = m_BuffCombineUIPanel;

			AddBeforeEvent("Main Menu Scene", M_Game.FinallizeOnGameScene);

			AddAfterEvent("Main Menu Scene", M_Game.InitializeOnMainMenuScene);
		}
		/// <summary>
		/// 마무리화 함수
		/// </summary>
		protected override void Finallize()
		{
			base.Finallize();
		}
		#endregion
		#endregion
	}
}