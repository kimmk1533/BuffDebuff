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

			Enemy enemy = GetBuilder("Golem")
				.SetActive(true)
				.SetAutoInit(true)
				.SetParent(null)
				.SetPosition(position)
				.Spawn();
		}
	}
}