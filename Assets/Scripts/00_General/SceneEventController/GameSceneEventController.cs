using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuffDebuff
{
	public class GameSceneEventController : SceneEventController
	{
		#region 변수
		private Camera m_MainGameCamera = null;
		private Camera m_MiniMapCamera = null;
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

		protected override void Initialize()
		{
			base.Initialize();
		}
		protected override void Finallize()
		{
			base.Finallize();
		}
	}
}