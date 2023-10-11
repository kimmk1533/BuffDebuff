using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 최대 체력 고정 증가
 */
[System.Serializable]
public class 체력_증가 : AbstractBuff, IOnBuffAdded, IOnBuffRemoved
{
	public 체력_증가(BuffData buffData) : base(buffData)
	{

	}
	
	public void OnBuffAdded<TStat, TController, TAnimator>(Character<TStat, TController, TAnimator> character) where TStat : CharacterStat, new() where TController : Controller2D where TAnimator : CharacterAnimator
	{

	}
	public void OnBuffRemoved<TStat, TController, TAnimator>(Character<TStat, TController, TAnimator> character) where TStat : CharacterStat, new() where TController : Controller2D where TAnimator : CharacterAnimator
	{

	}
}