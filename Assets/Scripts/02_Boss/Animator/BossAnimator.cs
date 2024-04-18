using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuffDebuff
{
	public class BossAnimator : CharacterAnimator
	{
		#region 변수
		private Boss m_Boss;
		private BossCharacter m_BossCharacter;
		#endregion

		public override void Initialize()
		{
			base.Initialize();

			this.Safe_GetComponentInParent<Boss>(ref m_Boss);
			this.Safe_GetComponentInParent<BossCharacter>(ref m_BossCharacter);
		}
	}
}
