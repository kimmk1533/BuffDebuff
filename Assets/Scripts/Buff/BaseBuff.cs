using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BaseBuff
{
	private string m_Name;
	private int m_Code;
	public enum E_Grade
	{
		Normal = 0,
		Uncommon,
		Rare,
		Unique,
		Epic,
		Legendary,
		God,

		Max
	}
	private E_Grade m_Grade;
	private string m_Description;

	public BaseBuff(string name, int code, E_Grade grade, string description)
	{
		m_Name = name;
		m_Code = code;
		m_Grade = grade;
		m_Description = description;

		OnBuffInitialize = new BuffHandler();
		OnBuffFinalize = new BuffHandler();
		OnBuffUpdate = new BuffHandler();
		OnBuffJump = new BuffHandler();
		OnBuffDash = new BuffHandler();
		OnBuffGetDamage = new BuffHandler();
		OnBuffAttackStart = new BuffHandler();
		OnBuffGiveDamage = new BuffHandler();
		OnBuffAttackEnd = new BuffHandler();
	}

	public delegate void OnBuffHandler(ref Character character);
	public class BuffHandler
	{
		public event OnBuffHandler OnBuffEvent;
		public void OnBuffInvoke(ref Character character)
		{
			OnBuffEvent?.Invoke(ref character);
		}
	}

	public BuffHandler OnBuffInitialize;
	public BuffHandler OnBuffFinalize;
	public BuffHandler OnBuffUpdate;
	public BuffHandler OnBuffJump;
	public BuffHandler OnBuffDash;
	public BuffHandler OnBuffGetDamage;
	public BuffHandler OnBuffAttackStart;
	public BuffHandler OnBuffGiveDamage;
	public BuffHandler OnBuffAttackEnd;
}