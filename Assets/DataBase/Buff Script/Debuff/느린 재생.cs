using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 체력 회복 주기 증가
 */
[System.Serializable]
public class 느린_재생 : AbstractBuff, IOnBuffAdded, IOnBuffRemoved
{
	public 느린_재생(BuffData buffData) : base(buffData)
	{

	}
	
	public void OnBuffAdded<T>(Character<T> character) where T : CharacterStat, new()
	{

	}
	public void OnBuffRemoved<T>(Character<T> character) where T : CharacterStat, new()
	{

	}
}