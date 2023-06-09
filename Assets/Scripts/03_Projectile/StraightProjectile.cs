using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StraightProjectile : Projectile
{
	public override void Initialize(float moveSpeed, float lifeTime)
	{
		base.Initialize(moveSpeed, lifeTime);


	}

	protected override void Update()
	{
		Move();
	}

	protected override void DeSpawn()
	{
		M_Project.DeSpawn("Straight", this);
	}
	protected override void RotateToTarget()
	{

	}
	protected override void Move()
	{
		transform.position += transform.rotation * Vector2.up * m_MoveSpeed * Time.deltaTime;
	}
}