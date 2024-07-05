using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuffDebuff
{
	public sealed class ProjectileAnimator : MonoBehaviour
	{
		#region 변수
		private Animator m_Animator;
		#endregion

		#region 매니저
		private static ProjectileManager M_Projectile => ProjectileManager.Instance;
		#endregion

		public void Initialize()
		{
			this.NullCheckGetComponent<Animator>(ref m_Animator);
		}
		public void Finallize()
		{

		}
	}
}