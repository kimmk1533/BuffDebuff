using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractBuff : IEquatable<AbstractBuff>, IOnBuffCondition
{
	[SerializeField]
	protected int m_Count;
	[SerializeField]
	protected BuffData m_DebugBuffData;
	protected readonly BuffData m_BuffData;

	public int count
	{
		get { return m_Count; }
		set { m_Count = value; }
	}
	public BuffData data
	{
		get { return m_BuffData; }
	}

	public AbstractBuff(BuffData buffData)
	{
		m_Count = 0;
		m_DebugBuffData = m_BuffData = buffData;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
			return false;

		AbstractBuff buff = obj as AbstractBuff;
		if (buff == null)
			return false;

		return Equals(buff);
	}
	public bool Equals(AbstractBuff other)
	{
		if (other == null)
			return false;
		return this.data.code == other.data.code;
	}
	public override int GetHashCode()
	{
		return data.code.GetHashCode();
	}

	#region Interface
	// 버프가 처음 추가됐을 때
	public virtual void OnBuffInitialize(Character character)
	{

	}
	// 버프가 모두 제거됐을 때
	public virtual void OnBuffFinalize(Character character)
	{

	}
	// 버프가 추가될 때 마다
	public virtual void OnBuffAdded(Character character)
	{

	}
	// 버프가 제거될 때 마다
	public virtual void OnBuffRemoved(Character character)
	{

	}
	// 매 프레임마다
	public virtual void OnBuffUpdate()
	{

	}
	// 점프할 때
	public virtual void OnBuffJump()
	{

	}
	// 대쉬할 때
	public virtual void OnBuffDash()
	{

	}
	// 대미지를 받을 때
	public virtual void OnBuffGetDamage()
	{

	}
	// 공격을 시작할 때 (애니메이션 시작)
	public virtual void OnBuffAttackStart()
	{

	}
	// 대미지를 줄 때
	public virtual void OnBuffGiveDamage()
	{

	}
	// 공격을 끝낼 때 (애니메이션 종료)
	public virtual void OnBuffAttackEnd()
	{

	}
	#endregion
}