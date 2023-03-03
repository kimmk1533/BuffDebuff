using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IOnBuff
{

}

public enum E_BuffCondition
{
	Initialize,
	Finalize,
	Update,
	Jump,
	Dash,
	GetDamage,
	AttackStart,
	GiveDamage,
	AttackEnd,

	Max
}

// 버프를 얻은 순간
public interface IOnBuffStart : IOnBuff
{
	public void OnBuffStart(ref Character character);
}
// 매 프레임마다
public interface IOnBuffUpdate : IOnBuff
{
	public void OnBuffUpdate();
}
// 점프했을 때
public interface IOnBuffJump : IOnBuff
{
	public void OnBuffJump();
}
// 대쉬했을 때
public interface IOnBuffDash : IOnBuff
{
	public void OnBuffDash();
}
// 대미지를 받을 때
public interface IOnBuffGetDamage : IOnBuff
{
	public void OnBuffGetDamage();
}
// 공격 시작할 때
public interface IOnBuffAttackStart : IOnBuff
{
	public void OnBuffAttackStart();
}
// 대미지를 줄 때
public interface IOnBuffGiveDamage : IOnBuff
{
	public void OnBuffGiveDamage();
}
// 공격을 끝낼 때
public interface IOnBuffAttackEnd : IOnBuff
{
	public void OnBuffAttackEnd();
}