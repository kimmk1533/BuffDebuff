using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
	// Game Managers
	private SpreadSheet.SpreadSheetManager M_SpreadSheet => SpreadSheet.SpreadSheetManager.Instance;
	private BuffManager M_Buff => BuffManager.Instance;
	private GridManager M_Grid => GridManager.Instance;
	private StageManager M_Stage => StageManager.Instance;
	private PlayerManager M_Player => PlayerManager.Instance;
	private ProjectileManager M_Projectile => ProjectileManager.Instance;
	private EnemyManager M_Enemy => EnemyManager.Instance;

	// UI Managers
	private BuffUIManager M_BuffUI => BuffUIManager.Instance;

	private void Awake()
	{
		Initialize();
	}

	public void Initialize()
	{
		// Game Init
		//M_SpreadSheet.Initialize();

		M_Buff.Initialize();
		M_Buff.LoadAllBuff();

		M_Stage.Initialize();

		M_Grid.Initialize();

		M_Player.Initialize();

		M_Projectile.Initialize();

		M_Enemy.Initialize();

		// UI Init
		M_BuffUI.Initialize();
	}
}