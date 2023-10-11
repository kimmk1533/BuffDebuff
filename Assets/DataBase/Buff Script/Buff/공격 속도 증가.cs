using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 공격속도 고정 증가
 */
[System.Serializable]
public class 공격_속도_증가 : AbstractBuff, IOnBuffAdded, IOnBuffRemoved
{
	public 공격_속도_증가(BuffData buffData) : base(buffData)
	{

	}
	
	public void OnBuffAdded<TStat, TController, TAnimator>(Character<TStat, TController, TAnimator> character) where TStat : CharacterStat, new() where TController : Controller2D where TAnimator : CharacterAnimator
	{

	}
	public void OnBuffRemoved<TStat, TController, TAnimator>(Character<TStat, TController, TAnimator> character) where TStat : CharacterStat, new() where TController : Controller2D where TAnimator : CharacterAnimator
	{

	}
}