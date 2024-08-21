using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using DataDictionary = DoubleKeyDictionary<int, string, (BuffDebuff.Enemy enemy, BuffDebuff.EnemyData data)>;

namespace BuffDebuff
{
	public class EnemyManager : ObjectManager<EnemyManager, Enemy>
	{
		#region Enum
		private enum E_EnemyType
		{
			PrototypeHero,
			Golem,
			DemonSlime,
		}
		#endregion

		#region 변수
		[SerializeField]
		private E_EnemyType m_Debug_SpawnEnemyType;

		private DataDictionary m_EnemyDataMap = null;
		#endregion

		public override void Initialize()
		{
			base.Initialize();

			if (m_EnemyDataMap == null)
				m_EnemyDataMap = new DataDictionary();

			LoadAllEnemyData();
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

		private void LoadAllEnemyData()
		{
			string path = EnemySOManager.resourcesPath;
			EnemyData[] enemyDatas = Resources.LoadAll<EnemyData>(path);

			for (int i = 0; i < enemyDatas.Length; ++i)
			{
				EnemyData enemyData = enemyDatas[i];

				if (m_EnemyDataMap.ContainsKey1(enemyData.code) == true)
					continue;

				// 적 원본 로드
				Enemy origin = Resources.Load<Enemy>(enemyData.assetPath);

				// 초기 스탯 설정
				m_ObjectPoolMap[enemyData.title].onItemInstantiated += (Enemy enemy) =>
				{
					enemy.SetInitStat(EnemyStat.Clone(enemyData));
				};

				// 딕셔너리에 추가
				m_EnemyDataMap.Add(enemyData.code, enemyData.title, (origin, enemyData));
			}
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

		[ContextMenu("Load Origin")]
		protected override void LoadOrigin()
		{
			base.LoadOrigin_Inner();
		}
	}
}