using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 근접 공격 범위 감소
 */
[System.Serializable]
public class 공격_범위_감소 : AbstractBuff, IOnBuffAdded, IOnBuffRemoved
{
	public 공격_범위_감소(BuffData buffData) : base(buffData)
	{

	}
	
	public void OnBuffAdded<TStat, TController, TAnimator>(Character<TStat, TController, TAnimator> character) where TStat : CharacterStat, new() where TController : Controller2D where TAnimator : CharacterAnimator
	{

	}
	public void OnBuffRemoved<TStat, TController, TAnimator>(Character<TStat, TController, TAnimator> character) where TStat : CharacterStat, new() where TController : Controller2D where TAnimator : CharacterAnimator
	{

	}
}