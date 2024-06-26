using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuffDebuff
{
	public class GameManager : Singleton<GameManager>
	{
		#region 변수
		private bool m_IsInGame = false;
		#endregion

		#region 프로퍼티
		public bool isInGame => m_IsInGame;
		#endregion

		#region 매니저
		// System Managers
		private static InputManager M_Input => InputManager.Instance;
		private static BuffInventory M_BuffInventory => BuffInventory.Instance;

		// Game Managers
		private static PlayerManager M_Player => PlayerManager.Instance;
		private static EnemyManager M_Enemy => EnemyManager.Instance;
		private static ProjectileManager M_Projectile => ProjectileManager.Instance;

		private static BuffManager M_Buff => BuffManager.Instance;

		private static RoomManager M_Room => RoomManager.Instance;
		private static StageManager M_Stage => StageManager.Instance;
		private static WarpManager M_Warp => WarpManager.Instance;
		private static GridManager M_Grid => GridManager.Instance;

		// UI Managers
		private static BuffUIManager M_BuffUI => BuffUIManager.Instance;
		#endregion

		private void Awake()
		{
			Initialize();
		}
		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				FinallizeGame();

				LoadingSceneManager.LoadScene("Main Menu Scene");
			}
		}
		private void OnApplicationQuit()
		{
			Finallize();
		}

		public void Initialize()
		{
			// 인풋 매니저 초기화
			M_Input.Initialize();
			// 버프 인벤토리 초기화
			M_BuffInventory.Initialize();

			// 플레이어 매니저 초기화
			M_Player.Initialize();
			// 적 매니저 초기화
			M_Enemy.Initialize();
			// 투사체 매니저 초기화
			M_Projectile.Initialize();
			// 버프 매니저 초기화
			M_Buff.Initialize();

			// 방 매니저 초기화
			M_Room.Initialize();
			// 워프 매니저 초기화
			M_Warp.Initialize();
			// 스테이지 매니저 초기화
			M_Stage.Initialize();
		}
		public void Finallize()
		{
			// 스테이지 매니저 마무리
			M_Stage.Finallize();
			// 워프 매니저 마무리
			M_Warp.Finallize();
			// 방 매니저 마무리
			M_Room.Finallize();

			// 버프 매니저 마무리
			M_Buff.Finallize();
			// 투사체 매니저 마무리
			M_Projectile.Finallize();
			// 적 매니저 마무리
			M_Enemy.Finallize();

			// 플레이어 매니저 마무리
			M_Player.Finallize();

			// 버프 인벤토리 마무리
			M_BuffInventory.Finallize();
			// 인풋 매니저 마무리
			M_Input.Finallize();
		}

		public void InitializeEvent()
		{
			// 스테이지 생성 이벤트
			M_Player.InitializeStageGenEvent();

			// 방 클리어 이벤트
			M_BuffUI.InitializeRoomClearEvent();
		}
		public void FinallizeEvent()
		{
			// 스테이지 생성 이벤트
			M_Player.FinallizeStageGenEvent();

			// 방 클리어 이벤트
			M_BuffUI.FinallizeRoomClearEvent();
		}

		public void InitializeGame()
		{
			// 플레이어 매니저 초기화
			M_Player.InitializeGame();

			// 적 매니저 초기화
			M_Enemy.InitializeGame();
			// 투사체 매니저 초기화
			M_Projectile.InitializeGame();
			// 버프 매니저 초기화
			M_Buff.InitializeGame();

			// 방 매니저 초기화
			M_Room.InitializeGame();
			// 워프 매니저 초기화
			M_Warp.InitializeGame();
			// 스테이지 매니저 초기화
			M_Stage.InitializeGame();

			// 버프 UI 매니저 초기화
			M_BuffUI.InitializeGame();

			m_IsInGame = true;
		}
		// Game Scene 벗어날 때 호출
		public void FinallizeGame()
		{
			// 버프 UI 매니저 마무리
			M_BuffUI.FinallizeGame();

			// 스테이지 매니저 마무리
			M_Stage.FinallizeGame();
			// 워프 매니저 마무리
			M_Warp.FinallizeGame();
			// 방 매니저 마무리
			M_Room.FinallizeGame();

			// 버프 매니저 마무리
			M_Buff.FinallizeGame();
			// 투사체 매니저 마무리
			M_Projectile.FinallizeGame();
			// 적 매니저 마무리
			M_Enemy.FinallizeGame();

			// 플레이어 매니저 마무리
			M_Player.FinallizeGame();

			m_IsInGame = false;
		}
	}
}