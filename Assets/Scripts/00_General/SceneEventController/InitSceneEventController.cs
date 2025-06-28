using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuffDebuff
{
	public class InitSceneEventController : SceneEventController
	{
		#region 기본 템플릿
		#region 변수

		#endregion

		#region 프로퍼티

		#endregion

		#region 이벤트

		#region 이벤트 함수

		#endregion
		#endregion

		#region 매니저
		private static GameManager M_Game => GameManager.Instance;
		#endregion

		#region 유니티 콜백 함수
		protected override void OnApplicationQuit()
		{
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

			M_Game.Initialize();

			AddAfterEvent("Main Menu Scene", M_Game.InitializeOnMainMenuScene);

			SceneLoader.LoadScene("Main Menu Scene");
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