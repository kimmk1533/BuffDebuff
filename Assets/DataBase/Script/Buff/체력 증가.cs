using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 최대 체력 고정 증가
 */
[System.Serializable]
public class 체력_증가 : AbstractBuff
{
	public 체력_증가(BuffData buffData) : base(buffData)
	{

	}

	#region 버프 구현
	// 버프가 처음 추가됐을 때
	public override void OnBuffInitialize(Character character)
	{

	}
	// 버프가 모두 제거됐을 때
	public override void OnBuffFinalize(Character character)
	{

	}
	// 버프가 추가될 때 마다
	public override void OnBuffAdded(Character character)
	{
		var buffStat = character.buffStat;

		buffStat.MaxHp += 100f;

		character.buffStat = buffStat;
	}
	// 버프가 제거될 때 마다
	public override void OnBuffRemoved(Character character)
	{

	}
	// 매 프레임마다
	public override void OnBuffUpdate()
	{

	}
	// 점프할 때
	public override void OnBuffJump()
	{

	}
	// 대쉬할 때
	public override void OnBuffDash()
	{

	}
	// 대미지를 받을 때
	public override void OnBuffGetDamage()
	{

	}
	// 공격을 시작할 때 (애니메이션 시작)
	public override void OnBuffAttackStart()
	{

	}
	// 대미지를 줄 때
	public override void OnBuffGiveDamage()
	{

	}
	// 공격을 끝낼 때 (애니메이션 종료)
	public override void OnBuffAttackEnd()
	{

	}
	#endregion
}