using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollisionChecker2D))]
public class ProjectileController : RaycastController
{
	#region 변수
	#endregion

	#region 프로퍼티

	#endregion

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