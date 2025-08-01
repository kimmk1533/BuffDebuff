using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BuffDebuff
{
	public class GameManager : SerializedSingleton<GameManager>
	{
		#region 기본 템플릿
		#region 변수
		private bool m_IsInGame = false;
		#endregion

		#region 프로퍼티
		public bool isInGame => m_IsInGame;
		#endregion

		#region 이벤트

		#region 이벤트 함수
		#endregion
		#endregion

		#region 매니저
		// System Managers
		private static InputManager M_Input => InputManager.Instance;

		// Game Managers
		private static PlayerManager M_Player => PlayerManager.Instance;
		private static EnemyManager M_Enemy => EnemyManager.Instance;
		private static ProjectileManager M_Projectile => ProjectileManager.Instance;

		private static BuffManager M_Buff => BuffManager.Instance;

		private static RoomManager M_Room => RoomManager.Instance;
		private static StageManager M_Stage => StageManager.Instance;
		private static WarpManager M_Warp => WarpManager.Instance;

		// UI Managers
		private static BuffUIManager M_BuffUI => BuffUIManager.Instance;
		#endregion

		#region 초기화 & 마무리화 함수
		/// <summary>
		/// 초기화 함수 (Init Scene 진입 시, 즉 게임 실행 시 호출)
		/// </summary>
		public override void Initialize()
		{
			base.Initialize();
			base.InitializeMain();

			// 인풋 매니저 초기화
			M_Input.Initialize();
			M_Input.InitializeMain();

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

			// 버프 UI 매니저 초기화
			M_BuffUI.Initialize();
		}
		/// <summary>
		/// 마무리화 함수 (게임 종료 시 호출)
		/// </summary>
		public override void Finallize()
		{
			base.FinallizeMain();
			base.Finallize();

			// 버프 UI 매니저 마무리화
			M_BuffUI.Finallize();

			// 스테이지 매니저 마무리화
			M_Stage.Finallize();
			// 워프 매니저 마무리화
			M_Warp.Finallize();
			// 방 매니저 마무리화
			M_Room.Finallize();

			// 버프 매니저 마무리화
			M_Buff.Finallize();
			// 투사체 매니저 마무리화
			M_Projectile.Finallize();
			// 적 매니저 마무리화
			M_Enemy.Finallize();
			// 플레이어 매니저 마무리화
			M_Player.Finallize();

			// 인풋 매니저 마무리화
			M_Input.FinallizeMain();
			M_Input.Finallize();
		}

		/// <summary>
		/// 메인 초기화 함수 (Main Menu Scene 진입 시 호출)
		/// </summary>
		public void InitializeOnMainMenuScene()
		{

		}
		/// <summary>
		/// 메인 마무리화 함수 (Main Menu Scene 나갈 시 호출)
		/// </summary>
		public void FinallizeOnMainMenuScene()
		{

		}

		/// <summary>
		/// 메인 초기화 함수 (Game Scene 진입 시 호출)
		/// </summary>
		public void InitializeOnGameScene()
		{
			// 플레이어 매니저 초기화
			M_Player.InitializeMain();
			// 적 매니저 초기화
			M_Enemy.InitializeMain();
			// 투사체 매니저 초기화
			M_Projectile.InitializeMain();
			// 버프 매니저 초기화
			M_Buff.InitializeMain();

			// 방 매니저 초기화
			M_Room.InitializeMain();
			// 워프 매니저 초기화
			M_Warp.InitializeMain();
			// 스테이지 매니저 초기화
			M_Stage.InitializeMain();

			// 버프 UI 매니저 초기화
			M_BuffUI.InitializeMain();


			m_IsInGame = true;
		}
		/// <summary>
		/// 메인 마무리화 함수 (Game Scene 나갈 시 호출)
		/// </summary>
		public void FinallizeOnGameScene()
		{
			// 버프 UI 매니저 마무리화
			M_BuffUI.FinallizeMain();

			// 스테이지 매니저 마무리화
			M_Stage.FinallizeMain();
			// 워프 매니저 마무리화
			M_Warp.FinallizeMain();
			// 방 매니저 마무리화
			M_Room.FinallizeMain();

			// 버프 매니저 마무리화
			M_Buff.FinallizeMain();
			// 투사체 매니저 마무리화
			M_Projectile.FinallizeMain();
			// 적 매니저 마무리화
			M_Enemy.FinallizeMain();
			// 플레이어 매니저 마무리화
			M_Player.FinallizeMain();

			m_IsInGame = false;
		}
		#endregion

		#region 유니티 콜백 함수
		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				SceneLoader.LoadScene("Main Menu Scene");
			}
		}
		#endregion
		#endregion
	}
}