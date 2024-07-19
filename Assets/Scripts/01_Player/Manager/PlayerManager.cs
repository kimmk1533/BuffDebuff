using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BuffDebuff
{
	public class PlayerManager : Singleton<PlayerManager>
	{
		#region 변수
		public static readonly int playerMaxLevel = 100;

		private Player m_Player = null;
		private CameraFollow m_PlayerCamera = null;

		private DoubleKeyDictionary<int, string, (Player player, PlayerData data)> m_PlayerDataMap = null;
		#endregion

		#region 프로퍼티
		public Player player => m_Player;
		public int currentLevel
		{
			get
			{
				if (m_Player == null)
					return 0;

				return m_Player.currentLevel;
			}
		}
		#endregion

		#region 매니저
		private static StageManager M_Stage => StageManager.Instance;
		#endregion

		public void Initialize()
		{
			if (m_PlayerDataMap == null)
				m_PlayerDataMap = new DoubleKeyDictionary<int, string, (Player player, PlayerData data)>();

			LoadAllPlayerData();
		}
		public void Finallize()
		{

		}

		public void InitializeGame()
		{
			Camera.main.NullCheckGetComponent<CameraFollow>(ref m_PlayerCamera);

			m_Player.gameObject.SetActive(true);
			m_Player.Initialize();
			m_PlayerCamera.Initialize();
		}
		public void FinallizeGame()
		{
			m_PlayerCamera.Finallize();
			m_Player.Finallize();
			m_Player.gameObject.SetActive(false);

			m_Player = null;
		}

		public void InitializeStageGenEvent()
		{
			M_Stage.onStageGenerated += OnStageGenerated;
		}
		public void FinallizeStageGenEvent()
		{
			M_Stage.onStageGenerated -= OnStageGenerated;
		}

		private void LoadAllPlayerData()
		{
			string path = Path.Combine("Scriptable Object", "PlayerData");
			PlayerData[] playerDatas = Resources.LoadAll<PlayerData>(path);

			for (int i = 0; i < playerDatas.Length; ++i)
			{
				PlayerData playerData = playerDatas[i];

				if (m_PlayerDataMap.ContainsKey1(playerData.code) == true)
					continue;

				// 플레이어 원본 로드
				Player origin = Resources.Load<Player>(playerData.assetPath);

				// 플레이어 복제
				Player player = GameObject.Instantiate<Player>(origin, transform);
				// 초기 스탯 설정
				player.SetInitStat(PlayerStat.Clone(playerData));
				// 풀처럼 설정
				player.gameObject.SetActive(false);

				m_PlayerDataMap.Add(playerData.code, playerData.title, (player, playerData));
			}
		}
		public void SelectPlayerData(int code)
		{
			if (m_PlayerDataMap.ContainsKey1(code) == false)
				Debug.LogError("잘못된 Player Code 입니다. Code: " + code);

			m_Player = m_PlayerDataMap[code].player;
			if (m_Player == null)
			{
				PlayerData playerData = m_PlayerDataMap[code].data;

				Debug.LogError("PlayerData \"" + playerData.title + "\" 의 Asset Path가 잘못되었습니다.\n" +
					"Asset Path: " + playerData.assetPath);
			}
		}
		public void SelectPlayerData(string title)
		{
			if (m_PlayerDataMap.ContainsKey2(title) == false)
				Debug.LogError("잘못된 Player Title 입니다. Title: " + title);

			m_Player = m_PlayerDataMap[title].player;
			if (m_Player == null)
			{
				PlayerData playerData = m_PlayerDataMap[title].data;

				Debug.LogError("PlayerData \"" + playerData.title + "\" 의 Asset Path가 잘못되었습니다.\n" +
					"Asset Path: " + playerData.assetPath);
			}
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