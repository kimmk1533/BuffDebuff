using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using DataDictionary = DoubleKeyDictionary<int, string, (BuffDebuff.Player player, BuffDebuff.PlayerData data)>;

namespace BuffDebuff
{
	public class PlayerManager : SerializedSingleton<PlayerManager>
	{
		#region 변수
		public static readonly int playerMaxLevel = 100;

		private Player m_Player = null;
		private CameraFollow m_PlayerCamera = null;

		private DataDictionary m_PlayerDataMap = null;
		// 현재 레벨, 다음 레벨까지 필요 경험치
		private Dictionary<int, float> m_PlayerLevelMap = null;
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

		public override void Initialize()
		{
			base.Initialize();

			if (m_PlayerDataMap == null)
				m_PlayerDataMap = new DataDictionary();
			if (m_PlayerLevelMap == null)
				m_PlayerLevelMap = new Dictionary<int, float>();

			LoadAllLevelData();
			LoadAllPlayerData();
		}
		public override void Finallize()
		{
			base.Finallize();
		}

		public override void InitializeMain()
		{
			base.InitializeMain();

			Camera.main.NullCheckGetComponent<CameraFollow>(ref m_PlayerCamera);

			m_Player.gameObject.SetActive(true);
			m_Player.Initialize();
			m_PlayerCamera.Initialize();

			M_Stage.onStageGenerated += OnStageGenerated;
		}
		public override void FinallizeMain()
		{
			base.FinallizeMain();

			m_PlayerCamera.Finallize();
			m_Player.Finallize();
			m_Player.gameObject.SetActive(false);

			m_Player = null;

			M_Stage.onStageGenerated -= OnStageGenerated;
		}

		private void LoadAllPlayerData()
		{
			string path = PlayerSOManager.resourcesPath;
			PlayerData[] playerDatas = Resources.LoadAll<PlayerData>(path);

			for (int i = 0; i < playerDatas.Length; ++i)
			{
				PlayerData playerData = playerDatas[i];

				if (m_PlayerDataMap.ContainsKey1(playerData.code) == true)
					continue;

				// 플레이어 원본 로드
				Player origin = Resources.Load<Player>(playerData.assetPath);

				if (origin == null)
				{
					Debug.LogError("플레이어 원본이 null임.");
					return;
				}

				// 플레이어 복제
				Player player = GameObject.Instantiate<Player>(origin, transform);
				// 초기 스탯 설정
				PlayerStat playerStat = PlayerStat.Clone(playerData);
				var temp = playerStat.Xp;
				temp.max = m_PlayerLevelMap[playerStat.Level.current];
				playerStat.Xp = temp;
				player.SetInitStat(playerStat);
				// 풀처럼 설정
				player.gameObject.SetActive(false);

				m_PlayerDataMap.Add(playerData.code, playerData.title, (player, playerData));
			}
		}
		private void LoadAllLevelData()
		{
			string path = LevelSOManager.resourcesPath;
			LevelData[] levelDatas = Resources.LoadAll<LevelData>(path);

			for (int i = 0; i < levelDatas.Length; ++i)
			{
				LevelData levelData = levelDatas[i];

				m_PlayerLevelMap.Add(levelData.currentLevel, levelData.requiredXp);
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
		public float GetRequiredXp(int currentLevel)
		{
			return m_PlayerLevelMap[currentLevel];
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