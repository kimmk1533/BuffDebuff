using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 최대 점프 횟수 증가
 */
[System.Serializable]
public class 추가_점프 : AbstractBuff, IOnBuffAdded, IOnBuffRemoved
{
	public 추가_점프(BuffData buffData) : base(buffData)
	{

	}
	
	public void OnBuffAdded<TStat, TController, TAnimator>(Character<TStat, TController, TAnimator> character) where TStat : CharacterStat, new() where TController : Controller2D where TAnimator : CharacterAnimator
	{

	}
	public void OnBuffRemoved<TStat, TController, TAnimator>(Character<TStat, TController, TAnimator> character) where TStat : CharacterStat, new() where TController : Controller2D where TAnimator : CharacterAnimator
	{

	}
}