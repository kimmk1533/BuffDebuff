using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileManager : ObjectManager<ProjectileManager, Projectile>
{
	public override void Initialize()
	{
		base.Initialize();
	}
	public override void Finallize()
	{
		base.Finallize();
	}

	public override void InitializeGame()
	{
		base.InitializeGame();
	}
	public override void FinallizeGame()
	{
		base.FinallizeGame();
	}

#if UNITY_EDITOR
	[ContextMenu("Load Origin")]
	protected override void LoadOrigin()
	{
		base.LoadOrigin_Inner();
	}
#endif
}