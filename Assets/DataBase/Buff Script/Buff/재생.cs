using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 일정 시간마다 체력 회복
 */
[System.Serializable]
public class 재생 : AbstractBuff, IOnBuffTimer
{
	public 재생(BuffData buffData) : base(buffData)
	{

	}
	
	public void OnBuffTimer<TStat, TController, TAnimator>(Character<TStat, TController, TAnimator> character) where TStat : CharacterStat, new() where TController : Controller2D where TAnimator : CharacterAnimator
	{

	}
}