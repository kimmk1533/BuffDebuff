using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
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
	private BuffUIManager M_BuffUI => BuffUIManager.Instance;
	#endregion

	private void Awake()
	{
		Initialize();

		InitializeEvent();
	}

	public void Initialize()
	{
		// Game Init
		M_Player.Initialize();
		M_Enemy.Initialize();
		M_Projectile.Initialize();

		//M_SpreadSheet.Initialize();
		M_Buff.Initialize();

		M_Warp.Initialize();
		M_Room.Initialize();
		M_Stage.Initialize();
		M_Grid.Initialize();

		// UI Init
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