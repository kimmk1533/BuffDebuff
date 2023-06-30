using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 대쉬 종료 이후 일정 시간 무적
 */
[System.Serializable]
public class 대쉬_무적 : AbstractBuff, IOnBuffDash
{
	public 대쉬_무적(BuffData buffData) : base(buffData)
	{

	}
	
	public void OnBuffDash<T>(Character<T> character) where T : CharacterStat, new()
	{

	}
}