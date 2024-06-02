using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuffDebuff
{
	public class EnemyManager : ObjectManager<EnemyManager, Enemy>
	{
		private enum E_EnemyType
		{
			PrototypeHero,
			Golem,
			DemonSlime,
		}

		[SerializeField]
		private E_EnemyType m_Debug_SpawnEnemyType;

		public override void Initialize()
		{
			base.Initialize();
		}
		public override void Finallize()
		{
			base.Finallize();
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

				Enemy enemy = GetBuilder(m_Debug_SpawnEnemyType.ToString())
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
}