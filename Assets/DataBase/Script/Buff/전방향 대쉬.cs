using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 대쉬가 좌우에서 전방향으로 변경
 */
[System.Serializable]
public class 전방향_대쉬 : AbstractBuff, IOnBuffAdded, IOnBuffRemoved
{
	public 전방향_대쉬(BuffData buffData) : base(buffData)
	{

	}
	
	public void OnBuffAdded<T>(Character<T> character) where T : CharacterStat, new()
	{

	}
	public void OnBuffRemoved<T>(Character<T> character) where T : CharacterStat, new()
	{

	}
}