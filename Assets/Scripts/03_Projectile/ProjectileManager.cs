using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileManager : ObjectManager<ProjectileManager, Projectile>
{
	public override void Initialize()
	{
		base.Initialize();

		foreach (var item in m_Origins)
		{
			AddPool(item, transform);
		}
	}
}