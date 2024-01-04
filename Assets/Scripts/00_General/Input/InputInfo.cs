using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct InputInfo
{
	#region 변수
	[SerializeField, ReadOnly(true)]
	private string m_KeyName;
	[SerializeField, ReadOnly(true)]
	private float m_Value;

	[SerializeField, ReadOnly(true)]
	private KeyCode m_KeyCode;

	[SerializeField, ReadOnly]
	private bool m_IsInputDown;
	[SerializeField, ReadOnly]
	private bool m_IsInput;
	[SerializeField, ReadOnly]
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