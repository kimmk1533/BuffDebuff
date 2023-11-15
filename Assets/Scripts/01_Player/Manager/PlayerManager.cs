using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : Singleton<PlayerManager>
{
	private Player m_Player;

	public int maxLevel
	{
		get
		{
			if (Application.isEditor)
				return FindObjectOfType<PlayerCharacter>().maxStat.Level;

			return m_Player.maxLevel;
		}
	}
	public int currentLevel
	{
		get
		{
			if (Application.isEditor)
				return FindObjectOfType<PlayerCharacter>().currentStat.Level;

			return m_Player.currentLevel;
		}
	}

	private BuffManager M_Buff => BuffManager.Instance;

	public void Initialize()
	{
		var players = FindObjectsOfType<Player>();

		if (players.Length > 1)
		{
			Debug.LogError("There can be no more than one Player.");
			return;
		}

		m_Player = players[0];
		m_Player.Initialize();
	}
	public void InitializeBuffEvent()
	{
		M_Buff.onBuffAdded += AddBuff;
		M_Buff.onBuffRemoved += RemoveBuff;
	}

	public void AddXp(float xp)
	{
		m_Player.AddXp(xp);
	}

	private bool AddBuff(BuffData buffData)
	{
		return m_Player.AddBuff(buffData);
	}
	private bool RemoveBuff(BuffData buffData)
	{
		return m_Player.RemoveBuff(buffData);
	}
}