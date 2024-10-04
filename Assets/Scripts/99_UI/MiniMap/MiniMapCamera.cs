using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuffDebuff
{
	public class MiniMapCamera : MonoBehaviour
	{
		#region 변수
		private Camera m_Camera = null; 
		#endregion

		public void Initialize()
		{
			this.NullCheckGetComponent<Camera>(ref m_Camera);
		}
		public void Finallize()
		{

		}
	}
}