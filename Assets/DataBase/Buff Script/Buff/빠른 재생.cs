using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 체력 회복 주기 감소
 */
[System.Serializable]
public class 빠른_재생 : AbstractBuff, IOnBuffAdded, IOnBuffRemoved
{
	public 빠른_재생(BuffData buffData) : base(buffData)
	{

	}
	
	public void OnBuffAdded<TStat, TController, TAnimator>(Character<TStat, TController, TAnimator> character) where TStat : CharacterStat, new() where TController : Controller2D where TAnimator : CharacterAnimator
	{

	}
	public void OnBuffRemoved<TStat, TController, TAnimator>(Character<TStat, TController, TAnimator> character) where TStat : CharacterStat, new() where TController : Controller2D where TAnimator : CharacterAnimator
	{

	}
}