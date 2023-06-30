using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 최대 대쉬 횟수 증가
 */
[System.Serializable]
public class 대쉬_횟수_증가 : AbstractBuff, IOnBuffAdded, IOnBuffRemoved
{
	public 대쉬_횟수_증가(BuffData buffData) : base(buffData)
	{

	}
	
	public void OnBuffAdded<T>(Character<T> character) where T : CharacterStat, new()
	{

	}
	public void OnBuffRemoved<T>(Character<T> character) where T : CharacterStat, new()
	{

	}
}