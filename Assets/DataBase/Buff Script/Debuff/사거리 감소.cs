using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 투사체 생존 시간 감소
 */
[System.Serializable]
public class 사거리_감소 : AbstractBuff, IOnBuffAdded, IOnBuffRemoved
{
	public 사거리_감소(BuffData buffData) : base(buffData)
	{

	}
	
	public void OnBuffAdded<TStat, TController, TAnimator>(Character<TStat, TController, TAnimator> character) where TStat : CharacterStat, new() where TController : Controller2D where TAnimator : CharacterAnimator
	{

	}
	public void OnBuffRemoved<TStat, TController, TAnimator>(Character<TStat, TController, TAnimator> character) where TStat : CharacterStat, new() where TController : Controller2D where TAnimator : CharacterAnimator
	{

	}
}