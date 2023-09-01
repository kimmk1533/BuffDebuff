using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 투사체 사거리 증가
 */
[System.Serializable]
public class 사거리_증가 : AbstractBuff, IOnBuffAdded, IOnBuffRemoved
{
	public 사거리_증가(BuffData buffData) : base(buffData)
	{

	}
	
	public void OnBuffAdded<T>(Character<T> character) where T : CharacterStat, new()
	{

	}
	public void OnBuffRemoved<T>(Character<T> character) where T : CharacterStat, new()
	{

	}
}