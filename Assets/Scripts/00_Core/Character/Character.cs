using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Character<T> : MonoBehaviour where T : CharacterStat, new()
{
	// 최대 스탯
	[SerializeField]
	protected T m_MaxStat;
	[Space(10)]
	// 현재 스탯
	[SerializeField]
	protected T m_CurrentStat;

	public T maxStat => m_MaxStat;
	public T currentStat => m_CurrentStat;

	public abstract void Initialize();
}