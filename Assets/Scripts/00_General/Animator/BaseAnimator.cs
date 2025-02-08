using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BuffDebuff
{
	/*
	 * 함수명 설명
	 * Anim_Func: 다른 스크립트에서 호출할 함수
	 * AnimEvent_Func: 애니메이션 이벤트에서 호출할 함수
	 */
	public class BaseAnimator : SerializedMonoBehaviour
	{
		#region 변수
		protected SpriteRenderer m_SpriteRenderer = null;
		protected Animator m_Animator = null;
		#endregion

		#region 유니티 콜백 함수
		#endregion

		/// <summary>
		/// 초기화 함수
		/// </summary>
		public virtual void Initialize()
		{
			this.NullCheckGetComponent<SpriteRenderer>(ref m_SpriteRenderer);
			this.NullCheckGetComponent<Animator>(ref m_Animator);
		}
		/// <summary>
		/// 마무리화 함수
		/// </summary>
		public virtual void Finallize()
		{

		}
	}
}