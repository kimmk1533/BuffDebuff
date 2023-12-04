using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : ObjectManager<EnemyManager, Enemy>
{
	public override void Initialize()
	{
		base.Initialize();


	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Tab))
		{
			Vector2 position = UtilClass.GetMouseWorldPosition();
			
			Enemy enemy = Spawn("Golem");

			enemy.gameObject.SetActive(true);

			enemy.Initialize();

			enemy.transform.position = position;
		}
	}
}