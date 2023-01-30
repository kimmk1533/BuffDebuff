using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileManager : ObjectManager<ProjectileManager, Projectile>
{
	public Projectile Spawn(string key, GameObject target, E_MovingType movingType = E_MovingType.None)
	{
		Projectile item = Spawn(key);
		item.m_Target = target;
		item.m_MovingType = movingType;
		return item;
	}
	public Projectile Spawn(string key, Transform parent, GameObject target, E_MovingType movingType = E_MovingType.None)
	{
		Projectile item = Spawn(key, parent);
		item.m_Target = target;
		item.m_MovingType = movingType;
		return item;
	}
	public Projectile Spawn(string key, Vector3 position, Quaternion rotation, GameObject target, E_MovingType movingType = E_MovingType.None)
	{
		Projectile item = Spawn(key, position, rotation);
		item.m_Target = target;
		item.m_MovingType = movingType;
		return item;
	}
	public Projectile Spawn(string key, Vector3 position, Quaternion rotation, Transform parent, GameObject target, E_MovingType movingType = E_MovingType.None)
	{
		Projectile item = Spawn(key, position, rotation, parent);
		item.m_Target = target;
		item.m_MovingType = movingType;
		return item;
	}

	protected override void Awake()
	{
		base.Awake();

		AddPool("Projectile", transform);
	}

	private void Update()
	{
		#region temp
		if (Input.GetMouseButtonDown(0))
		{
			Vector2 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);

			Vector2 direction = (Vector2)m_Origin.m_Target.transform.position - position;
			direction.Normalize();
			float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

			Spawn("Projectile", position, Quaternion.Euler(new Vector3(0f, 0f, angle - 90f)), m_Origin.m_Target);
		}
		#endregion
	}
}