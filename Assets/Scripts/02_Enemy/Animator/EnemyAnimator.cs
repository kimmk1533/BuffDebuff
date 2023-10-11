using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAnimator : CharacterAnimator
{
	#region Enum
	private enum E_AnimState
	{
		None = 0,

		Idle = 0,
		Run = 1,

		Max
	}
	#endregion

	private Enemy m_Enemy;
	private EnemyCharacter m_EnemyCharacter;

	public override void Initialize()
	{
		base.Initialize();

		m_Enemy = GetComponentInParent<Enemy>();
		m_EnemyCharacter = GetComponentInParent<EnemyCharacter>();
	}
}