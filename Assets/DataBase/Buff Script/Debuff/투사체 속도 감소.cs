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
	
	public void OnBuffAdded<TStat, TController, TAnimator>(Character<TStat, TController, TAnimator> character) where TStat : CharacterStat, new() where TController : Controller2D where TAnimator : CharacterAnimator
	{

	}
	public void OnBuffRemoved<TStat, TController, TAnimator>(Character<TStat, TController, TAnimator> character) where TStat : CharacterStat, new() where TController : Controller2D where TAnimator : CharacterAnimator
	{

	}
}