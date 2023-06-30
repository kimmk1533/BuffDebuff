using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 받는 모든 회복량 감소
 */
[System.Serializable]
public class 회복량_감소 : AbstractBuff, IOnBuffAdded, IOnBuffRemoved
{
	public 회복량_감소(BuffData buffData) : base(buffData)
	{

	}
	
	public void OnBuffAdded<T>(Character<T> character) where T : CharacterStat, new()
	{

	}
	public void OnBuffRemoved<T>(Character<T> character) where T : CharacterStat, new()
	{

	}
}