using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuffDebuff
{
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
		#endregion

		#region 프로퍼티
		public Player player
		{
			get
			{
#if UNITY_EDITOR

				if (Application.isPlaying == false)
				{
					//string path = System.IO.Path.Combine(m_Path, m_PlayerCharacterName);
					string path = m_Path + "/" + m_PlayerCharacterName;
					return Resources.Load<Player>(path);
				}

#endif

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
		private static StageManager M_Stage => StageManager.Instance;
		#endregion

		public void Initialize()
		{

		}
		public void Finallize()
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

			if (m_Origin == null)
			{
				string path = System.IO.Path.Combine(m_Path, m_PlayerCharacterName);

				Player tempPlayer = Resources.Load<Player>(path);
				if (tempPlayer == null)
					Debug.LogError("PlayerManager의 m_Path 또는 m_PlayerCharacterName이 올바르지 않습니다.");

				m_Origin = tempPlayer;
			}

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

		public void InitializeStageGenEvent()
		{
			M_Stage.onStageGenerated += OnStageGenerated;
		}
		public void FinallizeStageGenEvent()
		{
			M_Stage.onStageGenerated -= OnStageGenerated;
		}

		public void AddXp(float xp)
		{
			m_Player.AddXp(xp);
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
}