using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class 대쉬_충전_속도_증가 : AbstractBuff
{
	public 대쉬_충전_속도_증가(BuffData buffData) : base(buffData)
	{

	}

	public override void OnBuffInitialize(Character character)
	{
		base.OnBuffInitialize(character);
	}
	public override void OnBuffFinalize(Character character)
	{
		base.OnBuffFinalize(character);
	}
	public override void OnBuffUpdate()
	{
		base.OnBuffUpdate();
	}
	public override void OnBuffJump()
	{
		base.OnBuffJump();
	}
	public override void OnBuffDash()
	{
		base.OnBuffDash();
	}
	public override void OnBuffGetDamage()
	{
		base.OnBuffGetDamage();
	}
	public override void OnBuffAttackStart()
	{
		base.OnBuffAttackStart();
	}
	public override void OnBuffGiveDamage()
	{
		base.OnBuffGiveDamage();
	}
	public override void OnBuffAttackEnd()
	{
		base.OnBuffAttackEnd();
	}
}