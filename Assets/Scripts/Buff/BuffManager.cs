using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GreenerGames;
using GoogleSheetsToUnity;

public sealed class BuffManager : Singleton<BuffManager>
{
	[SerializeField, ReadOnly]
	private string m_AssociatedSheet = "1vDHbPcXSj3IUdHfGhmYbG7bse7FmDJKVRJ8DDd-slDQ";
	[SerializeField, ReadOnly]
	private string m_AssociatedWorkSheet = "버프 목록";

	[SerializeField]
	private string m_StartCell = "C1";
	[SerializeField]
	private string m_EndCell = "J55";

	// [ 코드, 버프 ]
	private SecondaryKeyDictionary<int, string, Buff> m_BuffDictionary;
#if UNITY_EDITOR
	[SerializeField, ReadOnly]
	private DebugDictionary<string, int> m_Debug1;
	[SerializeField, ReadOnly]
	private DebugDictionary<int, Buff> m_Debug2;
#endif

	private void Awake()
	{
		Initialize();

		// 엑셀에서 읽어온 데이터로 딕셔너리에 버프 생성
		LoadBuffAll();
	}

	private void Initialize()
	{
		if (m_BuffDictionary == null)
		{
			m_BuffDictionary = new SecondaryKeyDictionary<int, string, Buff>();
		}
		else
		{
			m_BuffDictionary.primaryDictionary.Clear();
			m_BuffDictionary.secondaryKeyLink.Clear();
		}

#if UNITY_EDITOR
		if (m_Debug1 == null)
		{
			m_Debug1 = new DebugDictionary<string, int>();
		}
		else
		{
			m_Debug1.Clear();
		}
		if (m_Debug2 == null)
		{
			m_Debug2 = new DebugDictionary<int, Buff>();
		}
		else
		{
			m_Debug2.Clear();
		}
#endif
	}

	[ContextMenu("LoadBuff")]
	private void LoadBuffAll()
	{
		Initialize();

		string titleColumn = m_StartCell.Substring(0, 1);
		int titleRow = int.Parse(m_StartCell.Substring(1, m_StartCell.Length - 1));
		GSTU_Search search = new GSTU_Search(m_AssociatedSheet, m_AssociatedWorkSheet, m_StartCell, m_EndCell, titleColumn, titleRow);

		SpreadsheetManager.Read(search, LoadBuffCallBack, true);
	}
	private void LoadBuffCallBack(GstuSpreadSheet spreadSheet)
	{
		LoadBuff(spreadSheet);
		CodeBuff();
	}
	private void LoadBuff(GstuSpreadSheet spreadSheet)
	{
		Debug.Log("Load Buff is Started.");

		bool first = true;
		foreach (var item in spreadSheet.rows.primaryDictionary)
		{
			if (first)
			{
				first = false;
				continue;
			}

			var list = item.Value;

			// 명칭
			string name = list[0].value;

			// 코드
			int.TryParse(list[1].value, out int code);

			// 버프 종류
			string typeStr = list[2].value;
			switch (typeStr)
			{
				case "버프":
					typeStr = "Buff";
					break;
				case "디버프":
					typeStr = "Debuff";
					break;
			}
			System.Enum.TryParse(typeStr, out E_BuffType type);

			// 효과 종류
			string effectTypeStr = list[3].value;
			switch (effectTypeStr)
			{
				case "스탯형":
					effectTypeStr = "Stat";
					break;
				case "무기형":
					effectTypeStr = "Weapon";
					break;
				case "전투형":
					effectTypeStr = "Combat";
					break;
			}
			System.Enum.TryParse(effectTypeStr, out E_BuffEffectType effectType);

			// 등급
			System.Enum.TryParse(list[4].value, out E_BuffGrade grade);

			// 최대 스택
			int.TryParse(list[5].value, out int maxStack);

			// 적용 무기
			string weaponStr = list[6].value;
			switch (weaponStr)
			{
				case "공통":
					weaponStr = "All";
					break;
				case "근거리 무기":
					weaponStr = "Melee";
					break;
				case "원거리 무기":
					weaponStr = "Ranged";
					break;
			}
			System.Enum.TryParse(weaponStr, out E_BuffWeapon weapon);

			// 설명
			string description = list[8].value;

			BuffData buffData = new BuffData(name, code, type, effectType, grade, maxStack, weapon, description);
			Buff buff = new Buff(buffData);

			m_BuffDictionary.Add(code, buff, name);

#if UNITY_EDITOR
			m_Debug1.Add(name, code);
			m_Debug2.Add(code, buff);
#endif
		}

		Debug.Log("Load Buff is Completed.");
	}
	private void CodeBuff()
	{
		#region 001. 체력 증가
		m_BuffDictionary["체력 증가"]
			.OnBuffInitialize.OnBuffEvent += (Character character) =>
			{
				character.m_CurrentStat.MaxHP += 100f;
				//character.m_BuffStat.MaxHP += 100f;
			};
		m_BuffDictionary["체력 증가"]
			.OnBuffFinalize.OnBuffEvent += (Character character) =>
			{
				character.m_CurrentStat.MaxHP -= 100f;
				//character.m_BuffStat.MaxHP -= 100f;
			};
		#endregion

		#region 003. 재생
		m_BuffDictionary["재생"]
			.OnBuffInitialize.OnBuffEvent += (Character character) =>
			{
				character.m_CurrentStat.HPregen = 10f;
			};
		m_BuffDictionary["재생"]
			.OnBuffFinalize.OnBuffEvent += (Character character) =>
			{
				character.m_CurrentStat.HPregen = 0f;
			};
		#endregion

		#region 004. 빠른 재생
		m_BuffDictionary["빠른 재생"]
			.OnBuffInitialize.OnBuffEvent += (Character character) =>
			{
				character.m_CurrentStat.HPregenCooldown -= 1f;
				character.m_HealTimer.interval = character.m_CurrentStat.HPregenCooldown;
			};
		m_BuffDictionary["빠른 재생"]
			.OnBuffFinalize.OnBuffEvent += (Character character) =>
			{
				character.m_CurrentStat.HPregenCooldown += 1f;
				character.m_HealTimer.interval = character.m_CurrentStat.HPregenCooldown;
			};
		#endregion

		#region 005. 느린 재생
		m_BuffDictionary["느린 재생"]
			.OnBuffInitialize.OnBuffEvent += (Character character) =>
			{
				character.m_CurrentStat.HPregenCooldown += 1f;
				character.m_HealTimer.interval = character.m_CurrentStat.HPregenCooldown;
			};
		m_BuffDictionary["느린 재생"]
			.OnBuffFinalize.OnBuffEvent += (Character character) =>
			{
				character.m_CurrentStat.HPregenCooldown -= 1f;
				character.m_HealTimer.interval = character.m_CurrentStat.HPregenCooldown;
			};
		#endregion
	}

	public Buff GetBuff(int key)
	{
		return m_BuffDictionary[key];
	}
	public Buff GetBuff(string key)
	{
		return m_BuffDictionary[key];
	}
}