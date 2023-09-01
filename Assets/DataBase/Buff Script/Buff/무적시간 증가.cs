using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 모든 무적 시간 증가
 */
[System.Serializable]
public class 무적시간_증가 : AbstractBuff, IOnBuffAdded, IOnBuffRemoved
{
	public 무적시간_증가(BuffData buffData) : base(buffData)
	{

	}
	
	public void OnBuffAdded<T>(Character<T> character) where T : CharacterStat, new()
	{

	}
	public void OnBuffRemoved<T>(Character<T> character) where T : CharacterStat, new()
	{

	}
}