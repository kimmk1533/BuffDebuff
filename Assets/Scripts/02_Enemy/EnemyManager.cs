using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
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

		#region 기본 템플릿
		#region 변수
		[SerializeField]
		private E_EnemyType m_Debug_SpawnEnemyType;

		private DataDictionary m_EnemyDataMap = null;
		#endregion

		#region 프로퍼티
		#endregion

		#region 이벤트

		#region 이벤트 함수
		#endregion
		#endregion

		#region 매니저
		#endregion

		#region 초기화 & 마무리화 함수
		/// <summary>
		/// 초기화 함수 (Init Scene 진입 시, 즉 게임 실행 시 호출)
		/// </summary>
		public override void Initialize()
		{
			base.Initialize();

			if (m_EnemyDataMap == null)
				m_EnemyDataMap = new DataDictionary();

			LoadAllEnemyData();
		}
		/// <summary>
		/// 마무리화 함수 (게임 종료 시 호출)
		/// </summary>
		public override void Finallize()
		{
			base.Finallize();


		}

		/// <summary>
		/// 메인 초기화 함수 (본인 Main Scene 진입 시 호출)
		/// </summary>
		public override void InitializeMain()
		{
			base.InitializeMain();


		}
		/// <summary>
		/// 메인 마무리화 함수 (본인 Main Scene 나갈 시 호출)
		/// </summary>
		public override void FinallizeMain()
		{
			base.FinallizeMain();


		}
		#endregion

		#region 유니티 콜백 함수
		#endregion
		#endregion

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

				// 로드 예외 처리
				if (origin == null)
				{
					Debug.LogError(enemyData.title + " 원본이 null임.");
					return;
				}

				// 초기 스탯 설정
				m_ObjectPoolMap[enemyData.title].onItemInstantiated += (Enemy enemy) =>
				{
					enemy.initStat = EnemyStat.Clone(enemyData);
				};

				// 딕셔너리에 추가
				m_EnemyDataMap.Add(enemyData.code, enemyData.title, (origin, enemyData));
			}
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Tab))
			{
				Vector2 position = UtilClass.GetMouseWorldPosition2D();

				Enemy enemy = GetBuilder(m_Debug_SpawnEnemyType.ToString())
					.SetActive(true)
					.SetAutoInit(true)
					.SetParent(null)
					.SetPosition(position)
					.Spawn();
			}
		}
	}
}