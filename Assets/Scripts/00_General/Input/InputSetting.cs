using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BuffDebuff
{
	[CreateAssetMenu(fileName = "Input Setting", menuName = "Scriptable Objects/Input Setting", order = int.MinValue)]
	public class InputSetting : SerializedScriptableObject
	{
		#region 변수
		[SerializeField]
		[DictionaryDrawerSettings(KeyLabel = "Input Type", ValueLabel = "Input Info")]
		private Dictionary<E_InputType, InputInfo> m_InputMap = null;
		#endregion

		public InputInfo GetInputInfo(E_InputType inputType)
		{
			return m_InputMap[inputType];
		}
		public void SetInputInfo(E_InputType inputType, InputInfo info)
		{
			if (m_InputMap.ContainsKey(inputType) == true)
				m_InputMap[inputType] = info;
			else
				m_InputMap.Add(inputType, info);
		}
	}
}