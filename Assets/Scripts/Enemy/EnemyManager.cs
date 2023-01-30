using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : ObjectManager<EnemyManager, Enemy>
{
	protected override void Awake()
	{
		base.Awake();

		AddPool("Enemy", transform);
	}

	private void Update()
	{
		#region temp
		if (Input.GetKeyDown(KeyCode.Space))
		{
			Vector2 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);

			Enemy enemy = Spawn("Enemy");
			enemy.transform.position = position;
		}
		#endregion
	}
}