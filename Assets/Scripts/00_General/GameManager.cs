using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
	// Game Managers
	private PlayerManager M_Player => PlayerManager.Instance;
	private EnemyManager M_Enemy => EnemyManager.Instance;
	private ProjectileManager M_Projectile => ProjectileManager.Instance;

	private SpreadSheet.SpreadSheetManager M_SpreadSheet => SpreadSheet.SpreadSheetManager.Instance;
	private BuffManager M_Buff => BuffManager.Instance;

	private StageManager M_Stage => StageManager.Instance;
	private WarpManager M_Warp => WarpManager.Instance;
	private GridManager M_Grid => GridManager.Instance;

	// UI Managers
	private BuffUIManager M_BuffUI => BuffUIManager.Instance;

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

		M_Stage.Initialize();
		M_Warp.Initialize();
		M_Grid.Initialize();

		// UI Init
		M_BuffUI.Initialize();
	}
	public void InitializeEvent()
	{
		M_Buff.InitializeEvent();

		M_Player.InitializeEvent();

		M_BuffUI.InitializeEvent();
	}
}