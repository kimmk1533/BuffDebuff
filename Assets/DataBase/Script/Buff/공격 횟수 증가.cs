using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 타격 시 같은 대상에게만 추가 타격(다단 히트가 생김)
 */
[System.Serializable]
public class 공격_횟수_증가 : AbstractBuff, IOnBuffGiveDamage, IOnBuffTakeDamage
{
	public 공격_횟수_증가(BuffData buffData) : base(buffData)
	{

	}
	
	public void OnBuffGiveDamage<T>(Character<T> character) where T : CharacterStat, new()
	{

	}
	public void OnBuffTakeDamage<T>(Character<T> character) where T : CharacterStat, new()
	{

	}
}