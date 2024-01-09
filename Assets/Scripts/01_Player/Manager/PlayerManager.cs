using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BuffInventory))]
public class PlayerManager : Singleton<PlayerManager>
{
	#region 변수
	[SerializeField]
	private string m_Path = null;
	[SerializeField]
	private string m_PlayerCharacterName = null;

	private Player m_Origin = null;
	private Player m_Player = null;
	private CameraFollow m_PlayerCamera = null;

	private BuffInventory m_BuffInventory = null;
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
		m_Path = "Prefabs/01_Player";
		m_PlayerCharacterName = "fire_knight";

		this.Safe_GetComponent<BuffInventory>(ref m_BuffInventory);
		m_BuffInventory.Initialize();
	}
	public void Finallize()
	{
		m_BuffInventory.Finallize();
	}

	public void InitializeBuffEvent()
	{
		M_Buff.onBuffAdded += AddBuff;
		M_Buff.onBuffRemoved += RemoveBuff;
	}
	public void FinallizeBuffEvent()
	{
		M_Buff.onBuffAdded -= AddBuff;
		M_Buff.onBuffRemoved -= RemoveBuff;
	}

	public void InitializeStageGenEvent()
	{
		M_Stage.onStageGenerated += OnStageGenerated;
	}
	public void FinallizeStageGenEvent()
	{
		M_Stage.onStageGenerated -= OnStageGenerated;
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

		if (m_Origin == null)
			m_Origin = Resources.Load<Player>(System.IO.Path.Combine(m_Path, m_PlayerCharacterName));
		if (m_Player == null)
			m_Player = GameObject.Instantiate(m_Origin);
		Camera.main.Safe_GetComponent<CameraFollow>(ref m_PlayerCamera);

		m_Player.Initialize();
		m_PlayerCamera.Initialize();
	}
	public void FinallizeGame()
	{
		m_PlayerCamera.Finallize();
		m_Player.Finallize();
	}

	public void AddXp(float xp)
	{
		m_Player.AddXp(xp);
	}

	private bool AddBuff(BuffData buffData)
	{
		return m_BuffInventory.AddBuff(buffData);
	}
	private bool RemoveBuff(BuffData buffData)
	{
		return m_BuffInventory.RemoveBuff(buffData);
	}

	public bool HasBuff(string buffName)
	{
		return m_BuffInventory.Contains(buffName);
	}

	private void OnStageGenerated()
	{
		Room room = M_Stage.currentStage.currentRoom;
		StartRoom startRoom = room as StartRoom;

		if (startRoom != null)
			m_Player.transform.position = startRoom.startPos;
		else
			m_Player.transform.position = new Vector3(20f, 4f);
	}
}