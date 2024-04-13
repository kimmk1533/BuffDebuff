using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuffDebuff
{
	public class StartRoom : Room
	{
		#region 변수
		[SerializeField]
		private Transform m_StartPosition;
		#endregion

		#region 프로퍼티
		public Vector3 startPos => m_StartPosition.position;
		#endregion

		#region 이벤트
		#endregion

		#region 매니저
		#endregion

	}
}