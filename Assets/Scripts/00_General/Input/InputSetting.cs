using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AYellowpaper.SerializedCollections;

namespace BuffDebuff
{
	[CreateAssetMenu(fileName = "Input Setting", menuName = "Scriptable Object/Input Setting", order = int.MinValue)]
	public class InputSetting : ScriptableObject
	{
		#region 변수
		[SerializeField]
		[SerializedDictionary("Input Type", "Input Info")]
		private SerializedDictionary<E_InputType, InputInfo> m_InputMap;
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