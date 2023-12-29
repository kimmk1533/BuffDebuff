using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
	#region 변수
	private static UICamera M_UICamera => UICamera.Instance;
	#endregion

	#region 매니저
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

	//private void Awake()
	//{
	//	Initialize();
	//	InitializeGame();
	//	InitializeEvent();
	//}

	public void Initialize()
	{
		// UI 카메라 초기화
		M_UICamera.Initialize();

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

		// 워프 매니저 초기화
		M_Warp.Initialize();
		// 방 매니저 초기화
		M_Room.Initialize();
		// 스테이지 매니저 초기화
		M_Stage.Initialize();
		// 그리드 매니저 초기화
		M_Grid.Initialize();
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

		// 워프 매니저 초기화
		M_Warp.InitializeGame();
		// 방 매니저 초기화
		M_Room.InitializeGame();
		// 스테이지 매니저 초기화
		M_Stage.InitializeGame();
		// 그리드 매니저 초기화
		M_Grid.InitializeGame();

		// 버프 UI 매니저 초기화
		M_BuffUI.Initialize();
	}
	public void InitializeEvent()
	{
		#region Buff Event
		M_Buff.InitializeBuffEvent();
		M_Player.InitializeBuffEvent();
		M_BuffUI.InitializeBuffEvent();
		#endregion
	}
}