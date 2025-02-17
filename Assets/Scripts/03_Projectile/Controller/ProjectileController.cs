using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : RaycastController
{
	public override void Initialize()
	{
		base.Initialize();
	}

	public void Move(Vector2 moveAmount)
	{
		UpdateRaycastOrigins();

		transform.Translate(moveAmount, Space.World);
	}
}