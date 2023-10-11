using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 모든 무적 시간 증가
 */
[System.Serializable]
public class 무적시간_증가 : AbstractBuff, IOnBuffAdded, IOnBuffRemoved
{
	public 무적시간_증가(BuffData buffData) : base(buffData)
	{

	}
	
	public void OnBuffAdded<TStat, TController, TAnimator>(Character<TStat, TController, TAnimator> character) where TStat : CharacterStat, new() where TController : Controller2D where TAnimator : CharacterAnimator
	{

	}
	public void OnBuffRemoved<TStat, TController, TAnimator>(Character<TStat, TController, TAnimator> character) where TStat : CharacterStat, new() where TController : Controller2D where TAnimator : CharacterAnimator
	{

	}
}