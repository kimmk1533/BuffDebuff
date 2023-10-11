using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 자동으로 패링할 확률 감소
 */
[System.Serializable]
public class 패링_확률_감소 : AbstractBuff, IOnBuffAdded, IOnBuffRemoved
{
	public 패링_확률_감소(BuffData buffData) : base(buffData)
	{

	}
	
	public void OnBuffAdded<TStat, TController, TAnimator>(Character<TStat, TController, TAnimator> character) where TStat : CharacterStat, new() where TController : Controller2D where TAnimator : CharacterAnimator
	{

	}
	public void OnBuffRemoved<TStat, TController, TAnimator>(Character<TStat, TController, TAnimator> character) where TStat : CharacterStat, new() where TController : Controller2D where TAnimator : CharacterAnimator
	{

	}
}