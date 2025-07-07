using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using DataDictionary = DoubleKeyDictionary<int, string, BuffDebuff.PlayerData>;
using LevelDictionary = System.Collections.Generic.Dictionary<int, float>;

namespace BuffDebuff
{
	public class PlayerManager : ObjectManager<PlayerManager, Player>
	{
		#region 기본 템플릿
		#region 변수
		public static readonly int playerMaxLevel = 100;

		private PlayerData m_PlayerData = null;
		private Player m_Player = null;
		private CameraFollow m_PlayerCamera = null;

		private DataDictionary m_PlayerDataMap = null;
		// 현재 레벨, 다음 레벨까지 필요 경험치
		private LevelDictionary m_PlayerLevelMap = null;
		#endregion

		#region 프로퍼티
		public Player player => m_Player;
		public int currentLevel
		{
			get
			{
				if (m_Player == null)
					return 1;

				return currentStat.Level.current;
			}
		}

		protected PlayerStat currentStat => m_Player.currentStat;
		#endregion

		#region 매니저
		private static BuffManager M_Buff => BuffManager.Instance;

		private static StageManager M_Stage => StageManager.Instance;
		#endregion

		#region 이벤트
		#endregion

		#region 이벤트 함수
		private void OnStageGenerated()
		{
			Room room = M_Stage.currentStage.currentRoom;
			StartRoom startRoom = room as StartRoom;

			if (startRoom != null)
				m_Player.transform.position = startRoom.startPos;
			else
				m_Player.transform.position = new Vector3(20f, 4f);
		}
		#endregion

		#region 초기화 & 마무리화 함수
		/// <summary>
		/// 기본 초기화 함수 (Init Scene 진입 시, 즉 게임 실행 시 호출)
		/// </summary>
		public override void Initialize()
		{
			base.Initialize();

			if (m_PlayerDataMap == null)
				m_PlayerDataMap = new DataDictionary();
			if (m_PlayerLevelMap == null)
				m_PlayerLevelMap = new LevelDictionary();

			LoadAllLevelData();
			LoadAllPlayerData();
		}
		/// <summary>
		/// 기본 마무리화 함수 (게임 종료 시 호출)
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

			SpawnPlayer();
			SetBuffEvent();

			Camera.main.NullCheckGetComponent<CameraFollow>(ref m_PlayerCamera);
			m_PlayerCamera.Initialize();

			M_Stage.onStageGenerated += OnStageGenerated;
		}
		/// <summary>
		/// 메인 마무리화 함수 (본인 Main Scene 나갈 시 호출)
		/// </summary>
		public override void FinallizeMain()
		{
			base.FinallizeMain();

			m_PlayerCamera.Finallize();
			m_PlayerCamera = null;

			m_Player = null;

			M_Stage.onStageGenerated -= OnStageGenerated;
		}
		#endregion

		#region 유니티 콜백 함수
		#endregion
		#endregion

		private void SetBuffEvent()
		{
			M_Buff.inventory.AddOnBuffAddedEvent("체력 증가", (Buff buff) =>
			{
				BuffData buffData = buff.buffData;
				float value = buffData.buffValue;

				StatValue<float> hp = currentStat.Hp;

				if (buffData.buffValueType == Enum.E_BuffValueType.Plus)
				{
					hp.max += value;
					hp.current += value;
				}
				else if (buffData.buffValueType == Enum.E_BuffValueType.Multiply)
				{
					hp.max *= value;
					hp.current *= value;
				}

				currentStat.Hp = hp;
			});
			M_Buff.inventory.AddOnBuffRemovedEvent("체력 증가", (Buff buff) =>
			{
				BuffData buffData = buff.buffData;
				float value = buffData.buffValue;

				StatValue<float> hp = currentStat.Hp;

				if (buffData.buffValueType == Enum.E_BuffValueType.Plus)
				{
					hp.max -= value;
					hp.current -= value;
				}
				else if (buffData.buffValueType == Enum.E_BuffValueType.Multiply)
				{
					hp.max /= value;
					hp.current /= value;
				}

				currentStat.Hp = hp;
			});

			M_Buff.inventory.AddOnBuffAddedEvent("재생", (Buff buff) =>
			{
				BuffData buffData = buff.buffData;
				float value = buffData.buffValue;

				if (buffData.buffValueType == Enum.E_BuffValueType.Plus)
				{
					currentStat.HpRegen += value;
				}
				else if (buffData.buffValueType == Enum.E_BuffValueType.Multiply)
				{
					currentStat.HpRegen *= value;
				}
			});
			M_Buff.inventory.AddOnBuffRemovedEvent("재생", (Buff buff) =>
			{
				BuffData buffData = buff.buffData;
				float value = buffData.buffValue;

				if (buffData.buffValueType == Enum.E_BuffValueType.Plus)
				{
					currentStat.HpRegen -= value;
				}
				else if (buffData.buffValueType == Enum.E_BuffValueType.Multiply)
				{
					currentStat.HpRegen /= value;
				}
			});

			M_Buff.inventory.AddOnBuffAddedEvent("빠른 재생", (Buff buff) =>
			{
				BuffData buffData = buff.buffData;
				float value = buffData.buffValue;

				if (buffData.buffValueType == Enum.E_BuffValueType.Plus)
				{
					currentStat.HpRegenTime += value;
				}
				else if (buffData.buffValueType == Enum.E_BuffValueType.Multiply)
				{
					currentStat.HpRegenTime *= value;
				}
			});
			M_Buff.inventory.AddOnBuffRemovedEvent("빠른 재생", (Buff buff) =>
			{
				BuffData buffData = buff.buffData;
				float value = buffData.buffValue;

				if (buffData.buffValueType == Enum.E_BuffValueType.Plus)
				{
					currentStat.HpRegenTime -= value;
				}
				else if (buffData.buffValueType == Enum.E_BuffValueType.Multiply)
				{
					currentStat.HpRegenTime /= value;
				}
			});

			M_Buff.inventory.AddOnBuffAddedEvent("회복량 증가", (Buff buff) =>
			{
				BuffData buffData = buff.buffData;
				float value = buffData.buffValue;

				if (buffData.buffValueType == Enum.E_BuffValueType.Plus)
				{
					currentStat.HealScale += value;
				}
				else if (buffData.buffValueType == Enum.E_BuffValueType.Multiply)
				{
					currentStat.HealScale *= value;
				}
			});
			M_Buff.inventory.AddOnBuffRemovedEvent("회복량 증가", (Buff buff) =>
			{
				BuffData buffData = buff.buffData;
				float value = buffData.buffValue;

				if (buffData.buffValueType == Enum.E_BuffValueType.Plus)
				{
					currentStat.HealScale -= value;
				}
				else if (buffData.buffValueType == Enum.E_BuffValueType.Multiply)
				{
					currentStat.HealScale /= value;
				}
			});

			M_Buff.inventory.AddOnBuffAddedEvent("공격력 증가", (Buff buff) =>
			{
				BuffData buffData = buff.buffData;
				float value = buffData.buffValue;

				if (buffData.buffValueType == Enum.E_BuffValueType.Plus)
				{
					currentStat.AttackPower += value;
				}
				else if (buffData.buffValueType == Enum.E_BuffValueType.Multiply)
				{
					currentStat.AttackPower *= value;
				}
			});
			M_Buff.inventory.AddOnBuffRemovedEvent("공격력 증가", (Buff buff) =>
			{
				BuffData buffData = buff.buffData;
				float value = buffData.buffValue;

				if (buffData.buffValueType == Enum.E_BuffValueType.Plus)
				{
					currentStat.AttackPower -= value;
				}
				else if (buffData.buffValueType == Enum.E_BuffValueType.Multiply)
				{
					currentStat.AttackPower /= value;
				}
			});

			M_Buff.inventory.AddOnBuffAddedEvent("공격 속도 증가", (Buff buff) =>
			{
				BuffData buffData = buff.buffData;
				float value = buffData.buffValue;

				if (buffData.buffValueType == Enum.E_BuffValueType.Plus)
				{
					m_Player.attackSpeed += value;
				}
				else if (buffData.buffValueType == Enum.E_BuffValueType.Multiply)
				{
					m_Player.attackSpeed *= value;
				}
			});
			M_Buff.inventory.AddOnBuffRemovedEvent("공격 속도 증가", (Buff buff) =>
			{
				BuffData buffData = buff.buffData;
				float value = buffData.buffValue;

				if (buffData.buffValueType == Enum.E_BuffValueType.Plus)
				{
					m_Player.attackSpeed -= value;
				}
				else if (buffData.buffValueType == Enum.E_BuffValueType.Multiply)
				{
					m_Player.attackSpeed /= value;
				}
			});

			M_Buff.inventory.AddOnBuffAddedEvent("사거리 증가", (Buff buff) =>
			{
				BuffData buffData = buff.buffData;
				float value = buffData.buffValue;

				if (buffData.buffValueType == Enum.E_BuffValueType.Plus)
				{
					currentStat.AttackRange += value;
				}
				else if (buffData.buffValueType == Enum.E_BuffValueType.Multiply)
				{
					currentStat.AttackRange *= value;
				}
			});
			M_Buff.inventory.AddOnBuffRemovedEvent("사거리 증가", (Buff buff) =>
			{
				BuffData buffData = buff.buffData;
				float value = buffData.buffValue;

				if (buffData.buffValueType == Enum.E_BuffValueType.Plus)
				{
					currentStat.AttackRange -= value;
				}
				else if (buffData.buffValueType == Enum.E_BuffValueType.Multiply)
				{
					currentStat.AttackRange /= value;
				}
			});

			M_Buff.inventory.AddOnBuffAddedEvent("투사체 속도 증가", (Buff buff) =>
			{
				BuffData buffData = buff.buffData;
				float value = buffData.buffValue;

				if (buffData.buffValueType == Enum.E_BuffValueType.Plus)
				{
					currentStat.ShotSpeed += value;
				}
				else if (buffData.buffValueType == Enum.E_BuffValueType.Multiply)
				{
					currentStat.ShotSpeed *= value;
				}
			});
			M_Buff.inventory.AddOnBuffRemovedEvent("투사체 속도 증가", (Buff buff) =>
			{
				BuffData buffData = buff.buffData;
				float value = buffData.buffValue;

				if (buffData.buffValueType == Enum.E_BuffValueType.Plus)
				{
					currentStat.ShotSpeed -= value;
				}
				else if (buffData.buffValueType == Enum.E_BuffValueType.Multiply)
				{
					currentStat.ShotSpeed /= value;
				}
			});

			M_Buff.inventory.AddOnBuffAddedEvent("공격 횟수 증가", (Buff buff) =>
			{
				BuffData buffData = buff.buffData;
				float value = buffData.buffValue;

				if (buffData.buffValueType == Enum.E_BuffValueType.Plus)
				{
					currentStat.MultiHitCount += Mathf.RoundToInt(value);
				}
				else if (buffData.buffValueType == Enum.E_BuffValueType.Multiply)
				{
					currentStat.MultiHitCount *= Mathf.RoundToInt(value);
				}
			});
			M_Buff.inventory.AddOnBuffRemovedEvent("공격 횟수 증가", (Buff buff) =>
			{
				BuffData buffData = buff.buffData;
				float value = buffData.buffValue;

				if (buffData.buffValueType == Enum.E_BuffValueType.Plus)
				{
					currentStat.MultiHitCount -= Mathf.RoundToInt(value);
				}
				else if (buffData.buffValueType == Enum.E_BuffValueType.Multiply)
				{
					currentStat.MultiHitCount /= Mathf.RoundToInt(value);
				}
			});

			M_Buff.inventory.AddOnBuffAddedEvent("치명타 확률 증가", (Buff buff) =>
			{
				BuffData buffData = buff.buffData;
				float value = buffData.buffValue;

				if (buffData.buffValueType == Enum.E_BuffValueType.Plus)
				{
					currentStat.CriticalRate += value;
				}
				else if (buffData.buffValueType == Enum.E_BuffValueType.Multiply)
				{
					currentStat.CriticalRate *= value;
				}
			});
			M_Buff.inventory.AddOnBuffRemovedEvent("치명타 확률 증가", (Buff buff) =>
			{
				BuffData buffData = buff.buffData;
				float value = buffData.buffValue;

				if (buffData.buffValueType == Enum.E_BuffValueType.Plus)
				{
					currentStat.CriticalRate -= value;
				}
				else if (buffData.buffValueType == Enum.E_BuffValueType.Multiply)
				{
					currentStat.CriticalRate /= value;
				}
			});

			M_Buff.inventory.AddOnBuffAddedEvent("치명타 대미지 증가", (Buff buff) =>
			{
				BuffData buffData = buff.buffData;
				float value = buffData.buffValue;

				if (buffData.buffValueType == Enum.E_BuffValueType.Plus)
				{
					currentStat.CriticalDamageScale += value;
				}
				else if (buffData.buffValueType == Enum.E_BuffValueType.Multiply)
				{
					currentStat.CriticalDamageScale *= value;
				}
			});
			M_Buff.inventory.AddOnBuffRemovedEvent("치명타 대미지 증가", (Buff buff) =>
			{
				BuffData buffData = buff.buffData;
				float value = buffData.buffValue;

				if (buffData.buffValueType == Enum.E_BuffValueType.Plus)
				{
					currentStat.CriticalDamageScale -= value;
				}
				else if (buffData.buffValueType == Enum.E_BuffValueType.Multiply)
				{
					currentStat.CriticalDamageScale /= value;
				}
			});

			M_Buff.inventory.AddOnBuffAddedEvent("방어력 증가", (Buff buff) =>
			{
				BuffData buffData = buff.buffData;
				float value = buffData.buffValue;

				if (buffData.buffValueType == Enum.E_BuffValueType.Plus)
				{
					currentStat.Armor += value;
				}
				else if (buffData.buffValueType == Enum.E_BuffValueType.Multiply)
				{
					currentStat.Armor *= value;
				}
			});
			M_Buff.inventory.AddOnBuffRemovedEvent("방어력 증가", (Buff buff) =>
			{
				BuffData buffData = buff.buffData;
				float value = buffData.buffValue;

				if (buffData.buffValueType == Enum.E_BuffValueType.Plus)
				{
					currentStat.Armor -= value;
				}
				else if (buffData.buffValueType == Enum.E_BuffValueType.Multiply)
				{
					currentStat.Armor /= value;
				}
			});

			M_Buff.inventory.AddOnBuffAddedEvent("회피율 증가", (Buff buff) =>
			{
				BuffData buffData = buff.buffData;
				float value = buffData.buffValue;

				if (buffData.buffValueType == Enum.E_BuffValueType.Plus)
				{
					currentStat.Avoidability += value;
				}
				else if (buffData.buffValueType == Enum.E_BuffValueType.Multiply)
				{
					currentStat.Avoidability *= value;
				}
			});
			M_Buff.inventory.AddOnBuffRemovedEvent("회피율 증가", (Buff buff) =>
			{
				BuffData buffData = buff.buffData;
				float value = buffData.buffValue;

				if (buffData.buffValueType == Enum.E_BuffValueType.Plus)
				{
					currentStat.Avoidability -= value;
				}
				else if (buffData.buffValueType == Enum.E_BuffValueType.Multiply)
				{
					currentStat.Avoidability /= value;
				}
			});

			M_Buff.inventory.AddOnBuffAddedEvent("이동 속도 증가", (Buff buff) =>
			{
				BuffData buffData = buff.buffData;
				float value = buffData.buffValue;

				if (buffData.buffValueType == Enum.E_BuffValueType.Plus)
				{
					currentStat.MovementSpeed += value;
				}
				else if (buffData.buffValueType == Enum.E_BuffValueType.Multiply)
				{
					currentStat.MovementSpeed *= value;
				}
			});
			M_Buff.inventory.AddOnBuffRemovedEvent("이동 속도 증가", (Buff buff) =>
			{
				BuffData buffData = buff.buffData;
				float value = buffData.buffValue;

				if (buffData.buffValueType == Enum.E_BuffValueType.Plus)
				{
					currentStat.MovementSpeed -= value;
				}
				else if (buffData.buffValueType == Enum.E_BuffValueType.Multiply)
				{
					currentStat.MovementSpeed /= value;
				}
			});

			/* 추가 점프
			M_Buff.inventory.AddOnBuffAddedEvent("추가 점프", (Buff buff) =>
			{
				BuffData buffData = buff.buffData;
				float value = buffData.buffValue;

				if (buffData.buffValueType == Enum.E_BuffValueType.Plus)
				{
					currentStat. += value;
				}
				else if (buffData.buffValueType == Enum.E_BuffValueType.Multiply)
				{
					currentStat. *= value;
				}
			});
			M_Buff.inventory.AddOnBuffRemovedEvent("추가 점프", (Buff buff) =>
			{
				BuffData buffData = buff.buffData;
				float value = buffData.buffValue;

				if (buffData.buffValueType == Enum.E_BuffValueType.Plus)
				{
					currentStat. -= value;
				}
				else if (buffData.buffValueType == Enum.E_BuffValueType.Multiply)
				{
					currentStat. /= value;
				}
			});
			*/

			M_Buff.inventory.AddOnBuffAddedEvent("최대 대쉬 횟수 증가", (Buff buff) =>
			{
				BuffData buffData = buff.buffData;
				float value = buffData.buffValue;
				int intValue = Mathf.RoundToInt(value);

				if (buffData.buffValueType == Enum.E_BuffValueType.Plus)
				{
					StatValue<int> dashCount = currentStat.DashCount;

					dashCount.max += intValue;
					dashCount.current += intValue;

					currentStat.DashCount = dashCount;
				}
				else if (buffData.buffValueType == Enum.E_BuffValueType.Multiply)
				{
					StatValue<int> dashCount = currentStat.DashCount;

					dashCount.max *= intValue;
					dashCount.current *= intValue;

					currentStat.DashCount = dashCount;
				}
			});
			M_Buff.inventory.AddOnBuffRemovedEvent("최대 대쉬 횟수 증가", (Buff buff) =>
			{
				BuffData buffData = buff.buffData;
				float value = buffData.buffValue;
				int intValue = Mathf.RoundToInt(value);

				if (buffData.buffValueType == Enum.E_BuffValueType.Plus)
				{
					StatValue<int> dashCount = currentStat.DashCount;

					dashCount.max -= intValue;
					dashCount.current -= intValue;

					currentStat.DashCount = dashCount;
				}
				else if (buffData.buffValueType == Enum.E_BuffValueType.Multiply)
				{
					StatValue<int> dashCount = currentStat.DashCount;

					dashCount.max /= intValue;
					dashCount.current /= intValue;

					currentStat.DashCount = dashCount;
				}
			});

			M_Buff.inventory.AddOnBuffAddedEvent("대쉬 충전 속도 증가", (Buff buff) =>
			{
				BuffData buffData = buff.buffData;
				float value = buffData.buffValue;

				if (buffData.buffValueType == Enum.E_BuffValueType.Plus)
				{
					currentStat.DashRechargeTime += value;
				}
				else if (buffData.buffValueType == Enum.E_BuffValueType.Multiply)
				{
					currentStat.DashRechargeTime *= value;
				}
			});
			M_Buff.inventory.AddOnBuffRemovedEvent("대쉬 충전 속도 증가", (Buff buff) =>
			{
				BuffData buffData = buff.buffData;
				float value = buffData.buffValue;

				if (buffData.buffValueType == Enum.E_BuffValueType.Plus)
				{
					currentStat.DashRechargeTime -= value;
				}
				else if (buffData.buffValueType == Enum.E_BuffValueType.Multiply)
				{
					currentStat.DashRechargeTime /= value;
				}
			});

			/* 템플릿
			M_Buff.inventory.AddOnBuffAddedEvent("", (Buff buff) =>
			{
				BuffData buffData = buff.buffData;
				float value = buffData.buffValue;
			
				if (buffData.buffValueType == Enum.E_BuffValueType.Plus)
				{
					currentStat. += value;
				}
				else if (buffData.buffValueType == Enum.E_BuffValueType.Multiply)
				{
					currentStat. *= value;
				}
			});
			M_Buff.inventory.AddOnBuffRemovedEvent("", (Buff buff) =>
			{
				BuffData buffData = buff.buffData;
				float value = buffData.buffValue;
			
				if (buffData.buffValueType == Enum.E_BuffValueType.Plus)
				{
					currentStat. -= value;
				}
				else if (buffData.buffValueType == Enum.E_BuffValueType.Multiply)
				{
					currentStat. /= value;
				}
			});
			*/
		}

		private void LoadAllPlayerData()
		{
			string path = PlayerSOManager.resourcesPath;
			PlayerData[] playerDatas = Resources.LoadAll<PlayerData>(path);

			for (int i = 0; i < playerDatas.Length; ++i)
			{
				PlayerData playerData = playerDatas[i];

				if (m_PlayerDataMap.ContainsKey1(playerData.code) == true)
					continue;

				// 플레이어 원본 로드
				Player origin = Resources.Load<Player>(playerData.assetPath);

				// 로드 예외 처리
				if (origin == null)
				{
					Debug.LogError("플레이어 원본이 null임.");
					return;
				}

				// 초기 스탯 설정
				m_ObjectPoolMap[playerData.title].onItemInstantiated += (Player player) =>
				{
					PlayerStat playerStat = PlayerStat.Clone(playerData);

					StatValue<float> tempXp = playerStat.Xp;
					tempXp.max = m_PlayerLevelMap[playerStat.Level.current];
					playerStat.Xp = tempXp;

					player.initStat = playerStat;
				};

				m_PlayerDataMap.Add(playerData.code, playerData.title, playerData);
			}
		}
		private void LoadAllLevelData()
		{
			string path = LevelSOManager.resourcesPath;
			LevelData[] levelDatas = Resources.LoadAll<LevelData>(path);

			for (int i = 0; i < levelDatas.Length; ++i)
			{
				LevelData levelData = levelDatas[i];

				m_PlayerLevelMap.Add(levelData.currentLevel, levelData.requiredXp);
			}
		}

		public void SelectPlayerData(int code)
		{
			if (m_PlayerDataMap.ContainsKey1(code) == false)
				Debug.LogError("잘못된 Player Code 입니다. Code: " + code);

			m_PlayerData = m_PlayerDataMap[code];
		}
		public void SelectPlayerData(string title)
		{
			if (m_PlayerDataMap.ContainsKey2(title) == false)
				Debug.LogError("잘못된 Player Title 입니다. Title: " + title);

			m_PlayerData = m_PlayerDataMap[title];
		}

		private void SpawnPlayer()
		{
			m_Player = GetBuilder(m_PlayerData.title)
				.SetAutoInit(true)
				.SetActive(true)
				.Spawn();
		}

		public void AddXp(float xp)
		{
			m_Player.AddXp(xp);
		}
		public float GetRequiredXp(int currentLevel)
		{
			return m_PlayerLevelMap[currentLevel];
		}
	}
}