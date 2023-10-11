using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 대쉬가 좌우에서 전방향으로 변경
 */
[System.Serializable]
public class 전방향_대쉬 : AbstractBuff, IOnBuffAdded, IOnBuffRemoved
{
	public 전방향_대쉬(BuffData buffData) : base(buffData)
	{

	}
	
	public void OnBuffAdded<TStat, TController, TAnimator>(Character<TStat, TController, TAnimator> character) where TStat : CharacterStat, new() where TController : Controller2D where TAnimator : CharacterAnimator
	{

	}
	public void OnBuffRemoved<TStat, TController, TAnimator>(Character<TStat, TController, TAnimator> character) where TStat : CharacterStat, new() where TController : Controller2D where TAnimator : CharacterAnimator
	{

	}
}