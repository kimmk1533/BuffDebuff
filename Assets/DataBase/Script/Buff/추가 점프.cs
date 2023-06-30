using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 최대 점프 횟수 증가
 */
[System.Serializable]
public class 추가_점프 : AbstractBuff, IOnBuffAdded, IOnBuffRemoved
{
	public 추가_점프(BuffData buffData) : base(buffData)
	{

	}
	
	public void OnBuffAdded<T>(Character<T> character) where T : CharacterStat, new()
	{

	}
	public void OnBuffRemoved<T>(Character<T> character) where T : CharacterStat, new()
	{

	}
}