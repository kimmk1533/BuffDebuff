using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Character
{
	// 초기 스탯
	protected readonly CharacterStat m_InitialStat;
	// 레벨당 스탯(성장 스탯)
	protected readonly CharacterStat m_LevelStat;
	// 현재 스탯
	[SerializeField]
	protected CharacterStat m_CurrentStat;
	// 버프 스탯
	[SerializeField]
	protected CharacterStat m_BuffStat;

	public CharacterStat buffStat
	{
		get { return m_BuffStat; }
		set { m_BuffStat = value; }
	}
	public CharacterStat finalStat => m_CurrentStat + m_BuffStat;

	protected UtilClass.Timer m_HealTimer;
	protected UtilClass.Timer m_DashTimer;

	protected Dictionary<int, AbstractBuff> m_BuffList;

	private BuffManager M_Buff => BuffManager.Instance;

	public Character()
	{
		m_InitialStat = new CharacterStat()
		{
			HpRegen = 5f,
			HpRegenTime = 1f,

			DashCount = 3,
			DashRechargeTime = 1f,

			AttackRange = 5f,
		};

		m_CurrentStat = m_InitialStat;
		m_BuffStat = new CharacterStat();
		m_BuffList = new Dictionary<int, AbstractBuff>();

		m_HealTimer = new UtilClass.Timer(m_InitialStat.HpRegenTime);
		m_DashTimer = new UtilClass.Timer(m_InitialStat.DashRechargeTime);
	}

	protected void HpRegenTimer()
	{
		if (m_CurrentStat.Hp >= finalStat.MaxHp)
			return;

		if (m_HealTimer.Update(true))
		{
			float hp = m_CurrentStat.Hp + finalStat.HpRegen;

			m_CurrentStat.Hp = Mathf.Clamp(hp, 0f, finalStat.MaxHp);
		}
	}
	protected void DashTimer()
	{
		if (m_CurrentStat.DashCount >= m_InitialStat.DashCount)
			return;

		if (m_DashTimer.Update(true))
		{
			++m_CurrentStat.DashCount;
		}
	}
	public void Update()
	{
		HpRegenTimer();

		DashTimer();

		foreach (var item in m_BuffList.Values)
		{
			item.OnBuffUpdate();
		}
	}
	public void Jump()
	{
		foreach (var item in m_BuffList.Values)
		{
			item.OnBuffJump();
		}
	}
	public bool Dash()
	{
		if (m_CurrentStat.DashCount <= 0)
			return false;

		--m_CurrentStat.DashCount;

		foreach (var item in m_BuffList.Values)
		{
			item.OnBuffDash();
		}

		return true;
	}

	public void AddBuff(int code)
	{
		BuffData buffData = M_Buff.GetBuffData(code);

		this.AddBuff(buffData);
	}
	public void AddBuff(string name)
	{
		if (name == null || name == string.Empty)
			return;

		BuffData buffData = M_Buff.GetBuffData(name);

		this.AddBuff(buffData);
	}
	public void AddBuff(BuffData buffData)
	{
		if (buffData == null)
			return;

		if (m_BuffList.TryGetValue(buffData.code, out AbstractBuff buff) &&
			buff != null)
		{
			if (buff.count < buffData.maxStack)
			{
				++buff.count;
				buff.OnBuffAdded(this);
			}
			else
				Debug.Log("버프 최대");

			return;
		}

		buff = M_Buff.CreateBuff(buffData);

		m_BuffList.Add(buffData.code, buff);

		buff.OnBuffInitialize(this);
		buff.OnBuffAdded(this);
	}
	public bool RemoveBuff(int code)
	{
		BuffData buffData = M_Buff.GetBuffData(code);

		return this.RemoveBuff(buffData);
	}
	public bool RemoveBuff(string name)
	{
		if (name == null || name == string.Empty)
			return false;

		BuffData buff = M_Buff.GetBuffData(name);

		return this.RemoveBuff(buff);
	}
	public bool RemoveBuff(BuffData buffData)
	{
		if (buffData == null)
			return false;

		if (m_BuffList.TryGetValue(buffData.code, out AbstractBuff buff) &&
			buff != null)
		{
			if (buff.count > 0)
			{
				--buff.count;
			}
			else
			{
				buff.OnBuffFinalize(this);

				m_BuffList.Remove(buffData.code);
			}

			buff.OnBuffRemoved(this);

			return true;
		}

		Debug.Log("버프 없는데 제거");

		return false;
	}

	[System.Serializable]
	public struct CharacterStat
	{
		#region 체력
		// 최대 체력
		public float MaxHp;
		// 현재 체력
		public float Hp;
		#endregion
		#region 회복
		// 체력 재생량 (체력 재생 시간마다 한 번 재생)
		public float HpRegen;
		// 체력 재생 시간
		public float HpRegenTime;
		// 힐 배율
		public float HealScale;
		#endregion
		#region 공격
		// 공격력
		public float Attack;
		// 공격 속도
		public float AttackSpeed;
		// 공격 크기
		public float AttackScale;
		// 공격 사거리(투사체 생존 시간)
		public float AttackRange;
		// 타격 수
		public int MultiHitCount;
		// 치명타 확률
		public float CriticalRate;
		// 치명타 대미지 배율
		public float CriticalDamageScale;
		#endregion
		#region 수비
		// 방어력
		public float Armor;
		// 회피율
		public float Avoidability;
		#endregion
		#region 대쉬
		// 대쉬 횟수
		public int DashCount;
		// 대쉬 충전 속도
		public float DashRechargeTime;
		#endregion
		#region 이동
		// 이동 속도
		public float MoveSpeed;
		#endregion
		#region 시야
		// 시야 거리
		public float Sight;
		#endregion

		private static readonly CharacterStat zeroStat = new CharacterStat()
		{
			#region 체력
			// 최대 체력
			MaxHp = 0.0f,
			// 현재 체력
			Hp = 0.0f,
			#endregion
			#region 회복
			// 체력 재생량 (체력 재생 시간마다 한 번 재생)
			HpRegen = 0.0f,
			// 체력 재생 시간
			HpRegenTime = 0.0f,
			// 힐 배율
			HealScale = 0.0f,
			#endregion
			#region 공격
			// 공격력
			Attack = 0.0f,
			// 공격 속도
			AttackSpeed = 0.0f,
			// 근접 공격 범위
			AttackScale = 0.0f,
			// 투사체 공격 사거리
			AttackRange = 0.0f,
			// 타격 수
			MultiHitCount = 0,
			// 치명타 확률
			CriticalRate = 0.0f,
			// 치명타 대미지 배율
			CriticalDamageScale = 0.0f,
			#endregion
			#region 수비
			// 방어력
			Armor = 0.0f,
			// 회피율
			Avoidability = 0.0f,
			#endregion
			#region 대쉬
			// 대쉬 횟수
			DashCount = 0,
			// 대쉬 충전 속도
			DashRechargeTime = 0.0f,
			#endregion
			#region 이동
			// 이동 속도
			MoveSpeed = 0.0f,
			#endregion
			#region 시야
			// 시야 거리
			Sight = 0.0f,
			#endregion
		};
		private static readonly CharacterStat oneStat = new CharacterStat()
		{
			#region 체력
			// 최대 체력
			MaxHp = 1.0f,
			// 현재 체력
			Hp = 1.0f,
			#endregion
			#region 회복
			// 체력 재생량 (체력 재생 시간마다 한 번 재생)
			HpRegen = 1.0f,
			// 체력 재생 시간
			HpRegenTime = 1.0f,
			// 힐 배율
			HealScale = 1.0f,
			#endregion
			#region 공격
			// 공격력
			Attack = 1.0f,
			// 공격 속도
			AttackSpeed = 1.0f,
			// 근접 공격 범위
			AttackScale = 1.0f,
			// 투사체 공격 사거리
			AttackRange = 1.0f,
			// 타격 수
			MultiHitCount = 1,
			// 치명타 확률
			CriticalRate = 1.0f,
			// 치명타 대미지 배율
			CriticalDamageScale = 1.0f,
			#endregion
			#region 수비
			// 방어력
			Armor = 1.0f,
			// 회피율
			Avoidability = 1.0f,
			#endregion
			#region 대쉬
			// 대쉬 횟수
			DashCount = 1,
			// 대쉬 충전 속도
			DashRechargeTime = 1.0f,
			#endregion
			#region 이동
			// 이동 속도
			MoveSpeed = 1.0f,
			#endregion
			#region 시야
			// 시야 거리
			Sight = 1.0f,
			#endregion
		};
		public static CharacterStat zero
		{
			get
			{
				return zeroStat;
			}
		}
		public static CharacterStat one
		{
			get
			{
				return oneStat;
			}
		}

		public static CharacterStat operator +(CharacterStat s1, CharacterStat s2)
		{
			CharacterStat result = new CharacterStat();

			// 최대 체력
			result.MaxHp = s1.MaxHp + s2.MaxHp;
			// 현재 체력
			result.Hp = s1.Hp + s2.Hp;
			// 체력 재생량
			result.HpRegen = s1.HpRegen + s2.HpRegen;
			// 체력 재생 시간
			result.HpRegenTime = s1.HpRegenTime + s2.HpRegenTime;
			// 모든 힐 배율
			result.HealScale = s1.HealScale + s2.HealScale;
			// 방어력
			result.Armor = s1.Armor + s2.Armor;
			// 회피율
			result.Avoidability = s1.Avoidability + s2.Avoidability;

			// 공격력
			result.Attack = s1.Attack + s2.Attack;
			// 공격 속도
			result.AttackSpeed = s1.AttackSpeed + s2.AttackSpeed;
			// 근접 공격 범위
			result.AttackScale = s1.AttackScale + s2.AttackScale;
			// 투사체 공격 사거리
			result.AttackRange = s1.AttackRange + s2.AttackRange;
			// 타격 수
			result.MultiHitCount = s1.MultiHitCount + s2.MultiHitCount;
			// 치명타 확률
			result.CriticalRate = s1.CriticalRate + s2.CriticalRate;
			// 치명타 대미지 배율
			result.CriticalDamageScale = s1.CriticalDamageScale + s2.CriticalDamageScale;

			// 대쉬 횟수
			result.DashCount = s1.DashCount + s2.DashCount;
			// 대쉬 충전 속도
			result.DashRechargeTime = s1.DashRechargeTime + s2.DashRechargeTime;
			// 이동 속도
			result.MoveSpeed = s1.MoveSpeed + s2.MoveSpeed;
			// 시야 거리
			result.Sight = s1.Sight + s2.Sight;

			return result;
		}
	}
}