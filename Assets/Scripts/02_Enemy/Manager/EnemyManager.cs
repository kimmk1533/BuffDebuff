using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BuffInventory))]
public class EnemyManager : ObjectManager<EnemyManager, Enemy>
{
	#region 변수
	private BuffInventory m_BuffInventory = null;
	#endregion

	public override void Initialize()
	{
		base.Initialize();

		this.Safe_GetComponent<BuffInventory>(ref m_BuffInventory);
		m_BuffInventory.Initialize();
	}
	public override void Finallize()
	{
		base.Finallize();

		m_BuffInventory.Finallize();
	}

	public override void InitializeGame()
	{
		base.InitializeGame();
	}
	public override void FinallizeGame()
	{
		base.FinallizeGame();
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

#if UNITY_EDITOR
	[ContextMenu("Load Origin")]
	protected override void LoadOrigin()
	{
		base.LoadOrigin_Inner();
	}
#endif
}