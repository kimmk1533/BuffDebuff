using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuffDebuff
{
	public class BossManager : ObjectManager<BossManager, Boss>
	{
		#region 변수

		#endregion

#if UNITY_EDITOR
		[ContextMenu("Load Origin")]
		protected override void LoadOrigin()
		{
			LoadOrigin_Inner();
		}
#endif
	}
}
