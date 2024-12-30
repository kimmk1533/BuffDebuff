using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BuffDebuff
{
	[System.Serializable]
	public struct InputInfo
	{
		#region 변수
		[SerializeField]
		private string m_KeyName;
		[SerializeField]
		private float m_Value;

		[SerializeField]
		private KeyCode m_KeyCode;

		[SerializeField, Sirenix.OdinInspector.ReadOnly]
		[FoldoutGroup("Debug")]
		private bool m_IsInputDown;
		[SerializeField, Sirenix.OdinInspector.ReadOnly]
		[FoldoutGroup("Debug")]
		private bool m_IsInput;
		[SerializeField, Sirenix.OdinInspector.ReadOnly]
		[FoldoutGroup("Debug")]
		private bool m_IsInputUp;
		#endregion

		#region 프로퍼티
		public string keyName
		{
			get => m_KeyName;
		}
		public float value
		{
			get => m_Value;
		}
		public KeyCode keyCode
		{
			get => m_KeyCode;
			set => m_KeyCode = value;
		}
		public bool isInputDown
		{
			get => m_IsInputDown;
			set => m_IsInputDown = value;
		}
		public bool isInput
		{
			get => m_IsInput;
			set => m_IsInput = value;
		}
		public bool isInputUp
		{
			get => m_IsInputUp;
			set => m_IsInputUp = value;
		}
		#endregion
	}
}