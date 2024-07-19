using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuffDebuff
{
	public class InitSceneLoader : SceneLoader
	{
		protected override void Initialize()
		{
			base.Initialize();

			LoadingSceneManager.LoadScene("Main Menu Scene");
		}
		protected override void Finallize()
		{
			base.Finallize();
		}
	}
}