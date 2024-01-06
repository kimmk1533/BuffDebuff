using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AwakeObject : MonoBehaviour
{
	[System.Flags]
	private enum E_Type
	{
		DestroyObject = 1 << 0,
		DontDestroyOnLoad = 1 << 1,
		SetActiveTrue = 1 << 2,
		SetActiveFalse = 1 << 3,
	}

	[SerializeField]
	private E_Type m_Type;

	private void Awake()
	{
		Initialize();
	}

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
	}
}