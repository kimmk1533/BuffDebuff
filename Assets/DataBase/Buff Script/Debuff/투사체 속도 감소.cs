using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 투사체 이동 속도 감소
 */
[System.Serializable]
public class 투사체_속도_감소 : AbstractBuff, IOnBuffAdded, IOnBuffRemoved
{
	public 투사체_속도_감소(BuffData buffData) : base(buffData)
	{

	}
	
	public void OnBuffAdded<T>(Character<T> character) where T : CharacterStat, new()
	{

	}
	public void OnBuffRemoved<T>(Character<T> character) where T : CharacterStat, new()
	{

	}
}