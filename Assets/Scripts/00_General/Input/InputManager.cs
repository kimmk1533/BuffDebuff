using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BuffDebuff
{
	public enum E_InputType
	{
		PlayerMoveUp,
		PlayerMoveDown,
		PlayerMoveLeft,
		PlayerMoveRight,
		PlayerAttack,
		PlayerJump,
		PlayerDash,
		PlayerSkill01,

		Max
	}

	[DefaultExecutionOrder(-100)]
	public class InputManager : Singleton<InputManager>
	{
		#region 변수
		[SerializeField]
		private InputSetting m_Setting = null;

		[SerializeField]
		[DictionaryDrawerSettings(KeyLabel = "Input Type", ValueLabel = "Input Info")]
		private Dictionary<E_InputType, InputInfo> m_InputMap = null;
		private Dictionary<string, List<E_InputType>> m_AxisMap = null;
		private Dictionary<string, float> m_AxisValueMap = null;
		#endregion

		#region 매니저
		private static GameManager M_Game => GameManager.Instance;
		private static BuffUIManager M_BuffUI => BuffUIManager.Instance;
		#endregion

		public void Initialize()
		{
			if (m_AxisMap == null)
				m_AxisMap = new Dictionary<string, List<E_InputType>>();
			if (m_AxisValueMap == null)
				m_AxisValueMap = new Dictionary<string, float>();

			UpdateInfoMapFromSetting();
			UpdateAxisMap();
		}
		public void Finallize()
		{
			foreach (var item in m_AxisMap)
			{
				item.Value.Clear();
			}
		}

		private void Update()
		{
			CheckInput();
		}
		private void CheckInput()
		{
			if (M_Game.isInGame == false)
				return;

			for (E_InputType inputType = 0; inputType < E_InputType.Max; ++inputType)
			{
				InputInfo input = m_InputMap[inputType];

				input.isInputDown = Input.GetKeyDown(input.keyCode);
				input.isInput = Input.GetKey(input.keyCode);
				input.isInputUp = Input.GetKeyUp(input.keyCode);

				m_InputMap[inputType] = input;
			}

			#region Player Attack & UI Click 충돌 해결
			if (M_BuffUI.isUIOpened == true)
			{
				InputInfo info = m_InputMap[E_InputType.PlayerAttack];

				info.isInputDown = false;
				info.isInput = false;
				info.isInputUp = false;

				m_InputMap[E_InputType.PlayerAttack] = info;
			}
			#endregion
		}

		public float GetAxisRaw(string axisName)
		{
			if (m_AxisMap.TryGetValue(axisName, out List<E_InputType> axisList) == false)
				return 0f;

			float value = 0f;

			for (int i = 0; i < axisList.Count; ++i)
			{
				E_InputType inputType = axisList[i];

				if (m_InputMap[inputType].isInput)
					value += m_InputMap[inputType].value;
			}

			return Mathf.Clamp(value, -1f, 1f);
		}
		public bool GetKeyDown(E_InputType type)
		{
			if (m_InputMap.TryGetValue(type, out InputInfo info) == false)
				return false;

			return info.isInputDown;
		}
		public bool GetKey(E_InputType type)
		{
			if (m_InputMap.TryGetValue(type, out InputInfo info) == false)
				return false;

			return info.isInput;
		}
		public bool GetKeyUp(E_InputType type)
		{
			if (m_InputMap.TryGetValue(type, out InputInfo info) == false)
				return false;

			return info.isInputUp;
		}

		[ContextMenu("Update InfoMap From Setting")]
		private void UpdateInfoMapFromSetting()
		{
			for (E_InputType inputType = 0; inputType < E_InputType.Max; ++inputType)
			{
				InputInfo input = m_Setting.GetInputInfo(inputType);

				input.isInputDown = false;
				input.isInput = false;
				input.isInputUp = false;

				if (m_InputMap.ContainsKey(inputType) == true)
					m_InputMap[inputType] = input;
				else
					m_InputMap.Add(inputType, input);
			}
		}
		[ContextMenu("Update Setting From InfoMap")]
		private void UpdateSettingFromInfoMap()
		{
			for (E_InputType inputType = 0; inputType < E_InputType.Max; ++inputType)
			{
				m_Setting.SetInputInfo(inputType, m_InputMap[inputType]);
			}
		}
		private void UpdateAxisMap()
		{
			for (E_InputType inputType = 0; inputType < E_InputType.Max; ++inputType)
			{
				InputInfo input = m_Setting.GetInputInfo(inputType);

				if (m_AxisMap.ContainsKey(input.keyName) == false)
					m_AxisMap.Add(input.keyName, new List<E_InputType>());
				if (m_AxisValueMap.ContainsKey(input.keyName) == false)
					m_AxisValueMap.Add(input.keyName, 0f);

				m_AxisMap[input.keyName].Add(inputType);
			}
		}
	}
}