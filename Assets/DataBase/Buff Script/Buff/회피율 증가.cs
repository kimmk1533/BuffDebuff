using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 회피율 고정 증가
 */
[System.Serializable]
public class 회피율_증가 : AbstractBuff, IOnBuffAdded, IOnBuffRemoved
{
	public 회피율_증가(BuffData buffData) : base(buffData)
	{

	}
	
	public void OnBuffAdded<T>(Character<T> character) where T : CharacterStat, new()
	{

	}
	public void OnBuffRemoved<T>(Character<T> character) where T : CharacterStat, new()
	{

	}
}