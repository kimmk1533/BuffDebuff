using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : ObjectManager<EnemyManager, Enemy>
{
	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			Vector2 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);

			Enemy enemy = Spawn("Enemy");
			enemy.transform.position = position;
			enemy.gameObject.SetActive(true);
		}
	}
}