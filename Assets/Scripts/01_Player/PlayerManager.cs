using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using DataDictionary = DoubleKeyDictionary<int, string, BuffDebuff.PlayerData>;
using LevelDictionary = System.Collections.Generic.Dictionary<int, float>;

namespace BuffDebuff
{
	public class PlayerManager : ObjectManager<PlayerManager, Player>
	{
		#region 기본 템플릿
		#region 변수
		public static readonly int playerMaxLevel = 100;

		private PlayerData m_PlayerData = null;
		private Player m_Player = null;
		private CameraFollow m_PlayerCamera = null;

		private DataDictionary m_PlayerDataMap = null;
		// 현재 레벨, 다음 레벨까지 필요 경험치
		private LevelDictionary m_PlayerLevelMap = null;
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

		#region 이벤트

		#region 이벤트 함수
		private void OnStageGenerated()
		{
			Room room = M_Stage.currentStage.currentRoom;
			StartRoom startRoom = room as StartRoom;

			if (startRoom != null)
				m_Player.transform.position = startRoom.startPos;
			else
				m_Player.transform.position = new Vector3(20f, 4f);
		}
		#endregion
		#endregion

		#region 초기화 & 마무리화 함수
		/// <summary>
		/// 기본 초기화 함수 (Init Scene 진입 시, 즉 게임 실행 시 호출)
		/// </summary>
		public override void Initialize()
		{
			base.Initialize();

			if (m_PlayerDataMap == null)
				m_PlayerDataMap = new DataDictionary();
			if (m_PlayerLevelMap == null)
				m_PlayerLevelMap = new LevelDictionary();

			LoadAllLevelData();
			LoadAllPlayerData();
		}
		/// <summary>
		/// 기본 마무리화 함수 (게임 종료 시 호출)
		/// </summary>
		public override void Finallize()
		{
			base.Finallize();


		}

		/// <summary>
		/// 메인 초기화 함수 (본인 Main Scene 진입 시 호출)
		/// </summary>
		public override void InitializeMain()
		{
			base.InitializeMain();

			SpawnPlayer();

			Camera.main.NullCheckGetComponent<CameraFollow>(ref m_PlayerCamera);
			m_PlayerCamera.Initialize();

			M_Stage.onStageGenerated += OnStageGenerated;
		}
		/// <summary>
		/// 메인 마무리화 함수 (본인 Main Scene 나갈 시 호출)
		/// </summary>
		public override void FinallizeMain()
		{
			base.FinallizeMain();

			m_PlayerCamera.Finallize();

			m_Player = null;

			M_Stage.onStageGenerated -= OnStageGenerated;
		}
		#endregion

		#region 유니티 콜백 함수
		#endregion

		#endregion

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

				// 로드 예외 처리
				if (origin == null)
				{
					Debug.LogError("플레이어 원본이 null임.");
					return;
				}

				// 초기 스탯 설정
				m_ObjectPoolMap[playerData.title].onItemInstantiated += (Player player) =>
				{
					PlayerStat playerStat = PlayerStat.Clone(playerData);

					StatValue<float> tempXp = playerStat.Xp;
					tempXp.max = m_PlayerLevelMap[playerStat.Level.current];
					playerStat.Xp = tempXp;

					player.initStat = playerStat;
				};

				m_PlayerDataMap.Add(playerData.code, playerData.title, playerData);
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

			m_PlayerData = m_PlayerDataMap[code];
		}
		public void SelectPlayerData(string title)
		{
			if (m_PlayerDataMap.ContainsKey2(title) == false)
				Debug.LogError("잘못된 Player Title 입니다. Title: " + title);

			m_PlayerData = m_PlayerDataMap[title];
		}

		private void SpawnPlayer()
		{
			m_Player = GetBuilder(m_PlayerData.title)
				.SetAutoInit(true)
				.SetActive(true)
				.Spawn();
		}

		public void AddXp(float xp)
		{
			m_Player.AddXp(xp);
		}
		public float GetRequiredXp(int currentLevel)
		{
			return m_PlayerLevelMap[currentLevel];
		}
	}
}