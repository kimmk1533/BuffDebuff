using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 대쉬가 충전되는 시간 감소
 */
[System.Serializable]
public class 대쉬_충전_속도_증가 : AbstractBuff, IOnBuffAdded, IOnBuffRemoved
{
	public 대쉬_충전_속도_증가(BuffData buffData) : base(buffData)
	{

	}
	
	public void OnBuffAdded<T>(Character<T> character) where T : CharacterStat, new()
	{

	}
	public void OnBuffRemoved<T>(Character<T> character) where T : CharacterStat, new()
	{

	}
}