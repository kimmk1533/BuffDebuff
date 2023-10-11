using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 투사체 사거리 증가
 */
[System.Serializable]
public class 사거리_증가 : AbstractBuff, IOnBuffAdded, IOnBuffRemoved
{
	public 사거리_증가(BuffData buffData) : base(buffData)
	{

	}
	
	public void OnBuffAdded<TStat, TController, TAnimator>(Character<TStat, TController, TAnimator> character) where TStat : CharacterStat, new() where TController : Controller2D where TAnimator : CharacterAnimator
	{

	}
	public void OnBuffRemoved<TStat, TController, TAnimator>(Character<TStat, TController, TAnimator> character) where TStat : CharacterStat, new() where TController : Controller2D where TAnimator : CharacterAnimator
	{

	}
}