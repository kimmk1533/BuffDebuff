using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : Singleton<PlayerManager>
{
	private Player m_Player;

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

		M_Buff.onBuffAdded += AddBuff;
		M_Buff.onBuffRemoved += RemoveBuff;
	}

	private bool AddBuff(int code)
	{
		return m_Player.AddBuff(code);
	}
	private bool AddBuff(BuffData buffData)
	{
		return m_Player.AddBuff(buffData);
	}
	private bool RemoveBuff(int code)
	{
		return m_Player.RemoveBuff(code);
	}
	private bool RemoveBuff(BuffData buffData)
	{
		return m_Player.RemoveBuff(buffData);
	}
}