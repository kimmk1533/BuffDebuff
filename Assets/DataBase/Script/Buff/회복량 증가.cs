using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 받는 모든 회복량 증가
 */
[System.Serializable]
public class 회복량_증가 : AbstractBuff, IOnBuffAdded, IOnBuffRemoved
{
	public 회복량_증가(BuffData buffData) : base(buffData)
	{

	}
	
	public void OnBuffAdded<T>(Character<T> character) where T : CharacterStat, new()
	{

	}
	public void OnBuffRemoved<T>(Character<T> character) where T : CharacterStat, new()
	{

	}
}