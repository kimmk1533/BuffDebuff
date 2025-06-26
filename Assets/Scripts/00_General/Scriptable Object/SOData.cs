using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BuffDebuff
{
	public class SOData : ScriptableObject
	{
		#region 변수
		// 에셋 경로
		[SerializeField, ReadOnly]
		protected string m_AssetPath;
		// 코드
		[SerializeField, ReadOnly]
		protected int m_Code;
		// 명칭
		[SerializeField, ReadOnly]
		protected string m_Title;
		#endregion

		#region 프로퍼티
		// 에셋 경로
		public string assetPath => m_AssetPath;
		// 코드
		public int code => m_Code;
		// 명칭
		public string title => m_Title;

		#endregion
	}
}