using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffManager : Singleton<BuffManager>
{
	public Dictionary<int, Buff> m_BuffDictionary;

	private void Start()
	{
		m_BuffDictionary = new Dictionary<int, Buff>();

		
	}
}