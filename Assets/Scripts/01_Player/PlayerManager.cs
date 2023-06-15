using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : Singleton<PlayerManager>
{
	private Player m_Player;

	private void Awake()
	{
		Initialize();
	}

	public void Initialize()
	{
		m_Player = FindObjectsOfType<Player>()[0];
	}
	public void AddBuff(int code)
	{
		m_Player.AddBuff(code);
	}
	public void RemoveBuff(int code)
	{
		m_Player.RemoveBuff(code);
	}
}