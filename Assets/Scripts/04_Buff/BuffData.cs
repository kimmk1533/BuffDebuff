using UnityEngine;

[System.Serializable]
public struct BuffData
{
	[SerializeField, ReadOnly]
	private string m_Name;
	[SerializeField, ReadOnly]
	private int m_Code;
	[SerializeField, ReadOnly]
	private E_BuffType m_Type;
	[SerializeField, ReadOnly]
	private E_BuffEffectType m_EffectType;
	[SerializeField, ReadOnly]
	private E_BuffGrade m_Grade;
	[SerializeField, ReadOnly]
	private int m_MaxStack;
	[SerializeField, ReadOnly]
	private E_BuffWeapon m_Weapon;
	[SerializeField, ReadOnly]
	private string m_Description;

	public string name
	{
		get { return m_Name; }
	}
	public int code
	{
		get { return m_Code; }
	}
	public E_BuffType type
	{
		get { return m_Type; }
	}
	public E_BuffEffectType effectType
	{
		get { return m_EffectType; }
	}
	public E_BuffGrade grade
	{
		get { return m_Grade; }
	}
	public int maxStack
	{
		get { return m_MaxStack; }
	}
	public E_BuffWeapon weapon
	{
		get { return m_Weapon; }
	}
	public string description
	{
		get { return m_Description; }
	}

	public BuffData(string _name, int _code, E_BuffType _type, E_BuffEffectType _effectType, E_BuffGrade _grade, int _maxStack, E_BuffWeapon _weapon, string _description)
	{
		m_Name = _name;
		m_Code = _code;
		m_Type = _type;
		m_EffectType = _effectType;
		m_Grade = _grade;
		m_MaxStack = _maxStack;
		m_Weapon = _weapon;
		m_Description = _description;
	}
}

#region enum
public enum E_BuffType
{
	Buff,
	Debuff,
	Bothbuff,

	Max
}
public enum E_BuffEffectType
{
	Stat,
	Weapon,
	Combat,

	Max
}
public enum E_BuffGrade
{
	Normal = 0,
	Uncommon,
	Rare,
	Unique,
	Epic,
	Legendary,
	GOD,

	Max
}
public enum E_BuffWeapon
{
	All,
	Melee,
	Ranged,

	Max
}
public enum E_BuffInvokeCondition
{
	Initialize,     // 버프를 얻을 때
	Finalize,       // 버프를 잃을 때
	Update,         // 일정 시간마다
	Jump,           // 점프 시
	Dash,           // 대쉬 시
	GiveDamage,     // 타격 시
	GetDamage,      // 피격 시
	AttackStart,    // 공격 시작 시
	Attack,         // 공격 시
	AttackEnd,      // 공격 종료 시
	Kill,           // 적 처치 시
	Death,          // 사망 시
	NextStage,      // 스테이지를 넘어갈 시

	Max
}
public enum E_BuffStat
{

}
#endregion

#region interface
public interface IOnBuffCondition
{

}
// 버프를 얻은 순간
public interface IOnBuffStart : IOnBuffCondition
{
	public void OnBuffStart(ref Character character);
}
// 매 프레임마다
public interface IOnBuffUpdate : IOnBuffCondition
{
	public void OnBuffUpdate();
}
// 점프했을 때
public interface IOnBuffJump : IOnBuffCondition
{
	public void OnBuffJump();
}
// 대쉬했을 때
public interface IOnBuffDash : IOnBuffCondition
{
	public void OnBuffDash();
}
// 대미지를 받을 때
public interface IOnBuffGetDamage : IOnBuffCondition
{
	public void OnBuffGetDamage();
}
// 공격 시작할 때
public interface IOnBuffAttackStart : IOnBuffCondition
{
	public void OnBuffAttackStart();
}
// 대미지를 줄 때
public interface IOnBuffGiveDamage : IOnBuffCondition
{
	public void OnBuffGiveDamage();
}
// 공격을 끝낼 때
public interface IOnBuffAttackEnd : IOnBuffCondition
{
	public void OnBuffAttackEnd();
}
#endregion