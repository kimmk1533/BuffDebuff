using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : Singleton<PlayerManager>
{
	#region 변수
	[SerializeField]
	private string m_Path;
	[SerializeField]
	private string m_PlayerCharacterName;

	private Player m_Player;
	private CameraFollow m_PlayerCamera;
	#endregion

	#region 프로퍼티
	public Player player
	{
		get
		{
			if (Application.isPlaying == false)
				return Resources.Load<Player>(System.IO.Path.Combine(m_Path, m_PlayerCharacterName));

			return m_Player;
		}
	}
	public int maxLevel
	{
		get
		{
			return player.maxLevel;
		}
	}
	public int currentLevel
	{
		get
		{
			return player.currentLevel;
		}
	}
	#endregion

	#region 매니저
	private static BuffManager M_Buff => BuffManager.Instance;
	private static StageManager M_Stage => StageManager.Instance;
	#endregion

	public void Initialize()
	{

	}
	public void InitializeGame()
	{
		//Player[] players = FindObjectsOfType<Player>();

		//if (players.Length > 1)
		//{
		//	Debug.LogError("There can be no more than one Player.");
		//	return;
		//}

		//m_Player = players[0];

		if (m_Player == null)
			m_Player = GameObject.Instantiate(Resources.Load<Player>(System.IO.Path.Combine(m_Path, m_PlayerCharacterName)));
		m_Player.Initialize();

		Camera.main.Safe_GetComponent<CameraFollow>(ref m_PlayerCamera);
		m_PlayerCamera.Initialize();

		InitializeStageGenEvent();
	}
	public void InitializeBuffEvent()
	{
		M_Buff.onBuffAdded += AddBuff;
		M_Buff.onBuffRemoved += RemoveBuff;
	}
	public void InitializeStageGenEvent()
	{
		M_Stage.onStageGenerated += () =>
		{
			m_Player.transform.position = (M_Stage.currentStage.currentRoom as StartRoom).startPos;
		};
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