using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class AwakeObject : MonoBehaviour
{
	#region Enum
	[System.Flags]
	private enum E_Type
	{
		DestroyObject = 1 << 0,
		DontDestroyOnLoad = 1 << 1,
		SetActiveTrue = 1 << 2,
		SetActiveFalse = 1 << 3,
		MakeError = 1 << 4,
	}
	#endregion

	#region 변수
	[SerializeField]
	private E_Type m_Type = (E_Type)(0);
	#endregion

	#region 유니티 콜백 함수
	private void Awake()
	{
		Initialize();
	}
	#endregion

	public void Initialize()
	{
		if (m_Type.HasFlag(E_Type.DestroyObject) == true)
		{
			Destroy(gameObject);
			return;
		}

		if (m_Type.HasFlag(E_Type.DontDestroyOnLoad) == true)
			DontDestroyOnLoad(gameObject);

		if (m_Type.HasFlag(E_Type.SetActiveTrue) == true && m_Type.HasFlag(E_Type.SetActiveFalse) == false)
			gameObject.SetActive(true);

		if (m_Type.HasFlag(E_Type.SetActiveTrue) == false && m_Type.HasFlag(E_Type.SetActiveFalse) == true)
			gameObject.SetActive(false);

		if (m_Type.HasFlag(E_Type.MakeError) == true)
			Debug.LogError("");
	}
}