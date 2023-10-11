using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 이동속도 고정 증가
 */
[System.Serializable]
public class 이동_속도_증가 : AbstractBuff, IOnBuffAdded, IOnBuffRemoved
{
	public 이동_속도_증가(BuffData buffData) : base(buffData)
	{

	}
	
	public void OnBuffAdded<TStat, TController, TAnimator>(Character<TStat, TController, TAnimator> character) where TStat : CharacterStat, new() where TController : Controller2D where TAnimator : CharacterAnimator
	{

	}
	public void OnBuffRemoved<TStat, TController, TAnimator>(Character<TStat, TController, TAnimator> character) where TStat : CharacterStat, new() where TController : Controller2D where TAnimator : CharacterAnimator
	{

	}
}