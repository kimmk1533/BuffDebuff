using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

	// Game Managers
	private static PlayerManager M_Player => PlayerManager.Instance;
	private static EnemyManager M_Enemy => EnemyManager.Instance;
	private static ProjectileManager M_Projectile => ProjectileManager.Instance;

	private static SpreadSheet.SpreadSheetManager M_SpreadSheet => SpreadSheet.SpreadSheetManager.Instance;
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
	private void OnApplicationQuit()
	{
		Finallize();
	}

	public void Initialize()
	{
		// 인풋 매니저 초기화
		M_Input.Initialize();
		// 스프레드시트 매니저 초기화
		//M_SpreadSheet.Initialize();

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
		//// 그리드 매니저 초기화
		//M_Grid.Initialize();
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

		// 인풋 매니저 마무리
		M_Input.Finallize();
	}

	public void InitializeEvent()
	{
		// 스테이지 생성 이벤트
		M_Player.InitializeStageGenEvent();

		// 버프 추가 제거 이벤트
		M_Player.InitializeBuffEvent();
		M_BuffUI.InitializeBuffEvent();
	}
	public void FinallizeEvent()
	{
		// 스테이지 생성 이벤트
		M_Player.FinallizeStageGenEvent();

		// 버프 추가 제거 이벤트
		M_Player.FinallizeBuffEvent();
		M_BuffUI.FinallizeBuffEvent();
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
		//// 그리드 매니저 초기화
		//M_Grid.InitializeGame();

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