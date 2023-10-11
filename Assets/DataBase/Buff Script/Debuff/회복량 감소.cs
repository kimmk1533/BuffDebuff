using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 받는 모든 회복량 감소
 */
[System.Serializable]
public class 회복량_감소 : AbstractBuff, IOnBuffAdded, IOnBuffRemoved
{
	public 회복량_감소(BuffData buffData) : base(buffData)
	{

	}
	
	public void OnBuffAdded<TStat, TController, TAnimator>(Character<TStat, TController, TAnimator> character) where TStat : CharacterStat, new() where TController : Controller2D where TAnimator : CharacterAnimator
	{

	}
	public void OnBuffRemoved<TStat, TController, TAnimator>(Character<TStat, TController, TAnimator> character) where TStat : CharacterStat, new() where TController : Controller2D where TAnimator : CharacterAnimator
	{

	}
}