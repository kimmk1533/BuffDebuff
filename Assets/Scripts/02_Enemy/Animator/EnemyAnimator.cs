using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuffDebuff
{
	public class EnemyAnimator : CharacterAnimator
	{
		#region Enum
		private enum E_AnimState
		{
			None = 0,

			Idle = 0,
			Run = 1,

			Max
		}
		#endregion

		#region 변수
		private Enemy m_Enemy;
		private EnemyCharacter m_EnemyCharacter;
		#endregion

		public override void Initialize()
		{
			base.Initialize();

			this.Safe_GetComponentInParent<Enemy>(ref m_Enemy);
			this.Safe_GetComponentInParent<EnemyCharacter>(ref m_EnemyCharacter);
		}
	}
}