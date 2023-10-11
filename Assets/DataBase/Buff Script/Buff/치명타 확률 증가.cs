using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 치명타 확률 증가
 */
[System.Serializable]
public class 치명타_확률_증가 : AbstractBuff, IOnBuffAdded, IOnBuffRemoved
{
	public 치명타_확률_증가(BuffData buffData) : base(buffData)
	{

	}
	
	public void OnBuffAdded<TStat, TController, TAnimator>(Character<TStat, TController, TAnimator> character) where TStat : CharacterStat, new() where TController : Controller2D where TAnimator : CharacterAnimator
	{

	}
	public void OnBuffRemoved<TStat, TController, TAnimator>(Character<TStat, TController, TAnimator> character) where TStat : CharacterStat, new() where TController : Controller2D where TAnimator : CharacterAnimator
	{

	}
}