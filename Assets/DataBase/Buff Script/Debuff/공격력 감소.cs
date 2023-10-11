using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 공격력 고정 감소
 */
[System.Serializable]
public class 공격력_감소 : AbstractBuff, IOnBuffAdded, IOnBuffRemoved
{
	public 공격력_감소(BuffData buffData) : base(buffData)
	{

	}
	
	public void OnBuffAdded<TStat, TController, TAnimator>(Character<TStat, TController, TAnimator> character) where TStat : CharacterStat, new() where TController : Controller2D where TAnimator : CharacterAnimator
	{

	}
	public void OnBuffRemoved<TStat, TController, TAnimator>(Character<TStat, TController, TAnimator> character) where TStat : CharacterStat, new() where TController : Controller2D where TAnimator : CharacterAnimator
	{

	}
}