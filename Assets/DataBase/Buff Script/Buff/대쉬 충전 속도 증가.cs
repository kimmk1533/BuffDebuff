using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 대쉬가 충전되는 시간 감소
 */
[System.Serializable]
public class 대쉬_충전_속도_증가 : AbstractBuff, IOnBuffAdded, IOnBuffRemoved
{
	public 대쉬_충전_속도_증가(BuffData buffData) : base(buffData)
	{

	}
	
	public void OnBuffAdded<TStat, TController, TAnimator>(Character<TStat, TController, TAnimator> character) where TStat : CharacterStat, new() where TController : Controller2D where TAnimator : CharacterAnimator
	{

	}
	public void OnBuffRemoved<TStat, TController, TAnimator>(Character<TStat, TController, TAnimator> character) where TStat : CharacterStat, new() where TController : Controller2D where TAnimator : CharacterAnimator
	{

	}
}