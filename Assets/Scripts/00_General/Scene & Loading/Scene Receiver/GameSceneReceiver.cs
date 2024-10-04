using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuffDebuff
{
	public class GameSceneReceiver : SceneReceiver
	{
		#region 변수
		private Camera m_MainGameCamera = null;
		private Camera m_MiniMapCamera = null;
		#endregion

		protected override void Initialize()
		{
			SceneLoader[] objs = GameObject.FindObjectsOfType<SceneLoader>();
			for (int i = 0; i < objs.Length; ++i)
			{
				objs[i].GetSceneEvent("Game Scene")?.Invoke();
			}

			base.Initialize();
		}
		protected override void Finallize()
		{

		}
	}
}