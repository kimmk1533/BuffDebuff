using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCharacter : Character<EnemyCharacterStat>
{
	private void Update()
	{
		HpRegenTimer();
		AttackTimer();
	}

	public override void Initialize()
	{
		// Stat Init
		m_CurrentStat = new EnemyCharacterStat(m_MaxStat);

		// Timer Init
		m_HealTimer = new UtilClass.Timer(m_CurrentStat.HpRegenTime);
		m_AttackTimer = new UtilClass.Timer(1f / m_CurrentStat.AttackSpeed, true);
	}
}