using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 타격 시 같은 대상에게만 추가 타격(다단 히트가 생김)
 */
[System.Serializable]
public class 공격_횟수_증가 : AbstractBuff
{
	public 공격_횟수_증가(BuffData buffData) : base(buffData)
	{

	}

	#region 버프 구현
	public override void OnBuffInitialize(Character character)
	{

	}
	public override void OnBuffFinalize(Character character)
	{

	}
	public override void OnBuffAdded(Character character)
	{

	}
	public override void OnBuffRemoved(Character character)
	{

	}
	public override void OnBuffUpdate()
	{

	}
	public override void OnBuffJump()
	{

	}
	public override void OnBuffDash()
	{

	}
	public override void OnBuffGetDamage()
	{

	}
	public override void OnBuffAttackStart()
	{

	}
	public override void OnBuffGiveDamage()
	{

	}
	public override void OnBuffAttackEnd()
	{

	}
	#endregion
}