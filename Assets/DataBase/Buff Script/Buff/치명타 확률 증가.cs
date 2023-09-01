using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 치명타 확률 증가
 */
[System.Serializable]
public class 치명타_확률_증가 : AbstractBuff, IOnBuffAdded, IOnBuffRemoved
{
	public 치명타_확률_증가(BuffData buffData) : base(buffData)
	{

	}
	
	public void OnBuffAdded<T>(Character<T> character) where T : CharacterStat, new()
	{

	}
	public void OnBuffRemoved<T>(Character<T> character) where T : CharacterStat, new()
	{

	}
}