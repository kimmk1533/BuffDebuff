using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuffDebuff
{
	public class InitSceneEventController : SceneEventController
	{
		#region 매니저
		private static GameManager M_Game => GameManager.Instance;
		#endregion

		#region 유니티 콜백 함수
		protected override void OnApplicationQuit()
		{
			M_Game.Finallize();
		}
		#endregion

		protected override void Initialize()
		{
			base.Initialize();

			SceneLoader.LoadScene("Main Menu Scene");
		}
		protected override void Finallize()
		{
			base.Finallize();
		}
	}
}