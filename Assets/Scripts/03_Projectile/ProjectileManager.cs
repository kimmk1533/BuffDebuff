using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileManager : ObjectManager<ProjectileManager, Projectile>
{
	public override void Initialize(bool autoInit = true)
	{
		base.Initialize(autoInit);
	}
}