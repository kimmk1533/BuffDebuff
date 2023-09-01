using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 회피율 고정 감소
 */
[System.Serializable]
public class 회피율_감소 : AbstractBuff, IOnBuffAdded, IOnBuffRemoved
{
	public 회피율_감소(BuffData buffData) : base(buffData)
	{

	}
	
	public void OnBuffAdded<T>(Character<T> character) where T : CharacterStat, new()
	{

	}
	public void OnBuffRemoved<T>(Character<T> character) where T : CharacterStat, new()
	{

	}
}