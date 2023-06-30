using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 투사체 생존 시간 감소
 */
[System.Serializable]
public class 사거리_감소 : AbstractBuff, IOnBuffAdded, IOnBuffRemoved
{
	public 사거리_감소(BuffData buffData) : base(buffData)
	{

	}
	
	public void OnBuffAdded<T>(Character<T> character) where T : CharacterStat, new()
	{

	}
	public void OnBuffRemoved<T>(Character<T> character) where T : CharacterStat, new()
	{

	}
}